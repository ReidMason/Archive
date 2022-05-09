using UnityEngine;

using System;

using NoxCore.Data.Fittings;

namespace NoxCore.Fittings.Weapons
{
    public class OverflyProjectileLauncher : UnguidedProjectileLauncher
    {
        float proximity;

        public override void init(DeviceData deviceData = null)
        {
            base.init(deviceData);

            proximity = UnguidedLauncherData.EffectPrefab.GetComponent<GuidedProximityBombController>().proximity;
        }

        public override void update()
        {
            base.update();

            if (isActiveOn() == true && isFlippingActivation() == false)
            {
                if (FireGroup != null && FireGroup.Target.HasValue == true)
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
                            AllowFiring = false;
                        }
                    }
                    else
                    {
                        Vector2 targetPosition;

                        if (FireGroup.Target.Value.system != null)
                        {
                            targetPosition = FireGroup.Target.Value.system.transform.position;
                        }
                        else
                        {
                            targetPosition = FireGroup.Target.Value.structure.transform.position;
                        }

                        if (Vector2.Distance(transform.position, targetPosition) <= proximity)
                        {
                            AllowFiring = true;
                        }
                        else
                        {
                            AllowFiring = false;
                        }
                    }
                }
            }
        }
    }
}