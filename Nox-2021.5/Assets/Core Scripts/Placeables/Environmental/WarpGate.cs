using UnityEngine;

using NoxCore.GameModes;
using NoxCore.Placeables.Ships;

namespace NoxCore.Placeables
{
    public class WarpGate : MonoBehaviour
    {
        public delegate void WarpEventDispatcher(object sender, WarpEventArgs args);
        public event WarpEventDispatcher WarpGateActivated;

        public SceneReference warpToScene;
        public Vector2 warpToPosition;
        public bool useWarpRotation;
        public float warpRotation;
        public float delay;

        protected bool _active;
        public bool active
        {
            get
            {
                return _active;
            }
            set
            {
                _active = value;

                if (_active == true && pSystem != null) pSystem.Play();
                else if (pSystem != null) pSystem.Stop();
            }
        }

        protected ParticleSystem pSystem;

        void Awake()
        {
            pSystem = GetComponent<ParticleSystem>();
        }

        void Start()
        {
            pSystem.Stop();
        }

        void OnEnable()
        {
            WarpGateActivated += OnWarp;
        }

        void OnDisable()
        {
            WarpGateActivated -= OnWarp;
        }

        public void Call_Warp(object sender, WarpEventArgs args)
        {
            if (WarpGateActivated != null)
            {
                WarpGateActivated(sender, args);
            }

            GameMode.Call_WarpGateActivated(sender, args);
        }

        public virtual void OnWarp(object sender, WarpEventArgs args)
        {
            Ship warpingShip = args.warpShipGO.GetComponent<Ship>();

            warpingShip.StructureRigidbody.velocity = Vector2.zero;
            warpingShip.Call_WarpOut(sender, args);
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (active == false) return;

            if (other.tag == "Ship")
            {
                //Debug.Log("Triggered warp gate");

                if (useWarpRotation == true)
                {
                    Call_Warp(this, new WarpEventArgs(other.gameObject, warpToScene, gameObject.scene.path, warpToPosition, warpRotation, delay));
                }
                else
                {
                    Call_Warp(this, new WarpEventArgs(other.gameObject, warpToScene, gameObject.scene.path, warpToPosition, other.transform.rotation.eulerAngles.z, delay));
                }                
            }
        }
    }
}
