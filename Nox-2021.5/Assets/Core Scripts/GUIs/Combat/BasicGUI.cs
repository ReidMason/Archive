using UnityEngine;
using UnityEngine.UI;
using System.Collections;
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
	public class BasicGUI : NoxGUI
	{
		Text clock;

		// Use this for initialization
		public override void init()
		{
			base.init();

			GameObject timerGO = GameObject.Find("Clock");

			if (timerGO != null)
			{
				clock = timerGO.GetComponent<Text>();
			}


			showFactions = false;
            showNames = false;

            enabled = true;
		}

		protected override void Update ()
		{
			base.Update();	
		}

		protected override void OnGUI()
		{
			base.OnGUI();

			clock.text = timer.getTimeStr();
		}
	}
}
