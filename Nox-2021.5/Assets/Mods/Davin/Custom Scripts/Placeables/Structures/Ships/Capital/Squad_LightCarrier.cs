using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using NoxCore.Controllers;
using NoxCore.Data.Placeables;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Fittings.Weapons;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;

namespace Davin.Placeables.Ships
{
    public class Squad_LightCarrier : LightCarrier
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            // change any default values in the parent class here
            //TotalCost = 6000;

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

            StructureSocketInfo socketInfo = addDefaultSocket("DockingPorts/LargeDockingPort", "LeftPort", new Vector2(-59.5f, 17.6f));

            DockingSocketInfo dockingSocketInfo = socketInfo as DockingSocketInfo;
            if (dockingSocketInfo != null)
            {
                dockingSocketInfo.minDockingSize = StructureSize.SMALL;
                dockingSocketInfo.maxDockingSize = StructureSize.LARGE;
            }

            socketInfo = addDefaultSocket("DockingPorts/LargeDockingPort", "RightPort", new Vector2(59.5f, 17.6f));

            addDefaultSocket("Hangars/MediumHangar", "Hangar", new Vector2(0f, -60f));

            addDefaultSocket("EngineBays/MassiveEngineBay", "LeftEngine", new Vector2(-32.5f, -228.53f));
            addDefaultSocket("EngineBays/MassiveEngineBay", "RightEngine", new Vector2(-32.5f, -228.53f));

            addDefaultSocket("ShieldPods/MassiveShieldPod", "ShieldPod", new Vector2(0, 31.0f));

            addDefaultSocket("WeaponBays/MediumLauncher", "Launcher", new Vector2(0f, 94f));

            TurretSocketInfo turretSocketInfo;

