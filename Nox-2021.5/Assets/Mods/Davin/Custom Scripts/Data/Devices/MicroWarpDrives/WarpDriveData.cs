using System;

using UnityEngine;

namespace NoxCore.Data.Fittings
{
    [CreateAssetMenu(fileName = "WarpDriveData", menuName = "ScriptableObjects/Fittings/Devices/WarpDrive")]
    public class WarpDriveData : DeviceData, IWarpDriveData
    {
        [Header("Warp Drive")]

        public float __maxWarpBubbleTime;
        [NonSerialized] protected float _maxWarpBubbleTime;
        public float MaxWarpBubbleTime { get { return _maxWarpBubbleTime; } set { _maxWarpBubbleTime = value; } }

        public float __warpSpeedPercentage;
        [NonSerialized] protected float _warpSpeedPercentage;
        public float WarpSpeedPercentage { get { return _warpSpeedPercentage; } set { _warpSpeedPercentage = value; } }

        public float __alignAccuracy;
        [NonSerialized] protected float _alignAccuracy;
        public float AlignAccuracy { get { return _alignAccuracy; } set { _alignAccuracy = value; } }

        public float __cooldownDuration;
        [NonSerialized] protected float _cooldownDuration;
        public float CooldownDuration { get { return _cooldownDuration; } set { _cooldownDuration = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            MaxWarpBubbleTime = __maxWarpBubbleTime;
            WarpSpeedPercentage = __warpSpeedPercentage;
            AlignAccuracy = __alignAccuracy;
            CooldownDuration = __cooldownDuration;
        }
    }
}