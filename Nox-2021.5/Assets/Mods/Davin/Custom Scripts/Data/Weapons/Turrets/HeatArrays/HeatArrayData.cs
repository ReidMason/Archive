using System;

using UnityEngine;

using NoxCore.Data.Fittings;


namespace Davin.Data.Fittings
{
    [CreateAssetMenu(fileName = "HeatArrayData", menuName = "ScriptableObjects/Fittings/Weapons/HeatArray")]
    public class HeatArrayData : RotatingTurretData, IHeatArrayData
    {
        [Header("Heat Array")]

        public float __effectDuration;
        [NonSerialized] protected float _effectDuration;
        public float EffectDuration { get { return _effectDuration; } set { _effectDuration = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            EffectDuration = __effectDuration;
        }
    }
}