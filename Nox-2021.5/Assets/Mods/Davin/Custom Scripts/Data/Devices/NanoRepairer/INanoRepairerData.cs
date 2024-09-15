using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoxCore.Data.Fittings
{
    public interface INanoRepairerData : IDeviceData
    {
        float RepairRate { get; set; }
    }
}