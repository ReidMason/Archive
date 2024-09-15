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
using NoxCore.Fittings.Weapons;
using NoxCore.Controllers;

namespace NoxCore.GUIs
{
    public class ScannerMonitor : StructureMonitor
    {
        public override void init()
        {
            base.init();
            monitorName = "Scanners";
        }

        protected override void updateReadout(Structure camTarget)
        {
            base.updateReadout(camTarget);

            // Short Range Scanner
            IScanner scanner = camTarget.scanner;

            if (scanner != null)
            {
                readoutInfo.Append("Short Range Scanner:" + scanner.getState());

                // display first 8 only
                int i = 0;

                foreach (GameObject obj in scanner.getObjectsInRange())
                {
                    if (obj != null)
                    {
                        readoutInfo.Append("\n" + obj.name);
                        i++;

                        if (i == 8) break;
                    }
                }
            }
        }
    }
}
