using System;

using UnityEngine;

namespace NoxCore.Data.Fittings
{
    [CreateAssetMenu(fileName = "NanoRepairerData", menuName = "ScriptableObjects/Fittings/Devices/NanoRepairer")]
    public class NanoRepairerData : DeviceData, INanoRepairerData
    {
        [Header("Nano Repairer")]

        public float __repairRate;
        [NonSerialized] protected float _repairRate;
        public float RepairRate { get { return _repairRate; } set { _repairRate = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            RepairRate = __repairRate;
        }
    }
}