using UnityEngine;
using System.Collections;

using NoxCore.Data.Fittings;
using NoxCore.Fittings.Devices;
using NoxCore.Effects;
using NoxCore.Placeables;

namespace NoxCore.Fittings.Weapons
{
    public class PowerSapper : RotatingTurret
    {
        public float effectDuration;
        public int sapAmount;

        protected bool effectVisible;
        public override void init(DeviceData deviceData = null)
        {
            base.init();

            Target = null;

            requiredSocketTypes.Add("POWERSAPPER");
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

            (GameObject structure, GameObject system) lockedTarget = LockedTarget.GetValueOrDefault();

            Structure structureHit = lockedTarget.structure.GetComponent<Structure>();

            if (structureHit.AllShieldsFailed == true)
            {
                IPowerGrid powerGrid = structureHit.getDevice<IPowerGrid>() as IPowerGrid;

                powerGrid.consumeFixedPower(sapAmount);
            }

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

                        effectVisible = false;

                        FireTimer = 0;
                    }
                    else
                    {
                        FireTimer += Time.deltaTime;

                        if (FireTimer > effectDuration)
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