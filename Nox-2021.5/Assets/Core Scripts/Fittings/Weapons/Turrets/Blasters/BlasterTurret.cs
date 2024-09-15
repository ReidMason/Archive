using UnityEngine;

using System;

using NoxCore.Data.Fittings;
using NoxCore.Placeables;

namespace NoxCore.Fittings.Weapons
{
	public class BlasterTurret : RotatingTurret, IBlasterTurret, ILauncher
	{
        [Header("Blaster Turret")]

        public BlasterTurretData __blasterTurretData;
        [NonSerialized]
        protected BlasterTurretData _blasterTurretData;
        public BlasterTurretData BlasterTurretData { get { return _blasterTurretData; } set { _blasterTurretData = value; } }

        public override void init(DeviceData deviceData = null)
        {
            if (deviceData == null)
            {
                BlasterTurretData = Instantiate(__blasterTurretData);
                base.init(BlasterTurretData);
            }
            else
            {
                BlasterTurretData = deviceData as BlasterTurretData;
                base.init(deviceData);
            }

            requiredSocketTypes.Add("BLASTER");

            Projectile projectile = BlasterTurretData.EffectPrefab.GetComponent<Projectile>();

            if (projectile != null)
            {
                projectile.init();
            }
        }

        public override float damageModifier(GameObject collidedObject, float damage, Weapon weapon, (GameObject structure, GameObject system)? target, Projectile projectile = null)
        {
            float weaponModifier = 1;
            damage = base.damageModifier(collidedObject, damage, weapon, target, projectile);

            Structure hitStructure = target.GetValueOrDefault().structure.GetComponent<Structure>();

            if (hitStructure == null)
            {
                hitStructure = collidedObject.GetComponent<Structure>();
            }

            if (hitStructure.AllShieldsFailed == false)
            {
                weaponModifier = BlasterTurretData.ShieldDamageModifier;
            }
            else
            {
                weaponModifier = BlasterTurretData.HullDamageModifier;
            }

            return damage * weaponModifier;
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
            GameObject projectile = BlasterTurretData.EffectPrefab.Spawn(transform.position);

            projectile.GetComponent<Projectile>().fire(this);
        }

        protected override void fired()
        {
            base.fired();

            if (BlasterTurretData.EffectPrefab != null)
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