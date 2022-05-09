using UnityEngine;

using System;

using NoxCore.Data.Fittings;

namespace NoxCore.Fittings.Weapons
{
    public class GuidedProjectileLauncher : ProjectileLauncher, IGuidedLauncher
    {
        [Header("Guided Launcher")]
        
        public GuidedLauncherData __guidedLauncherData;
        [NonSerialized]
        protected GuidedLauncherData _guidedLauncherData;
        public GuidedLauncherData GuidedLauncherData { get { return _guidedLauncherData; } set { _guidedLauncherData = value; } }

        public override void init(DeviceData deviceData = null)
        {
            if (deviceData == null)
            {
                GuidedLauncherData = Instantiate(__guidedLauncherData);
                base.init(GuidedLauncherData);
            }
            else
            {
                GuidedLauncherData = deviceData as GuidedLauncherData;
                base.init(deviceData);
            }

            LockTimer = 0;
        }

        public override void reset()
        {
            base.reset();

            LockTimer = 0;
        }

        public override void update()
        {
            base.update();

            if (isActiveOn() == true && isFlippingActivation() == false)
            {
                if (TargetIsAcquired == true && TargetIsLocked == false)
                {
                    LockTimer += Time.deltaTime;

                    if (LockTimer >= GuidedLauncherData.LockTime)
                    {
                        lockTarget();
                    }
                }

                if (LockedTarget != null && TargetIsLocked == true)
                {
                    fire();
                }
            }
        }
    }
}