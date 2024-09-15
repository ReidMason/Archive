using UnityEngine;
using System.Collections;

using NoxCore.Fittings.Weapons;
using NoxCore.GUIs;
using NoxCore.Placeables;

namespace Davin.Fittings.Weapons
{
    public class GuidedTauntBomb : GuidedProximityBomb
    {
        public string taunt;

        public override void hasCollided(NoxObject collidedObject = null)
        {
            if (armed == true)
            {
                // trigger in-game effect
                weaponStructure.Gamemode.Gui.setMessage(weaponStructure.Name + ": " + taunt);
            }

            base.hasCollided(collidedObject);
        }
    }
}
