using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Controllers;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Fittings.Weapons;
using NoxCore.Helm;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Utilities;

namespace Davin.AI
{
    [RequireComponent(typeof(BasicThreatEvaluator))]
    public class BetterBoardingAI : AIStateController, IBoard, IPlayMusic
    {
        enum DisablePhase { ORBITALHEAVY, SEEKSAFETY, MEDIUMLASERS, HEAVYLASERS, LIGHTLASERS, ORBIT };

        protected Ship ship;

        //BasicThreatEvaluator threatSys;

        protected SeekBehaviour seekBehaviour;
        protected OrbitBehaviour orbitBehaviour;

        public Structure boardingTarget;
        protected DockingPort dockingPort;
        protected bool insideTetherRange;

        AudioSource shipAudio;
        AudioClip dockingMusic;

        float shortestWeaponRange = 1000;
        List<(Weapon weapon, float dps)> targetPriorities = new List<(Weapon, float)>();

        Weapon primaryTarget;
        List<LaserTurret> enemyHeavyLasers = new List<LaserTurret>();
        List<LaserTurret> enemyMediumLasers = new List<LaserTurret>();
        List<LaserTurret> enemyLightLasers = new List<LaserTurret>();
        List<ProjectileLauncher> enemyMissileLaunchers = new List<ProjectileLauncher>();

        ProjectileLauncher bombLauncher;
        List<LaserTurret> lasers = new List<LaserTurret>();

        DisablePhase disablePhase = DisablePhase.ORBITALHEAVY;

        bool primaryDestroyed;
        int numLightLasersDestroyed, numMediumLasersDestroyed, numHeavyLasersDestroyed;
        int numBombsLaunched;

        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, helm);

            ship = structure as Ship;

            shipAudio = ship.GetComponent<AudioSource>();
            dockingMusic = Resources.Load<AudioClip>("Audio/Blue Danube (shorter)");

            //threatSys = GetComponent<BasicThreatEvaluator>();

            aiActions.Add("SEEK", seekAction);
            aiActions.Add("DISABLE", disableAction);
            aiActions.Add("DOCK", dockAction);
            aiActions.Add("BOARD", boardAction);

            state = "SEEK";

            seekBehaviour = Helm.getBehaviourByName("SEEK") as SeekBehaviour;
            orbitBehaviour = Helm.getBehaviourByName("ORBIT") as OrbitBehaviour;

            foreach (Weapon weapon in structure.Weapons)
            {
                // subscribe to own events
                weapon.WeaponFired += AI_WeaponFired;    

                shortestWeaponRange = Mathf.Min(weapon.WeaponData.MaxRange, shortestWeaponRange);

                LaserTurret laser = weapon as LaserTurret;

                if (laser != null)
                {
                    lasers.Add(laser);
                }

                ProjectileLauncher launcher = weapon as ProjectileLauncher;

                if (launcher != null)
                {
                    bombLauncher = launcher;
                }
            }

