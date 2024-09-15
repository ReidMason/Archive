using UnityEngine;
using System.Collections.Generic;

using NoxCore.Fittings.Modules;
using NoxCore.Placeables;

// http://www.illusioncatalyst.com/notes_files/mathematics/line_nu_sphere_intersection.php

namespace NoxCore.Effects
{
	[RequireComponent (typeof(LineRenderer))]
	public class IndirectWeaponBeamEffect : BeamEffect
	{
        public struct BeamHitInfo
        {
            public GameObject gameObject;
            public bool objectIs2D;
            public float distance;
            public Vector2 point;
        }

        public List<BeamHitInfo> beamHitInfos;

        public override void setupVFX(Transform prefabRoot, int sortingOrderOffset = 0)
        {
            base.setupVFX(prefabRoot, sortingOrderOffset);

            beamHitInfos = new List<BeamHitInfo>();
        }

        protected bool isTargetValid()
        {
            if (targetableWeapon.LockedTarget == null) return false;    // no locked target?

            (GameObject structure, GameObject system) lockedTarget = targetableWeapon.LockedTarget.GetValueOrDefault();
            (Structure structure, Module module) lockedTargetInfo = targetableWeapon.LockedTargetInfo.GetValueOrDefault();

            if (lockedTarget.system != null && lockedTargetInfo.module.destroyed == true) return false;       // destroyed modue?
            else if (lockedTarget.structure != null && lockedTargetInfo.structure.Destroyed == true) return false;  // destroyed structure?
            else return true;
        }

