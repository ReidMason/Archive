using System.Collections.Generic;
using System;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using NoxCore.Helm;
using NoxCore.Placeables;

namespace NoxCore.Controllers
{
    public abstract class PlayerController : StructureController
    {
        private Dictionary<int, Action> keyBinds;

        protected List<Transform> waypointMarkers;
        public List<Transform> WaypointMarkers { get { return waypointMarkers; } }

        protected GameObject waypointMarkerPrefab;

        // note: uses lazy instantiation so that it will always be set between scene loads if the Markers GameObject is present in the right place in the hierarchy
        protected Transform _markersGO;
        public Transform MarkersGO
        {
            get
            {
                if (_markersGO == null)
                {
                    _markersGO = GameObject.Find("Managers/UI Manager/Markers").transform;
                }

                return _markersGO;
            }
        }

        public float waypointProximity;

        public override void boot(Structure structure, HelmController helm = null)
        {
            // subscribe to own events

            base.boot(structure, helm);

            waypointMarkers = new List<Transform>();

            waypointMarkerPrefab = Resources.Load("Placeables/Environmental/Markers/Waypoint", typeof(GameObject)) as GameObject;

            waypointProximity = 50;

            keyBinds = new Dictionary<int, Action>();
        }

        protected Vector3 getSpaceCoords()
        {
            return Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
        }

        protected virtual Vector2? setHelmDestination()
        {
            if (waypointMarkers.Count > 0) return waypointMarkers[0].transform.position;
            else return null;
        }

        public void clearAllWaypoints()
        {
            foreach (Transform waypointMarker in waypointMarkers)
            {
                waypointMarker.gameObject.Recycle();
            }

            waypointMarkers.Clear();
        }

        protected bool hasClickedInFreeSpace()
        {
            bool hitFreespace = true;

            // test for hitting normal GameObjects using ray
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit2D hit2D = Physics2D.GetRayIntersection(ray, Mathf.Infinity, 1 << LayerMask.NameToLayer("Structure") | 1 << LayerMask.NameToLayer("Ship") | 1 << LayerMask.NameToLayer("UI"));

            if (hit2D.collider != null)
            {
                hitFreespace = false;
            }

            //Set up the new Pointer Event
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            List<RaycastResult> results = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            pointerData.position = Input.mousePosition;

            // get the one GraphicsRaycaster object that is not disabled
            GraphicRaycaster [] raycasters = FindObjectsOfType<GraphicRaycaster>();

            if (raycasters.Length > 0)
            {
                foreach(GraphicRaycaster raycaster in raycasters)
                {
                    if (raycaster.gameObject.activeInHierarchy == false) continue;

                    raycaster.Raycast(pointerData, results);
                    /*
                    //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
                    foreach (RaycastResult result in results)
                    {
                        Debug.Log("Hit " + result.gameObject.name);
                    }
                    */
                    if (results.Count > 0)
                    {
                        hitFreespace = false;
                        break;
                    }
                }                
            }

            return hitFreespace;
        }

        public virtual void generalUpdate()
        {
            if (Input.GetMouseButtonDown(0))
            {
                // check if clicking on something and if not set new waypoint
                if (hasClickedInFreeSpace() == true)
                {
                    Vector3 spaceCoords = getSpaceCoords();
                    Vector3 pos = new Vector3(spaceCoords.x, spaceCoords.y, 0.0f);

                    GameObject waypoint = waypointMarkerPrefab.Spawn(pos);
                    waypoint.name = "Waypoint " + (waypointMarkers.Count + 1);
                    waypoint.transform.parent = MarkersGO;

                    waypointMarkers.Add(waypoint.transform);

                    Helm.destination = waypointMarkers[0].position;
                    Helm.desiredThrottle = 1.0f;
                    Helm.RangeToDestination = Vector2.Distance(Helm.Position, Helm.destination.GetValueOrDefault());
                }
            }

            if (Input.GetMouseButtonDown(1))
            {
                if (hasClickedInFreeSpace() == true)
                {
                    clearAllWaypoints();
                }
            }
        }

        public override void start(float initialDelay = 0)
        {
            runningState = RunningState.RUNNING;
        }

        public override void stop()
        {
            runningState = RunningState.STOPPED;
        }

        void OnGUI()
        {
            Event e = Event.current;

            if (e != null && e.isKey)
            {
                KeyCode keyPressed = e.keyCode;
                Action keyEvent;

                keyBinds.TryGetValue((int)keyPressed, out keyEvent);

                if (keyEvent != null)
                {
                    if (Input.GetKeyDown(keyPressed)) keyEvent.Invoke();
                }
            }
        }

        public virtual void bindKey(KeyCode key, Action unityAction)
        {
            keyBinds.Add((int)key, unityAction);
        }

        ////////////////////////////////////
        /*
            Event dispatchers for all player controllers
        */
        ////////////////////////////////////		


        ///////////////////////////////////////////
        /*
            Handlers for all player controller events
        */
        ///////////////////////////////////////////		
    }
}
