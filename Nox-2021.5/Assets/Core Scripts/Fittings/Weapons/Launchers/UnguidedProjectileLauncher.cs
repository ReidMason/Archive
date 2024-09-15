using UnityEngine;

using System;

using NoxCore.Data.Fittings;

namespace NoxCore.Fittings.Weapons
{
    public class UnguidedProjectileLauncher : Weapon, IUnguidedLauncher
    {
        [Header("Unguided Launcher")]

        public UnguidedLauncherData __unguidedLauncherData;
        [NonSerialized]
        protected UnguidedLauncherData _unguidedLauncherData;
        public UnguidedLauncherData UnguidedLauncherData { get { return _unguidedLauncherData; } set { _unguidedLauncherData = value; } }

        protected Transform firePoint;

        protected bool allowFiring;
        public bool AllowFiring { get { return allowFiring; } set { allowFiring = value; } }

        public override void init(DeviceData deviceData = null)
        {
            if (deviceData == null)
            {
                UnguidedLauncherData = Instantiate(__unguidedLauncherData);
                base.init(UnguidedLauncherData);
            }
            else
            {
                UnguidedLauncherData = deviceData as UnguidedLauncherData;
                base.init(deviceData);
            }

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
            GameObject projectile = UnguidedLauncherData.EffectPrefab.Spawn(transform.position);

            projectile.GetComponent<Projectile>().fire(this);
        }

        protected override void fired()
        {
            base.fired();

            if (UnguidedLauncherData.EffectPrefab != null)
            {
                launch();
            }
        }

        public override void update()
        {
            base.update();

            if (isActiveOn() == true && isFlippingActivation() == false)
            {
                if (AllowFiring == true)
                {
                    fire();
                }
            }
        }
    }
}