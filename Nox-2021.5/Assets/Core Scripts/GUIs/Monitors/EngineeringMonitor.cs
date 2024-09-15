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
using NoxCore.Controllers;

namespace NoxCore.GUIs
{
    public class EngineeringMonitor : StructureMonitor
    {
        public override void init()
        {
            base.init();
            monitorName = "Engineering";
        }

        protected override void updateReadout(Structure camTarget)
        {
            base.updateReadout(camTarget);

            //AI
            //include the current AI here & state

            readoutInfo.Append("Thermal Control System:" + camTarget.thermalcontrol.getState() + camTarget.thermalcontrol.getCurrentHeat().ToString("F2") + " / " + camTarget.thermalcontrol.getHeatCapacity().ToString("F2") + " - " + camTarget.thermalcontrol.getHeatPercentage());
            // Thermal Control System
            readoutInfo.Append("\nThermal Control System:" + camTarget.thermalcontrol.getState() + camTarget.thermalcontrol.getCurrentHeat().ToString("F2") + " / " + camTarget.thermalcontrol.getHeatCapacity().ToString("F2") + " - " + camTarget.thermalcontrol.getHeatPercentage());

            // Hull Strength
            readoutInfo.Append("\nHull: " + camTarget.HullStrength);

            float totalShieldCharge = 0;
            float maximumShieldCharge = 0;

            // Shields
            foreach (ShieldGenerator shield in camTarget.shields)
            {
                totalShieldCharge += shield.CurrentCharge;
                maximumShieldCharge += shield.ShieldGeneratorData.MaxCharge;
            }

            readoutInfo.Append("\nCombined Shield Strength: " + (int)totalShieldCharge + " / " + (int)maximumShieldCharge + "   " + (int)(totalShieldCharge * 100 / maximumShieldCharge) + "pc");

            foreach (ShieldGenerator shield in camTarget.shields)
            {
                readoutInfo.Append("\n" + shield.gameObject.name + shield.getState() + (int)shield.CurrentCharge + "   " + (int)(shield.CurrentCharge * 100 / shield.ShieldGeneratorData.MaxCharge) + "pc");
            }            

            // Engines
            Ship shipCamTarget = camTarget as Ship;

            if (shipCamTarget != null)
            {
                foreach (Engine engine in shipCamTarget.engines)
                {
                    readoutInfo.Append("\n" + engine.gameObject.name + engine.getState());
                }
            }

        }
    }
}
