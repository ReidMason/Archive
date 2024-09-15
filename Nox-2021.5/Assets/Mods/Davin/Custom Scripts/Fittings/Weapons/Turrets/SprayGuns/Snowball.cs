using UnityEngine;

using NoxCore.Effects;
using NoxCore.Fittings.Weapons;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Utilities;

namespace Davin.Fittings.Weapons
{
    public class Snowball : GuidedProjectile
    {
        // flight parameters
        [Header("Specific Settings")]
        public float proximity;

        [ShowOnly]
        public float distanceTravelled;

        [ShowOnly]
        public float maxRange;

        public Color32 __colour;
        public float __minDropRadius;
        public float __maxDropRadius;
        public float __spread;

        public float impactForce;

        protected bool spawnedInsideTarget;

        private ParticleSystem m_Particle;
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

            m_Particle = GetComponent<ParticleSystem>();
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

                /* perform weapon effect (e.g. damage target structure/module/weapon or other effect) */
                Structure hitStructure = collidedObject as Structure;

                if (hitStructure != null)
                {
                    SpriteRenderer targetRenderer = hitStructure.GetComponent<SpriteRenderer>();

                    Texture2D targetTexture = targetRenderer.sprite.texture;

                    if (targetTexture == null)
                    {
                        targetRenderer.material.mainTexture = targetTexture;
                    }

                    float pixelsPerUnit = targetRenderer.sprite.pixelsPerUnit;
                    string origName = targetRenderer.sprite.name;

                    Texture2D newTexture = Instantiate(targetRenderer.sprite.texture) as Texture2D;

                    Vector2 randomCentre = new Vector2(UnityEngine.Random.Range(0, newTexture.width), UnityEngine.Random.Range(0, newTexture.height));

                    int randomDropRadius = (int)(UnityEngine.Random.Range(__minDropRadius, __maxDropRadius));

                    int numMipMaps = targetRenderer.sprite.texture.mipmapCount;

                    for (int mipMapLevel = 1; mipMapLevel <= numMipMaps; mipMapLevel++)
                    {
                        Color32[] colouredPixels = newTexture.GetPixels32(mipMapLevel - 1);

                        float scaling = Mathf.Pow(2, mipMapLevel - 1);

                        int width = (int)((float)newTexture.width / scaling);
                        int height = (int)((float)newTexture.height / scaling);

                        int dropRadius = (int)(randomDropRadius / scaling);
                        int dropRadiusSqr = dropRadius * dropRadius;

                        Vector2 dropCentre = randomCentre / scaling;

                        int xStart = Mathf.Max(0, (int)(dropCentre.x - dropRadius));
                        int xEnd = Mathf.Min(width, (int)(dropCentre.x + dropRadius));
                        int yStart = Mathf.Max(0, (int)(dropCentre.y - dropRadius));
                        int yEnd = Mathf.Min(height, (int)(dropCentre.y + dropRadius));

                        for (int x = xStart; x < xEnd; x++)
                        {
                            int xDiff = (int)(x - dropCentre.x);

                            for (int y = yStart; y < yEnd; y++)
                            {
                                int yDiff = (int)(y - dropCentre.y);

                                float magSqr = (xDiff * xDiff) + (yDiff * yDiff);

                                if (magSqr <= dropRadiusSqr)
                                {
                                    int index = x + (y * width);

                                    // blob visual effect
                                    float strength = 1 - (magSqr / dropRadiusSqr);

                                    // bubble visual effect
                                    // float strength = (magSqr / dropRadiusSqr);

                                    if (colouredPixels[index].a > 0)
                                    {
                                        colouredPixels[index] = Color32.Lerp(colouredPixels[index], __colour, strength);
                                    }
                                }
                            }
                        }

                        newTexture.SetPixels32(colouredPixels, mipMapLevel - 1);
                    }

                    newTexture.Apply(false);

                    targetRenderer.sprite = Sprite.Create(newTexture, targetRenderer.sprite.rect, new Vector2(0.5f, 0.5f), pixelsPerUnit);
                    targetRenderer.sprite.name = origName;
                    targetRenderer.material.mainTexture = newTexture;

                    // impact
                    Ship hitShip = hitStructure as Ship;

                    if (hitShip != null)
                    {
                        Vector2 direction = (hitShip.transform.position - transform.position).normalized;

                        hitShip.addDelayedForce(direction * impactForce);
                    }
                }
            }

            recycleImmediate();
        }

        public override bool fire(IWeapon weapon)
        {
            base.fire(weapon);

            m_Particle.startColor = __colour;
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
            setInitialDirection(Quaternion.Euler(0, 0, Random.Range(-__spread, __spread)) * transform.rotation);

            myRigidbody.velocity = transform.up * flightSpeed;

            boltController.setInitialBearing(Mathf.Atan2(-myRigidbody.velocity.y, myRigidbody.velocity.x) + (Mathf.PI / 2.0f));

            D.log("Projectile", "Bolt fired. Target: " + lockedTarget.name);

            return true;
        }
    }
}
