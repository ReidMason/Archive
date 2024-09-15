using UnityEngine;
using System.Collections;

using NoxCore.GameModes;

namespace NoxCore.Debugs
{
	public interface ICloakDebuggable : ISystemDebuggable 
	{
		void debugCloak(object sender, DebugEventArgs args);
		void debugDecloak(object sender, DebugEventArgs args);
	}
}