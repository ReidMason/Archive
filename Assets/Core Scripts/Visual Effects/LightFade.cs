using UnityEngine;
using System.Collections;

namespace NoxCore.Effects
{
    public class LightFade : MonoBehaviour
    {
        protected Light lightSource;

        void Awake()
        {
            lightSource = GetComponent<Light>();
        }

        // Update is called once per frame
        void Update()
        {
            lightSource.range = Mathf.Lerp(lightSource.range, 0, Time.deltaTime);
        }
    }
}