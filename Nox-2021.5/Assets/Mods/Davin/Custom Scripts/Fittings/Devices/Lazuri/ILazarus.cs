using UnityEngine;
using System.Collections;

using NoxCore.Controllers;
using NoxCore.Fittings.Devices;
using NoxCore.Placeables;

namespace Davin.Fittings.Devices
{
    public interface ILazarus : IDevice
    {
        void Lazarus_RaiseFromDead(object sender, TargetDestroyedEventArgs args);
    }
}