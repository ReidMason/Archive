using UnityEngine;
using System;

using NoxCore.Helm;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;

namespace NoxCore.Controllers
{
    public class MessageEventArgs : EventArgs
    {
        public Structure sender;

        public MessageEventArgs(Structure sender)
        {
            this.sender = sender;
        }
    }

    public abstract class StructureController : Controller
    {
        protected bool _booted { get; set; }
        public bool booted
        {
            get { return _booted; }
            set
            {
                _booted = value;

                if (_booted == true)
                {
                    D.log("Controller", _structure.gameObject.name + "'s controller is online");
                }
                else
                {
                    D.log("Controller", _structure.gameObject.name + "'s controller has gone offline");
                }
            }
        }

        public Vector2? startSpot;

        [Range(-360, 360)]
        public float? startRotation;
        public bool warpIn;

        // TODO - change this to work with an OnTriggerExit event
        public bool invulnerableToArenaBoundary;

        [SerializeField]
        protected bool _GeneratesStats;
        public bool GeneratesStats { get { return _GeneratesStats; } set { _GeneratesStats = value; } }

        [SerializeField]
        protected Structure _structure;
        public Structure structure { get { return _structure; } set { _structure = value; } }

        // reference to helm
        protected HelmController _Helm;
        public HelmController Helm { get { return _Helm; } set { _Helm = value; } }

        public virtual void boot(Structure structure, HelmController helm = null)
        {
            base.init();

            // subscribe to own events

            this.structure = structure;

            if (helm != null)
            {
                Helm = helm;
                // D.log("Controller", "AI linked to ship's helm");
            }
        }

        private void setBearing()
        {
            Ship ship = structure as Ship;

            if (ship != null)
            {
                Vector2 heading = new Vector2(ship.transform.forward.x, ship.transform.forward.y);

                float bearing = 0;

                if (heading.x != 0 && heading.y != 0) bearing = (Mathf.Atan2(-heading.y, heading.x) * Mathf.Rad2Deg) + 90;

                if (bearing < 0) bearing += 360;

                ship.Bearing = bearing;
            }
        }

        public virtual void setInitialLocationAndRotation(Vector2 location, float rotation)
        {
            startSpot = location;

            D.log("Controller", structure.name + " initial location set to " + location.ToString());

            startRotation = rotation;

            D.log("Controller", structure.name + " initial rotation set to " + rotation.ToString());
        }

        public virtual void setInitialLocationAndRotation(Vector2 location, Quaternion? rotation)
        {
            startSpot = location;

            D.log("Controller", structure.name + " initial location set to " + location.ToString());

            if (rotation != null)
            {
                startRotation = rotation.GetValueOrDefault().eulerAngles.z;
                
                D.log("Controller", structure.name + " initial rotation set to " + rotation.ToString());
            }
        }

        public virtual bool canRespawn()
        {
            if (structure != null)
            {
                return structure.CanRespawn;
            }

            // no structure to respawn so return false
            return false;
        }

        public virtual float getDespawnTime()
        {
            if (structure != null)
            {
                return structure.StructureData.DespawnTime;
            }

            // no structure to despawn so return 0
            return 0;
        }

        public virtual float getRespawnTime()
        {
            if (structure != null)
            {
                return structure.StructureData.RespawnTime;
            }

            // no structure to respawn so return large number
            return Mathf.Infinity;
        }

        //public abstract IEnumerator update();
        public abstract void update();

        ////////////////////////////////////
        /*
			Event dispatchers for all AI controllers
		*/
        ////////////////////////////////////		

        ///////////////////////////////////////////
        /*
			Handlers for AI controller events
		*/
        ///////////////////////////////////////////	

    }
}
