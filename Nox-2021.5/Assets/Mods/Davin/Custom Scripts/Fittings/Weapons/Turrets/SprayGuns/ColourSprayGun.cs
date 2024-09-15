using System;

using UnityEngine;

using NoxCore.Data.Fittings;
using NoxCore.Fittings.Weapons;

using Davin.Data.Fittings;

namespace Davin.Fittings.Weapons
{
	public class ColourSprayGun : RotatingTurret, IColourSprayGun
	{
		[Header("Colour Spray Gun")]

		public ColourSprayGunData __colourSprayGunData;
		[NonSerialized]
		protected ColourSprayGunData _colourSprayGunData;
		public ColourSprayGunData ColourSprayGunData { get { return _colourSprayGunData; } set { _colourSprayGunData = value; } }

		public override void init(DeviceData deviceData = null)
		{
			if (deviceData == null)
			{
				ColourSprayGunData = Instantiate(__colourSprayGunData);
				base.init(ColourSprayGunData);
			}
			else
			{
				ColourSprayGunData = deviceData as ColourSprayGunData;
				base.init(deviceData);
			}

			Target = null;

			requiredSocketTypes.Add("SPRAY GUN");

			Projectile projectile = ColourSprayGunData.EffectPrefab.GetComponent<Projectile>();

			if (projectile != null)
			{
				projectile.init();
			}
		}

		public override void reset()
		{
			base.reset();

			Target = null;
		}


		public override void destroy()
		{
			base.destroy();
		}

		public override void explode(int repeatedNumExplosions = 0)
		{
			base.explode(repeatedNumExplosions);
		}

		protected override bool canFire()
		{
			if (base.canFire() == true)
			{
				// include the following line if the weapon cannot fire whilst the host ship/structure is cloaked
				if (structure.gameObject.layer == LayerMask.NameToLayer("Cloaked")) return false;

				return true;
			}

			return false;
		}

		public void launch()
		{
			GameObject projectile = ColourSprayGunData.EffectPrefab.Spawn(transform.position);

			projectile.GetComponent<Projectile>().fire(this);
		}

		protected override void fired()
		{
			base.fired();

			if (ColourSprayGunData.EffectPrefab != null)
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
				else if (LockedTarget != null && TargetIsLocked == true)
				{
					fire();
				}
			}
		}
	}
}