            booted = true;
        }

        public void setBoardingTarget(Structure boardingTarget)
        {
            this.boardingTarget = boardingTarget;

            boardingTarget.TargetDestroyed += AI_TargetDestroyed;

            calculateTargetPriorities();
        }

        protected void calculateTargetPriorities()
        {
            foreach (Weapon weapon in boardingTarget.Weapons)
            {
                if (weapon.DeviceData.Type == "LASERTURRET")
                {
                    if (weapon.DeviceData.SubType == "ORBITALHEAVY") primaryTarget = weapon;
                    else if (weapon.DeviceData.SubType == "HEAVY")
                    {
                        enemyHeavyLasers.Add(weapon as LaserTurret);
                    }
                    else if(weapon.DeviceData.SubType == "MEDIUM")
                    {
                        enemyMediumLasers.Add(weapon as LaserTurret);
                    }
                    else if (weapon.DeviceData.SubType == "LIGHT")
                    {
                        enemyLightLasers.Add(weapon as LaserTurret);
                    }
                }
                else
                {
                    enemyMissileLaunchers.Add(weapon as ProjectileLauncher);
                }

                targetPriorities.Add((weapon, weapon.getDPS()));
            }

            targetPriorities.Sort((a, b) => a.dps.CompareTo(b.dps));
            targetPriorities.Reverse();
        }

        public void startMusic()
        {
            if (shipAudio != null && dockingMusic != null)
            {
                shipAudio.PlayOneShot(dockingMusic);
            }
        }

        public void stopMusic()
        {
            if (shipAudio != null)
            {
                shipAudio.Stop();
            }
        }

        /*
        protected bool boardingTargetOutOfMissiles()
        {
            foreach(Weapon weapon in boardingTarget.weapons)
            {
                ProjectileLauncher launcher = weapon as ProjectileLauncher;

                if (launcher != null)
                {
                    if (launcher.Ammo > 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
        */

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

        protected GameObject getNextTarget(Weapon weapon)
        {
            bool attackingLasers = false;
            List<LaserTurret> enemyLasers = null;

            if (disablePhase == DisablePhase.LIGHTLASERS)
            {
                enemyLasers = enemyLightLasers;
                attackingLasers = true;
            }
            else if (disablePhase == DisablePhase.MEDIUMLASERS)
            {
                enemyLasers = enemyMediumLasers;
                attackingLasers = true;
            }
            else if (disablePhase == DisablePhase.HEAVYLASERS)
            {
                enemyLasers = enemyHeavyLasers;
                attackingLasers = true;
            }

            if (attackingLasers == true)
            { 
                Weapon closestWeapon = null;
                float nearestDistance = 100000;

                foreach (Weapon enemyWeapon in enemyLasers)
                {
                    if (enemyWeapon.destroyed == false)
                    {
                        float distanceToWeapon = Vector2.Distance(weapon.transform.position, enemyWeapon.transform.position);

                        if (distanceToWeapon < nearestDistance)
                        {
                            closestWeapon = enemyWeapon;
                        }
                    }
                }

                return closestWeapon.gameObject;
            }

            foreach((Weapon weapon, float dps) target in targetPriorities)
            {
                if (target.weapon.destroyed == false && Vector2.Distance(weapon.gameObject.transform.position, target.weapon.gameObject.transform.position) <= weapon.WeaponData.MaxRange)
                {
                    return target.weapon.gameObject;
                }
            }

            return null;
        }

        protected virtual Vector2 setHelmDestination()
        {
            switch (state)
            {
                case "SEEK":
                    if (boardingTarget != null)
                    {
                        return boardingTarget.gameObject.transform.position;
                    }
                    break;

                case "DISABLE":
                    // maybe do something helm related here if not doing something like orbiting
                    if (disablePhase == DisablePhase.SEEKSAFETY)
                    {
                        return new Vector2(0, -1500);
                    }
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
                            Gui.setMessage(ship.Name + " found " + dockingPort.gameObject.name + " on " + dockingPort.transform.parent.name);

                            seekBehaviour.LookAheadDistance = dockingPort.tetherDistance;

                            if (Vector2.Distance(transform.position, dockingPort.transform.position) > dockingPort.tetherDistance)
                            {
                                Gui.setMessage(structure.Name + " is heading for the docking port on " + boardingTarget.Name);
                            }

                            return dockingPort.transform.position;
                        }
                    }
                    else
                    {
                        if (Vector2.Distance(transform.position, dockingPort.transform.position) > structure.HalfLength)
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
            processState();

            foreach (ShieldGenerator shieldGenerator in structure.shields)
            {
                if (shieldGenerator.isShieldUp() == false && shieldGenerator.CurrentCharge > shieldGenerator.ShieldGeneratorData.MinCharge)
                {
                    shieldGenerator.raiseShield();
                }
            }
        }

        public string seekAction()
        {
            // D.log("Controller", "Processing SEEK state");

            if (structure.scanner.isActiveOn() == true)
            {
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

                    // draw a line to the destination
                    if (Helm.destination != null && Cam.followTarget != null)
                    {
                        if (Cam.followTarget != null && Cam.followTarget.gameObject == structure.gameObject)
                        {
                            //Debug.DrawLine(structure.transform.position, Helm.destination.GetValueOrDefault(), Color.blue, Time.deltaTime, true);
                        }
                    }

                    if (structure.scanner.getEnemiesInRange().Contains(boardingTarget))
                    {
                        Gui.setMessage("Enemy station acquired on scan");
                        Gui.setMessage("Evaluating mission options...");
                        return "DISABLE";
                    }
                }
            }

            return "SEEK";
        }

        protected void disableOrbitalHeavy()
        {
            if (orbitBehaviour != null)
            {
                if (orbitBehaviour.Active == false)
                {
                    orbitBehaviour.enableExclusively();
                }

                orbitBehaviour.OrbitObject = boardingTarget.transform;
                orbitBehaviour.OrbitRange = bombLauncher.WeaponData.MaxRange - 100;
                orbitBehaviour.clockwise = true;

                if (primaryTarget != null)
                {
                    bombLauncher.acquireTarget(boardingTarget.gameObject, primaryTarget.gameObject);
                }
            }
        }

        protected void seekSafety()
        {
            if (seekBehaviour != null)
            {
                if (seekBehaviour.Active == false)
                {
                    seekBehaviour.enableExclusively();
                }

                Helm.destination = setHelmDestination();

                // draw a line to the destination
                if (Helm.destination != null && Cam.followTarget != null)
                {
                    if (Cam.followTarget != null && Cam.followTarget.gameObject == structure.gameObject)
                    {
                        //Debug.DrawLine(structure.transform.position, Helm.destination.GetValueOrDefault(), Color.blue, Time.deltaTime, true);
                    }
                }
            }
        }

        protected void disableNextTargetPriority(bool clockwise, float? range = null)
        {
            if (orbitBehaviour != null)
            {
                if (orbitBehaviour.Active == false)
                {
                    orbitBehaviour.enableExclusively();
                }

                orbitBehaviour.OrbitObject = boardingTarget.transform;

                if (range == null)
                {
                    orbitBehaviour.OrbitRange = shortestWeaponRange;
                }
                else
                {
                    orbitBehaviour.OrbitRange = range.GetValueOrDefault();
                }

                orbitBehaviour.clockwise = clockwise;

                foreach (Weapon weapon in structure.Weapons)
                {
                    GameObject targetModule = getNextTarget(weapon);

                    if (targetModule != null)
                    {
                        TargetableWeapon tWeap = (TargetableWeapon)weapon;

                        if (tWeap != null)
                        {
                            tWeap.acquireTarget(boardingTarget.gameObject, targetModule);
                        }
                    }
                }
            }
        }

        public string disableAction()
        {
            if (structure.scanner.isActiveOn() == true)
            {
                switch (disablePhase)
                {
                    case DisablePhase.ORBITALHEAVY:
                        disableOrbitalHeavy();
                            
                        if (primaryDestroyed == true)
                        {                                
                            disablePhase = DisablePhase.SEEKSAFETY;                                
                        }
                        break;

                    case DisablePhase.SEEKSAFETY:
                        seekSafety();

                        if (Vector2.Distance(transform.position, boardingTarget.transform.position) > 1000)
                        { 
                            if (structure.shields[0].CurrentCharge > 850)
                            {
                                disablePhase = DisablePhase.MEDIUMLASERS;
                            }
                        }
                        break;

                    case DisablePhase.MEDIUMLASERS:
                        disableNextTargetPriority(false, 500);

                        if (numMediumLasersDestroyed == enemyMediumLasers.Count)
                        {
                            disablePhase = DisablePhase.HEAVYLASERS;
                        }
                        break;

                    case DisablePhase.HEAVYLASERS:
                        disableNextTargetPriority(true, 650);

                        if (numHeavyLasersDestroyed == enemyHeavyLasers.Count)
                        {
                            disablePhase = DisablePhase.LIGHTLASERS;
                        }
                        break;

                    case DisablePhase.LIGHTLASERS:
                        disableNextTargetPriority(false, 450);

                        if (numLightLasersDestroyed == enemyLightLasers.Count)
                        {
                            disablePhase = DisablePhase.ORBIT;
                        }
                        break;

                    case DisablePhase.ORBIT:
                        disableNextTargetPriority(false);
                        break;
                }
            }

            // perform evaluation to determine if a dock is attempted
            bool targetDisabled = false;

            if (targetDisabled == true)
            {
                // set the helm destimation to null to pick up the docking port position in the dockAction
                Helm.destination = null;

                startMusic();

                return "DOCK";
            }

            return "DISABLE";
        }

        public string dockAction()
        {
            // D.log("Controller", "Processing DOCK state");

            if (ship != null)
            {
                // ok, we are using the standard seek steering behaviour here
                // is this the most suitable steering behaviour to use
                // what would be better?					
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

                // have we reached the docking port?
                if (dockingPort != null)
                {
                    if (Vector2.Distance(transform.position, dockingPort.transform.position) <= dockingPort.tetherDistance)
                    {
                        insideTetherRange = true;

                        (bool requestGranted, string message) dockingReport = dockingPort.requestDocking(ship);

                        if (dockingReport.requestGranted == true)
                        {
                            // TODO - we could have a tether pull the ship to the exact docking port position

                            // basically, we have achieved the requirements to dock at the docking port (right angle and speed) so now there is a small amount of time before we are actually docked
                            Gui.setMessage(gameObject.name + " is docking with " + boardingTarget.Name);

                            // we should turn off our engines at this point so we don't drift away from the docking port
                            Helm.disableAllBehaviours();

                            foreach (IEngine engine in ship.engines)
                            {
                                engine.deactivate();
                            }

                            return "BOARD";
                        }
                        else
                        {
                            if (Gui.previousMessage != dockingReport.message)
                            {
                                Gui.setMessage(dockingReport.message);
                            }
                        }
                    }
                    else
                    {
                        if (insideTetherRange == true)
                        {
                            Gui.setMessage(ship.Name + " overflew the target docking port on " + boardingTarget.Name);
                            insideTetherRange = false;
                        }
                    }
                }
            }

            return "DOCK";
        }

        public string boardAction()
        {
            // D.log("Controller", "Processing BOARD state");

            return "BOARD";
        }

        public void AI_WeaponFired(object sender, WeaponFiredEventArgs args)
        {
            if (args.weaponFired == bombLauncher)
            {
                numBombsLaunched++;

                if (numBombsLaunched == 3)
                {
                    primaryDestroyed = true;
                    Helm.destination = null;
                }
            }
        }

        public void AI_TargetDestroyed(object sender, TargetDestroyedEventArgs args)
        {
            if (args.moduleAttacked == primaryTarget)
            {
                primaryDestroyed = true;
            }

            if (args.moduleAttacked.DeviceData.Type == "LASERTURRET")
            {
                if (args.moduleAttacked.DeviceData.SubType == "LIGHT")
                {
                    numLightLasersDestroyed++;
                }
                else if (args.moduleAttacked.DeviceData.SubType == "MEDIUM")
                {
                    numMediumLasersDestroyed++;
                }
                else if(args.moduleAttacked.DeviceData.SubType == "HEAVY")
                {
                    numHeavyLasersDestroyed++;
                }
            }
        }
    }
}
