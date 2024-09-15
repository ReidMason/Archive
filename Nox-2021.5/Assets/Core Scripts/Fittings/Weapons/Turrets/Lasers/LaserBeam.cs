using UnityEngine;

using NoxCore.Effects;
using NoxCore.Placeables;

namespace NoxCore.Fittings.Weapons
{
    public class LaserBeam : Projectile, ITargetable
    {
        protected (GameObject structure, GameObject system)? target;
        public (GameObject structure, GameObject system)? Target { get { return target; } set { target = value; } }

        public GameObject lockedTarget;
        protected LineRenderer myLineRenderer;
        protected BeamVFXController beamController;

        public override void init()
        {
            base.init();

            myLineRenderer = GetComponent<LineRenderer>();

            beamController = GetComponent<BeamVFXController>();

            if (beamController != null)
            {
                beamController.setupVFX();
            }

            initialised = true;
        }

        // Use this for initialization
        protected override void OnEnable()
        {
            base.OnEnable();

            // D.log("Projectile", "Laser Beam Enabled");

            if (initialised == false) init();

            if (myLineRenderer != null)
            {
                myLineRenderer.enabled = true;
            }

            hasLaunched = false;
        }

        protected override void disable()
        {
            base.disable();

            if (myLineRenderer != null)
            {
                myLineRenderer.enabled = false;
            }

            if (beamController != null && beamController.isRunning == true)
            {
                beamController.stopVFX();
            }
        }

        // Update is called once per frame
        void Update()
        {
            base.update();

            if (myLineRenderer.enabled == true)
            {
                beamController.updateVFX();
            }
        }

        public override void hasCollided(NoxObject collidedObject = null)
        {
            IDamagable damagableObject = collidedObject as IDamagable;

            float damage = weapon.getDamage();

            if (damagableObject != null)
            {
                // trigger in-game effect
                damagableObject.takeDamage(collidedObject.gameObject, damage, weapon as Weapon, target, this);
            }
        }

        public override bool fire(IWeapon weapon)
        {
            this.weapon = weapon;

            TargetableWeapon targetableWeapon = weapon as TargetableWeapon;

            weaponStructure = targetableWeapon.getStructure();

            if (myLineRenderer != null)
            {
                myLineRenderer.sortingLayerName = weaponStructure.StructureRenderer.sortingLayerName;
                myLineRenderer.sortingOrder = weaponStructure.StructureRenderer.sortingOrder + 2;
            }

            if (beamController != null)
            {
                beamController.setSortingLayerOrder(weaponStructure.transform);
                beamController.startVFX();
            }

            Target = targetableWeapon.Target;

            if (Target != null)
            {
                GameObject targetStructure = Target.GetValueOrDefault().structure;
                GameObject targetSystem = Target.GetValueOrDefault().system;

                if (targetSystem == null)
                {
                    lockedTarget = targetSystem;
                }
                else
                {
                    lockedTarget = targetStructure;
                }
            }

            gameObject.transform.SetParent(projectileParent);

            D.log("Projectile", "Beam fired. Target: " + lockedTarget.name);

            return true;
        }
    }
}
