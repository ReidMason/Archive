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
    public class Squad_Dreadnought : Dreadnought
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            // change any default values in the parent class here
            //TotalCost = 5500;

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

            addDefaultSocket("EngineBays/LargeEngineBay", "FarOuterLeftEngine", new Vector2(-60.93f, -158.64f));
            addDefaultSocket("EngineBays/MediumEngineBay", "OuterLeftEngine", new Vector2(-31.4f, -155.1f));
            addDefaultSocket("EngineBays/TinyEngineBay", "InnerLeftEngine", new Vector2(-11.9f, -160.38f));
            addDefaultSocket("EngineBays/TinyEngineBay", "InnerRightEngine", new Vector2(11.9f, -160.38f));
            addDefaultSocket("EngineBays/MediumEngineBay", "OuterRightEngine", new Vector2(31.4f, -155.1f));
            addDefaultSocket("EngineBays/LargeEngineBay", "FarOuterRightEngine", new Vector2(60.93f, -158.64f));

            addDefaultSocket("ShieldPods/LargeShieldPod", "ShieldPod", new Vector2(0, 0));

            TurretSocketInfo turretSocketInfo;

            turretSocketInfo = addDefaultSocket("WeaponBays/LargeTurret", "ForeLeftWeapon", new Vector2(-8.68f, 11.06f)) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 180;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/LargeTurret", "ForeRightWeapon", new Vector2(14.06f, 1.4f)) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 180;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/LargeTurret", "ForeRearWeapon", new Vector2(-5.1f, -13.18f)) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 180;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/LargePlasmaEmitter", "ForeLeftPlasma", new Vector2(-17.39f, -1.98f)) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 180;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/LargePlasmaEmitter", "ForeRightForwardPlasma", new Vector2(7.1f, 16.3f)) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 180;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/LargePlasmaEmitter", "ForeRightRearPlasma", new Vector2(10.6f, -14.3f)) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 180;
            }

            addDefaultSocket("WeaponBays/MediumLauncher", "AftLeftForwardWeapon", new Vector2(-31.1f, -78.23f));

            addDefaultSocket("WeaponBays/MediumLauncher", "AftLeftRearWeapon", new Vector2(-31.1f, -96.72f));

            addDefaultSocket("WeaponBays/MediumLauncher", "AftRightForwardWeapon", new Vector2(31.1f, -78.23f));

            addDefaultSocket("WeaponBays/MediumLauncher", "AftRightRearWeapon", new Vector2(31.1f, -96.72f));

        }
        */
    }
}
