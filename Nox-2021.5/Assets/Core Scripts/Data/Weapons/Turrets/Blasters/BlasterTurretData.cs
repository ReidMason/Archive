using UnityEngine;

using System;

namespace NoxCore.Data.Fittings
{
    [CreateAssetMenu(fileName = "BlasterTurretData_", menuName = "ScriptableObjects/Fittings/Weapons/BlasterTurret")]
    public class BlasterTurretData : RotatingTurretData, IBlasterTurretData
    {
        [Header("Blaster Turret")]
        public float __shieldDamageModifier;
        [NonSerialized] protected float _shieldDamageModifier;
        public float ShieldDamageModifier { get { return _shieldDamageModifier; } set { _shieldDamageModifier = value; } }

        public float __hullDamageModifier;
        [NonSerialized] protected float _hullDamageModifier;
        public float HullDamageModifier { get { return _hullDamageModifier; } set { _hullDamageModifier = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            ShieldDamageModifier = __shieldDamageModifier;
            HullDamageModifier = __hullDamageModifier;
        }
    }
}