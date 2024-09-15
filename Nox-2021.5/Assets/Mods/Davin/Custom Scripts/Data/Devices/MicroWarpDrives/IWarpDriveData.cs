using System;

using UnityEngine;

namespace NoxCore.Data.Fittings
{
    public interface IWarpDriveData : IDeviceData
    {
        float MaxWarpBubbleTime { get; set; }
        float WarpSpeedPercentage { get; set; }
        float AlignAccuracy { get; set; }
        float CooldownDuration { get; set; }
    }
}


