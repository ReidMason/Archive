using System;

using UnityEngine;

namespace NoxCore.Data.Fittings
{
    [CreateAssetMenu(fileName = "LaserTurretData", menuName = "ScriptableObjects/Fittings/Weapons/LaserTurret")]
    public class LaserTurretData : RotatingTurretData, IRotatingTurretData
    {
        [Header("Laser Turret")]

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