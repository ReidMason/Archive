using UnityEngine;

using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Placeables;

namespace NoxCore.Fittings.Weapons
{
    public abstract class GuidedProjectile : Projectile, ITargetable
    {
        // target
        protected (GameObject structure, GameObject system)? target;
        public (GameObject structure, GameObject system)? Target { get { return target; } set { target = value; } }

        protected Structure lockedStructure;
        public GameObject lockedTarget;
        protected Vector2 lastKnownPosition;

        // Use this for initialization
        protected override void OnEnable()
        {
            // D.log("Projectile", "Missile Enabled");

            base.OnEnable();

            armed = false;
        }

        protected override void disable()
        {
            if (disabled == false)
            {
                base.disable();

                armed = false;

                // D.log("Projectile", "Missile Disabled");
            }
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
        {
            if (Destroyed == false)
            {
                if (DirectOrdnance == true)
                {
                    if (target != null)
                    {
                        (GameObject structure, GameObject system) projectileTarget = Target.GetValueOrDefault();

                        if (projectileTarget.system != null && other.gameObject != projectileTarget.system)
                        {
                            return;
                        }
                        else if (projectileTarget.system == null && other.gameObject != projectileTarget.structure)
                        {
                            return;
                        }
                    }
                }

                // D.log("Projectile", "Missile Collided");

                // D.log ("Damage to " + other.collider.name + ": " + damage);

                Structure hitStructure = other.GetComponent<Structure>();

                if (hitStructure == null)
                {
                    // must have hit a module
                    hitStructure = other.GetComponent<IModule>().getStructure();

                    // ignore trigger weapon and use proximty to target's central location
                    if (hitStructure.structureSize > StructureSize.LARGE && armed == false) return;
                }

                hasCollided(hitStructure);
            }
        }
   
        public override bool fire(IWeapon weapon)
        {
            base.fire(weapon);

            StructureSocket socket;

            TargetableWeapon targetableWeapon = weapon as TargetableWeapon;

            if (targetableWeapon != null)
            {
                Target = targetableWeapon.Target;
                socket = targetableWeapon.Socket;
            }
            else
            {
                Target = weapon.FireGroup.Target;
                socket = weapon.getSocket();
            }

            setInitialDirection(socket.transform.rotation);
            myRigidbody.velocity = socket.transform.up * flightSpeed;

            (GameObject structure, GameObject system) projectileTarget = Target.GetValueOrDefault();

            lockedStructure = projectileTarget.structure.GetComponent<Structure>();

            if (lockedStructure != null)
            {
                lockedStructure.Despawn += Projectile_LostTarget;
            }

            if (projectileTarget.system == null)
            {
                lockedTarget = projectileTarget.structure;
                D.log("Projectile", "Missile launched. Target: " + lockedTarget.name);
            }
            else
            {
                lockedTarget = projectileTarget.system;
                D.log("Projectile", "Missile launched. Target: " + projectileTarget.structure.name + " - " + lockedTarget.name);
            }

            return true;
        }

        public virtual void Projectile_LostTarget(object sender, DespawnEventArgs args)
        {
            if (lockedStructure != null)
            {
                lockedStructure.Despawn -= Projectile_LostTarget;
            }

            if (lockedTarget != null)
            {
                lastKnownPosition = lockedTarget.transform.position;
            }

            lockedTarget = null;
            lockedStructure = null;
        }
    }
}