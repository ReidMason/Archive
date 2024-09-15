using System;

using UnityEngine;

using NoxCore.Data.Fittings;

namespace Davin.Data.Fittings
{
    [CreateAssetMenu(fileName = "ColourSprayGunData", menuName = "ScriptableObjects/Fittings/Weapons/ColourSprayGun")]
    public class ColourSprayGunData : RotatingTurretData, IColourSprayGunData
    {
        [Header("Colour Spray Gun")]

        public Color32 __colour;
        [NonSerialized] protected Color32 _colour;
        public Color32 Colour { get { return _colour; } set { _colour = value; } }

        public float __minDropRadius;
        [NonSerialized] protected float _minDropRadius;
        public float MinDropRadius { get { return _minDropRadius; } set { _minDropRadius = value; } }

        public float __maxDropRadius;
        [NonSerialized] protected float _maxDropRadius;
        public float MaxDropRadius { get { return _maxDropRadius; } set { _maxDropRadius = value; } }

        public float __spread;
        [NonSerialized] protected float _spread;
        public float Spread { get { return _spread; } set { _spread = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            Colour = __colour;
            MinDropRadius = __minDropRadius;
            MaxDropRadius = __maxDropRadius;
            Spread = __spread;
        }
    }
}