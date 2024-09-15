using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using NoxCore.Controllers;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Weapons;
using NoxCore.GameModes;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Utilities;

namespace NoxCore.GUIs
{
    public class KobayashiMaruGUI : NoxGUI
    {
        protected KobayashiMaruMode kobayashiMaruMode;

        protected bool testCompleted;
        protected int waveNum;
        protected GameObject trappedShipTarget;

        Text clock;
        Text wave;

        // Use this for initialization
        public override void init()
        {
            base.init();

            kobayashiMaruMode = GameManager.Instance.Gamemode as KobayashiMaruMode;

            GameObject clockGO = GameObject.Find("Clock");

            if (clockGO != null)
            {
                clock = clockGO.GetComponent<Text>();
            }

            enabled = true;
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            clock.text = timer.getTimeStr();
                        
        }

    }
}