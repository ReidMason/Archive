using UnityEngine;
using System.Collections;

using NoxCore.GameModes;

namespace NoxCore.Debugs
{
	public interface IShieldDebuggable : ISystemDebuggable 
	{
		void debugIncrease(object sender, DebugEventArgs args, float amount);
		void debugDecrease(object sender, DebugEventArgs args, float amount);
		void debugRaise(object sender, DebugEventArgs args);
		void debugLower(object sender, DebugEventArgs args);
		void debugFail(object sender, DebugEventArgs args);
	}
}