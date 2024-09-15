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
    public class Squad_Cruiser : Cruiser
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            // change any default values in the parent class here

            //TotalCost = 2700;

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

            addDefaultSocket("EngineBays/MediumEngineBay", "LeftEngine", new Vector2(-19.9f, -80.02f));
            addDefaultSocket("EngineBays/MediumEngineBay", "RightEngine", new Vector2(19.9f, -80.02f));

            addDefaultSocket("ShieldPods/MediumShieldPod", "ShieldPod", new Vector2(0, 0));

            TurretSocketInfo turretSocketInfo;

            turretSocketInfo = addDefaultSocket("Mixed/MediumTurretOrLauncher", "FrontLeftWeapon", new Vector2(-8.12f, 45.11f)) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 180;
            }

            turretSocketInfo = addDefaultSocket("Mixed/MediumTurretOrLauncher", "RightWeapon", new Vector2(13.2f, 36.3f)) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 180;
            }

            turretSocketInfo = addDefaultSocket("Mixed/MediumTurretOrLauncher", "LeftWeapon", new Vector2(-5.1f, 22.7f)) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 180;
            }

            addDefaultSocket("WeaponBays/SmallLauncher", "BombBay", new Vector2(0, 0));
        }
        */
    }
}
