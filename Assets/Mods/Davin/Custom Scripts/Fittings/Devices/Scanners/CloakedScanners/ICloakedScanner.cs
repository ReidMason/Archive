using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace NoxCore.Fittings.Modules
{
	public interface ICloakedScanner : IModule
	{
		float getBeamDistance();
		float getDirectionalArcHalf();
		bool isNewSweep();
		List<GameObject> getObjectsInRange();
		List<GameObject> getNeutralsInRange();
		List<GameObject> getFriendliesInRange();
		List<GameObject> getEnemiesInRange();
	}
}