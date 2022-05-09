using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoxCore.Utilities
{
    public class NullableVector2 : MonoBehaviour
    {
        [HideInInspector, SerializeField] Vector2? nullableVector2;

        [ExposeProperty]
        public Vector2? Value
        {
            get { return nullableVector2; }
            set { nullableVector2 = value; }
        }
    }
}