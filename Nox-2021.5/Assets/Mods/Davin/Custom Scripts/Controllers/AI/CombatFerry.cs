using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;
using System.Linq;

using NoxCore.Controllers;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Fittings.Weapons;
using NoxCore.Helm;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Utilities;

using Davin.Messages;

namespace Davin.Controllers
{
    [RequireComponent(typeof(BasicThreatEvaluator))]
    public class CombatFerry : AIStateController
    {
        public List<Vector2> waypoints = new List<Vector2>();
        protected int currentWaypoint = 0;
        
        protected BasicThreatEvaluator threatSys;

        protected ArriveBehaviour arriveBehaviour;
        protected SeekBehaviour seekBehaviour;
        protected OrbitBehaviour orbitBehaviour;
        protected AvoidBehaviour avoidBehaviour;

        protected IThermalControl thermalControl;
        protected IComms comms;

        public Structure resupplyStation;
        protected DockingPort dockingPort;
        protected bool insideTetherRange;
        protected string previousDockingReport;

        protected bool justSpawnedIn, warpingOut;

        protected bool docked;

        public List<Weapon> weapons;

        protected float smallestMaxRange;

        protected bool suppressDockMessages;

        protected List<Structure> escorts = new List<Structure>();

        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, helm);

            structure.NotifyKilled += AI_NotifyKilled;

            GameEventManager.MatchIsWaitingToStart += AI_MatchIsWaitingToStart;

            threatSys = GetComponent<BasicThreatEvaluator>();

            aiActions.Add("FERRY", ferryAction);
            aiActions.Add("DOCK", dockAction);

            state = "FERRY";

            arriveBehaviour = Helm.getBehaviourByName("ARRIVE") as ArriveBehaviour;
            seekBehaviour = Helm.getBehaviourByName("SEEK") as SeekBehaviour;
            orbitBehaviour = Helm.getBehaviourByName("ORBIT") as OrbitBehaviour;
            avoidBehaviour = Helm.getBehaviourByName("AVOID") as AvoidBehaviour;

            if (orbitBehaviour != null)
            {
                orbitBehaviour.OrbitRange = structure.scanner.ScannerData.Radius;
            }

            thermalControl = structure.getDevice<IThermalControl>() as IThermalControl;
            comms = structure.getDevice<IComms>() as IComms;

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

            float smallestRange = 10000;

            foreach (Weapon weapon in structure.Weapons)
            {
                if (weapon.WeaponData.MaxRange < smallestRange)
                {
                    smallestRange = weapon.WeaponData.MaxRange;
                    smallestMaxRange = smallestRange;
                }
            }

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

        public override void reset()
        {
            base.reset();

            state = "FERRY";
            docked = false;
        }

        protected virtual Vector2 setHelmDestination()
        {
            switch (state)
            {
                case "FERRY":

                    suppressDockMessages = false;

                    if (justSpawnedIn == true)
                    {
                        justSpawnedIn = false;
                    }

                    if (warpingOut == true) return Vector2.zero;

                    if (currentWaypoint >= waypoints.Count)
                    {
                        warpingOut = true;
                        (structure as Ship).Call_WarpOut(this, new WarpEventArgs(structure.gameObject, null, SceneManager.GetActiveScene().name, null, null));
                        return Vector2.up * 1000;
                    }

                    Vector2 nextPoint = waypoints[currentWaypoint];

                    currentWaypoint++;

                    return nextPoint;

                case "DOCK":
                    if (dockingPort == null)
                    {
                        dockingPort = findDockingPort(resupplyStation.GetComponent<Structure>());

                        if (dockingPort == null)
                        {
                            if (suppressDockMessages == false)
                            {
                                Gui.setMessage("No docking port on boarding target. Bugging out...");
                                suppressDockMessages = true;
                            }

                            return startSpot.GetValueOrDefault();
                        }
                        else
                        {
                            if (suppressDockMessages == false)
                            {
                                Gui.setMessage(structure.Name + " found " + dockingPort.gameObject.name + " on " + dockingPort.transform.parent.name);

                                if (Vector2.Distance(transform.position, dockingPort.transform.position) > dockingPort.tetherDistance)
                                {
                                    Gui.setMessage(structure.Name + " is heading for the docking port on " + resupplyStation.Name);
                                }

                                suppressDockMessages = true;
                            }

                            return dockingPort.transform.position;
                        }
                    }
                    else
                    {
                        if (Vector2.Distance(transform.position, dockingPort.transform.position) > dockingPort.tetherDistance)
                        {
                            if (suppressDockMessages == false)
                            {
                                Gui.setMessage(structure.Name + " is heading for the docking port on " + resupplyStation.Name);
                                suppressDockMessages = true;
                            }
                        }

                        return dockingPort.transform.position;
                    }
            }

            return startSpot.GetValueOrDefault();
        }

        public void setDockingTarget(Structure dockingTarget)
        {
            this.resupplyStation = dockingTarget;
        }

        protected DockingPort findDockingPort(Structure dockingStructure)
        {
            foreach (StructureSocket socket in dockingStructure.StructureSockets)
            {
                DockingPort dockingPort = socket as DockingPort;

                if (dockingPort != null)
                {
                    return dockingPort;
                }
            }

            return null;
        }

