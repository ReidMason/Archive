using UnityEngine;
using System.Collections;

namespace NoxCore.Helm
{
	public interface ISteeringBehaviour
	{
		void toggleEnabled();
		void resetWeight();
		void resetWeightToDefault();
		void enable();
		void enableExclusively();
		void disable();
		Vector2 execute();
	}
}