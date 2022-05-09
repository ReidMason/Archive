using UnityEngine;
using System.Collections;

using NoxCore.Managers;

namespace NoxCore.Effects
{
    public class ExplosionVFXController : VFXController
    {
        public DelayedRecycler recycler;
        public delegate IEnumerator DelayedRecycler(float DelayedRecycler);
        protected float maxLifespan;
        public AudioSource audioSource;

        void Awake()
        {
            setupVFX(GameManager.Instance.EffectsParent);
        }

        public float getDuration()
        {
            return maxLifespan;
        }

        public override void setupVFX(Transform prefabRoot = null, int sortingOrderOffset = 0)
        {
            base.setupVFX(prefabRoot, sortingOrderOffset);

            // set the delegate for delay and recycling of gameObject
            recycler = DelayRecycle;

            foreach (ParticleSystem vfx in vfxs)
            {
                maxLifespan = Mathf.Max(maxLifespan, vfx.main.duration + vfx.main.startLifetime.constant);
            }

            // D.log ("Graphics", "Max lifetime: " + maxLifespan);

            audioSource = GetComponent<AudioSource>();
        }

// TODO - Note: there does not seem to be a public event for detecting when a particle system has finished
// could do this polling system but IsAlive reported to be buggy + seems insane to have to poll each update
/*  
        public override void updateVFX()
        {
            bool hasCompleted = true;

            foreach(ParticleSystem vfx in vfxs)
            {
                if (vfx.IsAlive() == true)
                {
                    hasCompleted = false;
                }
            }

            if (hasCompleted == true)
            {
                gameObject.Recycle();
            }
        }

        void Update()
        {
            updateVFX();
        }
*/

        void OnDisable()
        {
            // D.log ("Graphics", "Explosion Disabled");

            stopVFX();
        }

        void OnEnable()
        {
            // D.log ("Graphics", "Explosion Enabled");

            startVFX();

            if (audioSource != null && audioSource.clip != null)
            {
                audioSource.PlayOneShot(audioSource.clip);
            }

            StartCoroutine(DelayRecycle(maxLifespan));
        }

        IEnumerator DelayRecycle(float delay)
        {
            yield return new WaitForSeconds(delay);

            gameObject.Recycle();
 
            // D.log ("Graphics", "Explosion Recycled");
        }
    }
}