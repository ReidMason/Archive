using UnityEngine;

using System;

namespace NoxCore.Data.Fittings
{
    public abstract class WeaponData : ModuleData, IWeaponData, ISerializationCallbackReceiver
    {
        [Header("Weapon")]

        public float __maxAmmo;
        [NonSerialized] protected float _maxAmmo;
        public float MaxAmmo { get { return _maxAmmo; } set { _maxAmmo = value; } }

        public GameObject __effectPrefab;
        [NonSerialized] protected GameObject _effectPrefab;
        public GameObject EffectPrefab { get { return _effectPrefab; } set { _effectPrefab = value; } }

        public float __powerPerShot;
        [NonSerialized] protected float _powerPerShot;
        public float PowerPerShot { get { return _powerPerShot; } set { _powerPerShot = value; } }

        public float __heatPerShot;
        [NonSerialized] protected float _heatPerShot;
        public float HeatPerShot { get { return _heatPerShot; } set { _heatPerShot = value; } }

        public float __minRange;
        [NonSerialized] protected float _minRange;
        public float MinRange { get { return _minRange; } set { _minRange = value; } }

        public float __maxRange;
        [NonSerialized] protected float _maxRange;
        public float MaxRange { get { return _maxRange; } set { _maxRange = value; } }

        public float __baseDamage;
        [NonSerialized] protected float _baseDamage;
        public float BaseDamage { get { return _baseDamage; } set { _baseDamage = value; } }

        public float __fireRate;
        [NonSerialized] protected float _fireRate;
        public float FireRate { get { return _fireRate; } set { _fireRate = value; } }

        public bool __canReveal;
        [NonSerialized] protected bool _canReveal;
        public bool CanReveal { get { return _canReveal; } set { _canReveal = value; } }

        public bool __autoFire = true;
        [NonSerialized] protected bool _autoFire;
        public bool AutoFire { get { return _autoFire; } set { _autoFire = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            MaxAmmo = __maxAmmo;
            EffectPrefab = __effectPrefab;
            PowerPerShot = __powerPerShot;
            HeatPerShot = __heatPerShot;
            MinRange = __minRange;
            MaxRange = __maxRange;
            BaseDamage = __baseDamage;
            FireRate = __fireRate;
            CanReveal = __canReveal;
            AutoFire = __autoFire;
        }
    }
}