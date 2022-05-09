using UnityEngine;

using NoxCore.Data.Fittings;
using NoxCore.Fittings.Devices;

namespace Davin.Fittings.Devices
{
    public interface IWarpDrive : IDevice
    {
        WarpDriveData WarpDriveData { get; set; }
        float Range { get; set; }

        WarpSequence getWarpStatus();
        void engage();
        void disengage();
    }
}