            turretSocketInfo = addDefaultSocket("WeaponBays/MassiveTurret", "Turret", new Vector2(0f, 0f)) as TurretSocketInfo;
            // change any default values in the socket class here    
            if (turretSocketInfo != null)
            {
                turretSocketInfo.fixedFiringArc = true;
                turretSocketInfo.fireArcHalf = 180;
            }
        }
        */
        public override void respawn()
        {
            base.respawn();

            GameObject hierarchy = GameObject.Find("Placeables");

            List<IHangar> hangars = getSockets<IHangar>();

            foreach (IHangar hangar in hangars)
            {
                List<Ship> shipsInHangar = hangar.getShipsInHangar();
                Transform hangarTrans = hangar.getTransform();

                foreach (Ship ship in shipsInHangar)
                {
                    ship.transform.parent = hierarchy.transform;

                    ship.respawn();

                    ship.transform.position = hangarTrans.position;
                    ship.transform.rotation = hangarTrans.rotation;

                    ship.enableAllRenderers();
                    ship.enableAllColliders();
                }

                shipsInHangar.Clear();
            }
        }

        protected IEnumerator resupplyAtPort(DockingPortEventArgs args, float delay)
        {
            Ship ship = args.ship;
            float resupplyTimer = delay;

            while (true)
            {
                if (ship.Destroyed == false && args.port.DockState == DockState.DOCKED)
                {
                    if (resupplyTimer > 0)
                    {
                        resupplyTimer -= Time.deltaTime;
                        yield return new WaitForEndOfFrame();
                    }
                    else
                    {
                        ship.HullStrength = ship.MaxHullStrength;

                        foreach (Device device in ship.Devices)
                        {
                            if (device != null)
                            {
                                device.reset();
                            }
                        }

                        foreach (StructureSocket socket in ship.StructureSockets)
                        {
                            if (socket != null)
                            {
                                socket.reset();
                            }
                        }

                        foreach (Module module in ship.Modules)
                        {
                            if (module != null)
                            {
                                module.reset();
                            }
                        }

                        foreach (Weapon weapon in ship.Weapons)
                        {
                            if (weapon != null)
                            {
                                weapon.reset();
                            }
                        }

                        Gamemode.Gui.setMessage(args.ship.name + " has been resupplied");

                        args.port.releaseDockingClamp();
                        yield break;
                    }
                }
                else
                {
                    Call_DockReceiverUndocked(this, args);
                    yield break;
                }
            }
        }

        protected override void Structure_NotifyKilled(object sender, TargetDestroyedEventArgs args)
        {
            base.Structure_NotifyKilled(sender, args);

            List<IDockable> dockableSockets = getSockets<IDockable>();

            foreach (IDockable dockableSocket in dockableSockets)
            {
                List<Ship> dockedShips = dockableSocket.getDockedShips();

                for (int i = dockedShips.Count - 1; i >= 0; i--)
                {
                    StopCoroutine(resupplyAtPort(null, 0));

                    dockableSocket.requestUndocking(dockedShips[i]);

                    // docked ship takes proportional damage due to proximity based on relative size difference and max hull strength of structure
                    float damageProportion = structureSizeLimits[(int)(dockedShips[i].structureSize) + 1] / 10000.0f;

                    dockedShips[i].takeDamage(dockedShips[i].gameObject, damageProportion * MaxHullStrength, null, null);

                    DockingPortEventArgs dockingPortEventArgs = new DockingPortEventArgs(null, this, dockedShips[i]);

                    dockedShips[i].Call_DockInitiatorUndocked(this, dockingPortEventArgs);
                    Call_DockReceiverUndocked(this, dockingPortEventArgs);
                }
            }

            List<IHangar> hangars = getSockets<IHangar>();

            foreach (IHangar hangar in hangars)
            {
                List<Ship> shipsInHangar = hangar.getShipsInHangar();

                for (int j = shipsInHangar.Count - 1; j >= 0; j--)
                {
                    Ship hangarShip = shipsInHangar[j];

                    HangarEventArgs hangarEventArgs = new HangarEventArgs(hangar, this, hangarShip);

                    StopCoroutine(resupplyAtHangar(hangarEventArgs, 0));

                    hangar.emergencyLaunch(hangarShip);

                    // ship in hangar takes random proportional damage due to proximity based on relative size difference and max hull strength of structure
                    int lowestSize = (int)hangarShip.structureSize;
                    int highestSize = (int)(hangarShip.structureSize) + 1;
                    float randomSizeLimit = UnityEngine.Random.Range((float)structureSizeLimits[lowestSize], (float)structureSizeLimits[highestSize]);
                    float damageProportion = randomSizeLimit / 10000.0f;

                    hangarShip.takeDamage(hangarShip.gameObject, damageProportion * MaxHullStrength, null, null);
                }
            }
        }

        protected override void Structure_DockReceiverDocked(object sender, DockingPortEventArgs args)
        {
            // D.log("Structure", args.ship.gameObject.name + " has docked with " + args.portStructure.gameObject.name);            

            args.ship.transform.parent = transform;

            float delay = (args.ship.StructureData.Mass / 500.0f) + 2;

            Gamemode.Gui.setMessage(args.ship.name + " is being resupplied");

            StartCoroutine(resupplyAtPort(args, delay));
        }

        protected override void Structure_DockReceiverUndocked(object sender, DockingPortEventArgs args)
        {
            // D.log("Structure", args.ship.gameObject.name + " has undocked from " + args.portStructure.gameObject.name);

            StopCoroutine(resupplyAtPort(args, 0));

            GameObject hierarchy = GameObject.Find("Placeables");

            args.ship.transform.parent = hierarchy.transform;

            if (args.ship.Destroyed == true)
            {
                Gamemode.Gui.setMessage(args.ship.name + " was destroyed whilst being resupplied");
            }
        }

        public static IDockable StructureSocketToIDockable(StructureSocket socket)
        {
            return socket as IDockable;
        }

        protected IEnumerator resupplyAtHangar(HangarEventArgs args, float delay)
        {
            Ship ship = args.ship;
            float resupplyTimer = delay;

            while (true)
            {
                if (ship.Destroyed == false && ship.shipState == ShipState.LANDED)
                {
                    if (resupplyTimer > 0)
                    {
                        resupplyTimer -= Time.deltaTime;
                        yield return new WaitForEndOfFrame();
                    }
                    else
                    {
                        ship.HullStrength = ship.MaxHullStrength;

                        foreach (Device device in ship.Devices)
                        {
                            if (device != null)
                            {
                                device.reset();
                            }
                        }

                        foreach (StructureSocket socket in ship.StructureSockets)
                        {
                            if (socket != null)
                            {
                                socket.reset();
                            }
                        }

                        foreach (Module module in ship.Modules)
                        {
                            if (module != null)
                            {
                                module.reset();
                            }
                        }

                        foreach (Weapon weapon in ship.Weapons)
                        {
                            if (weapon != null)
                            {
                                weapon.reset();
                            }
                        }

                        Gamemode.Gui.setMessage(args.ship.name + " has been resupplied");

                        args.hangar.requestLaunch(args.ship);

                        yield break;
                    }
                }
                else
                {
                    Call_LaunchReceiverLaunched(this, args);
                    yield break;
                }
            }
        }

        public static IHangar StructureSocketToIHangar(StructureSocket socket)
        {
            return socket as IHangar;
        }

        protected override void Structure_LandingReceiverLanded(object sender, HangarEventArgs args)
        {
            // D.log("Structure", args.ship.gameObject.name + " has docked with " + args.portStructure.gameObject.name);            

            args.ship.transform.parent = transform;

            float delay = (args.ship.StructureData.Mass / 500.0f) + 2;

            Gamemode.Gui.setMessage(args.ship.name + " is being resupplied");

            StartCoroutine(resupplyAtHangar(args, delay));
        }

        protected override void Structure_LaunchReceiverLaunched(object sender, HangarEventArgs args)
        {
            // D.log("Structure", args.ship.gameObject.name + " has undocked from " + args.portStructure.gameObject.name);
            StopCoroutine(resupplyAtHangar(args, 0));

            GameObject hierarchy = GameObject.Find("Placeables");

            args.ship.transform.parent = hierarchy.transform;

            if (args.ship.Destroyed == true)
            {
                Gamemode.Gui.setMessage(args.ship.name + " was destroyed whilst being resupplied");
            }
        }
    }
}
