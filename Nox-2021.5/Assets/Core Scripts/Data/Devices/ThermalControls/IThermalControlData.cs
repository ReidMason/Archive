using UnityEngine;

namespace NoxCore.Data.Fittings
{
    public interface IThermalControlData : IDeviceData
    {
        float HeatCapacity { get; set; }
    }
}