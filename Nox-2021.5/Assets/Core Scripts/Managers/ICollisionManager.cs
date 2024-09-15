using UnityEngine;

namespace NoxCore.Managers
{
    public interface  ICollisionManager
    {
        LayerMask? getCollisionMask(string collisionMaskName);
    }
}