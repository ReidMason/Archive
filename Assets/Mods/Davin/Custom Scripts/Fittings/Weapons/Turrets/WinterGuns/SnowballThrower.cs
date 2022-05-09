using System;

using UnityEngine;

using NoxCore.Data.Fittings;
using NoxCore.Fittings.Weapons;

using Davin.Data.Fittings;

namespace Davin.Fittings.Weapons
{
	public class SnowballThrower : RotatingTurret, ISnowballThrower
	{
		[Header("Snowball Thrower")]

		public SnowballThrowerData __snowballThrowerData;
		[NonSerialized]
		protected SnowballThrowerData _snowballThrowerData;
		public SnowballThrowerData SnowballThrowerData { get { return _snowballThrowerData; } set { _snowballThrowerData = value; } }

		public Color m_ColourOfSnow;

		public override void init(DeviceData deviceData = null)
		{
			if (deviceData == null)
			{
				SnowballThrowerData = Instantiate(__snowballThrowerData);
				base.init(SnowballThrowerData);
			}
			else
			{
				SnowballThrowerData = deviceData as SnowballThrowerData;
				base.init(deviceData);
			}

			Target = null;

			requiredSocketTypes.Add("SPRAY GUN");

			Projectile projectile = SnowballThrowerData.EffectPrefab.GetComponent<Projectile>();

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
			GameObject projectile = SnowballThrowerData.EffectPrefab.Spawn(transform.position);

			Snowball snowball = projectile.GetComponent<Snowball>();
			if (snowball)
				snowball.__colour = m_ColourOfSnow;

			projectile.GetComponent<Projectile>().fire(this);
		}

		protected override void fired()
		{
			base.fired();

			if (SnowballThrowerData.EffectPrefab != null)
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
