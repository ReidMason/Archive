using UnityEngine;

using NoxCore.Effects;
using NoxCore.Fittings.Weapons;
using NoxCore.Data.Fittings;

namespace Davin.Fittings.Weapons
{
    public class RotationGun : RotatingTurret, IVisibleEffect
    {
        protected bool effectVisible;
        public float effectDuration;
        public float angle;

        public override void init(DeviceData deviceData = null)
        {
            base.init();

            Target = null;

            requiredSocketTypes.Add("ROTATOR");
        }

        public override void reset()
        {
            base.reset();

            Target = null;

            effectVisible = false;
        }

        public override void destroy()
        {
            effectVisible = false;

            base.destroy();
        }

        public override void explode(int repeatedNumExplosions = 0)
        {
            effectVisible = false;

            base.explode(repeatedNumExplosions);
        }

        public bool isEffectVisible()
        {
            return effectVisible;
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

            effectVisible = true;

            (GameObject structure, GameObject system) lockedTarget = LockedTarget.GetValueOrDefault();

            Quaternion rotation = Quaternion.LookRotation(lockedTarget.structure.transform.forward);
            rotation *= Quaternion.Euler(0, 0, angle);

            lockedTarget.structure.transform.rotation = rotation;
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
                        FireTimer = 0;
                    }
                    else
                    {
                        FireTimer += Time.deltaTime;

                        if (FireTimer > effectDuration)
                        {
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
                effectVisible = false;
            }
        }
    }
}