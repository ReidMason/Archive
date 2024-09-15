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
    public class PowerMonitor : StructureMonitor
    {
        public override void init()
        {
            base.init();
            monitorName = "Power";
        }

        protected override void updateReadout(Structure camTarget)
        {
            base.updateReadout(camTarget);

            // Powergrid
            readoutInfo.Append("Powergrid: " + camTarget.powergrid.getState() + camTarget.powergrid.getCurrentPower().ToString("F2") + " / " + camTarget.powergrid.getMaxPower().ToString("F2"));

            //requirements/Current Usage
            readoutInfo.Append("\nPower Requirements: " + camTarget.getPowerRequirements());

            // Power generators
            //readoutInfo.Append("\n\nPower Generators: ");

            //foreach (PowerCore powergenerator in camTarget.powergenerators)
            //{
            //    readoutInfo.Append("\n" + powergenerator.gameObject.name + powergenerator.getState() + powergenerator.getHeatPercentage() + powergenerator.powerGeneration);
            //}

        }
    }
}
