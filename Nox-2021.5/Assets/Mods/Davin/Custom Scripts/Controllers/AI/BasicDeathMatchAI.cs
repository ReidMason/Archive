using System.Collections.Generic;
using System;
using System.Linq;

using UnityEngine;

using NoxCore.Data;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Weapons;
using NoxCore.Helm;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Rules;

namespace NoxCore.Controllers
{
    [RequireComponent(typeof(BasicThreatEvaluator))]
    public class BasicDeathMatchAI : AIStateController
    {
        protected BasicThreatEvaluator threatSys;

        protected SeekBehaviour seekBehaviour;
        protected OrbitBehaviour orbitBehaviour;

        protected IComms comms;

        protected bool justSpawnedIn;

        protected List<Structure> squad;

        public List<LaserTurret> heavyLasers;
        public List<ProjectileLauncher> launchers;

        protected float smallestMaxRange;

        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, helm);

            GameEventManager.MatchIsWaitingToStart += AI_MatchIsWaitingToStart;

            threatSys = GetComponent<BasicThreatEvaluator>();

            aiActions.Add("SEARCH", searchAction);
            aiActions.Add("COMBAT", combatAction);

            state = "SEARCH";

            seekBehaviour = Helm.getBehaviourByName("SEEK") as SeekBehaviour;
            orbitBehaviour = Helm.getBehaviourByName("ORBIT") as OrbitBehaviour;

            if (orbitBehaviour != null)
            {
                orbitBehaviour.OrbitRange = structure.scanner.ScannerData.Radius;
            }

            comms = structure.getDevice<IComms>() as IComms;

            justSpawnedIn = true;

            float smallestRange = 10000;

            foreach(Weapon weapon in structure.Weapons)
            {
                if (weapon.WeaponData.MaxRange < smallestRange)
                {
                    smallestRange = weapon.WeaponData.MaxRange;
                    smallestMaxRange = smallestRange;
                }
            }

            booted = true;
        }

        protected virtual Vector2 setHelmDestination()
        {
            if (justSpawnedIn == true)
            {
                justSpawnedIn = false;

                return transform.forward * transform.position.magnitude * 2;
            }
            else
            {
                return Vector2.zero;
            }
        }

        public override void update()
        {
            if (booted == true)
            {
                foreach (ShieldGenerator shieldGenerator in structure.shields)
                {
                    if (shieldGenerator.isActiveOn() == false && shieldGenerator.CurrentCharge >= shieldGenerator.ShieldGeneratorData.MinCharge)
                    {
                        shieldGenerator.raiseShield();
                    }
                }
                /*
                foreach (IDevice device in structure.Devices)
                {
                    if (device.isActiveOn() == false)
                    {
                        device.activate();
                    }
                }

                foreach (IModule module in structure.Modules)
                {
                    if (module.isActiveOn() == false)
                    {
                        module.activate();
                    }
                }

                foreach (ProjectileLauncher launcher in launchers)
                {
                    launcher.activate();
                }

                foreach (LaserTurret laser in heavyLasers)
                {
                    laser.activate();
                }

                foreach (IWeapon weapon in structure.Weapons)
                {
                    if (weapon.isActiveOn() == false)
                    {
                        weapon.activate();
                    }
                }
                */
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
                #region target and orbit enemy
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

                    if (orbitBehaviour != null)
                    {
                        if (orbitBehaviour.Active == false)
                        {
                            orbitBehaviour.enableExclusively();
                        }
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
                #endregion
            }

            return "SEARCH";
            
        }

        protected void AI_MatchIsWaitingToStart(object sender)
        {
            FactionData faction = FactionManager.Instance.findFaction(structure.Faction.ID);

            List<Ship> ships = faction.FleetManager.getAllShips();

            squad = ships.Cast<Structure>().ToList();
        }
    }
}