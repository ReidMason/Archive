using UnityEngine;

using System.Collections;
using System;

using NoxCore.Buffs;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Helm;
using NoxCore.Placeables.Ships;

using Davin.Buffs;
using NoxCore.Data.Fittings;

namespace Davin.Fittings.Devices
{
    public enum WarpSequence { INACTIVE, ALIGNING, ALIGNED, WARPING };

    [RequireComponent(typeof(Cooldown))]
    public class MicroWarpDrive : Device, IWarpDrive
    {
        [Header("Warp Drive")]

        public WarpDriveData __warpDriveData;
        [NonSerialized]
        protected WarpDriveData _warpDriveData;
        public WarpDriveData WarpDriveData { get { return _warpDriveData; } set { _warpDriveData = value; } }

        protected float range;
        public float Range { get { return range; } set { range = value; } }

        protected float deactivationRange;
        public float DeactivationRange { get { return deactivationRange; } set { deactivationRange = value; } }

        protected Cooldown cooldown;
        public Cooldown Cooldown { get { return cooldown; } }

        protected WarpSequence warpSequence;

        protected Ship ship;
        protected HelmController Helm;

        protected MaxSpeedBuff maxSpeedBuff;

        protected float subWarpMaxSpeed;

        public override void init(DeviceData deviceData = null)
        {
            if (deviceData == null)
            {
                WarpDriveData = Instantiate(__warpDriveData);
                base.init(WarpDriveData);
            }
            else
            {
                WarpDriveData = deviceData as WarpDriveData;
                base.init(deviceData);
            }

            ship = structure as Ship;
            
            if (ship != null)
            {
                Helm = ship.Helm;
            }

            warpSequence = WarpSequence.INACTIVE;

            cooldown = GetComponent<Cooldown>();
        }

        public override void reset()
        {
            base.reset();

            if (warpSequence == WarpSequence.WARPING && maxSpeedBuff != null)
            {
                maxSpeedBuff.deactivate();
            }

            warpSequence = WarpSequence.INACTIVE;
        }

        public WarpSequence getWarpStatus()
        {
            return warpSequence;
        }

        public void engage()
        {
            if (cooldown.enabled == false)
            {
                activate();                
            }
        }

        public void disengage()
        {
            if (isActiveOn() == true && maxSpeedBuff != null)
            {
                maxSpeedBuff.deactivate();

                structure.StructureRigidbody.velocity = structure.StructureRigidbody.velocity.normalized * subWarpMaxSpeed;
            }
        }

        protected void warpBubbleCollapsed()
        {
            deactivate();

            maxSpeedBuff = null;
            structure.StructureData.Mass /= 0.01f;

            float maxSpeed = 0;

            foreach (StructureSocket structureSocket in structure.StructureSockets)
            {
                if (structureSocket != null && structureSocket.InstalledModule != null)
                {
                    IEngine engine = structureSocket.InstalledModule as IEngine;

                    if (engine != null)
                    {
                        if (engine.isActiveOn() == true && engine.isActiveOnSpawn() == true && engine.isDestroyed() == false)
                        {
                            maxSpeed += engine.getMaxSpeed();
                        }
                    }
                }
            }

            structure.StructureRigidbody.velocity = structure.StructureRigidbody.velocity.normalized * maxSpeed;

            ship.PrevVelocity = structure.StructureRigidbody.velocity;

            warpSequence = WarpSequence.INACTIVE;
        }

        protected void processWarpSequence()
        {
            switch(warpSequence)
            {
                case WarpSequence.INACTIVE:
                    StartCoroutine(align());
                    break;

                case WarpSequence.ALIGNED:
                    warp();
                    break;

                case WarpSequence.WARPING:
                    warping();
                    break;
            }
        }

        protected IEnumerator align()
        {
            float bearingToDestination;

            warpSequence = WarpSequence.ALIGNING;

            do
            {
                bearingToDestination = (Mathf.Atan2(-(Helm.Destination.y - Helm.Position.y), (Helm.Destination.x - Helm.Position.x)) * Mathf.Rad2Deg) + 90;

                if (bearingToDestination < 0) bearingToDestination += 360;

                if (Mathf.Abs(ship.Bearing - bearingToDestination) < WarpDriveData.AlignAccuracy) break;

                yield return new WaitForEndOfFrame();
            }
            while (warpSequence == WarpSequence.ALIGNING);

            warpSequence = WarpSequence.ALIGNED;

            cooldown.begin.Invoke(WarpDriveData.CooldownDuration);
        }

        protected void warp()
        {
            if (destroyed == false)
            {
                subWarpMaxSpeed = 0;

                foreach (IEngine engine in ship.engines)
                {
                    subWarpMaxSpeed += engine.getMaxSpeed();
                }

                maxSpeedBuff = new MaxSpeedBuff(ship.engines, BuffType.STANDARD, 1, WarpDriveData.WarpSpeedPercentage, true, WarpDriveData.MaxWarpBubbleTime);

                Buff buff = structure.BuffManager.addBuff(maxSpeedBuff);

                // the first time the MaxSpeedBuff is added, set a listener for when it is removed by the BuffManager
                if (buff == maxSpeedBuff)
                {
                    buff.removed.AddListener(warpBubbleCollapsed);
                }

                structure.StructureData.Mass *= 0.01f;

                warpSequence = WarpSequence.WARPING;
            }
        }

        protected void warping()
        {
            if (maxSpeedBuff == null) return;

            Range = Vector2.Distance(ship.transform.position, Helm.Destination);

            if (Range < DeactivationRange)
            {
                disengage();
            }
        }

        public override void update()
        {
            base.update();

            if (isActiveOn() == true && isFlippingActivation() == false)
            {
                processWarpSequence();
            }
        }
    }
}