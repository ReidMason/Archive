using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Utilities;

namespace NoxCore.Effects
{
    public abstract class VFXController : MonoBehaviour, IVisualEffect
    {
        // Note: the default controller assumes it is controlling a list of particle systems (could be third party components instead for instance e.g. Detonator)
        protected List<ParticleSystem> vfxs;

        [ShowOnly]
        public bool isRunning;

        public virtual bool getIsRunning()
        {
            return isRunning;
        }

        public virtual void setupVFX(Transform prefabRoot = null, int sortingOrderOffset = 0)
        {
            transform.SetParent(prefabRoot);

            vfxs = new List<ParticleSystem>();

            ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();

            if (particleSystems.Length > 0)
            {
                vfxs.AddRange(particleSystems);
            }
        }

        // TODO - why not just send through the prefab root's renderer here instead of its transform?
        public virtual void setSortingLayerOrder(Transform prefabRoot, int sortingOrderOffset = 0)
        {
            Renderer prefabRenderer = prefabRoot.GetComponent<Renderer>();

            if (prefabRenderer == null)
            {
                // try and get the structure's renderer instead if this is not a freely spawned vfx
                if (prefabRoot.parent != null)
                {
                    prefabRenderer = prefabRoot.parent.transform.parent.GetComponent<Renderer>();
                }
            }

            if (prefabRenderer != null && vfxs != null)
            {
                foreach (ParticleSystem vfx in vfxs)
                {
                    Renderer particleSystemRenderer = vfx.GetComponent<Renderer>();

                    if (particleSystemRenderer != null)
                    {
                        particleSystemRenderer.sortingLayerName = prefabRenderer.sortingLayerName;
                        particleSystemRenderer.sortingOrder = prefabRenderer.sortingOrder + 1 + sortingOrderOffset;
                    }
                }
            }
        }

        public virtual void startVFX()
        {
            isRunning = true;

            if (vfxs != null)
            {
                foreach (ParticleSystem vfx in vfxs)
                {
                    if (vfx != null)
                    {
                        vfx.Play();
                    }
                }
            }
        }

        public virtual void stopVFX()
        {
            isRunning = false;

            foreach (ParticleSystem vfx in vfxs)
            {
                if (vfx != null)
                {
                    vfx.Stop();
                }
            }
        }

        public virtual void updateVFX() {}
    }
}