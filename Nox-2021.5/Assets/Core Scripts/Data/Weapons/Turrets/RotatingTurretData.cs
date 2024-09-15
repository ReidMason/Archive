using System;

using UnityEngine;

namespace NoxCore.Data.Fittings
{
    [CreateAssetMenu(fileName = "LaserTurretData", menuName = "ScriptableObjects/Fittings/Weapons/LaserTurret")]
    public class RotatingTurretData : WeaponData, IRotatingTurretData
    {
        [Header("Rotating Turret")]

        public float __slewSpeed;
        [NonSerialized] protected float _slewSpeed;
        public float SlewSpeed { get { return _slewSpeed; } set { _slewSpeed = value; } }

        public float __trackingAngle;
        [NonSerialized] protected float _trackingAngle;
        public float TrackingAngle { get { return _trackingAngle; } set { _trackingAngle = value; } }

        public bool __transversalDamage;
        [NonSerialized] protected bool _transversalDamage;
        public bool TransversalDamage { get { return _transversalDamage; } set { _transversalDamage = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            SlewSpeed = __slewSpeed;
            TrackingAngle = __trackingAngle;
            TransversalDamage = __transversalDamage;
        }
    }
}