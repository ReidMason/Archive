using System;

using UnityEngine;

using NoxCore.Data.Fittings;
using NoxCore.Fittings.Weapons;

namespace Davin.Data.Fittings
{
    [CreateAssetMenu(fileName = "SelfDestructData", menuName = "ScriptableObjects/Fittings/Devices/SelfDestruct")]
    public class SelfDestructData : DeviceData, ISelfDestructData
    {
        [Header("Self Destruct")]

        public Explosion __explosion;
        [NonSerialized] protected Explosion _explosion;
        public Explosion Explosion { get { return _explosion; } set { _explosion = value; } }

        public float __radialDamage;
        [NonSerialized] protected float _radialDamage;
        public float RadialDamage { get { return _radialDamage; } set { _radialDamage = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            Explosion = __explosion;
            RadialDamage = __radialDamage;
        }
    }
}