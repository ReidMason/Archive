using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Fittings.Weapons;
using NoxCore.Placeables;
using NoxCore.Utilities;

namespace NoxCore.Controllers
{
    public class BasicThreatEvaluator : MonoBehaviour
    {
        public List<(Structure enemy, float threat)> calculateThreatRatios(Structure structure, List<Structure> enemiesInRange)
        {
            List<(Structure enemy, float threat)> threatRatios = new List<(Structure, float)>();

            foreach (Structure enemy in enemiesInRange)
            {
                threatRatios.Add( (enemy, calculateThreatRatio(structure, enemy)) );
            }

            // sort threats based on threat ratios           			
            threatRatios.Sort((a, b) => a.threat.CompareTo(b.threat));

            return threatRatios;
        }

        protected float calculateThreatRatio(Structure structure, Structure enemyStructure)
        {
            float timeToKillEnemy = timeToKill(structure, enemyStructure);
            float timeToKillMe = timeToKill(enemyStructure, structure);

            // TODO - check this!!!
            float threatRatio = (timeToKillMe + (3 * timeToKillEnemy)) / 4.0f;

            return threatRatio;
        }

        protected float timeToKill(Structure attacker, Structure defender)
        {
            // note: this just takes into account the hull strength and does not include current shield strength
            float defenderHealth = defender.HullStrength;
            float damagePerSecond = getDamage(attacker, defender);

            // if the damagePerSecond is zero, clamp the timeToKill to a suitably large value
            if (damagePerSecond != 0)
            {
                return defenderHealth / damagePerSecond;
            }

            // TODO - why not return Mathf.Infinity?
            return 1000000.0f;
        }

        protected float getDamage(Structure attacker, Structure defender)
        {
            float damagePerSecond = 0;

            if (attacker.Weapons == null) return damagePerSecond;

            // simple scheme that assumes all active weapons would be used against the defender
            foreach (Weapon weapon in attacker.Weapons)
            {
                if (weapon.isActiveOn())
                {
                    damagePerSecond += weapon.getDPS(defender.gameObject);
                }
            }

            return damagePerSecond;
        }
    }
}