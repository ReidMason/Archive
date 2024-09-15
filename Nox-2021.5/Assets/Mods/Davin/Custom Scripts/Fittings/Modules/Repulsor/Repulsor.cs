using UnityEngine;
using System.Collections;

using NoxCore.Debugs;
using NoxCore.GameModes;
using NoxCore.Fittings.Modules;
using NoxCore.Managers;
using NoxCore.Utilities;

using Davin.Debugs;
using NoxCore.Data.Fittings;

namespace Davin.Fittings.Modules
{
    public class Repulsor : Module, IRepulse, IRepulsorDebuggable
    {
        public GameObject effectRing;
        public float repulseRadius;
        public float repulsePower;
        protected int layerMask;
        public KeyCode debugKey;

        public override void init(DeviceData deviceData = null)
        {
            base.init();

            layerMask = 1 << LayerMask.NameToLayer("Ship") | 1 << LayerMask.NameToLayer("Structure") | 1 << LayerMask.NameToLayer("Cloaked");

            requiredSocketTypes.Add("REPULSOR");
        }

        public void repulse()
        {
            Collider2D [] colliders = Physics2D.OverlapCircleAll(transform.position, repulseRadius, layerMask);

            foreach (Collider2D collider in colliders)
            {
                if (structure.StructureCollider == collider) continue;

                collider.GetComponent<Rigidbody2D>().AddExplosionForce(repulsePower, transform.position, repulseRadius);
            }
        }

        public override void update()
        {
            base.update();

            if (Input.GetKeyDown(debugKey))
            {
                debugRepulse(this, new DebugEventArgs(false));
            }
        }

        public void debugRepulse(object sender, DebugEventArgs args)
        {
            repulse();

            GameManager.Instance.Gamemode.Gui.setMessage("DEBUG: " + DeviceData.Type + ":" + DeviceData.SubType + " has been triggered");
        }
    }
}