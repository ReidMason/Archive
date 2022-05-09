using UnityEngine;
using System.Collections;

using NoxCore.Effects;
using NoxCore.Placeables;
using NoxCore.Utilities;

namespace NoxCore.Fittings.Weapons
{
    [RequireComponent(typeof(Explosion))]
    public class GuidedProximityBomb : GuidedProjectile
    {
        // flight parameters
        [Header("Specific Settings")]
        [ShowOnly]
        public float currentFlightTime;
        public float maxFlightTime;
        public float proximity;

        // movement
        public float rotationSpeed;
        protected Quaternion lookRotation;
        protected Vector3 direction;
        protected Vector3 initVelocity;

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

                armed = false;

                // D.log("Projectile", "Missile Disabled");
            }
        }

        public Vector2 correctVelocity(Vector2 currentVelocity, Vector2 requestedVelocity, float maxTurn)
        {
            float Bearing = (Mathf.Atan2(-currentVelocity.y, currentVelocity.x) * Mathf.Rad2Deg) + 90;

            if (Bearing < 0) Bearing += 360;

            float newBearing;

            if (isTurningLeft(currentVelocity, requestedVelocity) == true)
            {
                newBearing = (Mathf.Deg2Rad * Bearing) - maxTurn;
                // Debug.Log ("Turning left - current bearing: " + Bearing + "   new bearing: " + newBearing);
            }
            else
            {
                newBearing = (Mathf.Deg2Rad * Bearing) + maxTurn;
                // Debug.Log ("Turning right - current bearing: " + Bearing + "   new bearing: " + newBearing);
            }

            return new Vector2(requestedVelocity.magnitude * Mathf.Sin(newBearing), requestedVelocity.magnitude * Mathf.Cos(newBearing));
        }

        public bool isTurningLeft(Vector2 currentVelocity, Vector2 newVelocity)
        {
            if (currentVelocity.x * newVelocity.y - currentVelocity.y * newVelocity.x > 0) return true;
            else return false;
        }

        void Update()
        {
            if (hasLaunched == true && Destroyed == false)
            {
                base.update();

                float Bearing = (Mathf.Atan2(-myRigidbody.velocity.y, myRigidbody.velocity.x) * Mathf.Rad2Deg) + 90;

                if (Bearing < 0) Bearing += 360;

                //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0, 0, -Bearing), 4 * Time.deltaTime);
                transform.rotation = Quaternion.Euler(0, 0, -Bearing);

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
                    float distToTarget = Vector2.Distance(transform.position, lastKnownPosition);

                    if (distToTarget <= proximity)
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
            }
        }

        void FixedUpdate()
        {
            if (Destroyed == false)
            {
                Vector2 steeringVector;

                if (lockedTarget != null)
                {
                    steeringVector = lockedTarget.transform.position - transform.position;
                }
                else
                {
                    steeringVector = lastKnownPosition - (Vector2)(transform.position);
                }

                Vector2 newVelocity = steeringVector.normalized * flightSpeed;
                Vector2 steeringForce = newVelocity - myRigidbody.velocity;

                Vector2 acceleration = steeringForce / myRigidbody.mass;

                // Debug.Log ("Acceleration: " + acceleration + "   mag: " + acceleration.magnitude);

                // get desired change in velocity
                newVelocity = myRigidbody.velocity + (acceleration * Time.deltaTime);

                // Debug.Log ("Pre Velocity: " + newVelocity.magnitude);

                // correct force based on maximum turning rate
                if (newVelocity.magnitude > 0)
                {
                    float maxTurn = rotationSpeed * Time.deltaTime;

                    float angleToDestination = Vector2.Angle(newVelocity, myRigidbody.velocity);

                    if (angleToDestination * Mathf.Deg2Rad > maxTurn)
                    {
                        // Debug.Log ("Turn too great. Angle: " + angleToDestination * Mathf.Rad2Deg + "   Max possible: " + maxTurn * Mathf.Rad2Deg);
                        // Debug.Log ("Current velocity: " + transform.rigidbody.velocity + "   requested velocity: " + newVelocity);				
                        newVelocity = correctVelocity(myRigidbody.velocity, newVelocity, maxTurn);
                        // Debug.Log ("Corrected velocity: " + newVelocity);	
                    }
                }

                // limit ship speed to maximum velocity
                if (newVelocity.magnitude > flightSpeed)
                {
                    // Debug.Log ("Velocity too high: " + newVelocity + "   mag: " + newVelocity.magnitude);
                    newVelocity = newVelocity.normalized * flightSpeed;
                    // Debug.Log ("Velocity reduced to: " + newVelocity + "   mag: " + newVelocity.magnitude);
                }

                // set new velocity
                myRigidbody.velocity = newVelocity;
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
                    damagableObject.takeDamage(collidedObject.gameObject, damage, weapon, target, this);
                }

                // add an explosive force and effect around the detonation point (use the collider transform's y-value to prevent it going out of it's own plane)
                if (explosion != null)
                {
                    explosion.radialExplosion(damage, weapon);

                    float explosionDuration = explosion.detonate(lockedTarget != null ? lockedTarget.GetComponent<Collider2D>() : null);

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

            return true;
        }

        public override void remove()
        {
            base.remove();

            currentFlightTime = maxFlightTime;
        }
    }
}