using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Data.Placeables;
using NoxCore.Controllers;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Placeables.Ships;

namespace Davin.Placeables.Ships
{
    public class Squad_Battleship : Battleship
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            // change any default values in the parent class here
            //TotalCost = 4400;

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

            addDefaultSocket("EngineBays/MediumEngineBay", "OuterLeftEngine", new Vector2(-28.8f, -132.5f));
            addDefaultSocket("EngineBays/SmallEngineBay", "InnerLeftEngine", new Vector2(-8.2f, -91.2f));
            addDefaultSocket("EngineBays/SmallEngineBay", "InnerRightEngine", new Vector2(8.2f, -91.2f));
            addDefaultSocket("EngineBays/MediumEngineBay", "OuterRightEngine", new Vector2(28.8f, -132.5f));

            addDefaultSocket("ShieldPods/LargeShieldPod", "ShieldPod1", new Vector2(0, 36));
            addDefaultSocket("ShieldPods/LargeShieldPod", "ShieldPod2", new Vector2(0, -36));

            TurretSocketInfo turretSocketInfo;

            turretSocketInfo = addDefaultSocket("WeaponBays/MediumTurret", "ForeWeapon1", new Vector2(0.12f, 44.87f)) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 72;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/MediumTurret", "ForeWeapon2", new Vector2(8.76f, 38.76f), -72) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 72;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/MediumTurret", "ForeWeapon3", new Vector2(5.31f, 28.41f), -144) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 72;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/MediumTurret", "ForeWeapon4", new Vector2(-5.31f, 28.41f), 144) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 72;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/MediumTurret", "ForeWeapon5", new Vector2(-8.76f, 38.76f), 72) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 72;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/MediumTurret", "AftWeapon1", new Vector2(-7.36f, -18.3f), 54) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 72;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/MediumTurret", "AftWeapon2", new Vector2(2.78f, -14.94f), -18) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 72;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/MediumTurret", "AftWeapon3", new Vector2(9.42f, -23.45f), -90) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 72;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/MediumTurret", "AftWeapon4", new Vector2(2.78f, -32.12f), -162) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 72;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/MediumTurret", "AftWeapon5", new Vector2(-7.36f, -28.7f), -234) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 72;
            }
        }
        */
    }
}
