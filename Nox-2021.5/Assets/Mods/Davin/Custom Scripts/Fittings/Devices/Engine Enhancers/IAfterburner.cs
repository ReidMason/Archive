using UnityEngine;
using System.Collections;

using NoxCore.Fittings.Devices;

namespace Davin.Fittings.Devices
{
    public interface IAfterburner : IDevice
    {
        void engage();
    }
}