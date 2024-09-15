using UnityEngine;
using System.Collections;

using NoxCore.Effects;
using NoxCore.Fittings.Modules;
using NoxCore.Placeables;
using NoxCore.Utilities;

namespace NoxCore.Fittings.Weapons
{
    [RequireComponent(typeof(Explosion))]
    public class UnguidedProximityBomb : UnguidedProjectile
    {
        // flight parameters
        [Header("Specific Settings")]
        [ShowOnly]
        public float currentFlightTime;
        public float maxFlightTime;
        public float proximity;

        // cached components
        protected Explosion explosion;
        protected SpriteSheetVFXController spriteSheetController;

        public override void init()
        {
            base.init();

            explosion = GetComponent<Explosion>();

            if (explosion != null)
            {
                explosion.init();
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
            // D.log("Projectile", "Missile Enabled");

            if (initialised == false) init();

            base.OnEnable();

            currentFlightTime = 0;
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

                // D.log("Projectile", "Missile Disabled");
            }
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

                if (currentFlightTime >= maxFlightTime)
                {
                    Destroyed = true;

                    if (myRenderer != null)
                    {
                        myRenderer.enabled = false;
                    }

                    if (spriteSheetController != null)
                    {
                        spriteSheetController.stopVFX();
                    }

                    recycleImmediate();
                    return;
                }
                /*
                Destroyed = true;

                if (myRenderer != null)
                {
                    myRenderer.enabled = false;
                }

                if (spriteSheetController != null)
                {
                    spriteSheetController.stopVFX();
                }

                recycleImmediate();
                */
            }
        }

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (Destroyed == false)
            {
                // D.log("Projectile", "Missile Collided");

                // D.log ("Damage to " + other.collider.name + ": " + damage);

                Structure hitStructure = other.GetComponent<Structure>();

                if (hitStructure != null)
                {
                    // ignore trigger weapon not armed and target is massive or larger and use proximty to central target
                    if (hitStructure.structureSize > StructureSize.LARGE && armed == false) return;

                    if (hitStructure.Faction.ID != FactionID || hitFriendlies == true)
                    {
                        hasCollided(hitStructure);
                    }
                }
            }
        }

        public override void hasCollided(NoxObject collidedObject = null)
        {
            Destroyed = true;

            if (spriteSheetController != null)
            {
                spriteSheetController.stopVFX();
            }

            if (armed == true)
            {
                IDamagable damagableObject = collidedObject as IDamagable;

                float damage = weapon.getDamage();

                if (damagableObject != null)
                {
                    // trigger in-game effect
                    damagableObject.takeDamage(collidedObject.gameObject, damage, weapon, null, this);
                }

                // add an explosive force and effect around the detonation point (use the collider transform's y-value to prevent it going out of it's own plane)
                if (explosion != null)
                {
                    explosion.radialExplosion(damage, weapon);

                    float explosionDuration = explosion.detonate();

                    recycleDelayed(explosionDuration);
                }
                else
                {
                    recycleImmediate();
                }
            }
            else
            {
                recycleImmediate();
            }
        }

        public override bool fire(IWeapon weapon)
        {
            base.fire(weapon);

            if (spriteSheetController != null)
            {
                spriteSheetController.setSortingLayerOrder(weaponStructure.transform);
                spriteSheetController.startVFX();
            }

            myRigidbody.velocity = weapon.getSocket().transform.up * flightSpeed;

            return true;
        }

        public override void remove()
        {
            base.remove();

            currentFlightTime = maxFlightTime;
        }
    }
}