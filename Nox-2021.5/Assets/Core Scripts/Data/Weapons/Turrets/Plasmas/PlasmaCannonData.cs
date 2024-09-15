using UnityEngine;
using System;

namespace NoxCore.Data.Fittings
{
    [CreateAssetMenu(fileName = "PlasmaCannonData", menuName = "ScriptableObjects/Fittings/Weapons/PlasmaCannon")]
    public class PlasmaCannonData : RotatingTurretData, IPlasmaCannonData
    {
        [Header("Plasma Canon")]
        public float __effectDuration;
        [NonSerialized] protected float _effectDuration;
        public float EffectDuration { get { return _effectDuration; } set { _effectDuration = value; } }

        public float __shieldDamageModifier;
        [NonSerialized] protected float _shieldDamageModifier;
        public float ShieldDamageModifier { get { return _shieldDamageModifier; } set { _shieldDamageModifier = value; } }

        public float __hullDamageModifier;
        [NonSerialized] protected float _hullDamageModifier;
        public float HullDamageModifier { get { return _hullDamageModifier; } set { _hullDamageModifier = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            EffectDuration = __effectDuration;
            ShieldDamageModifier = __shieldDamageModifier;
            HullDamageModifier = __hullDamageModifier;
        }
    }
}