using System;

using UnityEngine;

namespace NoxCore.Data.Fittings
{
    [CreateAssetMenu(fileName = "ThermalControlData", menuName = "ScriptableObjects/Fittings/Devices/ThermalControl")]
    public class ThermalControlData : DeviceData, IThermalControlData
    {
        [Header("Thermal Control")]

        public float __heatCapacity;
        [NonSerialized] protected float _heatCapacity;
        public float HeatCapacity { get { return _heatCapacity; } set { _heatCapacity = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            HeatCapacity = __heatCapacity;
        }
    }
}