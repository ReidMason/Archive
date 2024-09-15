using UnityEngine;

using NoxCore.Controllers;
using NoxCore.Data.Fittings;
using NoxCore.Fittings.Modules;

namespace NoxCore.Fittings.Weapons
{
	public interface IWeapon : IModule
	{
        WeaponData WeaponData { get; set; }

		float Ammo { get; set; } 

		FireGroup FireGroup { get; }
        bool isFiring();
        Transform getFirePoint();
        float getDamage();
		float getDPS(GameObject go);
		bool fire();
	}
}