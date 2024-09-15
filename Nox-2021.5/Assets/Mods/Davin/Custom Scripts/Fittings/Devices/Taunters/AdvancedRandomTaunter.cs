using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Data.Fittings;
using NoxCore.Fittings.Devices;
using NoxCore.Placeables;
using NoxCore.Managers;

namespace Davin.Fittings.Devices
{
    public class AdvancedRandomTaunter : Device, ITaunt
    {
        public List<string> attackTaunts = new List<string>();
        public List<string> defenceTaunts = new List<string>();

        [Range (0, 1)]
        public float chanceToTaunt;

        public override void init(DeviceData deviceData = null)
        {
            base.init();

            structure.TakenAnyDamage += Structure_TakenAnyDamage;
            structure.InstigatedAnyDamage += Structure_InstigatedAnyDamage;
        }

        protected void Structure_TakenAnyDamage(object sender, DamageEventArgs args)
        {
            if (isActiveOn() == true && isFlippingActivation() == false)
            {
                float rand = Random.value;

                if (rand < chanceToTaunt)
                {
                    taunt(defenceTaunts[Random.Range(0, defenceTaunts.Count)]);
                }
            }
        }

        protected void Structure_InstigatedAnyDamage(object sender, DamageEventArgs args)
        {
            if (isActiveOn() == true && isFlippingActivation() == false)
            {
                float rand = Random.value;

                if (rand < chanceToTaunt)
                {
                    taunt(attackTaunts[Random.Range(0, attackTaunts.Count)]);
                }
            }
        }

        public void taunt(string message)
        {
            GameManager.Instance.Gamemode.Gui.setMessage(structure.Name + ": " + message);
        }
    }
}