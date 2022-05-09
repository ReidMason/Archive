using UnityEngine;
using System.Collections.Generic;

using NoxCore.Data.Placeables;

namespace NoxCore.Placeables
{
    public class TriBaseMk1 : Station
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            // change any default values here
            //HullStrength = 15000;
            spin = 1.5f;

            if (noxObjectData != null)
            {
                base.init(Instantiate(noxObjectData));
            }
            else
            {
                base.init();
            }
        }
        /*
        public override void setDefaults()
        {
            base.setDefaults();

            // add default socket layout
            addDefaultSocket("ShieldPods/LargeShieldPod", "ShieldPod", new Vector2(0, 0));
            addDefaultSocket("WeaponBays/MediumWeaponBay", "Arm1Weapon", new Vector2(-204.0f, 174.0f));
            addDefaultSocket("WeaponBays/MediumWeaponBay", "Arm2Weapon", new Vector2(254.0f, 93.0f));
            addDefaultSocket("WeaponBays/MediumWeaponBay", "Arm3Weapon", new Vector2(-49.0f, -268.0f));
        }
        */
    }
}
