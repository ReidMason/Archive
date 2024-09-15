using UnityEngine;
using System.Collections;

using NoxCore.GameModes;

namespace NoxCore.Debugs
{
	public interface IAmmoDebuggable : IDebuggable
	{
		void debugAmmoMaximise(object sender, DebugEventArgs args);
		void debugAmmoMinimise(object sender, DebugEventArgs args);
		void debugAmmoIncrease(object sender, DebugEventArgs args, int amount);
		void debugAmmoDecrease(object sender, DebugEventArgs args, int amount);
		void debugAmmoIncrement(object sender, DebugEventArgs args);
		void debugAmmoDecrement(object sender, DebugEventArgs args);
	}
}