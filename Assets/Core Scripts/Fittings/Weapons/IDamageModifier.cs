using UnityEngine;

using NoxCore.Placeables;

namespace NoxCore.Fittings.Weapons
{
	public interface IDamageModifier
	{
		float damageModifier(GameObject collidedObject, float damage, Weapon weapon, (GameObject structure, GameObject system)? target, Projectile projectile = null);
	}
}