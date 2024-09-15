using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using NoxCore.Fittings.Weapons;
using NoxCore.Placeables;
using NoxCore.Effects;
using NoxCore.Utilities;

namespace Davin.Fittings.Weapons
{
    public class BlackHoleBomb : Projectile
    {
        /*
        Swaps the user with the bomb before exploding, explodes at the previous position of the user
        */
        public float proximity;

        public float spriteRotationRate;

        [ShowOnly]
        public float currentFlightTime;
        public float FlightTimeToExplode = 5;

        public GameObject explosion;
        public float explosionForce;
        public float explosionRadius;

        protected GameObject clonedExplosion;
        protected ExplosionVFXController boom;
        protected float boomDuration;

        protected SpriteSheetVFXController spriteSheetController;

        protected List<Structure> hitStructures = new List<Structure>();

        public override void init()
        {
            base.init();

            spriteSheetController = GetComponent<SpriteSheetVFXController>();

            if (spriteSheetController != null)
            {
                spriteSheetController.setupVFX();
            }

            initialised = true;
        }

        // Use this for initialization
        protected override void OnEnable()
        {
            // D.log("Projectile", "Missile Enabled");

            if (initialised == false) init();

            base.OnEnable();

            currentFlightTime = 0;
            armed = false;
        }


        protected override void disable()
        {
            if (disabled == false)
            {
                base.disable();

                if (spriteSheetController != null && spriteSheetController.isRunning == true)
                {
                    spriteSheetController.stopVFX();
                }

                armed = false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            base.update();

            transform.Rotate(new Vector3(0, 0, spriteRotationRate * Time.deltaTime));

            if (hasLaunched == true && Destroyed == false)
            {
                // increase current flight time
                currentFlightTime += Time.deltaTime;

                // arm projectile after arming time reached
                if (armed == false)
                {
                    if (currentFlightTime >= ArmingTime)
                    {
                        arm();
                    }
                }

                if (currentFlightTime >= FlightTimeToExplode)
                {
                    explode();
                }
            }

            if (hitStructures.Count > 0)
            {
                foreach (Structure HitStructure in hitStructures)
                {
                    HitStructure.gameObject.transform.position = Vector3.Lerp(this.gameObject.transform.position, HitStructure.gameObject.transform.position, 0.99f);
                }
            }

        }

        public void explode()
        {
            Destroyed = true;
            //swap process

            if (spriteSheetController != null)
            {
                spriteSheetController.stopVFX();
            }

            if (armed == true)
            {
                float distTravelled = currentFlightTime * flightSpeed;

                float damage = weapon.getDamage();

                // add an explosion force if explosion is set to occur
                if (myRigidbody != null && explosionForce != 0 && explosionRadius > 0)
                {
                    myRigidbody.AddExplosionForce(explosionForce, transform.position, explosionRadius);
                }

                if (explosionRadius > 0)
                {
                    // damage other ships or large structures in explosion range (what about modules?)
                    Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

                    foreach (Collider2D spaceObject in hitColliders)
                    {
                        if (spaceObject.tag == "Ship" || spaceObject.tag == "Structure" || spaceObject.tag == "Target")
                        {
                            Rigidbody2D rb = spaceObject.GetComponent<Rigidbody2D>();

                            if (rb != null)
                            {
                                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
                            }

                            float distToSpaceObject = Vector2.Distance(transform.position, spaceObject.transform.position);
                            float falloff = 1.0f - Mathf.Clamp01(distToSpaceObject / explosionRadius);

                            // D.log ("Radius damage to " + spaceObject.name + ": " + (damage * falloff));

                            Structure hitStructure = spaceObject.GetComponent<Structure>();
                            
                            if (hitStructure != null)
                            {
                                //Debug.Log("Hit");
                                hitStructures.Add(hitStructure);
                                hitStructure.takeDamage(hitStructure.gameObject, damage * falloff, weapon, (spaceObject.gameObject, null));
                            }
                        }
                    }
                }

                // add an explosive force around the detonation point (use the collider transform's y-value to prevent it going out of it's own plane)
                if (explosion != null)
                {
                    clonedExplosion = explosion.Spawn(transform);

                    boom = clonedExplosion.GetComponent<ExplosionVFXController>();

                    // make any changes to the explosion here (if any)

                    /// MAYBE PROBLEMATIC
                    boom.setSortingLayerOrder(this.transform);
                    ///

                    StartCoroutine(DelayedRecycle(boom.getDuration()));
                }
                else
                {
                    StartCoroutine(DelayedRecycle(5));
                }
            }
            else
            {
                StartCoroutine(DelayedRecycle(5));
            }
        }


        public override bool fire(IWeapon weapon)
        {
            this.weapon = weapon;
            
            weaponStructure = this.weapon.getStructure();
            
            if (myRenderer != null)
            {
                myRenderer.sortingLayerName = weaponStructure.StructureRenderer.sortingLayerName;
                myRenderer.sortingOrder = weaponStructure.StructureRenderer.sortingOrder - 2000;
            }

            if (spriteSheetController != null)
            {
                spriteSheetController.setSortingLayerOrder(this.transform);
                spriteSheetController.startVFX();
            }

            setInitialDirection(transform.rotation);

            if (projectileTrigger3D != null)
            {
                projectileTrigger3D.launch(weaponStructure);
            }
            myRigidbody.velocity = Vector2.up.Rotate(weapon.getStructure().transform.rotation.eulerAngles.z) * flightSpeed;

            gameObject.transform.SetParent(projectileParent);

            hasLaunched = true;

            return true;
        }

        public override void remove()
        {
            base.remove();

            currentFlightTime = FlightTimeToExplode;
        }

        IEnumerator DelayedRecycle(float delay)
        {
            // D.log("Projectile", "Missile recycler in: " + delay);

            yield return new WaitForSeconds(delay);

            disable();

            // D.log("Projectile", "Missile recycled");

            recycleImmediate();
        }
    }
}