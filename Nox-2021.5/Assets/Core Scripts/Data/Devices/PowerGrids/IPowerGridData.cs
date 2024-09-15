using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoxCore.Data.Fittings
{
    public interface IPowerGridData : IDeviceData
    {
        float MaxPower { get; set; }
    }
}