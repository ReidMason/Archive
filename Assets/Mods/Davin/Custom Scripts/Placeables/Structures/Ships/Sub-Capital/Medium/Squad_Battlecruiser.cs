using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using NoxCore.Buffs;
using NoxCore.Controllers;
using NoxCore.Data.Placeables;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Fittings.Weapons;
using NoxCore.Placeables.Ships;

using Davin.Buffs;

namespace Davin.Placeables.Ships
{
    public class Squad_Battlecruiser : Battlecruiser
    {       

        public override void init(NoxObjectData noxObjectData = null)
        {
            //set default values if different from base class
            //TotalCost = 3500;

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

            addDefaultSocket("EngineBays/MediumEngineBay", "LeftEngine", new Vector2(-22.3f, -100.93f));
            addDefaultSocket("EngineBays/MediumEngineBay", "RightEngine", new Vector2(22.3f, -100.93f));

            addDefaultSocket("ShieldPods/MediumShieldPod", "ShieldPod", new Vector2(0, 0));

            TurretSocketInfo turretSocketInfo;

            turretSocketInfo = addDefaultSocket("WeaponBays/MediumTurret", "FrontWeapon", new Vector2(0f, 70.5f)) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 60;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/MediumTurret", "LeftWeapon", new Vector2(-25.2f, 45.4f), 45) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 120;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/MediumTurret", "RightWeapon", new Vector2(25.2f, 45.4f), -45f) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 120;
            }

            turretSocketInfo = addDefaultSocket("Mixed/MediumGenericWeapon", "RearWeapon", new Vector2(0f, -38.1f), 180) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 180;
            }

            addDefaultSocket("WeaponBays/MediumLauncher", "LeftRearWeapon", new Vector2(-25.2f, 0f));

            addDefaultSocket("WeaponBays/MediumLauncher", "RightRearWeapon", new Vector2(25.2f, 0f));

            addDefaultSocket("WeaponBays/MediumLauncher", "BombBay", new Vector2(0, 0));
        }
        
        protected override void Structure_UltimateActivated(object sender)
        {
            base.Structure_UltimateActivated(sender);

            List<IWeapon> allWeapons = Weapons.Cast<IWeapon>().ToList<IWeapon>();

            BaseDamageBuff baseDamageBuff = new BaseDamageBuff(allWeapons, BuffType.STANDARD, 1, 200, true, 15);

            BuffManager.addBuff(baseDamageBuff);
        }
        */
    }
}
