using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System;
using NoxCore.Utilities;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Fittings;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Devices;

namespace NoxCore.GUIs
{
    public class CombatStatsMonitor : StructureMonitor
    {
        public override void init()
        {
            base.init();
            monitorName = "Combat Stats";
        }

        protected override void updateReadout(Structure camTarget)
        {
            base.updateReadout(camTarget);

            float totalShieldCharge = 0;

            foreach (ShieldGenerator shield in camTarget.shields)
            {
                totalShieldCharge += shield.CurrentCharge;
            }
           
            readoutInfo.Append("Time alive: " + Timer.formatTimer(camTarget.AliveTimer, false) + "\n");

            if (camTarget.Stats != null)
            {
                readoutInfo.Append("\nAverage Survival Time: " + Timer.formatTimer(camTarget.Stats.averageSurvivalTime, true));
                readoutInfo.Append("\nHull: " + camTarget.HullStrength);
                readoutInfo.Append("\nShields: " + totalShieldCharge);
                readoutInfo.Append("\nKills: " + camTarget.Stats.numKills);
                readoutInfo.Append("\nAssists: " + camTarget.Stats.numAssists);
                readoutInfo.Append("\nDeaths: " + camTarget.Stats.numDeaths);
                readoutInfo.Append("\nModules Destroyed: " + camTarget.Stats.numModulesDestroyed);
                readoutInfo.Append("\nDamage Caused: " + camTarget.Stats.totalDamageInflicted);
                readoutInfo.Append("\nDamage Taken: " + camTarget.Stats.totalDamageTaken);
                readoutInfo.Append("\nHull Damage Taken: " + camTarget.Stats.totalHullDamageTaken);
                readoutInfo.Append("\nShield Damage Taken: " + camTarget.Stats.totalShieldDamageTaken);
                readoutInfo.Append("\nModule Damage Taken: " + camTarget.Stats.totalArmourDamageTaken);
            }
        }
    }
}
