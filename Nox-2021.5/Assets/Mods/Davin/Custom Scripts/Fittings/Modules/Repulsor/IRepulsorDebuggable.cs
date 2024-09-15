using UnityEngine;
using System.Collections;

using NoxCore.Debugs;
using NoxCore.GameModes;

namespace Davin.Debugs
{
    public interface IRepulsorDebuggable : ISystemDebuggable
    {
        void debugRepulse(object sender, DebugEventArgs args);
    }
}