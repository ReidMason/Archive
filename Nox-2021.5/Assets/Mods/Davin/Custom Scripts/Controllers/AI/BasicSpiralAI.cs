using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Helm;
using NoxCore.Placeables;

namespace NoxCore.Controllers
{
	public class BasicSpiralAI : BasicNavigationalAI
	{
		public float radius = 300;
		protected float angle = 0;
		public float angleInterval = 30;

		public override void boot(Structure structure, HelmController helm = null)
		{
			base.boot(structure, helm);
		}

		protected override Vector2? setHelmDestination()
		{
			float xPos = radius * Mathf.Cos(angle * Mathf.Deg2Rad);
			float yPos = radius * Mathf.Sin(angle * Mathf.Deg2Rad);

			radius += 25;
			angle += angleInterval;

			return new Vector2(xPos, yPos);
		}
	}
}