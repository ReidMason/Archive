using UnityEngine;

using NoxCore.Effects;
using NoxCore.Placeables;

namespace NoxCore.Fittings.Weapons
{
    [RequireComponent(typeof(Explosion))]
    [RequireComponent(typeof(SpriteSheetVFXController))]
    public class UnguidedProximityBombController : UnguidedProjectileController
    {
        // flight parameters
        [Header("Proximity Settings")]
        public float proximity;

        // cached components
        protected SpriteSheetVFXController spriteSheetController;

        public override void init()
        {
            base.init();

            CircleCollider2D myCollider = GetComponent<CircleCollider2D>();
            myCollider.radius = proximity;

            spriteSheetController = GetComponent<SpriteSheetVFXController>();

            if (spriteSheetController != null)
            {
                spriteSheetController.setupVFX();
            }

            initialised = true;
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

                flightStatus();

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

                    return;
                }

                armStatus();
            }
        }

        public override void hasCollided(NoxObject collidedObject = null)
        {
            base.hasCollided(collidedObject);

            if (spriteSheetController != null)
            {
                spriteSheetController.stopVFX();
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

            myRigidbody.velocity = transform.up * flightSpeed;

            return true;
        }

        public override void remove()
        {
            base.remove();

            currentFlightTime = maxFlightTime;
        }
    }
}