using UnityEngine;

using System;

namespace NoxCore.Data.Fittings
{
    [CreateAssetMenu(fileName = "ShieldGeneratorData", menuName = "ScriptableObjects/Fittings/Modules/ShieldGenerator")]
    public class ShieldGeneratorData : ModuleData, IShieldGeneratorData
    {
        [Header("Shield Generator")]

        public float __shieldDelay;
        [NonSerialized] protected float _shieldDelay;
        public float ShieldDelay { get { return _shieldDelay; } set { _shieldDelay = value; } }

        [Range(0, 1)]
        public float __weakFraction;
        [NonSerialized] protected float _weakFraction;
        public float WeakFraction { get { return _weakFraction; } set { _weakFraction = value; } }

        [Range(0, 1)]
        public float __bleedFraction;
        [NonSerialized] protected float _bleedFraction;
        public float BleedFraction { get { return _bleedFraction; } set { _bleedFraction = value; } }

        [Header("Shield Charge")]

        public float __minCharge;
        [NonSerialized] protected float _minCharge;
        public float MinCharge { get { return _minCharge; } set { _minCharge = value; } }

        public float __maxCharge;
        [NonSerialized] protected float _maxCharge;
        public float MaxCharge { get { return _maxCharge; } set { _maxCharge = value; } }

        public float __rechargeRate;
        [NonSerialized] protected float _rechargeRate;
        public float RechargeRate { get { return _rechargeRate; } set { _rechargeRate = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            ShieldDelay = __shieldDelay;
            WeakFraction = __weakFraction;
            BleedFraction = __bleedFraction;
            MinCharge = __minCharge;
            MaxCharge = __maxCharge;
            RechargeRate = __rechargeRate;
        }
    }
}