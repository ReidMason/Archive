using UnityEngine;
using System.Collections.Generic;

using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Weapons;
using NoxCore.Helm;
using NoxCore.Placeables;
using NoxCore.Utilities;

namespace NoxCore.Controllers
{
    [RequireComponent(typeof(BasicThreatEvaluator))]
    public class CombatStationAI : AIStateController
    {
        BasicThreatEvaluator threatSys;

        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, helm);

            threatSys = GetComponent<BasicThreatEvaluator>();

            aiActions.Add("COMBAT", combatAction);

            state = "COMBAT";

            booted = true;
        }

        public virtual string combatAction()
        {
            if (structure.scanner.isActiveOn() == true)
            {
                List<Structure> enemiesInRange = structure.scanner.getEnemiesInRange();

                if (enemiesInRange.Count > 0)
                {
                    // get sorted threat ratios for all enemy ships and structures in range
                    List<(Structure structure, float id)> threats = threatSys.calculateThreatRatios(structure, enemiesInRange);

                    // tell all fire groups to acquire the first target's hull (hence null for 2nd parameter)
                    foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                    {
                        fireGroup.setTarget(threats[0].structure);
                    }
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
                }
            }

            return "COMBAT";
        }
    }
}