using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Data.Fittings;
using NoxCore.Fittings.Devices;
using NoxCore.Managers;

namespace Davin.Fittings.Devices
{
    public class BasicRandomTaunter : Device, ITaunt
    {
        public List<string> taunts = new List<string>();
        public float minTime, maxTime;
        protected float tauntTimer, nextTauntTime;

        public override void init(DeviceData deviceData = null)
        {
            base.init();

            nextTauntTime = Random.Range(minTime, maxTime);
        }

        public void taunt(string message)
        {
            GameManager.Instance.Gamemode.Gui.setMessage(structure.Name + ": " + message);
        }

        public override void update()
        {
            if (isActiveOn() == true && isFlippingActivation() == false)
            {
                tauntTimer += Time.deltaTime;

                if (tauntTimer >= nextTauntTime)
                {
                    taunt(taunts[Random.Range(0, taunts.Count)]);

                    tauntTimer = 0;
                    nextTauntTime = Random.Range(minTime, maxTime);
                }
            }
        }
    }
}