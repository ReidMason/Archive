using UnityEngine;
using System.Collections;

using NoxCore.Fittings.Modules;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Utilities;

namespace NoxCore.Fittings.Weapons
{
    [RequireComponent(typeof(BuffManager))]
    public abstract class Projectile : MonoBehaviour, IProjectile
	{
        [Header("Standard Settings")]
        // weapon system
        protected Structure weaponStructure;
        public Structure WeaponStructure { get { return weaponStructure; } }

        protected IWeapon weapon;

        [SerializeField]
        protected bool _DirectOrdnance;
        public bool DirectOrdnance { get { return _DirectOrdnance; } set { _DirectOrdnance = value; } }

        protected int _FactionID;
        public int FactionID { get { return _FactionID; } set { _FactionID = value; } }

        [SerializeField]
		protected float _ArmingTime;
		public float ArmingTime { get { return _ArmingTime; } set { _ArmingTime = value; } }

        protected bool _Destroyed;
        public bool Destroyed { get { return _Destroyed; } set { _Destroyed = value; } }

        protected IBuffManager _BuffManager;
        public IBuffManager BuffManager { get { return _BuffManager; } }

        // flight parameters
        public float flightSpeed;
        protected bool disabled;

        [ShowOnly]
		public bool armed;

        [ShowOnly]
        public bool hasLaunched;

        protected bool initialised = false;

        // cached components
        protected SpriteRenderer myRenderer;
        protected Rigidbody2D myRigidbody;
        protected Collider2D myCollider;
        protected ProjectileTrigger3D projectileTrigger3D;
        protected Transform projectileParent;

        public virtual void init()
        {
            _BuffManager = gameObject.GetComponent<BuffManager>();

            myRenderer = GetComponent<SpriteRenderer>();
            myRigidbody = GetComponent<Rigidbody2D>();
            myCollider = GetComponent<Collider2D>();

            Transform collider3D = transform.Find("Trigger3D");

            if (collider3D != null)
            {
                projectileTrigger3D = collider3D.GetComponent<ProjectileTrigger3D>();
                projectileTrigger3D.setup();
            }

            projectileParent = GameManager.Instance.ProjectilesParent;
        }

        // Use this for initialization
        protected virtual void OnEnable()
        {
            // D.log("Projectile", "Missile Enabled");

            if (initialised == false) init();

            disabled = false;

            if (myRenderer != null)
            {
                myRenderer.enabled = true;
            }

            if (myRigidbody != null)
            {
                myRigidbody.WakeUp();
            }

            myCollider.enabled = true;

            if (projectileTrigger3D != null)
            {
                projectileTrigger3D.enable();
            }

            hasLaunched = false;
            Destroyed = false;
        }

        protected virtual void disable()
        {
            if (disabled == false)
            {
                if (myRenderer != null)
                {
                    myRenderer.enabled = false;
                }

                if (myRigidbody != null)
                {
                    myRigidbody.Sleep();
                }

                ignoreColliders(myCollider, weaponStructure.gameObject, false);
                //Physics2D.IgnoreCollision(myCollider, weaponStructure.StructureCollider, false);

                myCollider.enabled = false;

                if (projectileTrigger3D != null)
                {
                    projectileTrigger3D.disable(weaponStructure);
                }

                disabled = true;

                // D.log("Projectile", "Missile Disabled");
            }
        }

        protected void recycleImmediate()
        {
            gameObject.Recycle();
        }

        protected void recycleDelayed(float delay)
        {
            StartCoroutine(DelayedRecycle(delay));
        }    

        protected void arm()
        {
            armed = true;
        }
		
		public virtual void setInitialDirection(Quaternion direction)
		{
            transform.rotation = direction;
		}
		
		public virtual void remove()
		{
			// D.log("Projectile", "Removing projectile: " + gameObject.name);
		}

        protected void ignoreColliders(Collider2D projectileCollider, GameObject structureGO, bool enable)
        {
            Collider2D[] colliders = structureGO.GetComponentsInChildren<Collider2D>();

            for (int i = 0; i < colliders.Length; i++)
            {
                Physics2D.IgnoreCollision(projectileCollider, colliders[i], enable);
            }
        }

        public virtual void hasCollided(NoxObject collidedObject = null) { }

        public void update()
        {
            BuffManager.update();
        }

        public virtual bool fire(IWeapon weapon)
        {
            this.weapon = weapon;

            weaponStructure = weapon.getStructure();

            FactionID = weapon.getStructure().Faction.ID;

            ignoreColliders(myCollider, weaponStructure.gameObject, true);
            //Physics2D.IgnoreCollision(myCollider, weaponStructure.StructureCollider);

            if (myRenderer != null)
            {
                myRenderer.sortingLayerName = weaponStructure.StructureRenderer.sortingLayerName;
                myRenderer.sortingOrder = weaponStructure.StructureRenderer.sortingOrder + 2;
            }

            if (projectileTrigger3D != null)
            {
                projectileTrigger3D.launch(weaponStructure);
            }

            gameObject.transform.SetParent(projectileParent);

            hasLaunched = true;

            return true;
        }

        private IEnumerator DelayedRecycle(float delay)
        {
            // D.log("Projectile", "Projectile recycler in: " + delay);

            disable();

            yield return new WaitForSeconds(delay);

            // D.log("Projectile", "Projectile recycled");

            recycleImmediate();
        }
    }
}