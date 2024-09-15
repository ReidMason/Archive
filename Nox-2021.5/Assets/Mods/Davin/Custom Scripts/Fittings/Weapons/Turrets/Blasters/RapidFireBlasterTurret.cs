using System;

using UnityEngine;

using NoxCore.Data.Fittings;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Weapons;

using Davin.Data.Fittings;

namespace Davin.Fittings.Weapons
{
    [RequireComponent(typeof(Cooldown))]
    public class RapidFireBlasterTurret : BlasterTurret, IRapidFireBlasterTurret, ILauncher
    {
        [Header("Rapid Fire")]

        [NonSerialized]
        protected RapidFireBlasterTurretData _rapidFireBlasterTurretData;
        public RapidFireBlasterTurretData RapidFireBlasterTurretData { get { return _rapidFireBlasterTurretData; } set { _rapidFireBlasterTurretData = value; } }

        protected int numBoltsFired;
        protected float lastFireTime;
        protected Cooldown cooldown;

        public override void init(DeviceData deviceData = null)
        {
            if (deviceData == null)
            {
                RapidFireBlasterTurretData = __blasterTurretData as RapidFireBlasterTurretData;
                base.init(RapidFireBlasterTurretData);
            }
            else
            {
                RapidFireBlasterTurretData = deviceData as RapidFireBlasterTurretData;
                base.init(deviceData);
            }

            requiredSocketTypes.Add("RAPIDFIREBLASTER");

            cooldown = GetComponent<Cooldown>();

            cooldown.maxTime = RapidFireBlasterTurretData.Cooldown;
        }

        protected override void fired()
        {
            if (cooldown.enabled) return;

            base.fired();

            // if there's been a delay between rapidly fired bolts longer than the normal cooldown period then reset the bolt counter
            if (numBoltsFired > 0 && (Time.time - lastFireTime > RapidFireBlasterTurretData.Cooldown)) numBoltsFired = 0;

            numBoltsFired++;

            // if we've fired the number of bolts rapidly that we can, start the cooldown timer
            if (numBoltsFired == RapidFireBlasterTurretData.BoltsToFire)
            {
                cooldown.enabled = true;
            }

            // record the game time for when we fired last
            lastFireTime = Time.time;
        }
    }
}