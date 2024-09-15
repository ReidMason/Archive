using UnityEngine;
using System.Collections;

using NoxCore.Managers;
using NoxCore.Placeables;

namespace NoxCore.Builders
{
	public class PlanetBuilder : Builder
	{
		public Vector3 position;
		public Vector3 rotation;
		public string resourcePath;
        public string planetName;

		public override void Build()
		{
			GameObject go = Instantiate(Resources.Load<GameObject>("Placeables/Environmental/Celestials/Planets/" + resourcePath));

			if (go != null)
			{
				GameObject hierarchy = GameObject.Find("Placeables");

				go.name = planetName;
				D.log("Content", "Building " + go.name);
				
				go.transform.position = position;
				go.transform.rotation = Quaternion.Euler(rotation);
				go.transform.parent = hierarchy.transform;

                Planet planet = go.GetComponent<Planet>();

                D.log("Content", "Finished building " + go.name);
                planet.spawn();
            }
        }
	}
}