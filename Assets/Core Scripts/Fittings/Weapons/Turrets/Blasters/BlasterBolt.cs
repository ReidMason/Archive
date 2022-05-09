using UnityEngine;

using NoxCore.Effects;
using NoxCore.Placeables;
using NoxCore.Utilities;

namespace NoxCore.Fittings.Weapons
{ 
    public class BlasterBolt : GuidedProjectile
    {
        // flight parameters
        [Header("Specific Settings")]
        public float proximity;

        [ShowOnly]
        public float distanceTravelled;

        [ShowOnly]
        public float maxRange;

        protected bool spawnedInsideTarget;

        // cached components
        protected BoltVFXController boltController;

        public override void init()
        {
            base.init();

            boltController = GetComponent<BoltVFXController>();

            if (boltController != null)
            {
                boltController.setupVFX();
            }

            initialised = true;
        }

        // Use this for initialization
        protected override void OnEnable()
        {
            // D.log("Projectile", "Missile Enabled");

            if (initialised == false) init();

            base.OnEnable();

            distanceTravelled = 0;
            armed = false;
        }

        protected override void disable()
        {
            if (disabled == false)
            {
                base.disable();

                if (boltController != null && boltController.isRunning == true)
                {
                    boltController.stopVFX();
                }

                disabled = true;
                armed = false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (hasLaunched == true && Destroyed == false)
            {
                // increase distance travelled
                distanceTravelled += (Time.deltaTime * myRigidbody.velocity.magnitude);

                if (distanceTravelled > maxRange)
                {
                    Destroyed = true;

                    if (myRenderer != null)
                    {
                        myRenderer.enabled = false;
                    }

                    if (boltController != null)
                    {
                        boltController.stopVFX();
                    }

                    recycleImmediate();
                    return;
                }

                armed = true;

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

            if (boltController != null)
            {
                boltController.stopVFX();
            }

            IDamagable damagableObject = collidedObject as IDamagable;

            float damage = weapon.getDamage();

            if (damagableObject != null)
            {
                // trigger in-game effect
                damagableObject.takeDamage(collidedObject.gameObject, damage, weapon as Weapon, target, this);
            }

            recycleImmediate();
        }

        public override bool fire(IWeapon weapon)
        {
            base.fire(weapon);

            TargetableWeapon targetableWeapon = weapon as TargetableWeapon;

            maxRange = targetableWeapon.WeaponData.MaxRange;

            if (boltController != null)
            {
                boltController.setSortingLayerOrder(weaponStructure.transform);
                boltController.startVFX();
            }

            transform.position = weapon.getFirePoint().position;

            target = targetableWeapon.Target.GetValueOrDefault();

            transform.LookAt2D(lockedTarget.transform.position, 90.0f);
            setInitialDirection(transform.rotation);

            myRigidbody.velocity = transform.up * flightSpeed;

            boltController.setInitialBearing(Mathf.Atan2(-myRigidbody.velocity.y, myRigidbody.velocity.x) + (Mathf.PI/2.0f));

            D.log("Projectile", "Bolt fired. Target: " + lockedTarget.name);

            return true;
        }
    }
}
