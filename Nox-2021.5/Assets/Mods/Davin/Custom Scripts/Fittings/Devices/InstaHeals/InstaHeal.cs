using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NoxCore.Data.Fittings;
using NoxCore.Fittings.Devices;

namespace Davin.Fittings.Devices
{
    public class InstaHeal : Device, IInstaHeal
    {
        [SerializeField]
        [Range(0,100)]
        protected float amount;
        public float Amount { get { return amount; } set { amount = Mathf.Clamp(value, 0, 100); } }

        protected bool used;

        public override void init(DeviceData deviceData = null)
        {
            base.init();

            reset();
        }

        public override void reset()
        {
            base.reset();

            // initialise data here
            used = false;
        }

        public void heal()
        {
            if (isActiveOn() == true && isFlippingActivation() == false && used == false)
            {
                float healAmount = structure.MaxHullStrength / amount;

                structure.HullStrength += healAmount;

                structure.HullStrength = Mathf.Clamp(structure.HullStrength, 0, structure.MaxHullStrength);

                used = true;
            }
        }
    }
}