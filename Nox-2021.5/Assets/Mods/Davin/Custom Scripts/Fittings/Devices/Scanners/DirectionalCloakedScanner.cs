using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Placeables;
using NoxCore.Fittings.Devices;
using NoxCore.Data.Fittings;

namespace Davin.Fittings.Devices
{
    public class CloakedScanner : Device
    {
        protected int layerMask;
        public int beamDistance;
        public float sweepDelay;
        public float sweepTimer;
        protected List<GameObject> objectsInRange;
        protected List<GameObject> neutralsInRange;
        protected List<GameObject> friendliesInRange;
        protected List<GameObject> enemiesInRange;

        protected bool _newSweep;
        public bool NewSweep { get { return _newSweep; } }

        public bool fixedDirectionalArc;

        [Range(0, 180)]
        public float directionalArcHalf;
        private float _directionalArc;
        public float DirectionalArc { get { return _directionalArc; } }

        public override void init(DeviceData deviceData = null)
        {
            base.init();
            
            layerMask = 1 << LayerMask.NameToLayer("Cloaked");

            sweepTimer = 0;

            objectsInRange = new List<GameObject>();
            neutralsInRange = new List<GameObject>();
            friendliesInRange = new List<GameObject>();
            enemiesInRange = new List<GameObject>();

            _newSweep = false;
            _directionalArc = directionalArcHalf * 2.0f;
        }

        public override void reset()
        {
            base.reset();

            sweepTimer = 0;

            clearScanner();

            _newSweep = false;
        }

        public float getBeamDistance()
        {
            return beamDistance;
        }

        public float getDirectionalArcHalf()
        {
            return directionalArcHalf;
        }

        public bool isNewSweep()
        {
            return NewSweep;
        }

        private void clearScanner()
        {
            objectsInRange.Clear();
            neutralsInRange.Clear();
            friendliesInRange.Clear();
            enemiesInRange.Clear();
        }

        public List<GameObject> getObjectsInRange()
        {
            return objectsInRange;
        }

        public List<GameObject> getNeutralsInRange()
        {
            return neutralsInRange;
        }

        public List<GameObject> getFriendliesInRange()
        {
            return friendliesInRange;
        }

        public List<GameObject> getEnemiesInRange()
        {
            return enemiesInRange;
        }

        protected bool isWithinDirectionalArc(Collider collider)
        {
            /*
            if (fixedDirectionalArc == true)
            {
                if (Vector3.Angle(collider.transform.position - transform.position, Socket.transform.forward) <= directionalArcHalf)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }*/

            return true;
        }

        protected virtual void detectObjects()
        {
            clearScanner();

            // NOTE: this method only works for GameObjects that have a collider
            Collider[] hitColliders = Physics.OverlapSphere(gameObject.transform.position, beamDistance, layerMask);

            for (int i = 0; i < hitColliders.Length; i++)
            {
                // TODO - may need/want to test that the object is valid or use a set of filters (possibly user-defined - gameplay?)?

                // make sure structure scanner is attached to is not in list
                if (hitColliders[i].gameObject != structure.gameObject)
                {
                    // check if structure detected is in detection cone
                    if (isWithinDirectionalArc(hitColliders[i]) == true)
                    {
                        Structure scannedStructure = hitColliders[i].gameObject.GetComponent<Structure>();

                        // check that the gameObject hit is derived from structure (TODO - or is a module? - gameplay?)
                        if (scannedStructure != null)
                        {
                            // don't duplicate any objects in scanner (TODO - do I still need to do this?)
                            if (!objectsInRange.Contains(hitColliders[i].gameObject))
                            {
                                GameObject scannedObject = hitColliders[i].gameObject;

                                objectsInRange.Add(scannedObject);

                                int factionID = scannedObject.GetComponent<NoxObject>().Faction.ID;

                                if (factionID == -1) neutralsInRange.Add(scannedObject);
                                else if (factionID == structure.Faction.ID) friendliesInRange.Add(scannedObject);
                                else enemiesInRange.Add(scannedObject);
                            }
                        }
                    }
                }
            }
        }

        public override void update()
        {
            base.update();

            if (isActiveOn() == true && isFlippingActivation() == false)
            {
                sweepTimer += Time.deltaTime;

                if (sweepTimer >= sweepDelay)
                {
                    detectObjects();
                    sweepTimer = 0;
                    _newSweep = true;

                    // TODO - display as graphical signal/noise line or other					
                }
                else
                {
                    _newSweep = false;
                }
            }
            else
            {
                sweepTimer = 0;
            }
        }
    }
}