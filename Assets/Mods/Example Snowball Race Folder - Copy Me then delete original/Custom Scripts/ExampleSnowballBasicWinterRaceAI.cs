using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Weapons;
using NoxCore.GameModes;
using NoxCore.GUIs;
using NoxCore.Helm;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Rules;
using NoxCore.Utilities;
using NoxCore.Controllers;
using Davin.Fittings.Weapons;

namespace Example.Snowball
{
    public class ExampleSnowballBasicWinterRaceAI : BasicNavigationalAI
    {
        // reference to the race timer
        protected Timer timer;

        // reference to the race game mode
        protected RaceMode raceMode;

        // reference to the race GUI
        protected NoxRaceGUI raceGUI;

        protected bool turnRight;
        protected float ranAngle;
        protected Vector3 origEndVector;
        protected float lazyTurnLength;

        protected List<Vector2> waypoints = new List<Vector2>();
        protected int currentWaypoint;
        protected int currentLap;

        protected bool raceEnded;

        SteeringBehaviour seekBehaviour;
        ExampleSnowballAvoidBehaviour avoidBehaviour;

        List<Structure> enemiesInRange = new List<Structure>();

        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, helm);

            // subscribe to global race events
            GameEventManager.MatchIsWaitingToStart += RaceMode_RaceWaitingToStart;
            RaceMode.RacerFinished += RaceMode_RacerFinished;

            raceEnded = false;

            turnRight = Random.value > 0.5f;
            ranAngle = Random.Range(-40.0f, 40.0f) * Mathf.Deg2Rad;
            origEndVector = Vector3.zero;
            lazyTurnLength = Random.Range(2, 5);

            // get game timer
            timer = GameObject.Find("Game Manager").GetComponent<Timer>();

            // cache game mode
            raceMode = GameManager.Instance.Gamemode as RaceMode;

            // cache steering behaviours
            seekBehaviour = Helm.getBehaviourByName("SEEK");
            avoidBehaviour = Helm.getBehaviourByName("AVOID") as ExampleSnowballAvoidBehaviour;

            // if avoiding collidable objects (e.g. asteroids) set the avoid behaviour to do so
            if (avoidBehaviour != null)
            {
                LayerMask avoidLayerMask = raceMode.shipCollisionMask ^ (1 << LayerMask.NameToLayer("NavPoint")) ^ (1 << LayerMask.NameToLayer("Environmental"));
                avoidBehaviour.setCollidables(avoidLayerMask);
            }

            // get list of navpoints and sort by ascending id
            List<(GameObject navPoint, int id)> navpointsAndIDs = new List<(GameObject, int)>();            

            GameObject[] navPoints = GameObject.FindGameObjectsWithTag("NavPoint");

            foreach (GameObject navpointGO in navPoints)
            {
                int? number = getTrailingNumber(navpointGO.name);

                if (number != null)
                {
                    (GameObject navPoint, int id) navpointAndID = (navpointGO, number.GetValueOrDefault());
                    navpointsAndIDs.Add(navpointAndID);
                }
            }

            navpointsAndIDs.Sort((n1, n2) => n1.id.CompareTo(n2.id));

            foreach ((GameObject navPoint, int id) navpointAndID in navpointsAndIDs)
            {
                waypoints.Add(new Vector2(navpointAndID.navPoint.transform.position.x, navpointAndID.navPoint.transform.position.y));
            }

