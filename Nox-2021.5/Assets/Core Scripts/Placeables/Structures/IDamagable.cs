using UnityEngine;

using NoxCore.Fittings.Weapons;

namespace NoxCore.Placeables
{
    public interface IDamagable
    {
        bool takeDamage(GameObject collidedObject, float damage, IWeapon weapon, (GameObject structure, GameObject system)? target, Projectile projectile = null);
    }
}