using UnityEngine;

using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Weapons;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Utilities;

namespace NoxCore.Effects
{
    public class PlasmaBeamVFXController : VFXController, IVisualEffect
    {
        public int zigs = 200;
        public float speed = 1f;
        public float scale = 1f;
        Perlin noise;
        float oneOverZigs;
        private ParticleSystem.Particle[] particles;
        protected Vector3 beamStartPosition, beamEndPosition;

        protected Structure weaponStructure;
        protected PlasmaCannon weapon;
        protected TargetableWeapon targetableWeapon;

        //The particle system, in this case sparks which will be created by the Laser
        protected ParticleSystem beamStartEffect, beamEndEffect;
        protected ParticleSystem shieldHitEffect, structureHitEffect;

        protected ParticleSystemRenderer shieldHitRenderer, structureHitRenderer;

        // cache any transforms here		
        Transform muzzle;

        // ray for testing where the beam actually hits
        protected Color origBeamColour;
        protected int beamCollisionMask;

        ParticleSystem pSystem;
        ParticleSystemRenderer pRenderer;
        ParticleSystem.EmissionModule pEmitter;
        
        // raycasts info
        protected RaycastHit hitInfo;
        protected static int shieldLayer;
        protected static int structureLayer;

        protected bool isTargetValid()
        {
            if (targetableWeapon.LockedTarget == null) return false;    // no locked target?

            (GameObject structure, GameObject system) lockedTarget = targetableWeapon.LockedTarget.GetValueOrDefault();
            (Structure structure, Module module) lockedTargetInfo = targetableWeapon.LockedTargetInfo.GetValueOrDefault();

            if (lockedTarget.system != null && lockedTargetInfo.module.destroyed == true) return false;       // destroyed modue?
            else if (lockedTarget.structure != null && lockedTargetInfo.structure.Destroyed == true) return false;  // destroyed structure?
            else return true;
        }

        public override void setupVFX(Transform prefabRoot = null, int sortingOrderOffset = 0)
        {
            base.setupVFX(prefabRoot, sortingOrderOffset);

            shieldLayer = (1 << LayerMask.NameToLayer("Shield"));
            structureLayer = (1 << LayerMask.NameToLayer("Ship")) | (1 << LayerMask.NameToLayer("Structure")) | (1 << LayerMask.NameToLayer("Module"));

            weapon = prefabRoot.GetComponent<PlasmaCannon>();
            weaponStructure = weapon.getStructure();
            targetableWeapon = weapon as TargetableWeapon;

            muzzle = weapon.gameObject.transform.Find("Muzzle");
            if (muzzle == null) muzzle = weapon.gameObject.transform;

            shieldHitEffect = transform.parent.FindChildStartsWith("ShieldHitEffect").GetComponent<ParticleSystem>();
            structureHitEffect = transform.parent.FindChildStartsWith("StructureHitEffect").GetComponent<ParticleSystem>();

            shieldHitRenderer = shieldHitEffect.GetComponent<ParticleSystemRenderer>();
            structureHitRenderer = structureHitEffect.GetComponent<ParticleSystemRenderer>();

            pSystem = GetComponent<ParticleSystem>();
            pRenderer = GetComponent<ParticleSystemRenderer>();
            pEmitter = pSystem.emission;

            oneOverZigs = 1f / (float)zigs;
            pEmitter.enabled = false;

            pSystem.Emit(zigs);

            particles = new ParticleSystem.Particle[zigs];
            pSystem.GetParticles(particles);
        }

        public override void startVFX()
        {
            isRunning = true;            

            if (beamEndEffect != null &&  beamEndEffect.isPlaying == false)
            {
                beamEndEffect.transform.rotation = Quaternion.identity;
                beamEndEffect.Play();
                beamEndEffect.transform.SetParent(GameManager.Instance.EffectsParent);

                // D.log ("Graphics", "Turret beam hit effect on");
            }

            pRenderer.enabled = true;
        }

        public override void stopVFX()
        {
            if (pRenderer != null)
            {
                pRenderer.enabled = false;
            }

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
                if (noise == null) noise = new Perlin();

                float timex = Time.time * speed * 0.1365143f;
                float timey = Time.time * speed * 1.21688f;
                float timez = Time.time * speed * 2.5564f;

                for (int i = 0; i < particles.Length; i++)
                {
/*                    Vector3 targetPos;

                    if (targetableWeapon.Target._2 == null)
                    {
                        targetPos = targetableWeapon.Target._1.transform.position;
                    }
                    else
                    {
                        targetPos = targetableWeapon.Target._2.transform.position;
                    }
*/
                    Vector3 position = Vector3.Lerp(transform.position, beamEndPosition, oneOverZigs * (float)i);

                    Vector3 offset = new Vector3(noise.Noise(timex + position.x, timex + position.y, timex + position.z),
                                                 noise.Noise(timey + position.x, timey + position.y, timey + position.z),
                                                 noise.Noise(timez + position.x, timez + position.y, timez + position.z));

                    position += (offset * scale * ((float)i * oneOverZigs));

                    particles[i].position = position;
                    particles[i].startColor = Color.white;
                    particles[i].startLifetime = 1f;
                }

                pSystem.SetParticles(particles, zigs);

                // move the End Effect particle system to the target
                if (beamEndEffect)
                {
                    beamEndEffect.transform.position = beamEndPosition;
                }
            }
        }

