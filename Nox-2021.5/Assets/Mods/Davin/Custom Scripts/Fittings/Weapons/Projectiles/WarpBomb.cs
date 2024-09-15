using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using NoxCore.Fittings.Weapons;
using NoxCore.Placeables;
using NoxCore.Effects;
using NoxCore.Utilities;

namespace Davin.Fittings.Weapons
{
    public class WarpBomb : Projectile
    {
        /*
        Swaps the user with the bomb before exploding, explodes at the previous position of the user
        */
        public float proximity;

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

        public override void init()
        {
            base.init();

            myRenderer = GetComponent<SpriteRenderer>();
            myRigidbody = GetComponent<Rigidbody2D>();
            myCollider = GetComponent<Collider2D>();

            Transform collider3D = transform.Find("Trigger3D");

            if (collider3D != null)
            {
                projectileTrigger3D = collider3D.GetComponent<ProjectileTrigger3D>();
                projectileTrigger3D.setup();
            }

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

        protected void recycle()
        {
            gameObject.Recycle();
        }

        // Update is called once per frame
        void Update()
        {
            if (hasLaunched == true && Destroyed == false)
            {
                base.update();
                
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
        }


        public void explode()
        {
            Destroyed = true;
            //swap process
            Vector3 tempPos = this.weapon.getStructure().transform.position;
            this.weapon.getStructure().transform.position = this.transform.position;
            this.transform.position = tempPos;

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
                            if (spaceObject.GetComponent<Rigidbody2D>() != null)
                            {
                                spaceObject.GetComponent<Rigidbody2D>().AddExplosionForce(explosionForce, transform.position, explosionRadius);
                            }

                            float distToSpaceObject = Vector2.Distance(transform.position, spaceObject.transform.position);
                            float falloff = 1.0f - Mathf.Clamp01(distToSpaceObject / explosionRadius);

                            // D.log ("Radius damage to " + spaceObject.name + ": " + (damage * falloff));

                            Structure hitStructure = spaceObject.GetComponent<Structure>();

                            if (hitStructure != null)
                            {
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
                    recycle();
                }
            }
            else
            {
                StartCoroutine(DelayedRecycle(0));
            }
        }


        public override bool fire(IWeapon weapon)
        {
            this.weapon = weapon;
            Structure workingStructure = this.weapon.getStructure();
            //Physics2D.IgnoreCollision(myCollider, this.weapon.getStructure().StructureCollider);

            if (myRenderer != null)
            {
                myRenderer.sortingLayerName = workingStructure.StructureRenderer.sortingLayerName;
                myRenderer.sortingOrder = workingStructure.StructureRenderer.sortingOrder + 2;
            }

            if (spriteSheetController != null)
            {
                spriteSheetController.setSortingLayerOrder(this.transform);
                spriteSheetController.startVFX();
            }

            setInitialDirection(transform.rotation);           

            if (projectileTrigger3D != null)
            {
                projectileTrigger3D.launch(workingStructure);
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

            disable();

            yield return new WaitForSeconds(delay);

            // D.log("Projectile", "Missile recycled");

            recycle();
        }

        public override void hasCollided(NoxObject collidedObject = null)
        {
            //we dont use this
        }
                
    }   
}