using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Fittings.Devices;
using NoxCore.Placeables;
using NoxCore.GUIs;
using NoxCore.Data.Fittings;

namespace Davin.Fittings.Devices
{
    public class CloakScanner : Scanner, ICloakingScanner
    {
        protected List<Structure> cloakedInRange;

        public override void init(DeviceData deviceData = null)
        {
            base.init();

            layerMask |= 1 << LayerMask.NameToLayer("Cloaked");

            cloakedInRange = new List<Structure>();
        }

        public List<Structure> getCloakedInRange()
        {
            return cloakedInRange;
        }

        public override void reset()
        {
            base.reset();

            cloakedInRange.Clear();
        }

        public override Collider2D[] scan()
        {
            // NOTE: this method only works for GameObjects that have a Collider2D component attached
            return Physics2D.OverlapCircleAll(gameObject.transform.position, ScannerData.Radius, layerMask);
        }

        protected override void detectObjects(Scanner sender) //subscribed to ScannerNewSweep delegate
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

        protected override void classifyTarget(Collider2D detectedObject)
        {
            base.classifyTarget(detectedObject);

            Structure scannedStructure = detectedObject.GetComponent<Structure>();

            // check that the gameObject hit is derived from structure (TODO - or is a module? - gameplay?)
            if (scannedStructure != null)
            {
                // don't duplicate any objects in scanner (TODO - do I still need to do this?)
                if (!cloakedInRange.Contains(scannedStructure))
                {
                    if (detectedObject.gameObject.layer == LayerMask.NameToLayer("Cloaked"))
                    {
                        // Note: you would normally have the AI controller send out a detection message to the faction
                        cloakedInRange.Add(scannedStructure);

                        NoxGUI.Instance.setMessage("Cloaked ship found at: " + scannedStructure.transform.position);
                    }
                }
            }
        }
    }
}