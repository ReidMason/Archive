using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using NoxCore.Data.Placeables;
using NoxCore.Fittings.Sockets;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;

namespace Davin.Placeables
{
    public class MaxSecPrison : Station
    {
        public float heavyPlatformSpin;
        public Transform standardWeaponPlatforms, heavyWeaponPlatforms;

        public override void init(NoxObjectData noxObjectData = null)
        {
            // change any default values here
            //HullStrength = 20000;
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

            StructureSocketInfo socketInfo = addDefaultSocket("DockingPorts/LargeDockingPort", "CentrePort", Vector2.zero);

            DockingSocketInfo dockingSocketInfo = socketInfo as DockingSocketInfo;
            if (dockingSocketInfo != null)
            {
                dockingSocketInfo.minDockingSize = StructureSize.SMALL;
                dockingSocketInfo.maxDockingSize = StructureSize.LARGE;
            }

            standardWeaponPlatforms = transform.Find("StandardWeaponPlatforms");
            heavyWeaponPlatforms = transform.Find("HeavyWeaponPlatforms");

            int i = 0;
            TurretSocketInfo turretSocketInfo = null;

            foreach (Transform trans in standardWeaponPlatforms.GetComponentsInChildren<Transform>())
            {
                if (trans != standardWeaponPlatforms)
                {
                    i++;
                    turretSocketInfo = addDefaultSocket("Mixed/SmallTurretOrLauncher", "StandardWeaponTurret" + i, new Vector2(trans.position.x, trans.position.y), trans.rotation.eulerAngles.z)  as TurretSocketInfo;
                    //socketInfo = addDefaultSocket("Mixed/SmallTurretOrLauncher", "StandardWeaponTurret" + i, new Vector2(trans.position.x, trans.position.y), trans.rotation.eulerAngles.z);

                    turretSocketInfo.parent = trans;

                    if (turretSocketInfo != null)
                    {
                        turretSocketInfo.fixedFiringArc = true;
                        turretSocketInfo.fireArcHalf = 120;
                    }

                    trans.gameObject.layer = gameObject.layer;
                }
            }

            i = 0;

            foreach (Transform trans in heavyWeaponPlatforms.GetComponentsInChildren<Transform>())
            {
                if (trans != heavyWeaponPlatforms)
                {
                    i++;
                    turretSocketInfo = addDefaultSocket("WeaponBays/LargeTurret", "HeavyWeaponTurret" + i, new Vector2(trans.position.x, trans.position.y), trans.rotation.eulerAngles.z) as TurretSocketInfo;
                    //socketInfo = addDefaultSocket("WeaponBays/LargeTurret", "HeavyWeaponTurret" + i, new Vector2(trans.position.x, trans.position.y), trans.rotation.eulerAngles.z);

                    turretSocketInfo.parent = trans;

                    if (turretSocketInfo != null)
                    {
                        turretSocketInfo.fixedFiringArc = true;
                        turretSocketInfo.fireArcHalf = 60;
                    }
                }
            }

            socketInfo = addDefaultSocket("WeaponBays/MegaTurret", "OrbitalWeaponTurret", new Vector2(0, -163f)) as TurretSocketInfo;

            if (turretSocketInfo != null)
            {
                turretSocketInfo.fireArcHalf = 180;
            }

            addDefaultSocket("WeaponBays/MediumLauncher", "LeftFrontLauncher", new Vector2(-146.6f, 303.2f));
            addDefaultSocket("WeaponBays/MediumLauncher", "RightFrontLauncher", new Vector2(146.6f, 303.2f));
            addDefaultSocket("WeaponBays/MediumLauncher", "LeftRearLauncher", new Vector2(-204.4f, -198.7f));
            addDefaultSocket("WeaponBays/MediumLauncher", "RightRearLauncher", new Vector2(204.4f, -198.7f));
            addDefaultSocket("WeaponBays/SmallLauncher", "LeftOuterLauncher", new Vector2(-402.6f, 212.0f));
            addDefaultSocket("WeaponBays/SmallLauncher", "RightOuterLauncher", new Vector2(402.6f, 212.0f));

            turretSocketInfo = addDefaultSocket("WeaponBays/MediumTurret", "LeftOuterFrontTurret", new Vector2(-243.8f, 146.9f), 45) as TurretSocketInfo;

            if (turretSocketInfo != null)
            {
                turretSocketInfo.fireArcHalf = 120;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/MediumTurret", "LeftInnerFrontTurret", new Vector2(-107.6f, 111.3f)) as TurretSocketInfo;

            if (turretSocketInfo != null)
            {
                turretSocketInfo.fireArcHalf = 80;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/MediumTurret", "RightInnerFrontTurret", new Vector2(107.6f, 111.3f)) as TurretSocketInfo;

            if (turretSocketInfo != null)
            {
                turretSocketInfo.fireArcHalf = 80;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/MediumTurret", "RightOuterFrontTurret", new Vector2(243.8f, 146.9f), -45) as TurretSocketInfo;

            if (turretSocketInfo != null)
            {
                turretSocketInfo.fireArcHalf = 120;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/MediumTurret", "LeftFrontTurret", new Vector2(-43.3f, 253.5f)) as TurretSocketInfo;

            if (turretSocketInfo != null)
            {
                turretSocketInfo.fireArcHalf = 180;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/MediumTurret", "RightFrontTurret", new Vector2(43.3f, 253.5f)) as TurretSocketInfo;

            if (turretSocketInfo != null)
            {
                turretSocketInfo.fireArcHalf = 180;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/SmallTurret", "OuterRingLeftFrontTurret", new Vector2(-228.2f, 451.0f)) as TurretSocketInfo;

            if (turretSocketInfo != null)
            {
                turretSocketInfo.fireArcHalf = 180;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/SmallTurret", "OuterRingRightFrontTurret", new Vector2(228.2f, 451.0f)) as TurretSocketInfo;

            if (turretSocketInfo != null)
            {
                turretSocketInfo.fireArcHalf = 180;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/SmallTurret", "OuterRingLeftTurret", new Vector2(-401.3f, -107.0f)) as TurretSocketInfo;

            if (turretSocketInfo != null)
            {
                turretSocketInfo.fireArcHalf = 180;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/SmallTurret", "OuterRingRightTurret", new Vector2(401.3f, -107.0f)) as TurretSocketInfo;

            if (turretSocketInfo != null)
            {
                turretSocketInfo.fireArcHalf = 180;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/SmallTurret", "OuterRingLeftAftTurret", new Vector2(-329.8f, -309.2f)) as TurretSocketInfo;

            if (turretSocketInfo != null)
            {
                turretSocketInfo.fireArcHalf = 180;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/SmallTurret", "OuterRingRightAftTurret", new Vector2(329.8f, -309.2f)) as TurretSocketInfo;

            if (turretSocketInfo != null)
            {
                turretSocketInfo.fireArcHalf = 180;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/SmallTurret", "OrbitalTurretProtectionLeft", new Vector2(-83.8f, -251.4f)) as TurretSocketInfo;

            if (turretSocketInfo != null)
            {
                turretSocketInfo.fireArcHalf = 180;
            }

            turretSocketInfo = addDefaultSocket("WeaponBays/SmallTurret", "OrbitalTurretProtectionRight", new Vector2(83.8f, -251.4f)) as TurretSocketInfo;

            if (turretSocketInfo != null)
            {
                turretSocketInfo.fireArcHalf = 180;
            }
        }
        */
        public override void Update()
        {
            base.Update();

            heavyWeaponPlatforms.Rotate(new Vector3(0, 0, heavyPlatformSpin * Time.deltaTime));
        }

        protected override void Structure_NotifyKilled(object sender, TargetDestroyedEventArgs args)
        {
            base.Structure_NotifyKilled(sender, args);

            List<IDockable> dockableSockets = getSockets<IDockable>();

            foreach (IDockable dockableSocket in dockableSockets)
            {
                List<Ship> dockedShips = dockableSocket.getDockedShips();

                foreach (Ship ship in dockedShips)
                {
                    ship.takeDamage(ship.gameObject, 0.1f * MaxHullStrength, null, null);

                    DockingPortEventArgs dpeArgs = new DockingPortEventArgs(null, this, ship);

                    ship.Call_DockInitiatorUndocked(this, dpeArgs);
                    Call_DockReceiverUndocked(this, dpeArgs);
                }
            }
        }

        protected override void Structure_DockReceiverDocked(object sender, DockingPortEventArgs args)
        {
            // D.log("Structure", args.ship.gameObject.name + " has docked with " + args.portStructure.gameObject.name);            

            args.ship.transform.parent = transform;

            Gamemode.Gui.setMessage(args.ship.name + " is being boarded");
        }

        protected override void Structure_DockReceiverUndocked(object sender, DockingPortEventArgs args)
        {
            // D.log("Structure", args.ship.gameObject.name + " has undocked from " + args.portStructure.gameObject.name);

            GameObject hierarchy = GameObject.Find("Placeables");

            args.ship.transform.parent = hierarchy.transform;

            if (args.ship.Destroyed == true)
            {
                Gamemode.Gui.setMessage(args.ship.name + " was destroyed whilst docked");
            }
        }

        public static IDockable StructureSocketToIDockable(StructureSocket socket)
        {
            return socket as IDockable;
        }
    }
}
