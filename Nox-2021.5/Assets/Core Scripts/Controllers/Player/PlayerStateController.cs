using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Weapons;
using NoxCore.Helm;
using NoxCore.Placeables;

namespace NoxCore.Controllers
{
    public class PlayerStateController : PlayerController
    {
        public delegate string actionHandler();
        protected Dictionary<string, actionHandler> playerActions;

        protected SeekBehaviour seekBehaviour;
        protected ArriveBehaviour arriveBehaviour;

        UnityEvent shutdown;

        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, helm);

            // create the state table
            playerActions = new Dictionary<string, actionHandler>();

            seekBehaviour = Helm.getBehaviourByName("SEEK") as SeekBehaviour;
            arriveBehaviour = Helm.getBehaviourByName("ARRIVE") as ArriveBehaviour;

            if (seekBehaviour != null) seekBehaviour.LookAheadDistance = waypointProximity * 2;
            if (arriveBehaviour != null) arriveBehaviour.SlowingRadius = waypointProximity;
            
            bindKey(KeyCode.S, shutdownAll);         

            StartCoroutine( moveAction() );

            booted = true;
        }

        public override void update()
        {
            if (booted == true)
            {
                generalUpdate();
            }
        }

        public void shutdownAll()
        {
            Gui.setMessage("SHUTTING DOWN!");

            foreach(Device device in structure.Devices)
            {
                device.deactivate();
            }

            foreach (Module module in structure.Modules)
            {
                module.deactivate();
            }

            foreach (Weapon weapon in structure.Weapons)
            {
                weapon.deactivate();
            }
        }

        IEnumerator moveAction()
        {
            while(true)
            {
                if (Helm.destination != null)
                {
                    if (WaypointMarkers.Count > 1)
                    {
                        if (seekBehaviour != null)
                        {
                            if (seekBehaviour.Active == false)
                            {
                                seekBehaviour.enableExclusively();
                            }

                            Helm.destination = setHelmDestination().GetValueOrDefault();

                            if (Helm.destination != null && Cam.followTarget != null && Cam.followTarget.gameObject == Helm.ShipStructure.gameObject)
                            {
                                Debug.DrawLine(structure.transform.position, Helm.destination.GetValueOrDefault(), Color.blue, Time.deltaTime, true);
                            }
                        }
                    }
                    else
                    {
                        if (arriveBehaviour != null)
                        {
                            if (arriveBehaviour.Active == false)
                            {
                                arriveBehaviour.enableExclusively();
                            }

                            Helm.destination = setHelmDestination().GetValueOrDefault();

                            if (Helm.destination != null && Cam.followTarget != null && Cam.followTarget.gameObject == Helm.ShipStructure.gameObject)
                            {
                                Debug.DrawLine(structure.transform.position, Helm.destination.GetValueOrDefault(), Color.blue, Time.deltaTime, true);
                            }
                        }
                    }
                }

                // are we close enough to a waypoint to switch to the next one?
                if (WaypointMarkers.Count > 0 && Helm.RangeToDestination < waypointProximity)
                {                    
                    // recycle waypoint marker
                    WaypointMarkers[0].gameObject.Recycle();

                    // remove the current waypoint
                    WaypointMarkers.RemoveAt(0);

                    // set the helm's destination to null
                    Helm.destination = null;

                    // if there is another waypoint then set to the new first waypoint
                    if (WaypointMarkers.Count > 0)
                    {
                        Helm.destination = WaypointMarkers[0].position;
                        Helm.RangeToDestination = Vector2.Distance(Helm.Position, Helm.destination.GetValueOrDefault());
                    }
                    else
                    {
                        Helm.desiredThrottle = 0.0f;
                    }
                }

                yield return null;
            }
        }
    }
}