        void Update()
        {
            if (weaponStructure != null && (weaponStructure.enabled == false || weaponStructure.SystemsInitiated == false)) return;

            if (weapon != null && weapon.isEffectVisible() == true)
            {
                if (targetableWeapon != null)
                {
                    bool validTarget = isTargetValid();

                    if (validTarget == true)
                    {
                        fireBeam();
                    }
                    else
                    {
                        stopVFX();
                    }
                }
                else
                {
                    fireBeam();
                }
            }
            else
            {
                stopVFX();
            }

            updateVFX();
        }

        private void fireBeam()
        {
            GameObject target;

            (GameObject structure, GameObject system) lockedTarget = targetableWeapon.LockedTarget.GetValueOrDefault();

            if (lockedTarget.system == null)
            {
                target = lockedTarget.structure;
            }
            else
            {
                target = lockedTarget.system;
            }

            // check if target is still valid (could have been destroyed or warped out)
            if (target == null) return;

            beamStartPosition = muzzle.position;
            beamEndPosition = target.transform.position;
            Vector3 beamDir = target.transform.position - muzzle.position;            

            Structure targetStructure = lockedTarget.structure.GetComponent<Structure>();

            pRenderer.sortingLayerName = weaponStructure.StructureRenderer.sortingLayerName;
            pRenderer.sortingOrder = weaponStructure.StructureRenderer.sortingOrder + 2;

            if (targetStructure.ShieldCollider == null || (targetStructure.ShieldCollider != null && targetStructure.ShieldCollider.enabled == false))
            {
                beamCollisionMask = structureLayer;
            }
            else
            {
                beamCollisionMask = shieldLayer;
            }

            #region check for collision with shield, structure or targeted module

            if (beamCollisionMask == shieldLayer)
            {
                beamEndEffect = shieldHitEffect;

                RaycastHit[] hitInfos = Physics.RaycastAll(beamStartPosition, beamDir, beamDir.magnitude, shieldLayer);

                if (hitInfos.Length > 0)
                {
                    beamEndPosition = hitInfos[hitInfos.Length - 1].point;                    

                    if (pRenderer.enabled == false)
                    {
                        MeshRenderer shieldRenderer = hitInfos[hitInfos.Length - 1].collider.GetComponent<MeshRenderer>();

                        if (shieldRenderer != null)
                        {
                            ParticleSystem.MainModule newMain = beamEndEffect.main;
                            newMain.startColor = shieldRenderer.material.color;
                        }

                        startVFX();
                    }
                }
                else
                {
                    // weapon is inside target structure shield mesh so need to fake where it hits (going for somewhere between halfway from muzzle to target)
                    beamEndPosition = beamStartPosition + ((target.transform.position - beamStartPosition) / (Random.value + 1));

                    if (pRenderer.enabled == false)
                    {
                        if (targetStructure != null)
                        {
                            Collider shieldCollider = targetStructure.ShieldCollider;

                            if (shieldCollider != null)
                            {
                                MeshRenderer shieldRenderer = shieldCollider.GetComponent<MeshRenderer>();

                                if (shieldRenderer != null)
                                {
                                    ParticleSystem.MainModule newMain = beamEndEffect.main;
                                    newMain.startColor = shieldRenderer.material.color;
                                }
                            }
                        }

                        startVFX();
                    }
                }
            }
            else
            {
                RaycastHit2D[] hitInfos = Physics2D.RaycastAll(beamStartPosition, beamDir, beamDir.magnitude, structureLayer);

                if (hitInfos.Length > 0)
                {
                    // last entry in hitInfos should be the weapon's target so use this as the hit
                    RaycastHit2D hitInfo = hitInfos[hitInfos.Length - 1];

                    beamEndEffect = structureHitEffect;

                    // cast ray into the scene at the muzzle position and record all Collider2D components hit
                    Ray ray = new Ray(beamStartPosition - Vector3.forward, Vector3.forward);
                    RaycastHit2D[] rayHitInfos = Physics2D.GetRayIntersectionAll(ray, 1.0f, structureLayer);

                    // is our beam start overlapping the target collider
                    bool insideTarget = false;

                    // iterate through all Collider2Ds hit by ray and find if one of the is the target collider
                    foreach (RaycastHit2D raycastHit in rayHitInfos)
                    {
                        if (raycastHit.collider.gameObject == target)
                        {
                            insideTarget = true;
                            break;
                        }
                    }

                    // if inside target collision mesh use the target position for the beam end, otherwise use the hitInfo position for the beam end
                    if (insideTarget == true)
                    {
                        beamEndPosition = target.transform.position;
                    }
                    else
                    {
                        beamEndPosition = hitInfo.point;
                    }

                    // D.log ("Graphics", "Hit structure");                    
                    if (pRenderer.enabled == false)
                    {                        
                        SpriteRenderer spriteRenderer = target.gameObject.GetComponent<SpriteRenderer>();

                        if (spriteRenderer != null)
                        {
                            // TODO - would be good to have a few positions on the structure that vary where the beam actually appears to hit
                            // could then read (or store) the pixel colours of these places and use those to change the colour of the beam end effect
                            // would make it look like coloured pieces of hull plating are falling off the ship when struck
                            //Texture2D tex = spriteRenderer.sprite.texture;
                            //beamEndEffect.startColor = tex.GetPixel(tex.width / 2, tex.height / 2);

                            ParticleSystem.MainModule newMain = beamEndEffect.main;
                            newMain.startColor = Color.grey;
                        }
                        else
                        {
                            ParticleSystem.MainModule newMain = beamEndEffect.main;
                            newMain.startColor = origBeamColour;
                        }

                        startVFX();
                    }
                }
            }
            #endregion
        }
    }
}