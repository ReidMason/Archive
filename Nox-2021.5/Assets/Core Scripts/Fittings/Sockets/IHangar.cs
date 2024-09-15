using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NoxCore.Placeables.Ships;
using NoxCore.Utilities;

namespace NoxCore.Fittings.Sockets
{
    public interface IHangar : ISocket
    {
        Transform getTransform();
        Vector2 getApproachVector();
        float getMaxLandingSpeed();
        void addShip(Ship ship);
        bool removeShip(Ship ship);
        List<Ship> getShipsInHangar();
        bool requestLaunch(Ship ship);
        bool emergencyLaunch(Ship ship);
    }
}