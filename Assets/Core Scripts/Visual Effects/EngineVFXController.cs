using UnityEngine;

using NoxCore.Fittings.Modules;
using NoxCore.Managers;
using NoxCore.Placeables.Ships;

namespace NoxCore.Effects
{
    [RequireComponent (typeof(TrailRenderer))]
    public class EngineVFXController : VFXController
    {
        protected TrailRenderer trail;
        protected Material trailMaterial;
        public Vector2 uvAnimationRate;
        protected Vector2 uvOffset;

        public AudioSource audioSource;

        protected Ship ship;

        public void setExhaustTrailColour(Engine engine)
        {
            trail.colorGradient = engine.EngineData.ExhaustColourGradient;
        }

        public override void setSortingLayerOrder(Transform prefabRoot, int sortingOrderOffset = 0)
        {
            trail.sortingLayerName = ship.StructureRenderer.sortingLayerName;
            trail.sortingOrder = ship.StructureRenderer.sortingOrder + sortingOrderOffset;
        }

        public override void setupVFX(Transform prefabRoot = null, int sortingOrderOffset = 0)
        {
            base.setupVFX(prefabRoot, sortingOrderOffset);

            Engine engine = prefabRoot.GetComponent<Engine>();

            ship = engine.getStructure() as Ship;

            audioSource = GetComponent<AudioSource>();

            trail = GetComponent<TrailRenderer>();
            trailMaterial = trail.material;

            if (engine.EngineData.UseCustomExhaustColours == true)
            {
                setExhaustTrailColour(engine);
            }
        }

        public override void startVFX()
        {
            base.startVFX();

            if (trail != null)
            {
                trail.enabled = true;
                trail.emitting = true;
            }
        }

        public override void stopVFX()
        {
            base.stopVFX();

            if (trail != null)
            {
                trail.enabled = false;
                trail.emitting = false;
            }
        }

        public void startTrailEmission()
        {
            if (trail != null)
            {
                trail.emitting = true;
            }
        }

        public void stopTrailEmission()
        {
            if (trail != null)
            {
                trail.emitting = false;
            }
        }

        public override void updateVFX()
        {
            if (ship != null && ship.Speed >= 0.1f)
            {
                if (isRunning == false && ship.silentRunning == false)
                {
                    startVFX();
                }

                if (trail.emitting == false)
                {
                    startTrailEmission();
                }

                if (audioSource.clip != null)
                {
                    if (ship.transform == GameManager.Instance.MainCamera.followTarget)
                    {
                        if (audioSource.isPlaying == false)
                        {
                            audioSource.PlayOneShot(audioSource.clip);
                        }
                    }
                }
            }
            else if (ship != null && ship.Speed < 0.1f)
            {
                if (isRunning == true)
                {
                    stopVFX();
                }

                if (audioSource.clip != null)
                {
                    if (ship.transform == GameManager.Instance.MainCamera.followTarget)
                    {
                        if (audioSource.isPlaying == false)
                        {
                            audioSource.Stop();
                        }
                    }
                }
            }

            if (isRunning == true)
            {
                uvOffset += (uvAnimationRate * Time.deltaTime);
                trailMaterial.SetTextureOffset("_MainTex", uvOffset);
            }
        }
    }
}