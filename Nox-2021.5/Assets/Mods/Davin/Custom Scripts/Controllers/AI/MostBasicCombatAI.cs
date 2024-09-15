using UnityEngine;
using System.Collections.Generic;

using NoxCore.Helm;
using NoxCore.Placeables;
using NoxCore.Rules;

using NoxCore.Fittings.Devices;

namespace NoxCore.Controllers
{
    public class MostBasicCombatAI : AIStateController
    {
        public List<Vector2> waypoints = new List<Vector2>();
        public int currentWaypoint = 0;

        protected SeekBehaviour seekBehaviour;
        protected OrbitBehaviour orbitBehaviour;
        protected AvoidBehaviour avoidBehaviour;

        List<Structure> enemiesInRange = new List<Structure>();

        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, helm);

            aiActions.Add("SEARCH", searchAction);
            aiActions.Add("COMBAT", combatAction);

            state = "SEARCH";

            waypoints.Add(new Vector2(-250, 0));
            waypoints.Add(new Vector2(0, 250));
            waypoints.Add(new Vector2(250, 0));
            waypoints.Add(new Vector2(0, -250));

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

            booted = true;
        }

        public override void reset()
        {
            base.reset();

            state = "SEARCH";
        }

        protected virtual Vector2 setHelmDestination()
        {
            Vector2 nextPoint = waypoints[currentWaypoint];

            currentWaypoint++;

            if (currentWaypoint == waypoints.Count)
            {
                currentWaypoint = 0;
            }

            return nextPoint;
        }

        public virtual string searchAction()
        {
            if (structure.Faction.EnemyStructures.Count == 0)
            {
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

        public virtual string combatAction()
        {
            enemiesInRange = structure.scanner.getEnemiesInRange();

            if (structure.scanner.isActiveOn() == true)
            {
                if (enemiesInRange.Count > 0)
                {
                    // tell all fire groups to acquire the first target's hull (hence null for 2nd parameter)
                    foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                    {
                        fireGroup.setTarget(enemiesInRange[0]);
                    }

                    if (orbitBehaviour != null && orbitBehaviour.Active == false)
                    {
                        orbitBehaviour.enableExclusively();

                        // use the first target as the ship/structure to orbit around
                        orbitBehaviour.OrbitObject = enemiesInRange[0].transform;

                        // use the first weapon's maximum range to determine a suitable orbit range (with a wiggle room factor e.g. -50 units)
                        if (structure.Weapons.Count > 0)
                        {
                            orbitBehaviour.OrbitRange = structure.Weapons[0].WeaponData.MaxRange - 50;
                        }
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
            else
            {
                return "SEARCH";
            }
        }
    }
}