using UnityEngine;
using System.Collections;

using NoxCore.Effects;
using NoxCore.Placeables;
using NoxCore.Utilities;

namespace NoxCore.Fittings.Weapons
{
    [RequireComponent(typeof(SplineMovement))]
    public class SplineProjectile : Projectile
    {
        // spline parameters
        public bool rootMotion;  
        protected SplineMovement splineMovement;

        // flight parameters
        protected Quaternion launchRotation;
        public Quaternion LaunchRotation { get { return launchRotation; } }        

        public bool hitFriendlies;
        public float speed;

        // cached components
        protected Explosion explosion;
        protected SpriteSheetVFXController spriteSheetController;

        public override void init()
        {
            base.init();

            myRenderer = GetComponent<SpriteRenderer>();
            myCollider = GetComponent<Collider2D>();

            Transform collider3D = transform.Find("Trigger3D");

            if (collider3D != null)
            {
                projectileTrigger3D = collider3D.GetComponent<ProjectileTrigger3D>();
                projectileTrigger3D.setup();
            }

            splineMovement = GetComponent<SplineMovement>();
            splineMovement.speed = speed;

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
            base.OnEnable();

            // D.log("Projectile", "Missile Enabled");

            armed = true;
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

        void OnTriggerEnter2D(Collider2D other)
        {
            if (Destroyed == false)
            {
                // D.log("Projectile", "Collided");

                // D.log ("Damage to " + other.collider.name + ": " + damage);

                Structure hitStructure = other.GetComponent<Structure>();

                if (hitStructure.Faction.ID != FactionID || hitFriendlies == true)
                {
                    hasCollided(hitStructure);
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

            launchRotation = Quaternion.AngleAxis(weaponStructure.transform.rotation.eulerAngles.z, Vector3.forward);

            if (spriteSheetController != null)
            {
                spriteSheetController.setSortingLayerOrder(weaponStructure.transform);
                spriteSheetController.startVFX();
            }

            splineMovement.initMovement();
            splineMovement.startMovement();

            if (rootMotion)
            {
                splineMovement.root = weapon.getFirePoint();
            }

            hasLaunched = true;

            return true;
        }
    }
}