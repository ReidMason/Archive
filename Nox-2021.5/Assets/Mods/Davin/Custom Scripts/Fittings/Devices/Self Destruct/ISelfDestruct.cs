using UnityEngine;
using System.Collections;

using NoxCore.Fittings.Devices;

namespace Davin.Fittings.Devices
{
    public interface ISelfDestruct : IDevice
    {
        bool countdownActive();
        void activateSelfDestruct();
        void deactivateSelfDestruct();
    }
}