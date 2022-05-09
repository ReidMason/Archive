using UnityEngine;
using System.Collections;

namespace NoxCore.Effects
{
    public class ExhaustVFXController : VFXController
    {
        public float getMaxLifespan()
        {
            float maxLifespan = 0.0f;

            foreach (ParticleSystem vfx in vfxs)
            {
                if (vfx != null)
                {
                    float lifespan = vfx.main.startLifetime.constant + vfx.main.duration;
                    //vfx.main.duration
                    //vfx.main.startLifetime.;

                    if (lifespan > maxLifespan)
                    {
                        maxLifespan = lifespan;
                    }
                }
            }

            return maxLifespan;
        }
    }
}