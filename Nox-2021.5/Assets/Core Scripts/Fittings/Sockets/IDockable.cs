using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NoxCore.Placeables;
using NoxCore.Placeables.Ships;

namespace NoxCore.Fittings.Sockets
{
    public interface IDockable : ISocket
    {
        StructureSize MinDockingSize { get; set; }
        StructureSize MaxDockingSize { get; set; }

        DockState DockState { get; }
        List<Ship> getDockedShips();
        void releaseDockingClamp();
        (bool requestGranted, string message) requestDocking(Ship ship);
        bool requestUndocking(Ship ship);
        bool isShipDocking(Ship ship);
        bool isShipUndocking(Ship ship);
        bool isShipDocked(Ship ship);
    }
}