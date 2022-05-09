using UnityEngine;

using System;

using NoxCore.Data.Fittings;
using NoxCore.Effects;
using NoxCore.Managers;
using NoxCore.Placeables;

namespace NoxCore.Fittings.Weapons
{
	public class LaserTurret : RotatingTurret, ILaserTurret, IVisibleEffect
	{
        [Header("Laser Turret")]

		public LaserTurretData __laserTurretData;
		[NonSerialized]
		protected LaserTurretData _laserTurretData;
		public LaserTurretData LaserTurretData { get { return _laserTurretData; } set { _laserTurretData = value; } }

		protected bool effectVisible;

		public override void init(DeviceData deviceData = null)
		{
			if (deviceData == null)
			{
				LaserTurretData = Instantiate(__laserTurretData);
				base.init(LaserTurretData);
			}
			else
			{
				LaserTurretData = deviceData as LaserTurretData;
				base.init(deviceData);
			}

			Target = null;

            requiredSocketTypes.Add("LASER");

            Projectile projectile = LaserTurretData.EffectPrefab.GetComponent<Projectile>();

            if (projectile != null)
            {
                projectile.init();
            }
        }	

		public override void reset()
		{
			base.reset();

            Target = null;

            foreach (IVisualEffect vfx in vfxs)
            {
                vfx.stopVFX();
            }

			effectVisible = false;
		}

		public bool isEffectVisible()
		{
			return effectVisible;
		}

		public override void destroy()
		{
            foreach (IVisualEffect vfx in vfxs)
            {
                vfx.stopVFX();
            }
			
			base.destroy();			
		}
		
		public override void explode(int repeatedNumExplosions = 0)
		{
            foreach (IVisualEffect vfx in vfxs)
            {
                vfx.stopVFX();
            }
			  
			base.explode(repeatedNumExplosions);
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

		protected override void fired()
		{
            base.fired();

            float damage = getDamage();

            (GameObject structure, GameObject system) lockedTarget = LockedTarget.GetValueOrDefault();

            lockedTarget.structure.GetComponent<Structure>().takeDamage(lockedTarget.structure, damage, this, LockedTarget);

			effectVisible = true;
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
						effectVisible = false;

						foreach (IVisualEffect vfx in vfxs)
                        {
                            if (vfx.getIsRunning() == true)
                            {
                                vfx.stopVFX();
                            }
                        }

						FireTimer = 0;
					}
					else
					{
						FireTimer += Time.deltaTime;
						
						if (FireTimer > LaserTurretData.EffectDuration)
						{
                            foreach (IVisualEffect vfx in vfxs)
                            {
                                if (vfx.getIsRunning() == true)
                                {
                                    vfx.stopVFX();
                                }
                            }

							effectVisible = false;
						}
					}
				}
				else if(LockedTarget != null && TargetIsLocked == true)
				{
					fire();
				}
            }
			else
			{
                foreach (IVisualEffect vfx in vfxs)
                {
                    if (vfx.getIsRunning() == true)
                    {
                        vfx.stopVFX();
                    }
                }

				effectVisible = false;
			}
		}
	}
}