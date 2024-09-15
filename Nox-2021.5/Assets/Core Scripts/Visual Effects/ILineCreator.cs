using UnityEngine;

namespace NoxCore.Effects
{
		public interface ILineCreator
		{
			int getLineWidth();
			Color getLineColour();
			Material getMaterial();
			Vector3 [] getPoints();
		}
}

