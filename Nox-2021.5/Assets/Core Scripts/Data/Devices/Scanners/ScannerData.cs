using System;

using UnityEngine;

namespace NoxCore.Data.Fittings
{
    [CreateAssetMenu(fileName = "ScannerData", menuName = "ScriptableObjects/Fittings/Devices/Scanner")]
    public class ScannerData : DeviceData, IScannerData
    {
        [Header("Scanner")]

        public float __radius;
        [NonSerialized] protected float _radius;
        public float Radius { get { return _radius; } set { _radius = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            Radius = __radius;
        }
    }
}