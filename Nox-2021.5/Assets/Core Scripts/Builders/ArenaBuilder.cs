using UnityEngine;

using NoxCore.Placeables;
using NoxCore.Rules;

namespace NoxCore.Builders
{
	public class ArenaBuilder : MonoBehaviour
	{
		public int roundDuration = 600;
		public float startRadius = 2000;
		public float endRadius = 2000;
		public Vector2 centre;
		public int resizeDelay = 0;
		public int resizeDuration = 0;
		public bool bounce = false;
		public int numSatellites = 32;
		public GameObject barrierSatellite;

		public ArenaBarrier init()
		{
			//GameObject go = Instantiate(Resources.Load<GameObject>("Placeables/Empty"));

			GameObject go = new GameObject();
			GameObject hierarchy = GameObject.Find("Placeables");

			go.name = "Arena Barrier";

			D.log("Content", "Building " + go.name);
				
			go.transform.position = new Vector3(0, 0, 0);
				
			ArenaBarrier noxObject = go.AddComponent<ArenaBarrier>();

			noxObject.init(startRadius, centre, numSatellites, barrierSatellite);

			go.transform.parent = hierarchy.transform;
	
			ArenaRules rules = go.AddComponent<ArenaRules>();
			rules.Init(roundDuration, startRadius, endRadius, resizeDelay, resizeDuration, bounce);

            D.log("Content", "Finished building " + go.name);
			noxObject.spawn();

			return noxObject;
		}
	}
}