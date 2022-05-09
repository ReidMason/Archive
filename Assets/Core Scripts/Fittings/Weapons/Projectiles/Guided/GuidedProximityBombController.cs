using UnityEngine;

using NoxCore.Effects;
using NoxCore.Placeables;
using NoxCore.Utilities;

namespace NoxCore.Fittings.Weapons
{
    public class GuidedProximityBombController : GuidedProjectileController
    {
        // cached components
        protected SpriteSheetVFXController spriteSheetController;

        public override void init()
        {
            base.init();

            spriteSheetController = GetComponent<SpriteSheetVFXController>();

            if (spriteSheetController != null)
            {
                spriteSheetController.setupVFX();
            }
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

                // D.log("Projectile", "Bomb Disabled");
            }
        }

        protected override void flightStatus()
        {
            increaseFlightTime();

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
            }
        }

        void Update()
        {
            if (hasLaunched == true && Destroyed == false)
            {
                base.update();

                flightStatus();

                if (Destroyed == true) return;

                armStatus();

                if (lockedTarget != null)
                {
                    targetCollisionStatus();
                }
                else
                {
                    debrisCollisionStatus();

                    if (Destroyed == true)
                    {
                        if (myRenderer != null)
                        {
                            myRenderer.enabled = false;
                        }

                        if (spriteSheetController != null)
                        {
                            spriteSheetController.stopVFX();
                        }

                        recycleImmediate();
                    }
                }

                if (lockedTarget != null)
                {
                    float distToTarget = Vector2.Distance(transform.position, lockedTarget.transform.position);

                    if (distToTarget <= proximity)
                    {
                        hasCollided(lockedStructure);
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
                float damage = causeDamage(collidedObject);

                Collider2D collider = null;

                // if hit an external fitting on the structure then pass the fitting's collider into the explode method
                if (Target != null)
                {
                    GameObject targetSystem = Target.GetValueOrDefault().system;

                    if (targetSystem != null)
                    {
                        collider = targetSystem.GetComponent<Collider2D>();
                    }
                    else if (collidedObject != null)
                    {
                        collider = collidedObject.GetComponent<Collider2D>();
                    }
                }

                float explosionDuration = explode(damage, collider);

                if (explosionDuration > 0)
                {
                    recycleDelayed(explosionDuration);
                    return;
                }
            }

            recycleImmediate();
        }

        public override bool fire(IWeapon weapon)
        {
            base.fire(weapon);

            if (spriteSheetController != null)
            {
                spriteSheetController.setSortingLayerOrder(weaponStructure.transform);
                spriteSheetController.startVFX();
            }

            return true;
        }
    }
}