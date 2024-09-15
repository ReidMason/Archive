using UnityEngine;
using System.Collections;

namespace NoxCore.Placeables
{
	public class Planet : NoxObject3D, ISpawnable
	{
		public float rotationRate = 1.0f;
		public Vector3 rotationAxis;
		public float radiusScale = 1.0f;
		public GameObject orbiting;

		public override void spawn(bool spawnEnabled = false)
		{
			transform.localScale = new Vector3(transform.localScale.x * radiusScale, transform.localScale.y * radiusScale, transform.localScale.z * radiusScale);

			base.spawn(spawnEnabled);
		}

		// Update is called once per frame
		void Update() 
		{
			if (orbiting == null)
			{
				transform.Rotate(rotationAxis, rotationRate * Time.deltaTime, Space.Self);
			}
			else
			{
				transform.RotateAround (orbiting.transform.position, rotationAxis, Time.deltaTime * rotationRate);
			}
		}
	}
}