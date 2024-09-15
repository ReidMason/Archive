using System;
using System.Collections.Generic;

using UnityEngine;

using NoxCore.Buffs;
using NoxCore.Data.Fittings;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Placeables.Ships;

using Davin.Buffs;
using Davin.Data.Fittings;

namespace Davin.Fittings.Devices
{
    [RequireComponent(typeof(Cooldown))]
    public class Afterburner : Device, IAfterburner
    {
        // custom device data goes here if needed
        public AfterburnerData __afterburnerData;
        [NonSerialized]
        protected AfterburnerData _afterburnerData;
        public AfterburnerData AfterburnerData { get { return _afterburnerData; } set { _afterburnerData = value; } }

        protected bool canEngage;
        public bool CanEngage { get { return canEngage; } set { canEngage = value; } }

        protected List<IEngine> engines;
        public List<IEngine> Engines { get { return engines; } set { engines = value; } }

        protected Ship ship;
        protected float subWarpMaxSpeed;

        protected Cooldown cooldown;
        public Cooldown Cooldown { get { return cooldown; } }

        public override void init(DeviceData deviceData = null)
        {
            if (deviceData == null)
            {
                AfterburnerData = Instantiate(__afterburnerData);
                base.init(AfterburnerData);
            }
            else
            {
                AfterburnerData = deviceData as AfterburnerData;
                base.init(deviceData);
            }

            ship = structure as Ship;

            cooldown = GetComponent<Cooldown>();
        }

        public override void reset()
        {
            base.reset();

            DeviceData.ActiveOn = false;
            DeviceData.ActiveOnSpawn = false;
        }

        public override void postFitting()
        {
            base.postFitting();

            if (ship != null)
            {
                Engines = ship.engines;
            }
        }

        /* custom device methods go here if needed	*/

        protected void disengage()
        {
            deactivate();
        }

        public void engage()
        {
            if (cooldown.enabled == false)
            {
                activate();

                subWarpMaxSpeed = 0;

                foreach (IEngine engine in Engines)
                {
                    subWarpMaxSpeed += engine.getMaxSpeed();
                }

                structure.StructureRigidbody.velocity = structure.StructureRigidbody.velocity.normalized * subWarpMaxSpeed;

                AfterburnerBuff afterburnerBuff = new AfterburnerBuff(Engines, AfterburnerData.CooldownBuff);

                // add the buff to the BuffManager and get a reference to the buff added (note: this could be a reference to the stacked buff, not the one we've just made)
                Buff buff = structure.BuffManager.addBuff(afterburnerBuff);

                // the first time the afterburnerBuff is added, set a listener for when it is removed by the BuffManager
                if (buff == afterburnerBuff)
                {
                    buff.removed.AddListener(disengage);
                }

                cooldown.begin.Invoke(AfterburnerData.CooldownBuff.Cooldown);
            }
        }

        public override void update()
        {
            base.update();

            if (Input.GetKeyDown(AfterburnerData.DebugKey))
            {                
                if (cooldown.enabled == false)
                {
                    engage();
                }
            }
        }
    }
}
