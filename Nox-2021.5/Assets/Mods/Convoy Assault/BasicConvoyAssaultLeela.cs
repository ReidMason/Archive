using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

using NoxCore.Data;
using NoxCore.Controllers;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Weapons;
using NoxCore.Helm;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Rules;
using NoxCore.Utilities;
using Davin.Fittings.Devices;
using Davin.Messages;

namespace Davin.Controllers
{
    [RequireComponent(typeof(BasicThreatEvaluator))]
    public class BasicConvoyAssaultLeela : AIStateController
    {
        protected Ship ship;

        public List<Vector2> waypoints = new List<Vector2>();
        public int currentWaypoint = 0;
        public bool forceWaypointNavigation;

        protected BasicThreatEvaluator threatSys;

        protected SeekBehaviour seekBehaviour;
        protected PursueBehaviour pursueBehaviour;
        protected OrbitBehaviour orbitBehaviour;
        protected AvoidBehaviour avoidBehaviour;

        protected bool justSpawnedIn;

        public Structure primaryTarget;

        protected List<Structure> oldTargets = new List<Structure>();

        protected Afterburner afterburner;

        protected IComms comms;

        [SerializeField]
        protected List<Structure> squad;

        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, helm);

            threatSys = GetComponent<BasicThreatEvaluator>();

            aiActions.Add("SEARCH", searchAction);
            aiActions.Add("COMBAT", combatAction);

            state = "SEARCH";

            // note: leave this as false if you want the ship to orbit its target
            forceWaypointNavigation = false;

            afterburner = structure.getDevice<Afterburner>() as Afterburner;

            seekBehaviour = Helm.getBehaviourByName("SEEK") as SeekBehaviour;
            //pursueBehaviour = Helm.getBehaviourByName("PURSUE") as PursueBehaviour;
            orbitBehaviour = Helm.getBehaviourByName("ORBIT") as OrbitBehaviour;
            avoidBehaviour = Helm.getBehaviourByName("AVOID") as AvoidBehaviour;

            if (orbitBehaviour != null)
            {
                orbitBehaviour.OrbitRange = structure.scanner.ScannerData.Radius;
            }

            if (avoidBehaviour != null)
            {
                // note: could add additional layers to the following.
                // E.G. avoidLayerMask = structure.Gamemode.getCollisionMask("ship").GetValueOrDefault() ^ (1 << LayerMask.NameToLayer("NavPoint"));
                LayerMask avoidLayerMask = structure.Gamemode.getCollisionMask("SHIP").GetValueOrDefault();
                avoidBehaviour.setCollidables(avoidLayerMask);
            }

            justSpawnedIn = true;

            #region waypoint setup
            List<(GameObject navPoint, int id)> navpointsAndIDs = new List<(GameObject, int)>();

            if (avoidBehaviour != null)
            {
                LayerMask avoidLayerMask = GameManager.Instance.Gamemode.getCollisionMask("SHIP").GetValueOrDefault() ^ (1 << LayerMask.NameToLayer("NavPoint"));
                avoidBehaviour.setCollidables(avoidLayerMask);
            }

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
            #endregion

            foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
            {
                foreach (Weapon weapon in fireGroup.getAllWeapons())
                {
                    ProjectileLauncher launcher = weapon as ProjectileLauncher;

                    if (launcher != null)
                    {
                        launcher.WeaponFired += LauncherFired;
                    }
                }
            }

            ship = structure as Ship;

            comms = structure.getDevice<IComms>() as IComms;

            booted = true;
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

        public void LauncherFired(object sender, WeaponFiredEventArgs args)
        {
            //Gui.setMessage(args.weaponFired + " has fired!");
        }

        protected virtual Vector2 setHelmDestination()
        {
            Vector2 nextPoint = waypoints[currentWaypoint];

            currentWaypoint++;

            if (currentWaypoint == waypoints.Count)
            {
                currentWaypoint = 0;
            }

            return nextPoint + new Vector2(-300, 400);
        }

        public virtual string searchAction()
        {
            if (structure.scanner.isActiveOn() == true)
            {
                if (structure.scanner.getEnemiesInRange().Count == 0)
                {
                    Helm.desiredThrottle = 0.25f;

                    #region search pattern
                    if (seekBehaviour != null && seekBehaviour.Active == false)
                    {
                        seekBehaviour.enableExclusively();
                        currentWaypoint = 0;
                    }

                    if (avoidBehaviour != null && avoidBehaviour.Active == false)
                    {
                        avoidBehaviour.enable();
                    }

                    // run search pattern
                    if (Helm.destination == null)
                    {
                        Helm.destination = setHelmDestination();
                    }

                    // don't allow a destination outside of the arena
                    if (ArenaRules.radius > 0)
                    {
                        if (Helm.destination.GetValueOrDefault().magnitude > ArenaRules.radius)
                        {
                            Helm.destination = null;
                        }
                    }

                    // draw a line to the destination
                    if (Helm.destination != null && Cam.followTarget != null && Cam.followTarget.gameObject == Helm.ShipStructure.gameObject)
                    {
                        Debug.DrawLine(structure.transform.position, Helm.destination.GetValueOrDefault(), Color.blue, Time.deltaTime, true);
                    }

                    return "SEARCH";
                    #endregion
                }
                else
                {
                    return "COMBAT";
                }
            }
            else
            {
                return "SEARCH";
            }
        }

