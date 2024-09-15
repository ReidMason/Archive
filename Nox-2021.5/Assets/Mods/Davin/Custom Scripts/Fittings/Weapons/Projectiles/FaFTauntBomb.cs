using UnityEngine;
using System.Collections;

using NoxCore.Effects;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Utilities;

namespace NoxCore.Fittings.Weapons
{
    public class FafTauntBomb : UnguidedProximityBomb
    {
        public string taunt;

        public override void hasCollided(NoxObject collidedObject = null)
        {
            if (armed == true)
            {
                // trigger in-game effect
                GameManager.Instance.Gamemode.Gui.setMessage(weaponStructure.Name + ": " + taunt);
            }

            base.hasCollided(collidedObject);
        }
   }
}