using UnityEngine;

using NoxCore.Placeables;
using NoxCore.Utilities;

namespace NoxCore.Fittings.Weapons
{
    [RequireComponent(typeof(SteeringProjectileMovement))]
    public class GuidedProjectileController : GuidedProjectile
    {
        // flight parameters
        [Header("Specific Settings")]
        [ShowOnly]
        public float currentFlightTime;
        public float maxFlightTime;
        public float proximity;

        // cached components
        protected Explosion explosion;
        protected SteeringProjectileMovement projectileMovement;

        public override void init()
        {
            base.init();

            explosion = GetComponent<Explosion>();

            if (explosion != null)
            {
                explosion.init();
            }

            projectileMovement = GetComponent<SteeringProjectileMovement>();
            projectileMovement.init();

            initialised = true;
        }

        // Use this for initialization
        protected override void OnEnable()
        {
            // D.log("Projectile", "Bomb Enabled");

            if (initialised == false) init();

            base.OnEnable();

            currentFlightTime = 0;
        }

        protected override void disable()
        {
            if (disabled == false)
            {
                base.disable();

                projectileMovement.disable();

                // D.log("Projectile", "Bomb Disabled");
            }
        }

        protected virtual void increaseFlightTime()
        {
            // increase current flight time
            currentFlightTime += Time.deltaTime;
        }

        protected virtual void flightStatus()
        {
            increaseFlightTime();

            if (currentFlightTime >= maxFlightTime)
            {
                Destroyed = true;

                if (myRenderer != null)
                {
                    myRenderer.enabled = false;
                }

                recycleImmediate();
            }
        }

        protected virtual void armStatus()
        {
            // arm projectile after arming time reached
            if (armed == false)
            {
                if (currentFlightTime >= ArmingTime)
                {
                    arm();
                }
            }
        }

        protected virtual void targetCollisionStatus()
        {
            if (lockedTarget != null)
            {
                float distToTarget = Vector2.Distance(transform.position, lockedTarget.transform.position);

                if (distToTarget <= proximity)
                {
                    hasCollided(lockedStructure);
                }
            }
            else
            {
                float distToTarget = Vector2.Distance(transform.position, projectileMovement.TargetLastPosition);

                if (distToTarget <= proximity)
                {
                    Destroyed = true;

                    recycleImmediate();
                }
            }
        }

        protected virtual void debrisCollisionStatus()
        {
            float distToTarget = Vector2.Distance(transform.position, projectileMovement.TargetLastPosition);

            if (distToTarget <= proximity)
            {
                hasCollided();
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
                        recycleImmediate();
                    }
                }
            }
        }

        protected virtual float causeDamage(NoxObject collidedObject = null)
        {
            IDamagable damagableObject = collidedObject as IDamagable;

            float damage = weapon.getDamage();

            if (damagableObject != null)
            {
                // trigger in-game effect
                damagableObject.takeDamage(collidedObject.gameObject, damage, weapon, target, this);
            }

            return damage;
        }

        protected virtual float explode(float damage, Collider2D collider)
        {
            // add an explosive force and effect around the detonation point (use the collider transform's y-value to prevent it going out of it's own plane)
            if (explosion != null)
            {
                explosion.radialExplosion(damage, weapon);

                if (collider != null)
                {
                    return explosion.detonate(collider);
                }
                else
                {
                    return explosion.detonate();
                }
            }

            return 0;
        }

        public override void hasCollided(NoxObject collidedObject = null)
        {
            Destroyed = true;

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

            projectileMovement.LockedTarget = lockedTarget;
            projectileMovement.setInitVelocity(transform.up * flightSpeed);

            return true;
        }

        public override void remove()
        {
            base.remove();

            currentFlightTime = maxFlightTime;
        }

        public override void Projectile_LostTarget(object sender, DespawnEventArgs args)
        {
            base.Projectile_LostTarget(sender, args);

            projectileMovement.TargetLastPosition = lastKnownPosition;
            projectileMovement.LockedTarget = null;
        }
    }
}