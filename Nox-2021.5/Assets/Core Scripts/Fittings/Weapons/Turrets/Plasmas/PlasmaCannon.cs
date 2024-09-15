using UnityEngine;

using System;

using NoxCore.Data.Fittings;
using NoxCore.Effects;
using NoxCore.Placeables;

namespace NoxCore.Fittings.Weapons
{
    public class PlasmaCannon : RotatingTurret, IPlasmaCannon, IVisibleEffect
    {
        [Header("Plasma Cannon")]

        public PlasmaCannonData __plasmaCannonData;
        [NonSerialized]
        protected PlasmaCannonData _plasmaCannonData;
        public PlasmaCannonData PlasmaCannonData { get { return _plasmaCannonData; } set { _plasmaCannonData = value; } }

        protected bool effectVisible;

        public override void init(DeviceData deviceData = null)
        {
            if (deviceData == null)
            {
                PlasmaCannonData = Instantiate(__plasmaCannonData);
                base.init(PlasmaCannonData);
            }
            else
            {
                PlasmaCannonData = deviceData as PlasmaCannonData;
                base.init(deviceData);
            }

            Target = null;

            requiredSocketTypes.Add("PLASMA");

            Projectile projectile = PlasmaCannonData.EffectPrefab.GetComponent<Projectile>();

            if (projectile != null)
            {
                projectile.init();
            }
        }

        public override void reset()
        {
            base.reset();

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

            effectVisible = false;

            base.destroy();
        }

        public override void explode(int repeatedNumExplosions = 0)
        {
            foreach (IVisualEffect vfx in vfxs)
            {
                vfx.stopVFX();
            }

            effectVisible = false;

            base.explode(repeatedNumExplosions);
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
                weaponModifier = PlasmaCannonData.ShieldDamageModifier;
            }
            else
            {
                weaponModifier = PlasmaCannonData.HullDamageModifier;
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

                        if (FireTimer > PlasmaCannonData.EffectDuration)
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
                else if (LockedTarget != null && TargetIsLocked == true)
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