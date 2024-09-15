using UnityEngine;
using System.Collections;

using NoxCore.Effects;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Utilities;

namespace NoxCore.Fittings.Weapons
{
    public class SineBomb : SplineProjectile
    {
        public override void hasCollided(NoxObject collidedObject = null)
        {
            base.hasCollided(collidedObject);
        }
    }
}