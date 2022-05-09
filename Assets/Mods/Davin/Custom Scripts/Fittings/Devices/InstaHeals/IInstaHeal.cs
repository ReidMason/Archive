using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NoxCore.Fittings.Devices;

namespace Davin.Fittings.Devices
{
    public interface IInstaHeal : IDevice
    {
        void heal();
    }
}