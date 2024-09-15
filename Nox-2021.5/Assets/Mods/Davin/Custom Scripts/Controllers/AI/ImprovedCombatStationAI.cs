using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Weapons;
using NoxCore.Helm;
using NoxCore.Placeables;
using NoxCore.Rules;
using NoxCore.Utilities;

namespace NoxCore.Controllers
{
    [RequireComponent(typeof(BasicThreatEvaluator))]
    public class ImprovedCombatStationAI : AIStateController
    {
        BasicThreatEvaluator threatSys;

        public Structure primaryTargetStructure;
        public Module primaryTargetSystem;

        public (Structure primaryTargetStructure, Module primaryTargetSystem) getPrimaryTarget()
        {
            return (primaryTargetStructure, primaryTargetSystem);
        }

        public void setPrimaryTarget(Structure primaryTargetStructure, Module primaryTargetSystem = null)
        {
            this.primaryTargetStructure = primaryTargetStructure;
            this.primaryTargetSystem = primaryTargetSystem;
        }

        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, helm);

            threatSys = GetComponent<BasicThreatEvaluator>();

            aiActions.Add("SEARCH", searchAction);
            aiActions.Add("COMBAT", combatAction);

            state = "SEARCH";

            booted = true;
        }

        public virtual string searchAction()
        {
            if (structure.scanner.isActiveOn() == true)
            {
                List<Structure> enemiesInRange = structure.scanner.getEnemiesInRange();

                if (enemiesInRange.Count > 0)
                {
                    return "COMBAT";
                }
            }

            return "SEARCH";
        }

        public virtual string combatAction()
        {
            if (structure.scanner.isActiveOn() == true)
            {
                List<Structure> enemiesInRange = structure.scanner.getEnemiesInRange();

                if (enemiesInRange.Count <= 0)
                {
                    List<Structure> enemies = structure.Faction.EnemyStructures;

                    if (enemies.Count <= 0)
                    {
                        foreach (Weapon weap in structure.Weapons)
                        {
                            TargetableWeapon tWeap = (TargetableWeapon)weap;

                            if (tWeap != null)
                            {
                                tWeap.unacquireTarget();
                            }
                        }
                    }
                    else
                    {
                        Structure targetStructure = null;
                        Module targetSystem = null;

                        if (primaryTargetStructure != null && primaryTargetSystem != null)
                        {
                            if (Vector2.Distance(transform.position, primaryTargetSystem.transform.position) < structure.scanner.ScannerData.Radius)
                            {
                                targetStructure = primaryTargetStructure;
                                targetSystem = primaryTargetSystem;
                            }
                        }
                        else if (primaryTargetStructure != null && primaryTargetSystem == null)
                        {
                            if (enemies.Contains(primaryTargetStructure))
                            {
                                targetStructure = primaryTargetStructure;
                            }
                        }
                        else
                        {
                            // get sorted threat ratios for all enemy ships and structures in range
                            List<(Structure enemy, float threat)> threats = threatSys.calculateThreatRatios(structure, enemies);

                            if (threats.Count > 0)
                            {
                                targetStructure = threats[0].enemy;
                            }

                            // note: could also select a target system here as well
                        }

                        if (targetStructure == null)
                        {
                            foreach (Weapon weap in structure.Weapons)
                            {
                                TargetableWeapon tWeap = (TargetableWeapon)weap;

                                if (tWeap != null)
                                {
                                    tWeap.unacquireTarget();
                                }
                            }
                        }
                        else
                        {
                            // tell all fire groups to acquire the first target's hull (hence null for 2nd parameter)
                            foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                            {
                                fireGroup.setTarget(targetStructure.gameObject, targetSystem.gameObject);
                            }
                        }
                    }

                    return "COMBAT";
                }
                else
                {
                    return "SEARCH";
                }

            }

            return "COMBAT";
        }
    }
}