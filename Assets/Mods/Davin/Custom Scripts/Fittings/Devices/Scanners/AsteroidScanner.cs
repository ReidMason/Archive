using System.Collections.Generic;
using UnityEngine;

using NoxCore.Data.Fittings;
using NoxCore.Fittings.Devices;

namespace Davin.Fittings.Devices
{
	public class AsteroidScanner : Scanner, IAsteroidScanner
	{
		// add your additional class data here
		protected List<GameObject> _asteroidsInRange = new List<GameObject>();
		public List<GameObject> AsteroidsInRange
		{
			get { return _asteroidsInRange; }
			set { _asteroidsInRange = value; }
		}

		// there has to be an init method if any additional initialisation is required
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

			// additional initialisation
			layerMask |= 1 << LayerMask.NameToLayer("Asteroid");
		}

		// if on respawning, anything needs to be reset, include a reset method.
		// this must override the parent reset method and call it
		public override void reset()
		{
			base.reset();

			clearScanner();
		}

		// clear the parent scanner data and the extra asteroid list
		public override void clearScanner()
		{
			base.clearScanner();

			AsteroidsInRange.Clear();
		}

		// classify the target according to the parent method then check if it
		// should also be classified as an asteroid and placed in the list
		protected override void classifyTarget(Collider2D detectedObject)
		{
			base.classifyTarget(detectedObject);

			if (detectedObject.tag == "Asteroid")
			{
				AsteroidsInRange.Add(detectedObject.gameObject);
			}
		}
	}
}