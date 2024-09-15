using UnityEngine;

namespace NoxCore.Fittings.Devices
{
    public interface IScan : IDevice
    {
        Collider2D[] scan();
    }
}