        public virtual string combatAction()
        {
            if (structure.scanner.isActiveOn() == true)
            {
                #region orbit enemy
                List<Structure> enemiesInRange = structure.scanner.getEnemiesInRange();

                if (enemiesInRange.Count > 0)
                {
                    bool getNewPrimaryTarget = false;

                    if (primaryTarget == null)
                    {
                        getNewPrimaryTarget = true;
                    }
                    else if (primaryTarget != null)
                    {
                        if (primaryTarget.GetComponent<Structure>().Destroyed == true)
                        {
                            getNewPrimaryTarget = true;
                        }
                        else
                        {
                            if (primaryTarget.GetComponent<Ship>().AreEnginesDisabled == true)
                            {
                                getNewPrimaryTarget = true;
                            }
                        }
                    }

                    if (getNewPrimaryTarget == true)
                    {
                        // get sorted threat ratios for all enemy ships and structures in range
                        List<(Structure structure, float threat)> threats = threatSys.calculateThreatRatios(structure, enemiesInRange);

                        foreach ((Structure enemy, float threat) threat in threats)
                        {
                            if (!oldTargets.Contains(threat.enemy))
                            {
                                primaryTarget = threat.enemy;
                                oldTargets.Add(primaryTarget);
                                break;
                            }
                        }
                    }

                    // tell all fire groups to acquire the first target's hull (hence null for 2nd parameter)
                    foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                    {
                        Ship enemyShip = primaryTarget.GetComponent<Ship>();

                        IEngine targetEngine = null;

                        foreach (IEngine engine in enemyShip.engines)
                        {
                            if (engine.isDestroyed() == false)
                            {
                                // target this one!
                                targetEngine = engine;
                                break;
                            }
                        }

                        fireGroup.setTarget(primaryTarget, targetEngine);

                        // for rotating turrets (deflection shooting)
                        List<IRotatingTurret> turrets = fireGroup.getAllWeaponsOfType<IRotatingTurret>();

                        foreach(IRotatingTurret turret in turrets)
                        {
                            Vector2 enginePosition = targetEngine.getGameObject().transform.position;

                            float distanceToEngine = Vector2.Distance(enginePosition, transform.position);

                            float shotSpeed = 1000;

                            Vector2 deflectionPosition = enginePosition + ((Vector2)enemyShip.transform.up * (distanceToEngine / shotSpeed));

                            turret.TargetPosition = deflectionPosition;
                        }
                    }

                    if (seekBehaviour != null && seekBehaviour.Active == false)
                    {
                        seekBehaviour.enableExclusively();
                    }

                    //                pursueBehaviour.Target = primaryTarget.GetComponent<Structure>().StructureRigidbody;
                    //                pursueBehaviour.offset = new Vector2(100, -100);

                    Helm.Destination = primaryTarget.transform.position + (primaryTarget.transform.up.normalized * -50) + (primaryTarget.transform.right.normalized * -50);

                    if (Vector2.Dot(gameObject.transform.up, primaryTarget.transform.up) >= 0.3f)
                    {
                        float dist = Vector2.Distance(transform.position, (Vector2)primaryTarget.transform.position);

                        if (dist < 150 || structure.StructureRigidbody.velocity.magnitude > primaryTarget.GetComponent<Structure>().StructureRigidbody.velocity.magnitude)
                        {
                            Helm.desiredThrottle -= Helm.desiredThrottle * 0.05f;
                        }
                        else
                        {
                            Helm.desiredThrottle += Helm.desiredThrottle * 0.25f;
                        }

                        if (dist > 300) afterburner.engage();
                    }
                    else
                    {
                        Helm.desiredThrottle = 0.25f;
                    }

                    if (avoidBehaviour != null && avoidBehaviour.Active == false)
                    {
                        avoidBehaviour.enable();
                    }


                    /*
                    if (orbitBehaviour != null)
                    {
                        if (orbitBehaviour.Active == false) orbitBehaviour.enableExclusively();

                        // use the first target as the ship/structure to orbit around
                        orbitBehaviour.OrbitObject = primaryTarget.transform;
                        orbitBehaviour.clockwise = true;

                        // use the first weapon's maximum range to determie a suitable orbit range
                        if (structure.Weapons.Count > 0)
                        {
                            orbitBehaviour.OrbitRange = 300;
                        }
                    }
                    */

                    ReceiveCommsMessage();
                }

                return "COMBAT";

            }
            else
            {
                foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                {
                    fireGroup.unacquireTarget();
                }

                return "SEARCH";
            }
            #endregion
        }

        void ReceiveCommsMessage()
        {
            if (comms != null)
            {
                if (comms.hasMessages() == true)
                {
                    foreach (EventArgs message in comms.getMessages().Reverse<EventArgs>())
                    {
                        // check type of message
                        // e.g.
                        DistressCallMessage distressCallMessage = message as DistressCallMessage;

                        if (distressCallMessage != null)
                        {
                            // do something with the information in the message
                            D.log("Oh, " + distressCallMessage.distressed + " is in distress");

                            comms.removeMessage(distressCallMessage);
                        }

                        // could be a different type of message so try to cast to each in turn and process the information contained inside the message's args
                        NewPrimaryTargetMessage newPrimaryTargetMessage = message as NewPrimaryTargetMessage;

                        if (newPrimaryTargetMessage != null)
                        {
                            // do something with the information in the message
                            D.log("Oh, I should be attacking " + newPrimaryTargetMessage.target.name);

                            comms.removeMessage(newPrimaryTargetMessage);
                            primaryTarget = newPrimaryTargetMessage.target;
                        }
                        
                    }
                }
            }
        }

        void SendNewTargetMessage(Structure target)
        {
            // example of sending a message
            if (target != null)
            {
                if (comms.isSending() == false)
                {
                    comms.broadcastMessage(squad, new NewPrimaryTargetMessage(target));
                }
            }
        }
    }
}