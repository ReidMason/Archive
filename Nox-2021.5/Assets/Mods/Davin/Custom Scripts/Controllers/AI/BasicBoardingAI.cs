using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

using NoxCore.Data;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Fittings.Weapons;
using NoxCore.Helm;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;

namespace NoxCore.Controllers
{
    [RequireComponent(typeof(BasicThreatEvaluator))]
    public class BasicBoardingAI : AIStateController, IBoard, IPlayMusic
    {
        protected Ship ship;

        BasicThreatEvaluator threatSys;

        protected ArriveBehaviour arriveBehaviour;
        protected SeekBehaviour seekBehaviour;
        protected OrbitBehaviour orbitBehaviour;

        protected IComms comms;

        public Structure boardingTarget;
        protected DockingPort dockingPort;
        protected bool insideTetherRange;
        protected string previousDockingReport;
        protected bool docked;

        protected List<Structure> squad;

        AudioSource shipAudio;
        AudioClip dockingMusic;

        protected float smallestMaxRange;

        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, helm);

            ship = structure as Ship;

            ship.HasWarpedIn += OnHasWarpedIn;

            shipAudio = ship.GetComponent<AudioSource>();
            dockingMusic = Resources.Load<AudioClip>("Audio/Blue Danube (shorter)");

            threatSys = GetComponent<BasicThreatEvaluator>();

            aiActions.Add("SEARCH", searchAction);
            aiActions.Add("DISABLE", disableAction);
            aiActions.Add("DOCK", dockAction);
            aiActions.Add("BOARD", boardAction);

            state = "SEARCH";

            arriveBehaviour = Helm.getBehaviourByName("ARRIVE") as ArriveBehaviour;
            seekBehaviour = Helm.getBehaviourByName("SEEK") as SeekBehaviour;
            orbitBehaviour = Helm.getBehaviourByName("ORBIT") as OrbitBehaviour;
            // We need to slow down earlier so we don't overshoot the boarding target
            arriveBehaviour.SlowingRadius = 500;

            if (orbitBehaviour != null)
            {
                orbitBehaviour.OrbitRange = structure.scanner.ScannerData.Radius;
            }

            comms = structure.getDevice<IComms>() as IComms;

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

        public void setBoardingTarget(Structure boardingTarget)
        {
            this.boardingTarget = boardingTarget;
        }

        public void startMusic()
        {
            if (shipAudio != null && dockingMusic != null)
            {
                if (!shipAudio.isPlaying)
                {
                    shipAudio.PlayOneShot(dockingMusic);
                }
            }
        }

