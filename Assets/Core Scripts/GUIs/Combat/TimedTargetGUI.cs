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
    public class TimedTargetGUI : NoxGUI
    {
        protected TargetRangeMode gameMode;

        protected float maxTimer;
        protected bool testCompleted;
        protected int maxTargets, targetsRemaining;
        protected GameObject watchTarget;

        Text clock;
        Text targets;

        // Use this for initialization
        public override void init()
        {
            base.init();

            gameMode = GameManager.Instance.Gamemode as TargetRangeMode;

            if (gameMode != null)
            {
                maxTargets = gameMode.maxTargets;
            }
            else
            {
                D.error("GameMode: {0}", "TimedTargetGUI cannot obtain max targets from custom game mode");
            }

            maxTimer = gameMode.maxTime;

            GameObject clockGO = GameObject.Find("Clock");

            if (clockGO != null)
            {
                clock = clockGO.GetComponent<Text>();
            }

            GameObject targetsGO = GameObject.Find("Targets Value");

            if (targetsGO != null)
            {
                targets = targetsGO.GetComponent<Text>();
            }

            enabled = true;
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            if (clock != null && timer != null)
            {
                clock.text = timer.getTimeStr();
            }

            targets.text = gameMode.targetsDestroyed + " of " + maxTargets;
        }       
    }
}