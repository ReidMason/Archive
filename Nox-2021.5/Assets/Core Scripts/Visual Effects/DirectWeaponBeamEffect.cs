using UnityEngine;

using NoxCore.Fittings.Modules;
using NoxCore.Placeables;

// http://www.illusioncatalyst.com/notes_files/mathematics/line_nu_sphere_intersection.php

namespace NoxCore.Effects
{
	[RequireComponent (typeof(LineRenderer))]
	public class DirectWeaponBeamEffect : BeamEffect, IVisualEffect
	{	
        protected bool isTargetValid()
        {
            if (targetableWeapon.LockedTarget == null) return false;    // no locked target?

            (GameObject structure, GameObject system) lockedTarget = targetableWeapon.LockedTarget.GetValueOrDefault();
            (Structure structure, Module module) lockedTargetInfo = targetableWeapon.LockedTargetInfo.GetValueOrDefault();

            if (lockedTarget.system != null && lockedTargetInfo.module.destroyed == true) return false;       // destroyed module?
            else if (lockedTarget.structure != null && lockedTargetInfo.structure.Destroyed == true) return false;  // destroyed structure?
            else return true;
        }

        // Update is called once per frame
        void Update()
        {
            if (weaponStructure != null && (weaponStructure.enabled == false || weaponStructure.SystemsInitiated == false)) return;

            IVisibleEffect visualEffect = weapon as IVisibleEffect;

            if (visualEffect != null && visualEffect.isEffectVisible() == true)
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

            if (target == null) return;

            beamStartPosition = muzzle.position;
            beamEndPosition = target.transform.position;
            Vector3 beamDir = target.transform.position - muzzle.position;
			
			Structure targetStructure = lockedTarget.structure.GetComponent<Structure>();

//            lineRenderer.sortingLayerName = targetStructure.StructureRenderer.sortingLayerName;
//            lineRenderer.sortingOrder = targetStructure.StructureRenderer.sortingOrder;

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
                shieldHitRenderer.sortingLayerName = targetStructure.StructureRenderer.sortingLayerName;
                shieldHitRenderer.sortingOrder = targetStructure.StructureRenderer.sortingOrder;

                beamEndEffect = shieldHitEffect;

                // if (Physics.Raycast(beamStartPosition, beamDir, out hitInfo, beamDir.magnitude, shieldLayer))
                RaycastHit[] hitInfos = Physics.RaycastAll(beamStartPosition, beamDir, beamDir.magnitude, shieldLayer);

                if (hitInfos.Length > 0)
                {
                    // last entry in hitInfos should be the weapon's target so use this as the hit
                    //beamEndPosition = hitInfo.point;
                    beamEndPosition = hitInfos[hitInfos.Length - 1].point;

                    if (lineRenderer.enabled == false)
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

                    if (lineRenderer.enabled == false)
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

//                    structureHitRenderer.sortingLayerName = targetStructure.StructureRenderer.sortingLayerName;
//                    structureHitRenderer.sortingOrder = targetStructure.StructureRenderer.sortingOrder;

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
                    if (lineRenderer.enabled == false)
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