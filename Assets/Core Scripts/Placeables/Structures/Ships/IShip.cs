using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NoxCore.Fittings.Sockets;
using NoxCore.Helm;

namespace NoxCore.Placeables.Ships
{
    public interface IShip : IStructure
    {
        float MaxForce { get; set; }
        float InertialRating { get; set; }
        void setSilentRunningFactor(float factor);
        void disengageSilentRunning();
        void engageSilentRunning();
        void attachHelmController(HelmController helm);
        void setSpawnInSpeed(float spawnInSpeedFraction);
        float angleTo(Vector2 v1, Vector2 v2);
        Vector2 correctVelocity(Vector2 currentVelocity, Vector2 requestedVelocity, float maxTurn);
        bool isTurningLeft(Vector2 currentVelocity, Vector2 newVelocity);        
    }
}