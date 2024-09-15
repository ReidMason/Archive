using UnityEngine;

using NoxCore.Effects;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Utilities;

namespace NoxCore.Fittings.Weapons
{
    public class Explosion : MonoBehaviour
    {
        public float explosionRadius;
        public float explosionForce;

        public GameObject explosion;
        protected GameObject clonedExplosion;
        protected ExplosionVFXController explosionVFXController;
        protected float boomDuration;

        public virtual void init()
        {}

        public void radialExplosion(float damage, IWeapon weapon, Vector2? position = null)
        {
            if (explosionRadius > 0)
            {
                Vector2 centre;

                if (position == null)
                {
                    centre = transform.position;
                }
                else
                {
                    centre = position.GetValueOrDefault();
                }

                // damage other ships/structure in explosion range (what about modules?)
                Collider2D[] hitColliders = Physics2D.OverlapCircleAll(centre, explosionRadius);

                foreach (Collider2D collidedObject in hitColliders)
                {
                    Rigidbody2D rb = collidedObject.GetComponent<Rigidbody2D>();

                    if (rb != null && explosionForce > 0)
                    {
                        Ship ship = collidedObject.GetComponent<Ship>();

                        if (ship)
                        {
                            ship.addDelayedForce(rb.CalculateExplosionForce(explosionForce, centre, explosionRadius));
                        }
                        else
                        {
                            rb.AddExplosionForce(explosionForce, centre, explosionRadius);
                        }
                    }

                    float distToSpaceObject = Vector2.Distance(centre, collidedObject.transform.position);
                    float falloff = 1.0f - Mathf.Clamp01(distToSpaceObject / explosionRadius);

                    // D.log ("Radius damage to " + spaceObject.name + ": " + (damage * falloff));

                    IDamagable damagableObject = collidedObject.GetComponent<Structure>();

                    if (damagableObject != null)
                    {
                        damagableObject.takeDamage(collidedObject.gameObject, damage * falloff, weapon, (collidedObject.gameObject, null));
                    }
                }
            }
        }

        public virtual float detonate(Collider2D collidedObject = null)
        {
            if (collidedObject != null)
            {
                clonedExplosion = explosion.Spawn(collidedObject.transform, new Vector3(0, 0, -1));
            }
            else
            {
                // TODO - note: this can fail if this transform resides in a higher prefab (not GameObject)
                clonedExplosion = explosion.Spawn(gameObject.transform, new Vector3(0, 0, -1));
            }

            explosionVFXController = clonedExplosion.GetComponent<ExplosionVFXController>();

            // make any changes to the explosion here (if any)
            if (collidedObject != null)
            {
                explosionVFXController.setSortingLayerOrder(collidedObject.transform);
            }
            else
            {
                explosionVFXController.setSortingLayerOrder(gameObject.transform);
            }

            return explosionVFXController.getDuration();
        }

        public virtual float detonate(Vector2 position, Collider2D collidedObject = null)
        {
            clonedExplosion = explosion.Spawn(GameManager.Instance.EffectsParent);

            clonedExplosion.transform.position = position;

            explosionVFXController = clonedExplosion.GetComponent<ExplosionVFXController>();

            // make any changes to the explosion here (if any)
            if (collidedObject != null)
            {
                explosionVFXController.setSortingLayerOrder(collidedObject.transform);
            }
            else
            {
                explosionVFXController.setSortingLayerOrder(GameManager.Instance.EffectsParent);
            }

            return explosionVFXController.getDuration();
        }
    }
}