using System.Collections.Generic;
using System;
using System.Linq;

using UnityEngine;

using NoxCore.Data;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Fittings.Weapons;
using NoxCore.Helm;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Rules;

using Davin.Fittings.Devices;

namespace NoxCore.Controllers
{
    [RequireComponent(typeof(BasicThreatEvaluator))]
    public class DMBoardingAI : AIStateController
    {
        protected BasicThreatEvaluator threatSys;

        protected ArriveBehaviour arriveBehaviour;
        protected SeekBehaviour seekBehaviour;
        protected OrbitBehaviour orbitBehaviour;

        protected IComms comms;
        protected IWarpDrive warpDrive;

        public Structure resupplyStation;
        protected DockingPort dockingPort;
        protected bool insideTetherRange;
        protected string previousDockingReport;

        protected bool justSpawnedIn;

        protected bool docked;

        protected List<Structure> squad;        

        public List<LaserTurret> heavyLasers;
        public List<ProjectileLauncher> launchers;

        protected float smallestMaxRange;

        protected bool suppressDockMessages;

        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, helm);

            structure.NotifyKilled += AI_NotifyKilled;

            GameEventManager.MatchIsWaitingToStart += AI_MatchIsWaitingToStart;

            threatSys = GetComponent<BasicThreatEvaluator>();

            aiActions.Add("SEARCH", searchAction);
            aiActions.Add("COMBAT", combatAction);
            aiActions.Add("DOCK", dockAction);

            state = "SEARCH";

            arriveBehaviour = Helm.getBehaviourByName("ARRIVE") as ArriveBehaviour;
            seekBehaviour = Helm.getBehaviourByName("SEEK") as SeekBehaviour;
            orbitBehaviour = Helm.getBehaviourByName("ORBIT") as OrbitBehaviour;

            if (orbitBehaviour != null)
            {
                orbitBehaviour.OrbitRange = structure.scanner.ScannerData.Radius;
            }

            comms = structure.getDevice<IComms>() as IComms;

            warpDrive = structure.getDevice<IWarpDrive>() as IWarpDrive;

            justSpawnedIn = true;

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

        public override void reset()
        {
            base.reset();

            state = "SEARCH";
            docked = false;
        }

        protected virtual Vector2 setHelmDestination()
        {
            switch(state)
            {
                case "SEARCH":

                    suppressDockMessages = false;

                    if (justSpawnedIn == true)
                    {
                        justSpawnedIn = false;

                        return transform.position + (transform.up * transform.position.magnitude * 2);
                    }
                    else
                    {
                        return Vector2.zero;
                    }

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
                        }
                    }

                    // example of sending a message
                    /*
                    if (structure.HullStrength < 0.2f * structure.MaxHullStrength)
                    {
                        if (comms.isSending() == false)
                        {
                            comms.broadcastMessage(squad, new DistressCallMessage(structure));
                        }
                    }
                    */
                }
            }
        }

        public virtual string searchAction()
        {
            if (structure.scanner.isActiveOn() == true)
            {
                if (structure.scanner.getEnemiesInRange().Count == 0)
                {
                    #region search pattern
                    if (seekBehaviour != null)
                    {
                        if (seekBehaviour.Active == false)
                        {
                            seekBehaviour.enableExclusively();
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
                    }
                    else
                    {
                        return null;
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
                List<Structure> enemiesInRange = structure.scanner.getEnemiesInRange();

                if (enemiesInRange.Count > 0)
                {
                    if (orbitBehaviour != null)
                    {
                        if (orbitBehaviour.Active == false)
                        {
                            orbitBehaviour.enableExclusively();
                        }
                    }

                    // get sorted threat ratios for all enemy ships and structures in range
                    List<(Structure structure, float threat)> threats = threatSys.calculateThreatRatios(structure, enemiesInRange);

                    // tell all fire groups to acquire the first target's hull (hence null for 2nd parameter)
                    foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                    {
                        fireGroup.setTarget(threats[0].structure);
                    }

                    // use the first target as the ship/structure to orbit around
                    orbitBehaviour.OrbitObject = threats[0].structure.transform;

                    // use the first weapon's maximum range to determine a suitable orbit range
                    if (structure.Weapons.Count > 0)
                    {
                        orbitBehaviour.OrbitRange = smallestMaxRange;
                    }

                    return "COMBAT";
                }
                else
                {
                    foreach (Weapon weap in structure.Weapons)
                    {
                        TargetableWeapon tWeap = (TargetableWeapon)weap;

                        if (tWeap != null)
                        {
                            tWeap.unacquireTarget();
                        }
                    }

                    return "SEARCH";
                }
            }
            else
            {
                return "SEARCH";
            }
        }

        public string dockAction()
        {
            // D.log("Controller", "Processing DOCK state");

            if (resupplyStation.Destroyed == true) return "SEARCH";

            if (docked == true) return "DOCK";

            Helm.destination = setHelmDestination();

            float distanceToPort = Vector2.Distance(transform.position, dockingPort.transform.position);

            if (distanceToPort > 1000 && warpDrive != null && warpDrive.getWarpStatus() == WarpSequence.INACTIVE)
            {
                warpDrive.engage();
            }

            // ok, we are using the standard seek steering behaviour here
            // is this the most suitable steering behaviour to use
            // what would be better?					
            if (arriveBehaviour != null)
            {
                if (arriveBehaviour.Active == false)
                {
                    arriveBehaviour.enableExclusively();
                }

                // run search pattern
                if (Helm.destination == null)
                {
                    Helm.destination = setHelmDestination();
                    // D.log ("DESTINATION: " + _helm.destination.ToString());
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
                return "SEARCH";
            }

            return "DOCK";
        }

        public void AI_NotifyKilled(object sender, TargetDestroyedEventArgs args)
        {
            state = "SEARCH";
        }

        protected void AI_MatchIsWaitingToStart(object sender)
        {
            FactionData faction = FactionManager.Instance.findFaction(structure.Faction.label);

            List<Ship> ships = faction.FleetManager.getAllShips();

            squad = ships.Cast<Structure>().ToList();
        }
    }
}