        public override void update()
        {
            if (booted == true)
            {
                processState();

                foreach (ShieldGenerator shieldGenerator in structure.shields)
                {
                    if (shieldGenerator.isActiveOn() == false && shieldGenerator.isShieldUp() == false && shieldGenerator.CurrentCharge >= shieldGenerator.ShieldGeneratorData.MinCharge)
                    {
                        shieldGenerator.raiseShield();
                    }
                }

                if (resupplyStation != null && resupplyStation.isDestroyed() == false && state != "DOCK" && structure.HullStrength < 0.5f * structure.MaxHullStrength)
                {
                    if (docked == false)
                    {
                        state = "DOCK";
                        Helm.destination = setHelmDestination();
                    }
                }

                if (thermalControl.isOverheated() == true)
                {
                    structure.Call_ActivateUltimate(this, new UltimateEventArgs(structure));
                }

                if (comms != null)
                {
                    if (comms.hasMessages() == true)
                    {
                        foreach (EventArgs message in comms.getMessages().Reverse<EventArgs>())
                        {
                            // check type of message
                            // e.g.
                            /*
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
                            }
                            */

                            RequestLocation requestLocationMessage = message as RequestLocation;

                            if (requestLocationMessage != null)
                            {
                                Debug.Log(requestLocationMessage.requester.name + " is requesting current location.");

                                if (comms.isSending() == false)
                                {
                                    comms.sendMessage(requestLocationMessage.requester, new StructureLocation(structure, Helm.Position));
                                }

                                comms.removeMessage(requestLocationMessage);
                            }

                            ReportingInMessage reportingInMessage = message as ReportingInMessage;

                            if (reportingInMessage != null)
                            {
                                Debug.Log(reportingInMessage.ship.name + " reporting in for escort duty.");
                                escorts.Add(reportingInMessage.ship);

                                comms.removeMessage(reportingInMessage);
                            }
                        }
                    }

                    // example of sending a message                    
                    if (structure.HullStrength < 0.2f * structure.MaxHullStrength)
                    {
                        if (comms.isSending() == false)
                        {
                            comms.broadcastMessage(escorts, new DistressCallMessage(structure));
                        }
                    }                    
                }
            }
        }

        public virtual string ferryAction()
        {
            if (structure.scanner.isActiveOn() == true)
            {
                List<Structure> enemiesInRange = structure.scanner.getEnemiesInRange();

                if (enemiesInRange.Count > 0)
                {
                    // get sorted threat ratios for all enemy ships and structures in range
                    List<(Structure structure, float threat)> threats = threatSys.calculateThreatRatios(structure, enemiesInRange);

                    // tell all fire groups to acquire the first target's hull (hence null for 2nd parameter)
                    foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                    {
                        fireGroup.setTarget(threats[0].structure);
                    }
                }

                if (seekBehaviour != null)
                {
                    if (seekBehaviour.Active == false)
                    {
                        seekBehaviour.enableExclusively();
                    }

                    if (Helm.destination == null)
                    {
                        Helm.destination = setHelmDestination();
                    }

                    // draw a line to the destination
                    if (Helm.destination != null && Cam.followTarget != null && Cam.followTarget.gameObject == Helm.ShipStructure.gameObject)
                    {
                        Debug.DrawLine(structure.transform.position, Helm.destination.GetValueOrDefault(), Color.blue, Time.deltaTime, true);
                    }
                }
            }

            return "FERRY";                
        }

        public string dockAction()
        {
            // D.log("Controller", "Processing DOCK state");

            if (resupplyStation.Destroyed == true) return "FERRY";

            if (docked == true) return "DOCK";

            if (structure != null)
            {
                Helm.destination = setHelmDestination();

                if (arriveBehaviour != null)
                {
                    if (arriveBehaviour.Active == false)
                    {
                        arriveBehaviour.enableExclusively();
                    }

                    if (Helm.destination == null)
                    {
                        Helm.destination = setHelmDestination();
                    }
                }

                // draw a line to the destination
                if (Helm.destination != null && Cam.followTarget != null)
                {
                    if (Cam.followTarget != null && Cam.followTarget.gameObject == structure.gameObject)
                    {
                        Debug.DrawLine(structure.transform.position, Helm.destination.GetValueOrDefault(), Color.blue, Time.deltaTime, true);
                    }
                }

                if (dockingPort != null)
                {
                    // have we reached the docking port?
                    if (Vector2.Distance(transform.position, dockingPort.transform.position) <= dockingPort.tetherDistance)
                    {
                        insideTetherRange = true;

                        (bool requestGranted, string message) dockingReport = dockingPort.requestDocking(structure as Ship);

                        if (dockingReport.requestGranted == true)
                        {
                            // TODO - we could have a tether visibly pull the ship to the exact docking port position

                            // basically, we have achieved the requirements to dock at the docking port (right angle, speed etc.) so now there is a small amount of time before we are actually docked
                            Gui.setMessage(dockingReport.message);

                            // we should turn off our engines at this point so we don't drift away from the docking port
                            Helm.disableAllBehaviours();

                            foreach (IEngine engine in (structure as Ship).engines)
                            {
                                engine.deactivate();
                            }

                            docked = true;
                        }
                        else
                        {
                            if (previousDockingReport != dockingReport.message)
                            {
                                Gui.setMessage(dockingReport.message);
                            }
                        }

                        previousDockingReport = dockingReport.message;
                    }
                    else
                    {
                        if (insideTetherRange == true)
                        {
                            Gui.setMessage(structure.Name + " overflew the target docking port on " + resupplyStation.Name);

                            insideTetherRange = false;
                        }
                    }

                }
                else
                {
                    Helm.destination = null;
                    return "FERRY";
                }
            }

            return "DOCK";
        }

        public void AI_NotifyKilled(object sender, TargetDestroyedEventArgs args)
        {
            state = "FERRY";
        }

        protected void AI_MatchIsWaitingToStart(object sender)
        {
            // anything to do here?
        }
    }
}