using System;

using UnityEngine;

namespace NoxCore.Data
{
    [CreateAssetMenu(fileName = "FeelerData", menuName = "ScriptableObjects/Helm/Advanced Feeler")]
    public class FeelerData : ScriptableObject, IFeelerData, ISerializationCallbackReceiver
    {
        public float __length;
        [NonSerialized] protected float _length;
        public float Length { get { return _length; } set { _length = value; } }

        [Range(-180, 180)] public float __direction;
        [NonSerialized] protected float _direction;
        public float Direction { get { return _direction; } set { _direction = value + 90; } }

        public Color __colour;
        [NonSerialized] protected Color _colour;
        public Color Colour { get { return _colour; } set { _colour = value; } }

        protected Vector2 _dir;
        public Vector2 Dir { get { return _dir; } set { _dir = value; } }

        public virtual void OnAfterDeserialize()
        {
            Length = __length;
            Direction = __direction;
            Colour = __colour;
        }

        public virtual void OnBeforeSerialize()
        { }
    }
}