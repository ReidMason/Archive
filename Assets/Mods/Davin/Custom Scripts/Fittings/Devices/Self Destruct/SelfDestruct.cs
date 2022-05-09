using System;
using System.Collections;

using UnityEngine;

using NoxCore.Data.Fittings;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Weapons;

using Davin.Data.Fittings;

namespace Davin.Fittings.Devices
{
    public class SelfDestruct : Device, ISelfDestruct
    {
        public SelfDestructData __selfDestructData;
        [NonSerialized] protected SelfDestructData _selfDestructData;
        public SelfDestructData SelfDestructData {  get { return _selfDestructData; } set { _selfDestructData = value; } }

        protected Explosion explosion;

        public override void init(DeviceData deviceData = null)
        {
            if (deviceData == null)
            {
                SelfDestructData = Instantiate(__selfDestructData);
                base.init(SelfDestructData);
            }
            else
            {
                SelfDestructData = deviceData as SelfDestructData;
                base.init(deviceData);
            }

            if (SelfDestructData.Explosion != null)
            {
                explosion = SelfDestructData.Explosion.GetComponent<Explosion>();
                explosion.init();
            }

            SelfDestructData.ActiveOn = false;
            SelfDestructData.ActiveOnSpawn = false;
        }

        public override void reset()
        {
            base.reset();

            SelfDestructData.ActiveOn = false;
            SelfDestructData.ActiveOnSpawn = false;
        }

        public bool countdownActive()
        {
            return isFlippingActive;
        }

        public void activateSelfDestruct()
        {
            if (SelfDestructData.ActiveOn == false && isFlippingActivation() == false)
            {
                activate();
            }
        }

        public void deactivateSelfDestruct()
        {
            if (isFlippingActivation() == true)
            {
                deactivate();
            }
        }

        public override void update()
        {
            base.update();

            if (SelfDestructData.ActiveOn == true)
            {
                float explosionDuration = explosion.detonate(structure.StructureCollider);

                // destroy own structure
                structure.takeDamage(structure.gameObject, Mathf.Infinity, null, null);

                // cause radial damage
                explosion.radialExplosion(SelfDestructData.RadialDamage, null, transform.position);

                // do I need this?
                deactivate();
            }
            else if (Input.GetKeyDown(SelfDestructData.DebugKey) && isFlippingActivation() == false)
            {
                activateSelfDestruct();
            }
        }
    }
}