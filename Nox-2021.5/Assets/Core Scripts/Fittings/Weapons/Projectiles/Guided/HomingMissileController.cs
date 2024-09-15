using UnityEngine;

using NoxCore.Effects;
using NoxCore.Placeables;
using NoxCore.Utilities;

namespace NoxCore.Fittings.Weapons
{
    [RequireComponent(typeof(SteeringProjectileMovement))]
    public class HomingMissileController : GuidedProjectileController
    {
        // cached components
        protected ExhaustVFXController exhaustVFXController;
        protected float exhaustLifespan;
        protected Transform nozzleT;

        public override void init()
        {
            base.init();

            nozzleT = transform.Find("Nozzle");

            if (nozzleT != null)
            {
                Transform exhaustT = nozzleT.FindChildStartsWith("Missile Exhaust");

                if (exhaustT != null)
                {
                    exhaustVFXController = exhaustT.GetComponent<ExhaustVFXController>();

                    if (exhaustVFXController != null)
                    {
                        exhaustVFXController.setupVFX(transform, 2);
                    }
                }
            }
        }

        protected override void disable()
        {
            if (disabled == false)
            {
                base.disable();

                if (exhaustVFXController != null && exhaustVFXController.isRunning == true)
                {
                    exhaustVFXController.stopVFX();
                }
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
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (hasLaunched == true && Destroyed == false)
            {
                base.update();

                flightStatus();

                armStatus();

                if (currentFlightTime >= maxFlightTime)
                {
                    if (exhaustVFXController != null)
                    {
                        exhaustVFXController.stopVFX();
                    }

                    recycleDelayed(exhaustLifespan);
                    return;
                }

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

                        if (exhaustVFXController != null)
                        {
                            exhaustVFXController.stopVFX();
                        }

                        recycleDelayed(exhaustLifespan);
                    }
                }
            }
        }

        public override void hasCollided(NoxObject collidedObject = null)
        {
            Destroyed = true;

            if (exhaustVFXController != null && exhaustVFXController.isRunning == true)
            {
                exhaustVFXController.stopVFX();
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
                    recycleDelayed(explosionDuration + exhaustLifespan);
                    return;
                }
            }

            if (exhaustLifespan > 0)
            {
                recycleDelayed(exhaustLifespan);
            }
            else
            {
                recycleImmediate();
            }
        }

        public override bool fire(IWeapon weapon)
        {
            base.fire(weapon);

            if (exhaustVFXController != null)
            {
                exhaustVFXController.setSortingLayerOrder(weaponStructure.transform, 2);
                exhaustVFXController.startVFX();
                exhaustLifespan = exhaustVFXController.getMaxLifespan();
            }

            return true;
        }
    }
}