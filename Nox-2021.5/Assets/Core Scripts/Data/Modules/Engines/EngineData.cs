using UnityEngine;

using System;

using com.spacepuppy;

namespace NoxCore.Data.Fittings
{
    [CreateAssetMenu(fileName = "EngineData", menuName = "ScriptableObjects/Fittings/Modules/Engine")]
    public class EngineData : ModuleData, IEngineData
    {
        [Header("Engine")]

        public float __maxSpeed;
        [NonSerialized] [ShowNonSerializedProperty("RUNTIME VALUES")] protected float _maxSpeed;
        public float MaxSpeed { get { return _maxSpeed; } set { _maxSpeed = value; } }

        public float __maxOverheatedSpeed;
        [NonSerialized] protected float _maxOverheatedSpeed;
        public float MaxOverheatedSpeed { get { return _maxOverheatedSpeed; } set { _maxOverheatedSpeed = value; } }

        [Header("Exhaust")]

        public bool __useCustomExhaustColours;
        [NonSerialized] [ShowNonSerializedProperty("RUNTIME VALUES")] protected bool _useCustomExhaustColours;
        public bool UseCustomExhaustColours { get { return _useCustomExhaustColours; } set { _useCustomExhaustColours = value; } }

        public Gradient __exhaustColourGradient;
        [NonSerialized] protected Gradient _exhaustColourGradient;
        public Gradient ExhaustColourGradient { get { return _exhaustColourGradient; } set { _exhaustColourGradient = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            MaxSpeed = __maxSpeed;
            MaxOverheatedSpeed = __maxOverheatedSpeed;
            UseCustomExhaustColours = __useCustomExhaustColours;
            ExhaustColourGradient = __exhaustColourGradient;
        }
    }
}