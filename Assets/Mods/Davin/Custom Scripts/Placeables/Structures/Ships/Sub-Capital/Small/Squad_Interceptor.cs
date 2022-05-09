using UnityEngine;
using System.Collections.Generic;

using NoxCore.Buffs;
using NoxCore.Controllers;
using NoxCore.Data.Placeables;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Placeables.Ships;

using Davin.Buffs;

namespace Davin.Placeables.Ships
{
    public class Squad_Interceptor : Interceptor
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            // change any default values in the parent class here
            //MaxForce = 150000;
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

            addDefaultSocket("EngineBays/TinyEngineBay", "ForeLeftEngine", new Vector2(-14.0f, 2.21f));
            addDefaultSocket("EngineBays/SmallEngineBay", "LeftEngine", new Vector2(-5.54f, -30.65f));
            addDefaultSocket("EngineBays/SmallEngineBay", "RightEngine", new Vector2(5.54f, -30.65f));
            addDefaultSocket("EngineBays/TinyEngineBay", "ForeRightEngine", new Vector2(14.0f, 2.21f));

            TurretSocketInfo turretSocketInfo;

            turretSocketInfo = addDefaultSocket("Mixed/SmallTurretOrLauncher", "ForwardLeftWeapon", new Vector2(-9.2f, 10f), 30) as TurretSocketInfo;
            // change any default values in the socket class here   
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 90;
            }

            turretSocketInfo = addDefaultSocket("Mixed/SmallTurretOrLauncher", "LeftWeapon", new Vector2(-6.7f, -6.48f), 110) as TurretSocketInfo;
            // change any default values in the socket class here   
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 90;
            }

            turretSocketInfo = addDefaultSocket("Mixed/SmallTurretOrLauncher", "ForwardRightWeapon", new Vector2(9.2f, 10f), -30) as TurretSocketInfo;
            // change any default values in the socket class here   
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 90;
            }

            turretSocketInfo = addDefaultSocket("Mixed/SmallTurretOrLauncher", "RightWeapon", new Vector2(6.7f, -6.48f), -110) as TurretSocketInfo;
            // change any default values in the socket class here   
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 90;
            }
        }

        protected override void Structure_UltimateActivated(object sender)
        {
            base.Structure_UltimateActivated(sender);

            MaxSpeedBuff maxSpeedBuff = new MaxSpeedBuff(engines, BuffType.STANDARD, 1, 500, true, 10);

            BuffManager.addBuff(maxSpeedBuff);
        }
        */
    }
}
