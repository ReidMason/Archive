using UnityEngine;
using System.Collections;

using NoxCore.GameModes;

namespace NoxCore.Debugs
{
	public interface ISystemDebuggable : IDebuggable
	{
		void debugExplode(object sender, DebugEventArgs args);
		void debugActivate(object sender, DebugEventArgs args);
		void debugDeactivate(object sender, DebugEventArgs args);		
	}
}