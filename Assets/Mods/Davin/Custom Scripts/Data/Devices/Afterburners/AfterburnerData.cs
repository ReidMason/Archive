using System;

using UnityEngine;

using NoxCore.Data.Fittings;
using NoxCore.Data;

namespace Davin.Data.Fittings
{
    [CreateAssetMenu(fileName = "AfterburnerData", menuName = "ScriptableObjects/Fittings/Devices/Afterburner")]
    public class AfterburnerData : DeviceData, IAfterburnerData
    {
        [Header("Afterburner")]

        public CooldownBuffData __cooldownBuff;
        [NonSerialized] protected CooldownBuffData _cooldownBuff;
        public CooldownBuffData CooldownBuff { get { return _cooldownBuff; } set { _cooldownBuff = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            CooldownBuff = __cooldownBuff;
        }
    }
}