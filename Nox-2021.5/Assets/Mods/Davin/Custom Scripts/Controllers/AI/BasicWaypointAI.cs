using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Helm;
using NoxCore.Placeables;

namespace NoxCore.Controllers
{
	public class BasicWaypointAI : BasicNavigationalAI
	{
		public List<Transform> waypoints = new List<Transform>(); 
		protected int currentWaypoint = 0;

		protected override Vector2? setHelmDestination()
		{
			Vector2 nextPoint = waypoints[currentWaypoint].position;

			currentWaypoint++;

			if (currentWaypoint == waypoints.Count)
			{
				currentWaypoint = 0;
			}

			return nextPoint;
		}
	}
}