using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using NoxCore.Controllers;
using NoxCore.Data;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Weapons;
using NoxCore.Helm;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Rules;

namespace Davin.Controllers
{
    [RequireComponent(typeof(BasicThreatEvaluator))]
    public class CombatEscortAI : AIStateController
    {
        BasicThreatEvaluator threatSys;

        protected SeekBehaviour seekBehaviour;
        protected OrbitBehaviour orbitBehaviour;
        protected AvoidBehaviour avoidBehaviour;
        
        protected IComms comms;

        protected Ship boardingShip;
        protected Structure boardingTarget;
        protected List<Weapon> boardingTargetWeapons;

        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, helm);

            Ship ship = structure as Ship;

            if (ship != null)
            {
                ship.HasWarpedIn += OnHasWarpedIn;
                ship.FleetHasWarpedIn += OnFleetHasWarpedIn;
            }

            aiActions.Add("SEARCH", searchAction);
            aiActions.Add("PROTECT", protectAction);

            state = "SEARCH";

            seekBehaviour = Helm.getBehaviourByName("SEEK") as SeekBehaviour;
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
            
            threatSys = GetComponent<BasicThreatEvaluator>();

            comms = structure.getDevice<IComms>() as IComms;

            boardingTargetWeapons = new List<Weapon>();

            booted = true;
        }

        protected virtual Vector2 setHelmDestination()
        {
            // perform some sort of search pattern to find the boarding ship
            // here's basic one that's quite poor
            if (boardingTarget != null)
            {
                return (Vector2)(boardingTarget.transform.position + (Vector3)(3000 * Random.insideUnitCircle));
            }
            else
            {
                return Vector2.zero;
            }
        }

        public void setBoardingTarget(Structure boardingTarget)
        {
            this.boardingTarget = boardingTarget;
        }

        public void setBoardingShip(Ship boardingShip)
        {
            this.boardingShip = boardingShip;
        }

        public virtual string searchAction()
        {
            if (structure.scanner.isActiveOn() == true)
            {
                // is the ship we are escorting still in scanner range? If so, follow it and protect it
                if (boardingShip != null && structure.scanner.getFriendliesInRange().Contains(boardingShip))
                {
                    Helm.destination = boardingShip.transform.position;
                    return "PROTECT";
                }

                if (seekBehaviour != null && seekBehaviour.Active == false)
                {
                    seekBehaviour.enableExclusively();
                }

                // set destination
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

            return "SEARCH";
        }

        public string protectAction()
        {
            if (structure.scanner.isActiveOn() == true)
            {
                // is the ship we are escorting still in scanner range? If not, run a search pattern to find it.
                if (boardingShip != null && !structure.scanner.getFriendliesInRange().Contains(boardingShip))
                {
                    Helm.destination = null;
                    return "SEARCH";
                }

                List<Structure> enemiesInRange = structure.scanner.getEnemiesInRange();

                if (enemiesInRange.Count > 0)
                {
                    // attack the station weaponry or enemy ships?
                    if (boardingTarget != null && enemiesInRange.Contains(boardingTarget))
                    {
                        boardingTargetWeapons = boardingTarget.Weapons;

                        // very simple scheme that gets the first non-destroyed weapon on the boarding target in scanner range and sets all weapons to shoot at it
                        Weapon targetWeapon = null;

                        foreach (Weapon weapon in boardingTargetWeapons)
                        {
                            if (weapon.destroyed == false)
                            {
                                targetWeapon = weapon;
                                break;
                            }
                        }

                        if (targetWeapon != null)
                        {
                            // tell all fire groups to acquire the selected weapon
                            foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                            {
                                fireGroup.setTarget(boardingTarget, targetWeapon);
                            }
                        }
                    }
                    else
                    {
                        // get sorted threat ratios for all enemy ships and structures in range
                        List<(Structure structure, float threat)> threats = threatSys.calculateThreatRatios(structure, enemiesInRange);

                        // tell all fire groups to acquire the first target's hull (hence null for 2nd parameter)
                        foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                        {
                            fireGroup.setTarget(threats[0].structure);
                        }
                    }
                }
                else
                {
                    foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                    {
                        fireGroup.unacquireTarget();
                    }
                }

                if (seekBehaviour != null && seekBehaviour.Active == false)
                {
                    seekBehaviour.enableExclusively();
                }

                Helm.destination = boardingShip.transform.position;

                // draw a line to the destination
                if (Helm.destination != null && Cam.followTarget != null && Cam.followTarget.gameObject == Helm.ShipStructure.gameObject)
                {
                    Debug.DrawLine(structure.transform.position, Helm.destination.GetValueOrDefault(), Color.blue, Time.deltaTime, true);
                }
            }

            return "PROTECT";
        }

        protected void OnHasWarpedIn(object sender, WarpEventArgs args)
        {
            // add any initial targeting etc. here
            
            setBoardingTarget(FindObjectOfType<Station>());
        }

        protected void OnFleetHasWarpedIn(object sender, FleetWarpedEventArgs args)
        {
            // add any inter-fleet setup for this ship here
            setBoardingShip(GameObject.Find("Delta Cuckoo").GetComponent<Ship>());
        }

        public void LauncherFired(object sender, WeaponFiredEventArgs args)
        {
            //Gui.setMessage(args.weaponFired + " has fired!");
        }
    }
}