        public void stopMusic()
        {
            if (shipAudio != null)
            {
                shipAudio.Stop();
            }
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

        protected virtual Vector2 setHelmDestination()
        {
            switch (state)
            {
                case "SEARCH":
                    if (boardingTarget != null)
                    {
                        return boardingTarget.gameObject.transform.position;
                    }
                    else
                    {
                        return Vector2.zero;
                    }

                case "DISABLE":
                    // maybe do something helm related here in order to attack your main target and any defenders
                    break;

                case "DOCK":
                    if (dockingPort == null)
                    {
                        dockingPort = findDockingPort(boardingTarget.GetComponent<Structure>());

                        if (dockingPort == null)
                        {
                            Gui.setMessage("No docking port on boarding target. Bugging out...");
                            return startSpot.GetValueOrDefault();
                        }
                        else
                        {
                            Gui.setMessage(structure.Name + " found " + dockingPort.gameObject.name + " on " + dockingPort.transform.parent.name);

                            if (Vector2.Distance(transform.position, dockingPort.transform.position) > dockingPort.tetherDistance)
                            {
                                Gui.setMessage(structure.Name + " is heading for the docking port on " + boardingTarget.Name);
                            }

                            return dockingPort.transform.position;
                        }
                    }
                    else
                    {
                        if (Vector2.Distance(transform.position, dockingPort.transform.position) > dockingPort.tetherDistance)
                        {
                            Gui.setMessage(structure.Name + " is heading for the docking port on " + boardingTarget.Name);
                        }

                        return dockingPort.transform.position;
                    }
            }

            // if not in any valid AI state then return to start spot
            return startSpot.GetValueOrDefault();
        }

        public override void update()
        {
            if (booted == true)
            {

                // Do not allow the cuckoo to do anything until there are under 5 remaining guns on the station
                var workingWeapons = boardingTarget.Weapons.Where(x => !x.destroyed).ToList();
                if (workingWeapons.Count < 5)
                {
                    processState();
                }

                foreach (ShieldGenerator shieldGenerator in structure.shields)
                {
                    if (shieldGenerator.isActiveOn() == false && shieldGenerator.CurrentCharge >= shieldGenerator.ShieldGeneratorData.MinCharge)
                    {
                        shieldGenerator.raiseShield();
                    }
                }

                // some kind of test to decide when to attempt to dock

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
                    if (boardingTarget != null && structure.scanner.getEnemiesInRange().Contains(boardingTarget))
                    {
                        Gui.setMessage("Enemy station acquired on scan");
                        Gui.setMessage("Evaluating mission options...");
                        return "DOCK";
                    }
                    else
                    {
                        return "SEARCH";
                    }
                }
            }
            else
            {
                return "SEARCH";
            }
        }

        public string disableAction()
        {
            if (structure.scanner.isActiveOn() == true)
            {

                if (orbitBehaviour != null)
                {
                    if (orbitBehaviour.Active == false)
                    {
                        orbitBehaviour.enableExclusively();
                    }
                }


                List<Structure> enemiesInRange = structure.scanner.getEnemiesInRange();

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

                    // normally, you'd evaluate your chances of docking and boarding successfully without getting killed
                    /*
                        * if (some kind of test == true)
                        * {
                        *     return "DOCK";
                        * }
                        * else
                        * {
                        *     return "DISABLE";
                        * }
                        */

                    // However... Guns? Who cares about a few guns? I'm going stright for the docking port...
                    /*
                    Gui.setMessage("Going for a hot board.");

                    startMusic();

                    Helm.destination = null;

                    return "DOCK";
                    */
                }

            return "DISABLE";
        }

        public virtual string combatAction()
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

                    if (seekBehaviour != null)
                    {
                        if (seekBehaviour.Active == false)
                        {
                            seekBehaviour.enableExclusively();
                        }

                        // set helm
                        if (Helm.destination == null)
                        {
                            Helm.destination = setHelmDestination();
                        }
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

                    Helm.destination = null;

                    return "SEARCH";
                }
            }

            return "COMBAT";
        }

        public string dockAction()
        {
            // D.log("Controller", "Processing DOCK state");

            if (boardingTarget.Destroyed == true) return "SEARCH";

            if (docked == true) return "DOCK";

            if (structure != null)
            {
                if (dockingPort == null)
                {
                    dockingPort = findDockingPort(boardingTarget);

                    if (dockingPort == null) return "SEARCH";
                }

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
                        //Debug.DrawLine(structure.transform.position, Helm.destination.GetValueOrDefault(), Color.blue, Time.deltaTime, true);
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

                            return "BOARD";
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
                            Gui.setMessage(structure.Name + " overflew the target docking port on " + boardingTarget.Name);

                            insideTetherRange = false;
                        }
                    }

                }
                else
                {
                    Helm.destination = null;
                    return "SEARCH";
                }
            }

            return "DOCK";
        }

        public string boardAction()
        {
            // D.log("Controller", "Processing BOARD state");

            return "BOARD";
        }

        protected void OnHasWarpedIn(object sender, WarpEventArgs args)
        {
            // add any initial targeting etc. here

            setBoardingTarget(FindObjectOfType<Station>());
        }
    }
}
