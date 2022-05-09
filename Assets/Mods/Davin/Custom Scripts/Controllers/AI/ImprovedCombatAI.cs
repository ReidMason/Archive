using UnityEngine;

using System.Collections.Generic;

using NoxCore.Helm;
using NoxCore.Fittings.Modules;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Rules;

namespace NoxCore.Controllers
{
    [RequireComponent(typeof(BasicThreatEvaluator))]
    public class ImprovedCombatAI : BasicCombatAI
    {
        BasicThreatEvaluator threatSys;

        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, helm);

            Ship ship = structure as Ship;

            if (ship != null)
            {
                ship.HasWarpedIn += OnHasWarpedIn;
                ship.SquadronHasWarpedIn += OnSquadronHasWarpedIn;
                ship.WingHasWarpedIn += OnWingHasWarpedIn;
                ship.FleetHasWarpedIn += OnFleetHasWarpedIn;
            }

            threatSys = GetComponent<BasicThreatEvaluator>();

            booted = true;
        }

        public override string combatAction()
        {
            List<Structure> enemiesInRange = structure.scanner.getEnemiesInRange();

            if (enemiesInRange.Count > 0)
            {
                List<(Structure enemy, float threat)> threats = threatSys.calculateThreatRatios(structure, enemiesInRange);

                // tell all fire groups to acquire the first target's hull (hence null for 2nd parameter)
                foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                {
                    fireGroup.setTarget(threats[0].enemy);
                }

                if (forceWaypointNavigation == true)
                {
                    if (seekBehaviour != null && seekBehaviour.Active == false)
                    {
                        seekBehaviour.enableExclusively();
                        currentWaypoint = 0;
                    }

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
                }
                else
                {
                    if (orbitBehaviour != null && orbitBehaviour.Active == false)
                    {
                        orbitBehaviour.enableExclusively();

                        // use the first target as the ship/structure to orbit around
                        orbitBehaviour.OrbitObject = threats[0].enemy.transform;

                        // use the first weapon's maximum range to determie a suitable orbit range
                        if (structure.Weapons.Count > 0)
                        {
                            orbitBehaviour.OrbitRange = structure.Weapons[0].WeaponData.MaxRange - 50;
                        }
                    }
                }

                if (avoidBehaviour != null && avoidBehaviour.Active == false)
                {
                    avoidBehaviour.enable();
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
        }

        protected void OnHasWarpedIn(object sender, WarpEventArgs args)
        {
            // add any initial targeting etc. here
        }

        protected void OnSquadronHasWarpedIn(object sender, SquadronWarpedEventArgs args)
        {
            // add any inter-squad/squadron setup for this ship here
        }

        protected void OnWingHasWarpedIn(object sender, WingWarpedEventArgs args)
        {
            // add any inter-wing setup for this ship here
        }

        protected void OnFleetHasWarpedIn(object sender, FleetWarpedEventArgs args)
        {
            // add any inter-fleet setup for this ship here
        }
    }
}