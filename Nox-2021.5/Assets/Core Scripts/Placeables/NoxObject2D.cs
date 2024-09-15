using UnityEngine;

using System;

using NoxCore.Data.Placeables;
using NoxCore.GameModes;
using NoxCore.GUIs;
using NoxCore.Managers;

namespace NoxCore.Placeables
{
    public class SpawnEventArgs : EventArgs
    {
        public Structure spawnedStructure;

        public SpawnEventArgs(Structure spawnedStructure)
        {
            this.spawnedStructure = spawnedStructure;
        }
    }

    public class DespawnEventArgs : EventArgs
    {
        public Structure despawnedStructure;

        public DespawnEventArgs(Structure despawnedStructure)
        {
            this.despawnedStructure = despawnedStructure;
        }
    }

    public class RespawnEventArgs : EventArgs
    {
        public Structure respawnedStructure;

        public RespawnEventArgs(Structure respawnedStructure)
        {
            this.respawnedStructure = respawnedStructure;
        }
    }

    public class DestroyEventArgs : EventArgs
    {
        public Structure destroyedStructure;

        public DestroyEventArgs(Structure destroyedStructure)
        {
            this.destroyedStructure = destroyedStructure;
        }
    }

    public class NoxObject2D : NoxObject, INoxObject2D, ISpawnable
    {
        // note: these events are non-static so a reference is required in order to bind to these
        public delegate void SpawnEventDispatcher(object sender, SpawnEventArgs args);
        public event SpawnEventDispatcher Spawn;

        public delegate void DespawnEventDispatcher(object sender, DespawnEventArgs args);
        public event DespawnEventDispatcher Despawn;

        public delegate void RespawnEventDispatcher(object sender, RespawnEventArgs args);
        public event RespawnEventDispatcher Respawn;

        public delegate void DestroyEventDispatcher(object sender, DestroyEventArgs args);
        public event DestroyEventDispatcher Destruct;

        public event RespawnEventDispatcher HasRespawned;

        [Tooltip("ScriptableObject asset containing primary data for the object")]
        [Header("NoxObject2D")]
        public NoxObject2DData noxObject2DData;
        protected NoxObject2DData _noxObject2DData;
        public NoxObject2DData NoxObject2DData { get { return _noxObject2DData; } set { _noxObject2DData = value; } }

        [Header("Setup Overrides")]
        [SerializeField] protected bool _SpawnHidden;
        public bool SpawnHidden { get { return _SpawnHidden; } set { _SpawnHidden = value; } }

        protected SpriteRenderer[] _objectRenderers;
		public SpriteRenderer[] objectRenderers { get { return _objectRenderers; } set { _objectRenderers = value; } }

        protected Collider2D[] _objectColliders;
		public Collider2D[] objectColliders { get { return _objectColliders; } set { _objectColliders = value; } }

        protected Rigidbody2D[] _objectRigidbodies;
		public Rigidbody2D[] objectRigidbodies { get { return _objectRigidbodies; } set { _objectRigidbodies = value; } }     

        protected float _length, _halfLength, _width, _halfWidth;
        public float Length { get { return _length; } set { _length = value; } }
        public float HalfLength { get { return _halfLength; } set { _halfLength = value; } }
        public float Width { get { return _width; } set { _width = value; } }
        public float HalfWidth { get { return _halfWidth; } set { _halfWidth = value; } }

        public override void init(NoxObjectData noxObjectData = null)
        {
            if (noxObjectData == null)
            {
                NoxObject2DData = noxObject2DData;
                base.init(noxObject2DData);
            }
            else
            {
                NoxObject2DData = noxObjectData as NoxObject2DData;
                base.init(noxObjectData);
            }

            D.log("Content", "Initialising NoxObject2D");

            HalfLength = GetComponent<Collider2D>().bounds.extents.y;
            HalfWidth = GetComponent<Collider2D>().bounds.extents.x;

            Length = HalfLength * 2;
            Width = HalfWidth * 2;

            // disable the game object (this will get set in the custom Game Mode class)
            enabled = false;

            // disable any shield mesh (this will get set in the post-fitting method in the Structure class)
            Transform shieldMesh = transform.Find("ShieldMesh");

            if (shieldMesh != null)
            {
                MeshRenderer shieldRenderer = shieldMesh.GetComponent<MeshRenderer>();

                if (shieldRenderer != null)
                {
                    shieldRenderer.enabled = false;
                }
            }
            else
            {
                D.warn("Structure: {0}", "No shield mesh GameObject attached to structure " + Name);
            }

            objectRenderers = GetComponentsInChildren<SpriteRenderer>();
            objectColliders = GetComponentsInChildren<Collider2D>();
            objectRigidbodies = GetComponentsInChildren<Rigidbody2D>();
        }

