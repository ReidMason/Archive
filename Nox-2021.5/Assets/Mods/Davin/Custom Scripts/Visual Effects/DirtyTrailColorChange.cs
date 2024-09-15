using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Davin.Effects
{
    public class DirtyTrailColorChange : MonoBehaviour
    {
        TrailRenderer engineTrail;
        float trailColorH;
        // Use this for initialization
        void Start()
        {
            engineTrail = GetComponent<TrailRenderer>();
            trailColorH = 0;
        }

        // Update is called once per frame
        void Update()
        {
            if (engineTrail != null)
            {
                engineTrail.startColor = Color.HSVToRGB(trailColorH, 1,1);
                trailColorH = trailColorH > 1 ? 0 : (trailColorH + 0.005f);
            }
        }
    }
}