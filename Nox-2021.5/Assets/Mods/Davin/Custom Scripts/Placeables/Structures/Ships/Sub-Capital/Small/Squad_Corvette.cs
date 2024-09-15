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
    public class Squad_Corvette : Corvette
    {

        public override void init(NoxObjectData noxObjectData = null)
        {
            // change any default values in the parent class here
            //TotalCost = 1400;

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

            addDefaultSocket("ShieldPods/SmallShieldPod", "ShieldPod", new Vector2(0f, 0f));

            addDefaultSocket("EngineBays/SmallEngineBay", "LeftEngine", new Vector2(-17f, -9.45f));
            addDefaultSocket("EngineBays/SmallEngineBay", "RightEngine", new Vector2(17f, -9.45f));

            TurretSocketInfo turretSocketInfo;

            turretSocketInfo = addDefaultSocket("WeaponBays/SmallTurret", "ForwardWeapon", new Vector2(0f, 15.4f), 0) as TurretSocketInfo;
            // change any default values in the socket class here   
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 20;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/SmallTurret", "LeftWeapon", new Vector2(-9.55f, -14.33f), 30) as TurretSocketInfo;
            // change any default values in the socket class here   
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 150;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/SmallTurret", "RightWeapon", new Vector2(9.55f, -14.33f), -30) as TurretSocketInfo;
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
