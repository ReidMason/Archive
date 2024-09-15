using UnityEngine;

using System;

using NoxCore.Fittings.Devices;
using NoxCore.Data.Fittings;

namespace Davin.Data.Fittings
{
    [CreateAssetMenu(fileName = "RapidFireBlasterTurretData_", menuName = "ScriptableObjects/Fittings/Weapons/RapidFireBlasterTurret")]
    public class RapidFireBlasterTurretData : BlasterTurretData, IRapidFireBlasterTurretData
    {
        [Header("Rapid Blaster Turret")]

        public int __boltsToFire;
        [NonSerialized] protected int _boltsToFire;
        public int BoltsToFire { get { return _boltsToFire; } set { _boltsToFire = value; } }

        public float __cooldown;
        [NonSerialized] protected float _cooldown;
        public float Cooldown { get { return _cooldown; } set { _cooldown = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            BoltsToFire = __boltsToFire;
            Cooldown = __cooldown;
        }
    }
}