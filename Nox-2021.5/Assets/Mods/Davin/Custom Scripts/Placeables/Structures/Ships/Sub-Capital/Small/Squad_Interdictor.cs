using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Controllers;
using NoxCore.Data.Placeables;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Placeables.Ships;

namespace Davin.Placeables.Ships
{
    public class Squad_Interdictor : Interdictor
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            // change any default values in the parent class here
            //TotalCost = 1800;

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

            addDefaultSocket("EngineBays/TinyEngineBay", "ForeLeftEngine", new Vector2(-16.86f, 2.9f));
            addDefaultSocket("EngineBays/SmallEngineBay", "LeftEngine", new Vector2(-6.45f, -36f));
            addDefaultSocket("EngineBays/SmallEngineBay", "RightEngine", new Vector2(6.45f, -36f));
            addDefaultSocket("EngineBays/TinyEngineBay", "ForeRightEngine", new Vector2(16.86f, 2.9f));

            addDefaultSocket("ShieldPods/SmallShieldPod", "ShieldPod", new Vector2(0, 6.3f));

            addDefaultSocket("WeaponBays/SmallPlasmaEmitter", "FrontWeapon", new Vector2(-0.28f, -2.67f));
            addDefaultSocket("WeaponBays/SmallLauncher", "RearLeftWeapon", new Vector2(-4.35f, -12.5f));
            addDefaultSocket("WeaponBays/SmallLauncher", "RearRightWeapon", new Vector2(6.05f, -11.32f));
        }
        */
    }
}