        // Update is called once per frame
        void Update () 
		{
            if (weaponStructure.enabled == false || weaponStructure.SystemsInitiated == false) return;

            if (weapon.isFiring() == true)
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

        void fireBeam()
		{
            ParticleSystem.MainModule newMain = beamEndEffect.main;
            newMain = beamEndEffect.main;

            if (beamVector == null)
            {
                D.warn("Weapon: {0}", "No value set for beamVector on weapon " + weapon.DeviceData.Type + " : " + weapon.DeviceData.SubType + " on structure " + weaponStructure.Name);
                return;
            }         

            #region find what the end of the beam hits by performing a combined 2D & 3D sweep-like test against colliders

            // clear the list of beamHitInfos
            beamHitInfos.Clear();

            Vector3 beamDir = beamVector.GetValueOrDefault();
            float beamMagnitude = beamDir.magnitude;

            beamStartPosition = muzzle.position;

            // raycast against all 2D colliders except those for our own structure
            RaycastHit2D[] hit2DInfos = Physics2D.RaycastAll(beamStartPosition, beamDir, beamMagnitude);

            // iterate through all Collider2Ds hit by ray and find the first one that isn't own our structure
            foreach (RaycastHit2D hit2DInfo in hit2DInfos)
            {
                // include any GameObject that isn't our own structure
                if (hit2DInfo.collider.gameObject != weaponStructure.gameObject)
                {
                    BeamHitInfo beamHitInfo = new BeamHitInfo();
                    beamHitInfo.gameObject = hit2DInfo.collider.gameObject;
                    beamHitInfo.objectIs2D = true;
                    beamHitInfo.distance = hit2DInfo.distance;
                    beamHitInfo.point = hit2DInfo.point;

                    beamHitInfos.Add(beamHitInfo);
                }
            }

            RaycastHit hit3DInfo;

            // raycast against all 3D colliders except those the origin is inside of (ignores own shield but could be inside another structure's shield which this will not detect)
            if (Physics.Raycast(beamStartPosition, beamDir, out hit3DInfo, beamMagnitude))
            {
                // insert this hit to the list of hit GameObjects based on the distance the hit was recorded

                bool hit3DInserted = false;

                for(int i = 0; i < beamHitInfos.Count; i++)
                {
                    if (hit3DInfo.distance < beamHitInfos[i].distance)
                    {
                        BeamHitInfo beamHitInfo = new BeamHitInfo();
                        beamHitInfo.gameObject = hit3DInfo.collider.gameObject;
                        beamHitInfo.objectIs2D = false;
                        beamHitInfo.distance = hit3DInfo.distance;
                        beamHitInfo.point = hit3DInfo.point;

                        beamHitInfos.Insert(i, beamHitInfo);
                        hit3DInserted = true;
                        break;
                    }
                }

                // the 3D collider hit is further away than any 2D collider so add to the end of the hitGOs list
                if (hit3DInserted == false)
                {
                    BeamHitInfo beamHitInfo = new BeamHitInfo();
                    beamHitInfo.gameObject = hit3DInfo.collider.gameObject;
                    beamHitInfo.objectIs2D = false;
                    beamHitInfo.distance = hit3DInfo.distance;
                    beamHitInfo.point = hit3DInfo.point;

                    beamHitInfos.Add(beamHitInfo);
                }
            }
            
            if (beamHitInfos.Count > 0)
            {
                // use the first beamHitInfo as the object hit by the beam
                BeamHitInfo beamHitInfo = beamHitInfos[0];

                // get the layer of the GameObject
                int layer = beamHitInfo.gameObject.layer;

                // hit a shield externally
                if (layer == shieldLayer)
                {
                    beamEndPosition = beamHitInfo.point;
                    beamEndEffect = shieldHitEffect;

                    if (lineRenderer.enabled == false)
                    {
                        MeshRenderer shieldRenderer = beamHitInfo.gameObject.GetComponent<MeshRenderer>();
                        Structure hitStructure = beamHitInfo.gameObject.transform.parent.GetComponent<Structure>();

//                        lineRenderer.sortingLayerName = hitStructure.StructureRenderer.sortingLayerName;
//                        lineRenderer.sortingOrder = hitStructure.StructureRenderer.sortingOrder;

                        if (shieldRenderer != null)
                        {
                            newMain.startColor = shieldRenderer.material.color;

                            if (hitStructure != null)
                            {
                                hitStructure.takeDamage(hitStructure.gameObject, weapon.getDamage(), weapon, (hitStructure.gameObject, null));
                            }
                        }

                        startVFX();
                    }
                }
                else if (layer == structureLayer)
                {
                    Structure hitStructure = beamHitInfo.gameObject.GetComponent<Structure>();

//                    lineRenderer.sortingLayerName = hitStructure.StructureRenderer.sortingLayerName;
//                    lineRenderer.sortingOrder = hitStructure.StructureRenderer.sortingOrder;

                    if (hitStructure.AllShieldsFailed == false)
                    {
                        // we've hit a structure but not it's shield (otherwise previous if statment would have fired) so we must be inside it's shield mesh so fake the hit against it's shield
                        beamEndPosition = beamStartPosition + ((hitStructure.transform.position - beamStartPosition) / 2.0f);
                        beamEndEffect = shieldHitEffect;

                        if (lineRenderer.enabled == false)
                        {
                            MeshRenderer shieldRenderer = hitStructure.ShieldRenderer;

                            if (shieldRenderer != null)
                            {
                                newMain.startColor = shieldRenderer.material.color;
                                hitStructure.takeDamage(hitStructure.gameObject, weapon.getDamage(), weapon, (hitStructure.gameObject, null));
                            }

                            startVFX();
                        }
                    }
                    else
                    {
                        // if we are directly above the hit structure, use it's position for the beam end location or the beam hit point itself
                        beamEndEffect = structureHitEffect;

                        // cast ray into the scene at the muzzle position and record all Collider2D components hit
                        Ray ray = new Ray(beamStartPosition - Vector3.forward, Vector3.forward);
                        RaycastHit2D[] rayHitInfos = Physics2D.GetRayIntersectionAll(ray, 1.0f, structureLayer);

                        // is our beam start overlapping the hit structure's collider
                        bool insideTarget = false;

                        // iterate through all Collider2Ds hit by ray and find if one of the is the target collider
                        foreach (RaycastHit2D raycastHit in rayHitInfos)
                        {
                            if (raycastHit.collider.gameObject == hitStructure.gameObject)
                            {
                                insideTarget = true;
                                break;
                            }
                        }

                        // if inside target collider use the target position for the beam end, otherwise use the hitInfo position for the beam end
                        if (insideTarget == true)
                        {
                            beamEndPosition = hitStructure.transform.position;
                        }
                        else
                        {
                            beamEndPosition = beamHitInfo.point;
                        }

                        if (lineRenderer.enabled == false)
                        {
                            SpriteRenderer spriteRenderer = hitStructure.gameObject.GetComponent<SpriteRenderer>();

                            if (spriteRenderer != null)
                            {
                                // TODO - would be good to have a few positions on the structure that vary where the beam actually appears to hit
                                // could then read (or store) the pixel colours of these places and use those to change the colour of the beam end effect
                                // would make it look like coloured pieces of hull plating are falling off the ship when struck
                                //Texture2D tex = spriteRenderer.sprite.texture;
                                //beamEndMain.startColor = tex.GetPixel(tex.width / 2, tex.height / 2);
                                newMain.startColor = Color.grey;
                            }
                            else
                            {                                
                                newMain.startColor = origBeamColour;
                            }

                            startVFX();
                        }
                    }
                }
                else
                {
                    // beam hit hit a non-structure based collider
                    beamEndPosition = beamHitInfo.point;

                    // TODO - this particle system effect should be retrieved from the struck object (using the default structureHitEffect for now)
                    beamEndEffect = structureHitEffect;

                    if (lineRenderer.enabled == false)
                    {
                        startVFX();
                    }
                }
            }
            else
            {
                // use the end of the beam as the end point with no end effect
                beamEndPosition = beamStartPosition + beamDir;

                // TODO - this particle system effect should be retrieved from the struck object (using the default structureHitEffect for now)
                beamEndEffect = structureHitEffect;

                if (lineRenderer.enabled == false)
                {
                    startVFX();
                }
            }
            #endregion
        }
	}
}