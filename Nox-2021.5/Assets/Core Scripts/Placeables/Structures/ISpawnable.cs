using UnityEngine;
using System.Collections;

namespace NoxCore.Placeables
{
	public interface ISpawnable
	{
		void spawn(bool spawnEnabled = false);
		void despawn();
		void respawn();
		void reset();	
		void hideObject();
		void showObject();
	}
}