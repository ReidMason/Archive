using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

using NoxCore.Cameras;
using NoxCore.Controllers;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Weapons;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Utilities;

namespace NoxCore.GUIs
{
	public class DeathMatchGUI : NoxGUI
	{
        protected Rect timerInfoRect;

        protected bool showFactionLabels = true;

        // Use this for initialization
        public override void init()
        {
            base.init();

            timerInfoRect = new Rect(Screen.width - 300, 5, 300, 20);

            enabled = true;
        }
    }
}
