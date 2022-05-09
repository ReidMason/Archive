using UnityEngine;
using System.Collections;

namespace NoxCore.Placeables
{
    public class Asteroid : NoxObject
    {
        public float spin;

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(new Vector3(0, 0, spin * Time.deltaTime));
        }
    }
}