using UnityEngine;
using UnityEngine.UI;

using NoxCore.GameModes;
using NoxCore.GUIs;
using NoxCore.Managers;

using Davin.GameModes;

namespace Davin.GUIs
{
    public class ConvoyAssaultGUI : NoxGUI
    {
        protected ConvoyAssaultMode convoyAssaultGameMode;

        protected bool testCompleted;
        
        Text clock;
        public Text convoyTotal;
        public Text convoyRemaining;
        public Text convoyDisabled;
        public Text convoyDestroyed;
        public Text convoyWarpedOut;

        // Use this for initialization
        public override void init()
        {
            base.init();

            convoyAssaultGameMode = GameManager.Instance.Gamemode as ConvoyAssaultMode;

            GameObject timerGO = GameObject.Find("Clock");

            if (timerGO != null)
            {
                clock = timerGO.GetComponent<Text>();
            }

            enabled = true;
        }

        public void setConvoyTotal(int total)
        {
            convoyTotal.text = total.ToString();
        }

        public void setConvoyRemaining(int remaining)
        {
            convoyRemaining.text = remaining.ToString();
        }

        public void setConvoyDisabled(int disabled)
        {
            convoyDisabled.text = disabled.ToString();
        }

        public void setConvoyDestroyed(int destroyed)
        {
            convoyDestroyed.text = destroyed.ToString();
        }

        public void setConvoyWarpedOut(int warped)
        {
            convoyWarpedOut.text = warped.ToString();
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            if (GameManager.Instance.Gamemode.matchState == GameMode.MatchState.INPROGRESS)
            {
                if (clock != null && timer != null)
                {
                    clock.text = timer.getTimeStr();
                }
            }
        }
    }
}