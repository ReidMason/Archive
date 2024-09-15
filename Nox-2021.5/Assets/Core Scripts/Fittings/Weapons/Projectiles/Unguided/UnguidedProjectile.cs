using UnityEngine;
using System.Collections;

using NoxCore.Placeables;

namespace NoxCore.Fittings.Weapons
{
    public abstract class UnguidedProjectile : Projectile
    {
        // flight parameters
        protected Quaternion launchRotation;
        public Quaternion LaunchRotation { get { return launchRotation; } }

        public bool hitFriendlies;

        // Use this for initialization
        protected override void OnEnable()
        {
            // D.log("Projectile", "Missile Enabled");

            base.OnEnable();

            armed = true;
        }

        protected virtual void OnTriggerEnter2D(Collider2D other)
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

        public override bool fire(IWeapon weapon)
        {
            base.fire(weapon);

            launchRotation = Quaternion.AngleAxis(weaponStructure.transform.rotation.eulerAngles.z, Vector3.forward);

            return true;
        }
    }
}
