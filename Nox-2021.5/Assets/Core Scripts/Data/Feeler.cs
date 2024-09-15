using UnityEngine;

namespace NoxCore.Data
{
    [CreateAssetMenu(fileName = "Feeler", menuName = "ScriptableObjects/Helm/Basic Feeler")]
    public class Feeler : ScriptableObject
    {
        public float length;

        [Range(-180, 180)] public float direction;
        protected float _direction;
        public float Direction { get { return _direction; } set { _direction = value; } }

        public Color colour;

        protected Vector2 _origin;
        public Vector2 Origin { get { return _origin; } set { _origin = value; } }
        
        protected Vector2 _dir;
        public Vector2 Dir { get { return _dir; } set { _dir = value; } }
    }
}