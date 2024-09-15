using UnityEngine;
using System.Collections;

using NoxCore.GameModes;

namespace NoxCore.Debugs
{
	public interface IStructureDebuggable : IDebuggable 
	{
		IEnumerator debugSelfDestruct(object sender, DebugEventArgs args);
	}
}