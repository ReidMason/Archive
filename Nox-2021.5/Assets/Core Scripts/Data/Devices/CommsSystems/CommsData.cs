using System;

using UnityEngine;

namespace NoxCore.Data.Fittings
{
    [CreateAssetMenu(fileName = "CommsSystemData", menuName = "ScriptableObjects/Fittings/Devices/CommsSystem")]
    public class CommsData : DeviceData, ICommsData
    {
        [Header("Comms System")]

        public float __roundTrip;
        [NonSerialized] protected float _roundTrip;
        public float RoundTrip { get { return _roundTrip; } set { _roundTrip = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            RoundTrip = __roundTrip;
        }
    }
}