using UnityEngine;

namespace NoxCore.Data.Fittings
{
    public interface ICommsData : IDeviceData
    {
        float RoundTrip { get; set; }
    }
}