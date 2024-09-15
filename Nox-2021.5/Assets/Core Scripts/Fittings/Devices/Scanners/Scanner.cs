using UnityEngine;

using System;
using System.Collections.Generic;

using NoxCore.Controllers;
using NoxCore.Data.Fittings;
using NoxCore.Managers;
using NoxCore.Placeables;

namespace NoxCore.Fittings.Devices
{
    public class Scanner : Device, IScanner
    {
        #region variables
        [Header("Scanner")]

        public ScannerData __scannerData;
        [NonSerialized]
        protected ScannerData _scannerData;
        public ScannerData ScannerData { get { return _scannerData; } set { _scannerData = value; } }

        // must be set to be the same as the AI tick rate of the structure's AI Controller
        protected float sweepDelay;
        public float SweepDelay { get { return sweepDelay; } set { sweepDelay = value; } }

        protected float sweepTimer;

        protected List<GameObject> objectsInRange;
        protected List<Structure> neutralsInRange;
        protected List<Structure> friendliesInRange;
        protected List<Structure> enemiesInRange;

        protected int layerMask;
        #endregion

        #region delegates 
        public delegate void ScannerDelegates(Scanner sender);
        public event ScannerDelegates ScannerNewSweep;
        #endregion

        #region initialise & reset
        public override void init(DeviceData deviceData = null)
        {
            if (deviceData == null)
            {
                ScannerData = Instantiate(__scannerData);
                base.init(ScannerData);
            }
            else
            {
                ScannerData = deviceData as ScannerData;
                base.init(deviceData);
            }

            ScannerNewSweep += detectObjects;

            layerMask = 1 << LayerMask.NameToLayer("Ship") | 1 << LayerMask.NameToLayer("Structure");

            sweepTimer = 0;

            if (gameObject.GetComponentInParent<AIController>() != null) gameObject.GetComponentInParent<AIController>().ControllerBoot += OnControllerBoot;

            objectsInRange = new List<GameObject>();
            neutralsInRange = new List<Structure>();
            friendliesInRange = new List<Structure>();
            enemiesInRange = new List<Structure>();
        }        

        public override void reset()
        {
            base.reset();

            sweepTimer = 0;

            clearScanner();
        }
        #endregion

        #region getters

        public List<GameObject> getObjectsInRange()
        {
            return objectsInRange;
        }

        public List<Structure> getNeutralsInRange()
        {
            return neutralsInRange;
        }

        public List<Structure> getFriendliesInRange()
        {
            return friendliesInRange;
        }

        public List<Structure> getEnemiesInRange()
        {
            return enemiesInRange;
        }
        #endregion

        #region Scanner Methods
        public virtual void clearScanner()
        {
            objectsInRange.Clear();
            neutralsInRange.Clear();
            friendliesInRange.Clear();
            enemiesInRange.Clear();
        }

        public virtual Collider2D[] scan()
        {
            // NOTE: this method only works for GameObjects that have a Collider2D component attached
            return Physics2D.OverlapCircleAll(gameObject.transform.position, ScannerData.Radius, layerMask);
        }        

        protected virtual void detectObjects(Scanner sender) //subscribed to ScannerNewSweep delegate
        {
            clearScanner();

            Collider2D[] hitColliders = scan();

            for (int i = 0; i < hitColliders.Length; i++)
            {
                // make sure structure scanner is attached to is not in list
                if (hitColliders[i].gameObject != structure.gameObject)
                {
                    classifyTarget(hitColliders[i]);
                }
            }
        }

        protected virtual void classifyTarget(Collider2D detectedObject)
        {
            GameObject scannedObject = detectedObject.gameObject;

            // don't duplicate any objects in scanner (TODO - do I still need to do this?)
            if (!objectsInRange.Contains(detectedObject.gameObject))
            {
                objectsInRange.Add(scannedObject);

                Structure scannedStructure = scannedObject.GetComponent<Structure>();

                if (scannedStructure != null)
                {
                    // no faction ID then ignore classification based on this
                    NoxObject noxObject = scannedObject.GetComponent<NoxObject>();

                    if (noxObject == null || noxObject.Faction == null) return;

                    int factionID = noxObject.Faction.ID;

                    if (factionID == -1 || factionID == 0) neutralsInRange.Add(scannedStructure);
                    else if (factionID == structure.Faction.ID) friendliesInRange.Add(scannedStructure);
                    else enemiesInRange.Add(scannedStructure);
                }
            }
            else
            {
                Debug.Log("Duplicate object found by scanner");
            }
        }
        #endregion

        public override void update()
        {
            base.update();

            if (isActiveOn() == true && isFlippingActivation() == false)
            {
                sweepTimer += Time.deltaTime;

                if (sweepTimer >= sweepDelay)
                {
                    Call_NewScannerSweep();
                    sweepTimer = 0;					
                }
            }
            else
            {
                sweepTimer = 0;
            }
        }

        #region Event Delegates
        public void Call_NewScannerSweep()
        {
            if (ScannerNewSweep != null)
            {
                ScannerNewSweep(this);
            }
        }

        public void OnControllerBoot(AIController sender)
        {
            sweepDelay = sender.AITickRate;
        }
        #endregion

    }
}