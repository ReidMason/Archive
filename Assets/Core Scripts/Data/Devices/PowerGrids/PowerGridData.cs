using System;

using UnityEngine;

namespace NoxCore.Data.Fittings
{
    [CreateAssetMenu(fileName = "PowerGridData", menuName = "ScriptableObjects/Fittings/Devices/PowerGrid")]
    public class PowerGridData : DeviceData, IPowerGridData
    {
        [Header("Power Grid")]
        
        public float __maxPower;
        [NonSerialized] protected float _maxPower;
        public float MaxPower { get { return _maxPower; } set { _maxPower = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            MaxPower = __maxPower;
        }
    }
}