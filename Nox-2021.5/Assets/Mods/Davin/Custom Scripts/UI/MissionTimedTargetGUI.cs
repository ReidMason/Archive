using UnityEngine;
using UnityEngine.UI;

using NoxCore.GameModes;
using NoxCore.GUIs;
using NoxCore.Managers;

namespace Davin.GUIs
{
    public class MissionTimedTargetGUI : NoxGUI
    {
        protected MissionTargetRangeMode gameMode;

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

            gameMode = GameManager.Instance.Gamemode as MissionTargetRangeMode;

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