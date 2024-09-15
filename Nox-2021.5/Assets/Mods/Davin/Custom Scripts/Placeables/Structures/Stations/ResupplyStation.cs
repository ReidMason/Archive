using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NoxCore.Data.Placeables;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Fittings.Weapons;
using NoxCore.Placeables.Ships;

namespace NoxCore.Placeables
{
    public class ResupplyStation : Station
    {
        public float minResupplyTime, maxResupplyTime;

        public override void init(NoxObjectData noxObjectData = null)
        {
            // change any default values in the parent class here

            if (noxObjectData != null)
            {
                base.init(Instantiate(noxObjectData));
            }
            else
            {
                base.init();
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
                        #region timer update

                        resupplyTimer -= Time.deltaTime;
                        yield return new WaitForEndOfFrame();

                        #endregion
                    }
                    else
                    {
                        #region resupply all the things!

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

                        ship.recalculateShields();

                        ship.recalculateHealthBar();

                        #endregion

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
        }

        protected override void Structure_DockReceiverDocked(object sender, DockingPortEventArgs args)
        {
            // D.log("Structure", args.ship.gameObject.name + " has docked with " + args.portStructure.gameObject.name);            

            base.Structure_DockReceiverDocked(sender, args);

            float delay = (args.ship.StructureData.Mass / 500.0f) + 2;

            Gamemode.Gui.setMessage(args.ship.name + " is being resupplied");

            StartCoroutine(resupplyAtPort(args, delay));
        }

        protected override void Structure_DockReceiverUndocked(object sender, DockingPortEventArgs args)
        {
            // D.log("Structure", args.ship.gameObject.name + " has undocked from " + args.portStructure.gameObject.name);

            base.Structure_DockReceiverUndocked(sender, args);

            StopCoroutine(resupplyAtPort(args, 0));

            if (args.ship.Destroyed == true)
            {
                Gamemode.Gui.setMessage(args.ship.name + " was destroyed whilst being resupplied");
            }
        }
    }
}