using UnityEngine;

using System;

using NoxCore.Data.Fittings;
using NoxCore.Utilities;

namespace NoxCore.Fittings.Weapons
{
	public abstract class ProjectileLauncher : TargetableWeapon, ILauncher
	{
		[NonSerialized]
		protected ProjectileLauncherData _projectileLauncherData;
		public ProjectileLauncherData ProjectileLauncherData { get { return _projectileLauncherData; } set { _projectileLauncherData = value; } }

		protected Transform firePoint;

		[ShowOnly]
		protected float _LockTimer;
		public float LockTimer { get { return _LockTimer; } set { _LockTimer = value; } }

		public override void init(DeviceData deviceData = null)
		{
			ProjectileLauncherData = deviceData as ProjectileLauncherData;

			base.init(deviceData);

			requiredSocketTypes.Add("LAUNCHER");

            firePoint = transform.Find("Muzzle");
            if (firePoint == null) firePoint = transform;
        }

        public override Transform getFirePoint()
        {
            return firePoint;
        }
		
		protected override bool canFire()
		{
			if (base.canFire() == true)
			{
				if (structure.gameObject.layer == LayerMask.NameToLayer("Cloaked")) return false;
				
				return true;
			}
			
			return false;			
		}

        public void launch()
        {
            GameObject projectile = ProjectileLauncherData.EffectPrefab.Spawn(transform.position);

            projectile.GetComponent<Projectile>().fire(this);
        }

        protected override void fired()
		{
			base.fired();
		
			if (ProjectileLauncherData.EffectPrefab != null)
			{
                launch();
            }
		}

        public override void update()
		{
			base.update();
			
			if (isActiveOn() == true && isFlippingActivation() == false)
			{		
				if (firing)
				{
					if (FireTimer >= WeaponData.FireRate)
					{
						firing = false;
						FireTimer = 0;
					}
					else
					{
						FireTimer += Time.deltaTime;
					}
				}
			}
		}	
	}
}