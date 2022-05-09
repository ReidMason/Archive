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
    public class Squad_Destroyer : Destroyer
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            // change any default values in the parent class here
            //TotalCost = 2000;

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

            addDefaultSocket("EngineBays/SmallEngineBay", "LeftEngine", new Vector2(-12.56f, -46.5f));
            addDefaultSocket("EngineBays/SmallEngineBay", "CentreEngine", new Vector2(0f, -46.5f));
            addDefaultSocket("EngineBays/SmallEngineBay", "RightEngine", new Vector2(12.56f, -46.5f));

            addDefaultSocket("ShieldPods/SmallShieldPod", "ShieldPod", new Vector2(0, 0));

            TurretSocketInfo turretSocketInfo;

            turretSocketInfo = addDefaultSocket("WeaponBays/SmallTurret", "FrontWeapon", new Vector2(0f, 27.6f)) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 120;
            }

            addDefaultSocket("WeaponBays/SmallLauncher", "CentreWeapon", new Vector2(-0.48f, 5.53f));

            turretSocketInfo = addDefaultSocket("WeaponBays/SmallTurret", "RearLeftWeapon", new Vector2(-7.8f, -7.9f), 40) as TurretSocketInfo;
            // change any default values in the socket class here
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 150;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/SmallTurret", "RearRightWeapon", new Vector2(9.7f, -4.9f), -40) as TurretSocketInfo;
            // change any default values in the socket class here
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 150;
            }
        }
        */
    }
}
