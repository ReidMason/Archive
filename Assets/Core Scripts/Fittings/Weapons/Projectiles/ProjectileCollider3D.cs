using UnityEngine;

using NoxCore.Placeables;

namespace NoxCore.Fittings.Weapons
{
    public class ProjectileCollider3D : MonoBehaviour
    {
        protected Projectile projectile;
        protected Collider myCollider;

        protected (GameObject structure, GameObject system) target;

        void Awake()
        {
            projectile = transform.parent.GetComponent<Projectile>();
            myCollider = GetComponent<Collider>();
        }

        public void enable()
        {
            if (myCollider != null)
            {
                myCollider.enabled = true;               
            }
        }

        public void disable(Structure launcherStructure)
        {
            if (myCollider != null)
            {
                ignoreColliders(myCollider, launcherStructure.gameObject, false);
                //Physics.IgnoreCollision(myCollider, launcherStructure.ShieldCollider, false);
                myCollider.enabled = false;
            }
        }

        public void launch(Structure launcherStructure)
        {
            ignoreColliders(myCollider, launcherStructure.gameObject, true);
            //Physics.IgnoreCollision(myCollider, launcherStructure.ShieldCollider);

            ITargetable targetableProjectile = projectile as ITargetable;

            if (targetableProjectile != null)
            {
                target = targetableProjectile.Target.GetValueOrDefault();
            }
        }

        protected void ignoreColliders(Collider projectileCollider, GameObject structureGO, bool enable)
        {
            Collider[] colliders = structureGO.GetComponentsInChildren<Collider>();

            for (int i = 0; i < colliders.Length; i++)
            {
                Physics.IgnoreCollision(projectileCollider, colliders[i], enable);
            }
        }

        void OnCollisionEnter(Collision other)
        {
            if (projectile != null && projectile.Destroyed == false)
            {
                if (projectile.DirectOrdnance == true)
                {
                    if (target.system != null && other.gameObject != target.system)
                    {
                        return;
                    }
                    else if (other.gameObject != target.structure)
                    {
                        return;
                    }
                }

                // D.log("Projectile", "Missile Collided");

                // D.log ("Damage to " + other.collider.name + ": " + damage);

                // hit a structure or a structure's shield?
                Structure hitStructure = null;

                if (other.collider.tag == "Shield")
                {
                    ITargetable targetableProjectile = projectile as ITargetable;

                    if (targetableProjectile != null)
                    {
                        if (targetableProjectile.Target.GetValueOrDefault().structure == other.transform.parent.gameObject)
                        {
                            hitStructure = other.transform.parent.GetComponent<Structure>();
                            projectile.hasCollided(hitStructure);
                        }
                    }
                    else
                    {
                        hitStructure = other.transform.parent.GetComponent<Structure>();
                        projectile.hasCollided(hitStructure);
                    }
                }
            }
        }
    }
}