        public override void OnEnable()
        {
            if (eventsSubscribedInto == false)
            {
                Spawn += NoxObject2D_Spawn;
                Despawn += NoxObject2D_Despawn;
                Respawn += NoxObject2D_Respawn;
                Destruct += NoxObject2D_Destruct;

                base.OnEnable();
            }
        }

        public override void OnDisable()
        {
            if (eventsSubscribedInto == true)
            {
                Spawn -= NoxObject2D_Spawn;
                Despawn -= NoxObject2D_Despawn;
                Respawn -= NoxObject2D_Respawn;
                Destruct -= NoxObject2D_Destruct;

                base.OnDisable();
            }
        }

		public override void spawn(bool spawnEnabled = false)
		{
            base.spawn(spawnEnabled);	
			
			// object has been initialised so now turn all renderers on
			// this is done here because when objects are built dynamically you can't set position and rotation when instantiating or it will offset sockets etc
			if (SpawnHidden == false)
			{
				enableAllRenderers();
			}
		}
		
		public virtual void despawn() {}

        public virtual void respawn() {}		
		
		public virtual void reset() {}

        public virtual void destroy() {}
				
		public virtual void disableAllRenderers()
		{
            if (objectRenderers != null)
            {
                foreach (SpriteRenderer renderer in objectRenderers)
                {
                    if (renderer != null) renderer.enabled = false;
                }
            }
		}		
		
		public virtual void enableAllRenderers()
		{
            if (objectRenderers != null)
            {
                foreach (SpriteRenderer renderer in objectRenderers)
                {
                    if (renderer != null) renderer.enabled = true;
                }
            }
		}
		
		public virtual void disableAllColliders()
		{
            if (objectColliders != null)
            {
                foreach (Collider2D collider in objectColliders)
                {
                    if (collider != null) collider.enabled = false;
                }
            }
		}
		
		public virtual void enableAllColliders()
		{
            if (objectColliders != null)
            {
                foreach (Collider2D collider in objectColliders)
                {
                    if (collider != null) collider.enabled = true;
                }
            }
		}
		
		public virtual void hideObject()
		{
			disableAllRenderers();
			disableAllColliders();

            NameLabel.ShowLabel(false);
            FactionLabel.ShowLabel(false);
        }

        public virtual void showObject()
		{
			enableAllRenderers();
			enableAllColliders();

            NameLabel.ShowLabel(NoxGUI.Instance.showNames);
            FactionLabel.ShowLabel(NoxGUI.Instance.showFactions);
        }

        public void Call_Spawn(object sender, SpawnEventArgs args)
        {
            GameMode.Call_Spawn(sender, args);

            if (Spawn != null)
            {
                Spawn(sender, args);
            }
        }

        public void Call_Despawn(object sender, DespawnEventArgs args)
        {
            GameMode.Call_Despawn(sender, args);
         
            if (Despawn != null)
            {
                Despawn(sender, args);
            }
        }

        public void Call_Respawn(object sender, RespawnEventArgs args)
        {
            GameMode.Call_Respawn(sender, args);

            if (Respawn != null)
            {
                Respawn(sender, args);
            }            
        }

        public void Call_HasRespawned(object sender, RespawnEventArgs args)
        {
            GameMode.Call_HasRespawned(sender, args);

            if (HasRespawned != null)
            {
                HasRespawned(sender, args);
            }
        }

        public void Call_Destruct(object sender, DestroyEventArgs args)
        {
            GameMode.Call_Destruct(sender, args);

            if (Destruct != null)
            {
                Destruct(sender, args);
            }
        }

        // TODO - should call a spawn event to start the actual spawn process (or something like this)
        public virtual void NoxObject2D_Spawn(object sender, SpawnEventArgs args)
        {
            D.log("Structure", "Spawning " + gameObject.name);

            //Invoke("spawn", SpawnTime);
        }

        public virtual void NoxObject2D_Despawn(object sender, DespawnEventArgs args)
        {
            D.log("Structure", "Despawning " + gameObject.name + " in " + NoxObject2DData.DespawnTime + " seconds");

            Invoke("despawn", NoxObject2DData.DespawnTime);
        }

        public virtual void NoxObject2D_Respawn(object sender, RespawnEventArgs args)
        {
            D.log("Structure", "Respawning " + gameObject.name + " in " + NoxObject2DData.RespawnTime + " seconds");

            Invoke("respawn", NoxObject2DData.RespawnTime);
        }

        public virtual void NoxObject2D_Destruct(object sender, DestroyEventArgs args)
        {
            destroy();
        }
    }
}