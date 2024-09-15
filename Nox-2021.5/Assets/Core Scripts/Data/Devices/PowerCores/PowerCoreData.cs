using System;

using UnityEngine;

namespace NoxCore.Data.Fittings
{
	[CreateAssetMenu(fileName = "PowerCoreData", menuName = "ScriptableObjects/Fittings/Devices/PowerCore")]
	public class PowerCoreData : DeviceData, IPowerCoreData
	{
        [Header("Power Core")]

        public float __powerGeneration;
        [NonSerialized] protected float _powerGeneration;
        public float PowerGeneration { get { return _powerGeneration; } set { _powerGeneration = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            PowerGeneration = __powerGeneration;
        }
    }
}