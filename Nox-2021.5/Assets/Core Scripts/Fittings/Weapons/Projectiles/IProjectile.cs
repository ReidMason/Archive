using UnityEngine;
using System.Collections;

using NoxCore.Fittings.Modules;
using NoxCore.Placeables;

namespace NoxCore.Fittings.Weapons
{
    public interface IProjectile
    {
        void init();
        void hasCollided(NoxObject collidedObject = null);
    }
}