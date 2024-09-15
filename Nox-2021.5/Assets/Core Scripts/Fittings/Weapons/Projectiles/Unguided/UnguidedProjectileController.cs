using UnityEngine;

using NoxCore.Placeables;
using NoxCore.Utilities;

namespace NoxCore.Fittings.Weapons
{
    public class UnguidedProjectileController : UnguidedProjectile
    {
        // flight parameters
        [Header("Specific Settings")]
        [ShowOnly]
        public float currentFlightTime;
        public float maxFlightTime;

        // cached components
        protected Explosion explosion;

        public override void init()
        {
            base.init();

            explosion = GetComponent<Explosion>();

            if (explosion != null)
            {
                explosion.init();
            }

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

        void Update()
        {
            if (hasLaunched == true && Destroyed == false)
            {
                base.update();

                flightStatus();

                if (Destroyed == true) return;

                armStatus();
            }
        }

        protected virtual float causeDamage(NoxObject collidedObject = null)
        {
            IDamagable damagableObject = collidedObject as IDamagable;

            float damage = weapon.getDamage();

            if (damagableObject != null)
            {
                // trigger in-game effect
                damagableObject.takeDamage(collidedObject.gameObject, damage, weapon, null, this);
            }

            return damage;
        }

        protected virtual float explode(float damage)
        {
            // add an explosive force and effect around the detonation point (use the collider transform's y-value to prevent it going out of it's own plane)
            if (explosion != null)
            {
                explosion.radialExplosion(damage, weapon);

                return explosion.detonate();
            }

            return 0;
        }

        public override void hasCollided(NoxObject collidedObject = null)
        {
            Destroyed = true;

            if (armed == true)
            {
                float damage = causeDamage(collidedObject);

                float explosionDuration = explode(damage);

                if (explosionDuration > 0)
                {
                    recycleDelayed(explosionDuration);
                    return;
                }
            }

            recycleImmediate();
        }

        public override void remove()
        {
            base.remove();

            currentFlightTime = maxFlightTime;
        }

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (Destroyed == false)
            {
                // D.log("Projectile", "Missile Collided");

                // D.log ("Damage to " + other.collider.name + ": " + damage);

                // hit a structure or a structure's shield?
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
    }
}