            // subscribe to WeaponFired events for desired weapons
            foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
            {
                foreach (Weapon weapon in fireGroup.getAllWeapons())
                {
                    // could filter by type and subscribe custom event handlers for each one here
                    weapon.WeaponFired += OnWeaponFired;
                    SnowballThrower snow = weapon as SnowballThrower;

                    if (snow != null)
                        snow.m_ColourOfSnow = structure.Faction.primaryColour;
                    else
                        Debug.LogError(name + " has a none snow weapon equipped");
                }
            }
        }

        protected int? getTrailingNumber(string name)
        {
            int number;

            var match = Regex.Match(name, @"(\d+)$");

            if (match.Success)
            {
                number = int.Parse(match.Groups[1].ToString());
                return number;
            }
            else
            {
                return null;
            }
        }

        public Vector2 getStartPosition()
        {
            return waypoints[0];
        }

        protected override Vector2? setHelmDestination()
        {
            if (currentWaypoint == waypoints.Count)
            {
                if (currentLap == raceMode.maxLaps - 1)
                {
                    D.log("Controller", "Reached finish line!");

                    RaceMode.Call_RacerFinished(this, new RaceMode.RaceModeEventArgs(structure, timer.getTime()));

                    float bearing = Helm.ShipStructure.Bearing;

                    Vector3 origEndVector = new Vector2(structure.StructureRigidbody.velocity.magnitude * Mathf.Sin(bearing * Mathf.Deg2Rad), structure.StructureRigidbody.velocity.magnitude * -Mathf.Cos(bearing * Mathf.Deg2Rad));
                    origEndVector *= lazyTurnLength;

                    return null;
                }
                else
                {
                    currentLap++;
                    currentWaypoint = 0;
                }
            }

            currentWaypoint++;

            raceGUI.racerAtNavpoint(structure.transform, currentWaypoint);

            Vector2 nextPoint;

            if (currentWaypoint == waypoints.Count)
            {
                nextPoint = waypoints[0];
            }
            else
            {
                nextPoint = waypoints[currentWaypoint];
            }

            return new Vector2(nextPoint.x, nextPoint.y);
        }

        protected Vector2? getHelmEndRaceTargetPoint()
        {
            // Debug.Log("Lazy turns");

            Vector2 shipPos = Helm.Position;

            Vector2? nextPoint = Vector2.zero;

            if (turnRight == true)
            {
                nextPoint = new Vector2(shipPos.x + (origEndVector.x * Mathf.Sin(ranAngle) + origEndVector.y * Mathf.Cos(ranAngle)), shipPos.y + (-origEndVector.x * Mathf.Cos(ranAngle) + origEndVector.y * Mathf.Sin(ranAngle)));
                turnRight = false;
            }
            else
            {
                nextPoint = new Vector2(shipPos.x + (origEndVector.x * Mathf.Sin(-ranAngle) + origEndVector.y * Mathf.Cos(-ranAngle)), shipPos.y + (-origEndVector.x * Mathf.Cos(-ranAngle) + origEndVector.y * Mathf.Cos(-ranAngle)));
                turnRight = true;
            }

            return nextPoint;
        }

        public override string navigateAction()
        {
            // D.log("Controller", "Processing RACE state");

            if (seekBehaviour != null && avoidBehaviour != null)
            {
                if (seekBehaviour.Active == false)
                {
                    seekBehaviour.enable();
                }

                if (avoidBehaviour.Active == false)
                {
                    avoidBehaviour.enable();
                }

                if (Helm.destination == null)
                {
                    if (raceEnded == false)
                    {
                        Helm.destination = setHelmDestination();
                    }
                    else
                    {
                        Helm.destination = getHelmEndRaceTargetPoint();
                    }
                }

                // draw a line to the destination
                if (Helm.destination != null && Cam.followTarget != null)
                {
                    if (Cam.followTarget != null && Cam.followTarget.gameObject == Helm.ShipStructure.gameObject)
                    {
                        //Debug.DrawLine(Helm.Position, Helm.destination.GetValueOrDefault(), Color.blue, Time.deltaTime, true);
                    }
                }

                // set target for weapons
                enemiesInRange = structure.scanner.getEnemiesInRange();

                if (enemiesInRange.Count > 0)
                {
                    // tell all fire groups to acquire the first target's hull (hence null for 2nd parameter)
                    foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                    {
                        fireGroup.setTarget(enemiesInRange[0]);
                    }
                }
                else
                {
                    foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                    {
                        fireGroup.unacquireTarget();
                    }
                }
            }

            return "NAVIGATE";
        }

        ///////////////////////////////////////////
        /*
            Handlers for the standard events go here
        */
        ///////////////////////////////////////////

        public void OnWeaponFired(object sender, WeaponFiredEventArgs args)
        {
            //Gui.setMessage(args.weaponFired + " has fired!");
        }

        ///////////////////////////////////////////
        /*
            Handlers for the custom game mode events go here
        */
        ///////////////////////////////////////////

        protected void RaceMode_RaceWaitingToStart(object sender)
        {
            raceGUI = GameManager.Instance.Gamemode.Gui as NoxRaceGUI;

            foreach(Weapon weapon in structure.Weapons)
            {
                weapon.WeaponData.AutoFire = true;
            }
        }

        protected void RaceMode_RacerFinished(object sender, RaceMode.RaceModeEventArgs args)
        {
            if (args.structure != structure) return;

            D.log("Event", "(caller:" + sender.ToString() + ") Handling racer finished");

            raceEnded = true;
            float raceTime = args.raceTime;
            string raceTimeStr = Timer.formatTimer(args.raceTime, true);

            raceGUI.racerFinished(structure.transform, raceTimeStr);
            // Debug.Log("End: " + str(origEndVector));

            raceGUI.setMessage("Congratulations " + args.structure.name + "! You reached the finish line in : " + raceTimeStr);
            D.log("AI", structure.gameObject.name + " race time: " + raceTime);
        }
    }
}