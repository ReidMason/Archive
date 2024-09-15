using System.Collections.Generic;

using UnityEngine;

using NoxCore.Data.Fittings;
using NoxCore.Placeables;

namespace NoxCore.Fittings.Devices
{
	public interface IScanner : IScan
	{
		ScannerData ScannerData { get; set; }

        List<GameObject> getObjectsInRange();
		List<Structure> getNeutralsInRange();
		List<Structure> getFriendliesInRange();
		List<Structure> getEnemiesInRange();
	}
}