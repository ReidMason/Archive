using UnityEngine;
using System.Collections;

using NoxCore.GameModes;

namespace NoxCore.Debugs
{
	public interface IDebuggable
	{
		void debugMaximise(object sender, DebugEventArgs args);
		void debugMinimise(object sender, DebugEventArgs args);
		void debugIncrease(object sender, DebugEventArgs args, int amount);
		void debugDecrease(object sender, DebugEventArgs args, int amount);
	}
}