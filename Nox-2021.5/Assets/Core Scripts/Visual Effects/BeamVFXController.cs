using UnityEngine;

using NoxCore.Fittings.Weapons;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Utilities;

namespace NoxCore.Effects
{
    public class BeamVFXController : VFXController
    {
        protected LineRenderer lineRenderer;
        public Vector2 uvAnimationRate;
        protected Vector2 uvOffset;
        protected Vector3 beamStartPosition, beamEndPosition;

        protected Structure weaponStructure;
        protected Weapon weapon;
        protected TargetableWeapon targetableWeapon;

        public Vector3? beamVector;
        protected bool beamMissed;

        // cache any transforms here
        protected Transform muzzle;

        // the particle system, in this case sparks which will be created by the Laser
        protected ParticleSystem beamStartEffect, beamEndEffect;
        protected ParticleSystem shieldHitEffect, structureHitEffect;

        protected ParticleSystemRenderer shieldHitRenderer, structureHitRenderer;

        // ray for testing where the beam actually hits
        protected Color origBeamColour;
        protected int beamCollisionMask;

        // raycasts info
        protected RaycastHit hitInfo;
        protected static int shieldLayer;
        protected static int structureLayer;

        public ParticleSystem getBeamEndEffect()
        {
            return beamEndEffect;
        }

        public override void setupVFX(Transform prefabRoot = null, int sortingOrderOffset = 0)
        {
            base.setupVFX(prefabRoot, sortingOrderOffset);

            shieldLayer = 1 << LayerMask.NameToLayer("Shield");
            structureLayer = 1 << LayerMask.NameToLayer("Ship") | 1 << LayerMask.NameToLayer("Structure");

            weapon = prefabRoot.GetComponent<Weapon>();
            weaponStructure = weapon.getStructure();
            targetableWeapon = weapon as TargetableWeapon;

            muzzle = weapon.gameObject.transform.Find("Muzzle");
            if (muzzle == null) muzzle = weapon.gameObject.transform;

            lineRenderer = GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;

            lineRenderer.sortingLayerName = weaponStructure.StructureRenderer.sortingLayerName;
            lineRenderer.sortingOrder = weaponStructure.StructureRenderer.sortingOrder + sortingOrderOffset;

            lineRenderer.enabled = false;

            Texture2D beamTexture = lineRenderer.material.mainTexture as Texture2D;

            origBeamColour = beamTexture.GetPixel(beamTexture.width / 2, beamTexture.height / 2);

            shieldHitEffect = transform.parent.FindChildStartsWith("ShieldHitEffect").GetComponent<ParticleSystem>();
            structureHitEffect = transform.parent.FindChildStartsWith("StructureHitEffect").GetComponent<ParticleSystem>();

            shieldHitRenderer = shieldHitEffect.GetComponent<ParticleSystemRenderer>();
            structureHitRenderer = structureHitEffect.GetComponent<ParticleSystemRenderer>();
        }

        public override void startVFX()
        {
            lineRenderer.enabled = true;
            isRunning = true;

            if (beamEndEffect != null && beamEndEffect.isPlaying == false)
            {
                beamEndEffect.transform.rotation = Quaternion.identity;
                beamEndEffect.Play();
                beamEndEffect.transform.SetParent(GameManager.Instance.EffectsParent);

                // D.log ("Graphics", "Turret beam hit effect on");
            }
        }

        public override void stopVFX()
        {
            lineRenderer.enabled = false;
            isRunning = false;

            if (beamEndEffect != null)
            {
                if (beamEndEffect.isPlaying == true)
                {
                    beamEndEffect.Stop();
                    beamEndEffect.Clear();
                }

                beamEndEffect.transform.SetParent(weapon.transform);

                // D.log ("Graphics", "Turret beam hit effect off");
            }
        }

        public override void updateVFX()
        {
            if (isRunning == true)
            {
                lineRenderer.SetPosition(0, beamStartPosition + new Vector3(0, 0, -1));
                lineRenderer.SetPosition(1, beamEndPosition + new Vector3(0, 0, -1));

                uvOffset += (uvAnimationRate * Time.deltaTime);

                lineRenderer.material.SetTextureOffset("_MainTex", uvOffset);
            }

            if (lineRenderer.enabled == true && beamEndEffect != null)
            {
                // move the End Effect particle system to the target
                beamEndEffect.transform.position = beamEndPosition;
            }
        }
    }
}
