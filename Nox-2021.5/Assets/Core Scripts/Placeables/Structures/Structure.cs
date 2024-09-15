using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;
using System;
using System.Text;

using Meters;

using NoxCore.Controllers;
using NoxCore.Cameras;
using NoxCore.Data;
using NoxCore.Data.Placeables;
using NoxCore.Debugs;
using NoxCore.Effects;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Fittings.Weapons;
using NoxCore.GameModes;
using NoxCore.GUIs;
using NoxCore.Managers;
using NoxCore.Stats;
using NoxCore.Utilities;
using NoxCore.Utilities.Geometry;
using NoxCore.Placeables.Ships;

using Davin.GUIs;

namespace NoxCore.Placeables
{
    #region Enumerators
    // note: when you import a sprite, divide it's max length/width in pixels by the size you want it to be in the game. This equals the pixels-per-unit value you need to set for the sprite itself.
    public enum StructureSize
	{
		TINY,					// 0 - 50m
		SMALL,					// 50 - 160m
		MEDIUM,					// 160 - 250m
		LARGE,					// 250 - 300m
		MASSIVE,				// 300 - 500m
        ENORMOUS,				// 500 - 1000m
        GIGANTIC,				// 1000 - 2000m
        COLOSSAL				// >2000m
	};
    #endregion

    #region Structure event argument classes

    public class UltimateEventArgs : EventArgs
    {
        Structure structure;

        public UltimateEventArgs(Structure structure)
        {
            this.structure = structure;
        }
    }

    public class WarpEventArgs : EventArgs
    {
        public GameObject warpShipGO;
        public Ship warpShip;
        public string warpToScenePath;
        public string warpFromScenePath;
        public Vector2? warpPosition;
        public float? warpRotation;
        public float delay;

        public WarpEventArgs(GameObject warpShipGO, string warpToScenePath, string warpFromScenePath, Vector2? warpPosition, float? warpRotation, float delay = 0)
        {
            this.warpShipGO = warpShipGO;
            warpShip = warpShipGO.GetComponent<Ship>();

            this.warpToScenePath = warpToScenePath;
            this.warpFromScenePath = warpFromScenePath;
            this.warpPosition = warpPosition;
            this.warpRotation = warpRotation;
            this.delay = delay;
        }
    }

    public class DamageEventArgs : EventArgs
    {
        public float damage;
        public Structure damagedStructure;
        public Structure damageCauser;

        public DamageEventArgs(float damage, Structure damagedStructure, Structure damageCauser)
        {
            this.damage = damage;
            this.damagedStructure = damagedStructure;
            this.damageCauser = damageCauser;
        }
    }

    public class ModuleDamageEventArgs : EventArgs
    {
        public Structure moduleAttacker;
        public Structure moduleOwner;
        public Module moduleHit;
        public float damage;
        public bool destroyed;

        public ModuleDamageEventArgs(Structure moduleAttacker, Structure moduleOwner, Module moduleHit, float damage, bool destroyed)
        {
            this.moduleAttacker = moduleAttacker;
            this.moduleOwner = moduleOwner;
            this.moduleHit = moduleHit;
            this.damage = damage;
            this.destroyed = destroyed;
        }
    }

    public class TargetDestroyedEventArgs : EventArgs
    {
        public Structure structureAttacked;
        public Module moduleAttacked;
        public Structure attacker;
        public Weapon weaponUsed;

        public TargetDestroyedEventArgs(Structure structureAttacked, Module moduleAttacked, Structure attacker, Weapon weaponUsed)
        {
            this.structureAttacked = structureAttacked;
            this.moduleAttacked = moduleAttacked;
            this.attacker = attacker;
            this.weaponUsed = weaponUsed;
        }
    }

    public class SurvivalTimeEventArgs : EventArgs
    {
        public Structure updatedStructure;
        public float updatedSurvivalTime;
        public bool combatFinished;

        public SurvivalTimeEventArgs(Structure updatedStructure, float updatedSurvivalTime, bool combatFinished)
        {
            this.updatedStructure = updatedStructure;
            this.updatedSurvivalTime = updatedSurvivalTime;
            this.combatFinished = combatFinished;
        }
    }

    public class AssistEventArgs : EventArgs
    {
        public Structure assister;
        public Structure killedStructure;
        public float killParticipation;

        public AssistEventArgs(Structure assister, Structure killedStructure, float killParticipation)
        {
            this.assister = assister;
            this.killedStructure = killedStructure;
            this.killParticipation = killParticipation;
        }
    }

    public class BoundaryEventArgs : EventArgs
    {
        public Structure structure;

        public BoundaryEventArgs(Structure structure)
        {
            this.structure = structure;
        }
    }
    #endregion

    [RequireComponent(typeof(FireGroupController))]
    [RequireComponent(typeof(BuffManager))]
    public abstract class Structure : NoxObject2D, IStructure, IDamagable, IStructureDebuggable
    {
        #region Delegates

        public delegate void UltimateEventHandler(object sender, UltimateEventArgs args);
        public event UltimateEventHandler ActivateUltimate;

        public delegate void DockingPortEventDispatcher(object sender, DockingPortEventArgs args);
        public event DockingPortEventDispatcher DockInitiatorDocked;
        public event DockingPortEventDispatcher DockReceiverDocked;
        public event DockingPortEventDispatcher DockInitiatorUndocked;
        public event DockingPortEventDispatcher DockReceiverUndocked;

        public delegate void HangarEventDispatcher(object sender, HangarEventArgs args);
        public event HangarEventDispatcher LandingInitiatorLanded;
        public event HangarEventDispatcher LandingReceiverLanded;
        public event HangarEventDispatcher LaunchInitiatorLaunched;
        public event HangarEventDispatcher LaunchReceiverLaunched;

        public delegate void BoundaryEventDispatcher(object sender, BoundaryEventArgs args);
        public event BoundaryEventDispatcher InBounds;
        public event BoundaryEventDispatcher OutOfBounds;

        public delegate void DamageEventDispatcher(object sender, DamageEventArgs args);
        public event DamageEventDispatcher InstigatedAnyDamage;
        public event DamageEventDispatcher TakenAnyDamage;

        public delegate void AssistEventDispatcher(object sender, AssistEventArgs args);
        public event AssistEventDispatcher NotifyAssister;

        public delegate void ModuleDamagedEventDispatcher(object sender, ModuleDamageEventArgs args);
        public event ModuleDamagedEventDispatcher ModuleDamaged;

        public delegate void TargetDestroyedEventDispatcher(object sender, TargetDestroyedEventArgs args);
        public event TargetDestroyedEventDispatcher TargetDestroyed;
        public event TargetDestroyedEventDispatcher NotifyKilled;
        public event TargetDestroyedEventDispatcher NotifyKiller;

        public delegate void SurvivalTimeEventDispatcher(object sender, SurvivalTimeEventArgs args);
        public event SurvivalTimeEventDispatcher SurvivalTimeUpdated;
        #endregion

        protected int uniqueID;

        [Header("Structure")]
        protected StructureData _structureData;
        public StructureData StructureData { get { return _structureData; } set { _structureData = value; } }

        #region variables
        [Header("Structure Command")]

        [Tooltip("ScriptableObject asset containing structure commander data")]
        [SerializeField] protected CommanderData _command;
        public CommanderData Command { get { return _command; } set { _command = value; } }

        [Header("Structure Tactical")]

        #region cached components

        // reference to the current game mode of the scene
        protected GameMode _Gamemode;
        public GameMode Gamemode { get { return _Gamemode; } set { _Gamemode = value; } }

        // reference to the controller for the structure
        protected StructureController _controller;
        public StructureController Controller { get { return _controller; } set { _controller = value; } }

        // reference to the structure's SpriteRenderer component
        protected SpriteRenderer _StructureRenderer;
        public SpriteRenderer StructureRenderer { get { return _StructureRenderer; } set { _StructureRenderer = value; } }

        // reference to the structure's Rigidbody2D component
        protected Rigidbody2D _StructureRigidbody;
        public Rigidbody2D StructureRigidbody { get { return _StructureRigidbody; } set { _StructureRigidbody = value; } }

        // reference to the structure's Collider2D component
        protected Collider2D _StructureCollider;
        public Collider2D StructureCollider { get { return _StructureCollider; } set { _StructureCollider = value; } }

        // reference to the structure's top-level transform (either the structure or ship's game object) 
        protected Transform _RootTransform;
        public Transform RootTransform { get { return _RootTransform; } set { _RootTransform = value; } }

        // reference to the structure's transform contaiing the shield mesh
        protected Transform _ShieldTransform;
        public Transform ShieldTransform { get { return _ShieldTransform; } set { _ShieldTransform = value; } }

        // reference to the structure's shield MeshRenderer component
        protected MeshRenderer _ShieldRenderer;
        public MeshRenderer ShieldRenderer { get { return _ShieldRenderer; } set { _ShieldRenderer = value; } }

        // reference to the structure shield's computed Collider component
        protected Collider _ShieldCollider;
        public Collider ShieldCollider { get { return _ShieldCollider; } set { _ShieldCollider = value; } }

        // reference to the structure's primary BuffManager component
        protected IBuffManager _BuffManager;
        public IBuffManager BuffManager { get { return _BuffManager; } }

        // reference to the structure's FireGroupController component
        protected FireGroupController _fireControl;
        public FireGroupController FireControl { get { return _fireControl; } set { _fireControl = value; } }

        // reference to the structure's StructureStats component
        protected StructureStats _Stats;
        public StructureStats Stats { get { return _Stats; } set { _Stats = value; } }

        #endregion

        #region properties set in editor
        [Header("Setup Overrides")]
        [Tooltip("Flag to determine if the structure can take damage")]
        [SerializeField] protected bool _CanBeDamaged;
        public bool CanBeDamaged { get { return _CanBeDamaged; } set { _CanBeDamaged = value; } }

        [Tooltip("Flag to determine if the structure can be respawned after being destroyed")]
        [SerializeField] protected bool _CanRespawn;
        public bool CanRespawn { get { return _CanRespawn; } set { _CanRespawn = value; } }

        [Tooltip("Override for the structure's initial hull strength (also current hull strength when playing)")]
        [SerializeField] protected float _HullStrength;
        public float HullStrength { get { return _HullStrength; } set { _HullStrength = value; } }

        [Tooltip("Override for the structure's initial max hull strength (also current max hull strength when playing)")]
        [SerializeField] protected float _MaxHullStrength;
        public float MaxHullStrength { get { return _MaxHullStrength; } set { _MaxHullStrength = value; } }
        #endregion

        #region properties calculated at runtime
        // current overall combined shield strength
        protected float _ShieldStrength;
        public float ShieldStrength { get { return _ShieldStrength; } set { _ShieldStrength = value; } }

        // overall combined max shield strength assuming full charge on all shields
        protected float _MaxShieldStrength;
        public float MaxShieldStrength { get { return _MaxShieldStrength; } set { _MaxShieldStrength = value; } }

        // flag to determine when all shields are down
        protected bool _AllShieldsFailed;
        public bool AllShieldsFailed { get { return _AllShieldsFailed; } set { _AllShieldsFailed = value; } }

        // flag to determine if the structure has been through its init chain
        protected bool _SystemsInitiated = false;
        public bool SystemsInitiated { get { return _SystemsInitiated; } set { _SystemsInitiated = value; } }

        // flag to determine if the structure has been destroyed
        protected bool _Destroyed;
        public bool Destroyed { get { return _Destroyed; } set { _Destroyed = value; } }

        // flag to determine if the structure is invisible (not renderered)
        protected bool _Invisible;
        public bool Invisible { get { return _Invisible; } set { _Invisible = value; } }

        // flag to determine if the structure is outside of any arena present in the scene
        protected bool _OutsideBoundary;
        public bool OutsideBoundary { get { return _OutsideBoundary; } set { _OutsideBoundary = value; } }

        // current amount of time the structure has been outside of any arena present in the scene
        protected float _BoundaryTime;
        public float BoundaryTime { get { return _BoundaryTime; } set { _BoundaryTime = value; } }

        // maximum allowed time for the structure to be outside of any arena present in the scene
        protected float _MaxBoundaryTime;
        public float MaxBoundaryTime { get { return _MaxBoundaryTime; } set { _MaxBoundaryTime = value; } }

        // simple delta timer used for timing the structure's violation of the arena boundary
        protected float _BoundaryTimer;
        public float BoundaryTimer { get { return _BoundaryTimer; } set { _BoundaryTimer = value; } }

        // reference to the structure that last hit this structure
        protected Structure _lastHitBy;
        public Structure LastHitBy { get { return _lastHitBy; } set { _lastHitBy = value; } }

        protected RaycastHit? _hitInfo;
        public RaycastHit? HitInfo { get { return _hitInfo; } set { _hitInfo = value; } }

        // total cost of the structure including hull cost and fittings
        protected uint _TotalCost;
        public uint TotalCost { get { return _TotalCost; } set { _TotalCost = value; } }

        // reference to all sockets on the structure
        protected List<StructureSocket> _structureSockets = new List<StructureSocket>();
        public List<StructureSocket> StructureSockets { get { return _structureSockets; } }

        // reference to all of the devices fitted to the structure
        protected List<Device> _devices = new List<Device>();
        public List<Device> Devices { get { return _devices; } }

        // reference to all of the modules fitted to the structure
        protected List<Module> _modules = new List<Module>();
        public List<Module> Modules { get { return _modules; } }

        // reference to all of the weapons fitted to the structure
        protected List<Weapon> _weapons = new List<Weapon>();
        public List<Weapon> Weapons { get { return _weapons; } }

        #endregion

        #region variables

        protected float initialLifeSpan;
        protected StopWatchWrapper _aliveTimer = new StopWatchWrapper();
        public StopWatchWrapper AliveTimer { get { return _aliveTimer; } }
        protected (bool destroyed, float damageOnDestroy) damageReport;  // used to see what was damaged when structure was hit	

        [Header("Structure Stats")]
        [Tooltip("The classification size of the structure (note: this is set automatically for ships but not stations)")]
        public StructureSize structureSize;
        protected static int[] structureSizeLimits = { 0, 50, 160, 250, 300, 500, 1000, 2000 };

        // structure and shield bounds
        protected Bounds structureBounds;
        protected Bounds shieldBounds;

        // reference to the structure's scrolling text transform and text component (if any)
        protected StructureScrollingText scrollingText;
        protected Transform scrollingTextTrans;

        // reference to the structure's health bar Transform component
        protected Transform healthBarTrans;
        
        // references to the image components of the health bar for hull and shield
        protected Image hullBar, shieldBar;
       
        // distance the health bar should be offset by (vertically)
        protected float healthBarOffset;

        #endregion

        [Header("Fittings")]

        // references to common devices and modules
        public IPowerGrid powergrid;
        public IThermalControl thermalcontrol;
        public IScanner scanner;
        public List<IPowerCore> powergenerators = new List<IPowerCore>();
        public List<IShieldGenerator> shields = new List<IShieldGenerator>();

        [Header("Structure Spawn")]
        [ShowOnly]
        [Tooltip("Flag to determine if the structure has been spawned in")]
        public bool spawnedIn;

        [Tooltip("Prefabs to use for structure explosion")]
        public GameObject explosionInitial, explosionFinal;
        
        // reference to the structure's ExplosionVFXController component
        protected ExplosionVFXController explosionVFXController;

        [Header("Structure UI")]

        [Tooltip("Offset for the structure's faction label")]
        public Vector3 factionLabelOffset;

        [Tooltip("Offset for the structure's name label")]
        public Vector3 nameLabelOffset;

        // reference to the structure's Outline component
        [Tooltip("Reference to the structure's Outline component")]
        [ShowOnly]
        public Outline outline;

        [Tooltip("Reference to the structure's RadialMenu root transform")]
        [SerializeField] protected Transform _RadialMenuParentTrans;
        public Transform RadialMenuParentTrans { get { return _RadialMenuParentTrans; } set { _RadialMenuParentTrans = value; } }

        protected TextMesh factionTextMesh;
        protected TextMesh nameTextMesh;

        protected StringBuilder powerInfo = new StringBuilder(3096);
        protected StringBuilder coresInfo = new StringBuilder(1024);
        protected StringBuilder devicesInfo = new StringBuilder(1024);
        protected StringBuilder modulesInfo = new StringBuilder(1024);

        protected float notifyHullLowPercent = 0.5f;
        protected float notifyHullBreachPercent = 0.2f;
        protected bool notifiedHullLow = false;
        protected bool notifiedHullBreached = false;
        #endregion

        #region initialisation & delegate subscriptions

        public override void init(NoxObjectData noxObjectData = null)
        {
            if (noxObjectData == null)
            {
                StructureData = Instantiate(noxObject2DData as StructureData);
                base.init(StructureData);
            }
            else
            {
                StructureData = noxObjectData as StructureData;
                base.init(noxObjectData);
            }

            D.log("Structure", "Initialising structure " + gameObject.name);

            if (gameObject.layer != LayerMask.NameToLayer("Structure") && gameObject.layer != LayerMask.NameToLayer("Ship"))
            {
                D.warn("Structure: {0}", "Layer not set to Structure or Ship on GameObject for structure " + Name);
            }

            // set reference to game mode
            Gamemode = GameManager.Instance.Gamemode;

            // cache root transform
            RootTransform = transform;

            // cache structure sprite renderer, rigidbody and collider
            StructureRenderer = GetComponent<SpriteRenderer>();
            StructureRigidbody = GetComponent<Rigidbody2D>();
            StructureCollider = GetComponent<Collider2D>();

            StructureController controller = GetComponent<StructureController>();

            if (controller != null)
            {
                attachController(controller);
            }

            if (StructureRenderer != null)
            {
                structureBounds = StructureRenderer.sprite.bounds;
            }

            // set up info bars
            Transform ui = transform.Find("UI");

            scrollingTextTrans = ui.Find("Structure Scrolling Text Label");

            if (scrollingTextTrans != null)
            {
                scrollingText = scrollingTextTrans.GetComponent<StructureScrollingText>();
                scrollingText.init();
            }

            GameObject healthBarPrefab = Resources.Load<GameObject>("UI/Health Bar");

            if (healthBarPrefab != null)
            {
                GameObject healthBarGO = Instantiate(healthBarPrefab);
                healthBarGO.name = "Health Bar";
                healthBarGO.transform.SetParent(ui);
                healthBarTrans = healthBarGO.transform;

                if (healthBarTrans != null)
                {
                    GameObject healthBarCanvasGO = healthBarTrans.Find("Canvas").gameObject;
                    healthBarCanvasGO.SetActive(false);

                    RectTransform rectTransform;
                    Image bkg;

                    // Hull
                    hullBar = healthBarCanvasGO.transform.Find("Hull Bar").GetComponent<Image>();
                    rectTransform = hullBar.GetComponent<RectTransform>();

                    setBarWidth(rectTransform);

                    // Shield
                    shieldBar = healthBarCanvasGO.transform.Find("Shield Bar").GetComponent<Image>();
                    rectTransform = shieldBar.GetComponent<RectTransform>();

                    setBarWidth(rectTransform);

                    // Background
                    bkg = healthBarCanvasGO.transform.Find("Bkg").GetComponent<Image>();
                    rectTransform = bkg.GetComponent<RectTransform>();

                    setBarWidth(rectTransform);
                }
            }

            //set up labels
            NameLabel.init();
            FactionLabel.init();

            // set up health bar
            Transform hBar = transform.Find("UI/Health Bar");

            nameTextMesh = NameLabel.GetComponent<TextMesh>();
            factionTextMesh = FactionLabel.GetComponent<TextMesh>();

            name = name.Truncate(36);

            // subscribe to own events

            enabled = false;

            // cache the shield transform
            ShieldTransform = transform.Find("ShieldMesh");

            if (ShieldTransform != null)
            {
                if (ShieldTransform.gameObject.layer != LayerMask.NameToLayer("Shield"))
                {
                    D.warn("Structure: {0}", "Layer for ShieldMesh child GameObject on structure " + Name + " is not set to Shield");
                }

                // set max bounds for UI based on shield extents
                //boundsOffset = Mathf.Max(ShieldTransform.localScale.x, ShieldTransform.localScale.y);

                // cache the shield mesh renderer
                ShieldRenderer = ShieldTransform.GetComponent<MeshRenderer>();

                bool success = false;

                // build a simplified convex mesh collider for the shield
                MeshFilter meshFilter = ShieldTransform.GetComponent<MeshFilter>();

                if (meshFilter != null)
                {
                    Mesh mesh = meshFilter.mesh;

                    do
                    {
                        success = true;

                        try
                        {
                            Mesh simpleMesh = SimpleConvex.BuildSimplifiedConvexMesh(mesh, 48);

                            MeshCollider meshCollider = ShieldTransform.gameObject.AddComponent<MeshCollider>();
                            ShieldCollider = meshCollider;
                            meshCollider.sharedMesh = simpleMesh;
                            meshCollider.convex = true;
                        }
                        catch (Exception e)
                        {
                            success = false;
                        }
                    }
                    while (success == false);
                }
            }
            else
            {
                // set max bounds for UI based on structure extents
                //boundsOffset = Mathf.Max(StructureCollider.bounds.extents.x, StructureCollider.bounds.extents.y);
            }

            // set up FireControl
            FireControl = GetComponent<FireGroupController>();

            // set up fittings
            Transform systemsParent = transform.Find("Systems");

            if (systemsParent != null)
            {
                // Devices
                foreach (Device device in systemsParent.GetComponentsInChildren<Device>())
                {
                    // restrict number of devices by ship data
                    if (StructureData != null && Devices.Count == StructureData.maxDevices)
                    {
                        Destroy(device.gameObject);
                    }
                    else
                    {
                        addDevice(device, systemsParent);
                    }
                }

                // Sockets, Modules and Weapons
                Transform socketsParent = transform.Find("Sockets");

                List<GameObject> flagForDestruction = new List<GameObject>();

                foreach (StructureSocket structureSocket in socketsParent.GetComponentsInChildren<StructureSocket>())
                {
                    // Note: prevents duplicate socket names
                    if (addSocket(structureSocket))
                    {
                        if (structureSocket.transform.childCount > 0)
                        {
                            // get the first (should be only) child GameObject
                            Transform moduleTrans = structureSocket.transform.GetChild(0);

                            Module module = moduleTrans.GetComponent<Module>();

                            Weapon weapon = module as Weapon;

                            if (weapon != null)
                            {
                                if (StructureData != null && Weapons.Count == StructureData.maxWeapons)
                                {
                                    flagForDestruction.Add(weapon.gameObject);
                                }
                                else
                                {
                                    addWeapon(structureSocket.gameObject, weapon);
                                }
                            }
                            else
                            {
                                if (StructureData != null && Modules.Count == StructureData.maxModules)
                                {
                                    flagForDestruction.Add(module.gameObject);
                                }
                                else
                                {
                                    addModule(structureSocket.gameObject, module);
                                }
                            }
                        }
                    }
                    else
                    {
                        flagForDestruction.Add(structureSocket.gameObject);
                    }
                }

                for (int i = flagForDestruction.Count - 1; i >= 0; i--)
                {
                    Destroy(flagForDestruction[i].gameObject);
                }
            }

            // set up the structure's default number of fire groups based on size and set each weapon's reference to the firegroup it is within
            if (FireControl != null)
            {
                setMaxFireGroups();
            }

            // set up the structure's buff manager
            _BuffManager = gameObject.GetComponent<BuffManager>();

            SystemsInitiated = false;

            powerInfo = new StringBuilder(3096);
            coresInfo = new StringBuilder(1024);
            devicesInfo = new StringBuilder(1024);
            modulesInfo = new StringBuilder(1024);
        }

        public override void OnEnable()
        {
            if (eventsSubscribedInto == false)
            {
                // subscribe to events
                Spawn += OnSpawn;
                Despawn += OnDespawn;
                Respawn += OnRespawn;
                HasRespawned += OnHasRespawned;
                Destruct += OnDestruct;

                ActivateUltimate += Structure_UltimateActivated;

                DockInitiatorDocked += Structure_DockInitiatorDocked;
                DockReceiverDocked += Structure_DockReceiverDocked;
                DockInitiatorUndocked += Structure_DockInitiatorUndocked;
                DockReceiverUndocked += Structure_DockReceiverUndocked;

                LandingInitiatorLanded += Structure_LandingInitiatorLanded;
                LandingReceiverLanded += Structure_LandingReceiverLanded;
                LaunchInitiatorLaunched += Structure_LaunchInitiatorLaunched;
                LaunchReceiverLaunched += Structure_LaunchReceiverLaunched;

                InBounds += Structure_InBounds;
                OutOfBounds += Structure_OutOfBounds;

                InstigatedAnyDamage += Structure_InstigatedAnyDamage;
                TakenAnyDamage += Structure_TakenAnyDamage;
                NotifyKiller += Structure_NotifyKiller;
                NotifyKilled += Structure_NotifyKilled;

                base.OnEnable();
            }
        }

        public override void OnDisable()
        {
            if (eventsSubscribedInto == true)
            {
                // unsubscribe from events
                Spawn -= OnSpawn;
                Despawn -= OnDespawn;
                Respawn -= OnRespawn;
                HasRespawned -= OnHasRespawned;
                Destruct -= OnDestruct;

                ActivateUltimate -= Structure_UltimateActivated;

                DockInitiatorDocked -= Structure_DockInitiatorDocked;
                DockReceiverDocked -= Structure_DockReceiverDocked;
                DockInitiatorUndocked -= Structure_DockInitiatorUndocked;
                DockReceiverUndocked -= Structure_DockReceiverUndocked;

                LandingInitiatorLanded -= Structure_LandingInitiatorLanded;
                LandingReceiverLanded -= Structure_LandingReceiverLanded;
                LaunchInitiatorLaunched -= Structure_LaunchInitiatorLaunched;
                LaunchReceiverLaunched -= Structure_LaunchReceiverLaunched;

                InBounds -= Structure_InBounds;
                OutOfBounds -= Structure_OutOfBounds;

                InstigatedAnyDamage -= Structure_InstigatedAnyDamage;
                TakenAnyDamage -= Structure_TakenAnyDamage;
                NotifyKiller -= Structure_NotifyKiller;
                NotifyKilled -= Structure_NotifyKilled;

                base.OnDisable();
            }
        }

        public int getStructureSizeLimit(StructureSize size)
        {
            return structureSizeLimits[(int)size+1];
        }

        public void setStructureSize(StructureSize? size = null)
        {
            if (size == null)
            {
                SpriteRenderer structureRenderer = GetComponent<SpriteRenderer>();

                if (structureRenderer != null)
                {
                    Sprite sprite = structureRenderer.sprite;

                    if (sprite != null)
                    {
                        int maxDimension = Mathf.Max(sprite.texture.width, sprite.texture.height);

                        if (maxDimension < structureSizeLimits[(int)StructureSize.SMALL]) structureSize = StructureSize.TINY;
                        else if (maxDimension < structureSizeLimits[(int)StructureSize.MEDIUM]) structureSize = StructureSize.SMALL;
                        else if (maxDimension < structureSizeLimits[(int)StructureSize.LARGE]) structureSize = StructureSize.MEDIUM;
                        else if (maxDimension < structureSizeLimits[(int)StructureSize.MASSIVE]) structureSize = StructureSize.LARGE;
                        else if (maxDimension < structureSizeLimits[(int)StructureSize.ENORMOUS]) structureSize = StructureSize.MASSIVE;
                        else if (maxDimension < structureSizeLimits[(int)StructureSize.GIGANTIC]) structureSize = StructureSize.ENORMOUS;
                        else if (maxDimension < structureSizeLimits[(int)StructureSize.COLOSSAL]) structureSize = StructureSize.GIGANTIC;
                        else structureSize = StructureSize.COLOSSAL;
                    }
                    else
                    {
                        D.warn("Structure: {0}", "Cannot access the texture to automatically set the structure size for structure " + Name);
                    }
                }
                else
                {
                    D.warn("Structure: {0}", "Cannot find a sprite renderer for structure " + Name);
                }
            }
            else
            {
                structureSize = size.GetValueOrDefault();
            }
        }

        private void setMaxFireGroups()
        {
            if (StructureData == null)
            {
                FireControl.FireGroups.Resize(1);
            }
            else
            {
                if (StructureData.MaxFireGroups < 0)
                {
                    switch (structureSize)
                    {
                        case StructureSize.TINY: StructureData.MaxFireGroups = 1; break;
                        case StructureSize.SMALL: StructureData.MaxFireGroups = 2; break;
                        case StructureSize.MEDIUM: StructureData.MaxFireGroups = 3; break;
                        case StructureSize.LARGE: StructureData.MaxFireGroups = 4; break;
                        case StructureSize.MASSIVE: StructureData.MaxFireGroups = 6; break;
                        case StructureSize.ENORMOUS: StructureData.MaxFireGroups = 9; break;
                        case StructureSize.GIGANTIC: StructureData.MaxFireGroups = 15; break;
                        case StructureSize.COLOSSAL: StructureData.MaxFireGroups = 20; break;
                        default: StructureData.MaxFireGroups = 1; break;
                    }
                }

                if (FireControl.FireGroups.Count > StructureData.MaxFireGroups)
                {
                    FireControl.FireGroups.Resize(StructureData.MaxFireGroups);
                }
            }
        }

        public float calculateAITick()
        {
            switch (structureSize)
            {
                case StructureSize.TINY: return 0.25f;
                case StructureSize.SMALL: return 0.3f;
                case StructureSize.MEDIUM: return 0.75f;
                case StructureSize.LARGE: return 1.0f;
                case StructureSize.MASSIVE: return 1.25f;
                case StructureSize.ENORMOUS: return 1.5f;
                case StructureSize.GIGANTIC: return 1.75f;
                case StructureSize.COLOSSAL: return 2.0f;
                default: return 1.0f;
            }
        }

        #endregion

        #region UI & Rendering
        private void setBarWidth(RectTransform rectTransform)
        {
            switch (structureSize)
            {
                case StructureSize.TINY: rectTransform.localScale -= new Vector3(0.75f, 0, 0); break;
                case StructureSize.SMALL: rectTransform.localScale -= new Vector3(0.4f, 0, 0); break;
                case StructureSize.MEDIUM: rectTransform.localScale += new Vector3(0.0f, 0, 0); break;
                case StructureSize.LARGE: rectTransform.localScale += new Vector3(0.35f, 0, 0); break;
                case StructureSize.MASSIVE: rectTransform.localScale += new Vector3(1f, 0, 0); break;
                case StructureSize.ENORMOUS: rectTransform.localScale += new Vector3(2.5f, 0, 0); break;
                case StructureSize.GIGANTIC: rectTransform.localScale += new Vector3(5f, 0, 0); break;
            }
        }

        public void recalculateHealthBar()
        {
            float maxHealth = MaxHullStrength + MaxShieldStrength;

            // Hull
            if (hullBar != null)
            {
                hullBar.fillAmount = Mathf.Clamp(HullStrength, 0, maxHealth) / maxHealth;
            }

            // Shields
            if (shieldBar != null)
            {
                if (AllShieldsFailed == false)
                {
                    shieldBar.fillAmount = Mathf.Clamp(HullStrength + ShieldStrength, 0, maxHealth) / maxHealth;
                }
                else
                {
                    shieldBar.fillAmount = 0;
                }
            }
        }

        public override void hideObject()
        {
            base.hideObject();

            if (ShieldCollider != null)
            {
                ShieldCollider.enabled = false;
                ShieldRenderer.gameObject.SetActive(false);
            }

            if (scrollingText != null)
            {
                scrollingText.gameObject.SetActive(false);
            }

            if (healthBarTrans != null)
            {
                healthBarTrans.gameObject.SetActive(false);
            }

            if (NameLabel != null)
            {
                NameLabel.gameObject.SetActive(false);
            }

            if (FactionLabel != null)
            {
                FactionLabel.gameObject.SetActive(false);
            }
        }

        public override void showObject()
        {
            base.showObject();

            if (scrollingText != null)
            {
                scrollingText.gameObject.SetActive(true);
            }

            if (healthBarTrans != null)
            {
                healthBarTrans.gameObject.SetActive(true);

                GameManager.Instance.Gamemode.Gui.setHealthBarMode(true);
            }

            if (NameLabel != null)
            {
                NameLabel.gameObject.SetActive(true);
            }

            if (FactionLabel != null)
            {
                FactionLabel.gameObject.SetActive(true);
            }

            if (ShieldCollider != null && AllShieldsFailed == false)
            {
                ShieldCollider.enabled = true;
                ShieldRenderer.gameObject.SetActive(true);
            }
        }

        public void showOutline(bool show)
        {
            if (outline != null)
            {
                outline.ShowHide_Outline(show);
            }
        }

        public void setSortingLayerOrder(string layerName = null, int? sortOrder = null)
        {
            string sortingLayerName;
            int sortingOrder;

            // default structure size index
            int structureSizeIndex = 0;

            if (layerName == null && sortOrder == null)
            {
                structureSizeIndex = (int)(structureSize);
                sortingLayerName = Enum.GetName(typeof(StructureSize), structureSizeIndex).CapitaliseFirstLetterOnly();
                sortingOrder = GameManager.Instance.SortLayerCounter[structureSizeIndex];
            }
            else
            {
                if (Enum.IsDefined(typeof(StructureSize), layerName.ToUpper()))
                {
                    sortingLayerName = layerName;
                }
                else
                {
                    D.warn("Structure", "Layer name " + layerName + " for " + gameObject.name + " is not a valid layer. Defaulting to " + Enum.GetName(typeof(StructureSize), structureSizeIndex).CapitaliseFirstLetterOnly());

                    sortingLayerName = Enum.GetName(typeof(StructureSize), structureSizeIndex).CapitaliseFirstLetterOnly();
                }

                sortingOrder = sortOrder.Value;
            }

            foreach (Renderer renderer in objectRenderers)
            {
                renderer.sortingLayerName = sortingLayerName;
                renderer.sortingOrder = sortingOrder;
            }

            foreach (StructureSocket structureSocket in StructureSockets)
            {
                if (structureSocket.SocketRenderer)
                {
                    structureSocket.SocketRenderer.sortingLayerName = layerName;
                    structureSocket.SocketRenderer.sortingOrder = sortingOrder + 1;
                }

                Module module = structureSocket.InstalledModule;

                if (module != null)
                {
                    if (module.FittingRenderer != null)
                    {
                        module.FittingRenderer.sortingOrder = sortingOrder + 2;

                        // go through module and add any effect renderers to renderers list
                    }
                    else
                    {
                        D.warn("Module: {0}", "A renderer on module " + module.name + " on structure " + Name + " is null");
                    }
                }
            }

            // go through all equipment and add any effect renderers to renderers list
            foreach (Device device in Devices)
            {
                List<IVisualEffect> vfxs = device.getVFXs();

                foreach (IVisualEffect vfx in vfxs)
                {
                    vfx.setSortingLayerOrder(device.transform);
                }
            }

            foreach (Module module in Modules)
            {
                List<IVisualEffect> vfxs = module.getVFXs();

                foreach (IVisualEffect vfx in vfxs)
                {
                    vfx.setSortingLayerOrder(module.transform);
                }
            }

            foreach (Weapon weapon in Weapons)
            {
                List<IVisualEffect> vfxs = weapon.getVFXs();

                foreach (IVisualEffect vfx in vfxs)
                {
                    vfx.setSortingLayerOrder(weapon.transform);
                }
            }

            if (ShieldRenderer != null)
            {
                ShieldRenderer.sortingLayerName = sortingLayerName;
                ShieldRenderer.sortingOrder = sortingOrder;
            }

            if (FactionLabel != null)
            {
                MeshRenderer factionLabelRenderer = FactionLabel.GetComponent<MeshRenderer>();

                if (factionLabelRenderer != null)
                {
                    factionLabelRenderer.sortingLayerName = sortingLayerName;
                    factionLabelRenderer.sortingOrder = sortingOrder;
                }
            }

            if (NameLabel != null)
            {
                MeshRenderer nameLabelRenderer = NameLabel.GetComponent<MeshRenderer>();

                if (nameLabelRenderer != null)
                {
                    nameLabelRenderer.sortingLayerName = sortingLayerName;
                    nameLabelRenderer.sortingOrder = sortingOrder;
                }
            }

            GameManager.Instance.SortLayerCounter[structureSizeIndex] += 3;
        }
        #endregion

        #region spawning/respawning
        public override void spawn(bool spawnEnabled = false)
        {
            base.spawn();

            //update labels (important for Factions, as they are retrieved on Spawn in NoxObject)
            NameLabel.GetComponent<NameLabel>().Reset();
            FactionLabel.GetComponent<FactionLabel>().Reset();

            outline = GetComponent<Outline>();

            if (outline != null)
            {
                outline.vOutlineType = Outline.OutlineType.OFF;
                outline.ShowOutline = false;
                outline.OutlineAllChild = false;
                outline.Initialise();
            }

            foreach (Device device in Devices)
            {
                List<IVisualEffect> vfxs = device.getVFXs();

                foreach (IVisualEffect vfx in vfxs)
                {
                    vfx.setupVFX(device.transform);
                }
            }

            foreach (Module module in Modules)
            {
                List<IVisualEffect> vfxs = module.getVFXs();

                foreach (IVisualEffect vfx in vfxs)
                {
                    vfx.setupVFX(module.transform);
                }
            }

            foreach (Weapon weapon in Weapons)
            {
                List<IVisualEffect> vfxs = weapon.getVFXs();

                foreach (IVisualEffect vfx in vfxs)
                {
                    vfx.setupVFX(weapon.transform);
                }
            }

            setSortingLayerOrder();

            // TODO - HullStrength should always refer to the Structure Data version
            // set hull strength based on StructureData (if set)
            if (StructureData != null && StructureData.HullStrength > 0)
            {
                HullStrength = StructureData.HullStrength;
            }

            if (StructureRigidbody != null)
            {
                StructureRigidbody.mass = StructureData.Mass;

                // if no despawn time set, base it on mass of structure
                if (StructureData.DespawnTime == 0)
                {
                    StructureData.DespawnTime = StructureData.Mass / 500.0f;
                }

                // if no respawn time set, base it on despawn time and classification
                if (StructureData.RespawnTime == 0)
                {
                    if (structureSize == StructureSize.TINY || structureSize == StructureSize.SMALL)
                    {
                        StructureData.RespawnTime = StructureData.DespawnTime + 5.0f;
                    }
                    else
                    {
                        StructureData.RespawnTime = StructureData.DespawnTime;
                    }
                }

                // if no hull strength set, base it on mass of rigidbody
                if (HullStrength == 0)
                {
                    HullStrength = (int)(StructureData.Mass);
                }
            }
            else if (HullStrength <= 0)     // if no hull strength set, assume it is infinite
            {
                HullStrength = Mathf.Infinity;
            }

            //  if no max hull strength set, set it to the spawn hull strength
            if (MaxHullStrength == 0)
            {
                MaxHullStrength = HullStrength;
            }

            // TODO - need a bridge system registration approach to the following instead of directly storing known devices (use a hash table / Dictionary?)
            foreach (Device device in Devices)
            {
                if (device != null)
                {
                    TotalCost += device.DeviceData.Cost;

                    IPowerGrid powergridDevice = device as IPowerGrid;
                    if (powergridDevice != null)
                    {
                        powergrid = powergridDevice;
                        continue;
                    }

                    IThermalControl thermalControlDevice = device as IThermalControl;
                    if (thermalControlDevice != null)
                    {
                        thermalcontrol = thermalControlDevice;

                        if (outline != null) showOutline(thermalcontrol.isOverheated());

                        continue;
                    }

                    IPowerCore powercoreDevice = device as IPowerCore;
                    if (powercoreDevice != null)
                    {
                        powergenerators.Add(powercoreDevice);
                        continue;
                    }

                    IScanner scannerDevice = device as IScanner;
                    if (scannerDevice != null)
                    {
                        scanner = scannerDevice;
                        continue;
                    }
                }
            }

            foreach (Device device in Devices)
            {
                if (device != null)
                {
                    device.postFitting();
                }
                else
                {
                    D.warn("Structure: {0}", "A device slot on structure " + Name + " is null");
                }
            }

            foreach (StructureSocket structureSocket in StructureSockets)
            {
                if (structureSocket != null)
                {
                    structureSocket.postFitting();

                    if (structureSocket.InstalledModule != null)
                    {
                        TotalCost += structureSocket.InstalledModule.DeviceData.Cost;

                        structureSocket.InstalledModule.postFitting();

                        IShieldGenerator shieldGenerator = structureSocket.InstalledModule as IShieldGenerator;

                        if (shieldGenerator != null)
                        {
                            shields.Add(shieldGenerator);
                        }
                    }
                }
                else
                {
                    D.warn("Structure: {0}", "A structure socket on structure " + Name + " is null");
                }
            }

            // disable the renderer of the shield mesh if no shields present
            if (shields.Count == 0)
            {
                AllShieldsFailed = true;

                if (ShieldTransform != null)
                {
                    if (ShieldRenderer != null) ShieldRenderer.enabled = false;
                    if (ShieldCollider != null) ShieldCollider.enabled = false;
                }
            }

            recalculateHealthBar();

            SystemsInitiated = true;

            if (spawnEnabled == true)
            {
                enabled = true;
            }

            // TODO - this is more of a post spawn call really - like a BuildCompleted event or similar
            Call_Spawn(this, new SpawnEventArgs(this));
        }

        public override void despawn()
        {
            base.despawn();

            spawnedIn = false;

            SystemsInitiated = false;

            if (StructureRigidbody != null)
            {
                StructureRigidbody.velocity = Vector2.zero;
                StructureRigidbody.freezeRotation = true;
                StructureRigidbody.Sleep();
            }

            hideObject();

            if (outline != null) showOutline(false);

            // spawn final structure explosion
            if (explosionFinal != null)
            {
                GameObject clonedExplosion = explosionFinal.Spawn(transform);
                clonedExplosion.transform.parent = GameManager.Instance.EffectsParent;

                explosionVFXController = clonedExplosion.GetComponent<ExplosionVFXController>();

                // make any changes to the explosion here (if any)
                explosionVFXController.setSortingLayerOrder(transform);
            }

            // call the destroy event dispatcher
            Call_Destruct(this, new DestroyEventArgs(this));
        }

        public override void respawn()
        {
            if (CanRespawn == true)
            {
                base.respawn();

                OutsideBoundary = false;
                BoundaryTimer = 0;
                /*
                if (StructureData.RespawnsAtStartSpot == true)
                {
                    if (Controller != null)
                    {
                        if (Controller.startSpot != null)
                        {
                            transform.position = Controller.startSpot.GetValueOrDefault();
                        }

                        if (Controller.startRotation != null)
                        {
                            transform.rotation = Quaternion.Euler(0, 0, Controller.startRotation.GetValueOrDefault());
                        }
                    }
                }
                */
                foreach (Device device in Devices)
                {
                    if (device != null)
                    {
                        device.reset();
                    }
                }

                foreach (StructureSocket socket in StructureSockets)
                {
                    if (socket != null)
                    {
                        socket.reset();
                    }
                }

                foreach (Module module in Modules)
                {
                    if (module != null)
                    {
                        module.reset();
                    }
                }

                foreach (Weapon weapon in Weapons)
                {
                    if (weapon != null)
                    {
                        weapon.reset();
                    }
                }

                reset();

                showObject();
                
                if (StructureRigidbody != null) StructureRigidbody.velocity = Vector2.zero;

                Call_HasRespawned(this, new RespawnEventArgs(this));
            }
        }

        public override void reset()
        {
            // restart the alive timer
            startSurvivalClock();

            HullStrength = MaxHullStrength;
            AllShieldsFailed = false;

            recalculateShields();

            Destroyed = false;

            if (healthBarTrans != null)
            {
                healthBarTrans.gameObject.SetActive(true);

                recalculateHealthBar();
            }

            if (NameLabel != null)
            {
                NameLabel.SetLabelColour(Color.white);
                NameLabel.SetBackgroundColour(Color.black);
            }

            if (FactionLabel != null)
            {
                FactionLabel.SetLabelColour(Color.white);
                FactionLabel.SetBackgroundColour(Color.black);
            }

            if (Stats != null)
            {
                Stats.assistList.Clear();
            }

            if (Controller != null)
            {
                Controller.booted = true;
                Controller.reset();
                Controller.start();
            }

            SystemsInitiated = true;

            base.reset();
        }

        public override void destroy()
        {
            foreach (Device device in Devices)
            {
                device.destroy();
            }

            foreach (Module module in Modules)
            {
                module.destroy();
            }

            foreach (Weapon weapon in Weapons)
            {
                weapon.destroy();
            }

            if (Gamemode.canRespawn(Controller, this) == true)
            {
                Ship ship = this as Ship;

                if (ship != null && (ship.Classification == ShipClassification.FIGHTER || ship.Classification == ShipClassification.BOMBER))
                {
                    return;
                }

                Call_Respawn(this, new RespawnEventArgs(this));
            }
        }
        #endregion

        #region getters
        public bool isInvisible()
        {
            return Invisible;
        }
        public bool isDestroyed()
        {
            return Destroyed;
        }

        /* device getters */
        #region device getters

        public T getDevice<T>() where T : IDevice
        {
            T matchingDevice = default(T);

            foreach (Device device in Devices)
            {
                matchingDevice = device.GetComponentInChildren<T>();

                if (matchingDevice != null) break;
            }

            return matchingDevice;
        }

        public List<T> getDevices<T>() where T : IDevice
        {
            List<T> matchingDevices = new List<T>();

            foreach (Device device in Devices)
            {
                T matchingDevice = device.GetComponentInChildren<T>();

                if (matchingDevice != null)
                {
                    matchingDevices.Add(matchingDevice);
                }
            }

            return matchingDevices;
        }

        public List<Device> getAllDevices()
        {
            return Devices;
        }

        public GameObject getDeviceGameObject<T>() where T : IDevice
        {
            foreach (Device device in Devices)
            {
                if (device is T)
                {
                    return device.gameObject;
                }
            }

            return null;
        }

        public List<GameObject> getDevicesGameObjects<T>() where T : IDevice
        {
            List<GameObject> matchingDevObjs = new List<GameObject>();

            foreach (Device device in Devices)
            {
                if (device is T)
                {
                    matchingDevObjs.Add(device.gameObject);
                }
            }

            return matchingDevObjs;
        }

        public List<GameObject> getAllDeviceGameObjects()
        {
            List<GameObject> devObjs = new List<GameObject>();

            foreach (Device device in Devices)
            {
                devObjs.Add(device.gameObject);
            }

            return devObjs;
        }
        #endregion

        /* socket getters */
        #region socket getters
        public StructureSocket getSocketByName(string socketName)
        {
            StructureSocket socket = null;

            Transform socketTrans = transform.Find(socketName);

            if (socketTrans != null)
            {
                socket = socketTrans.GetComponent<StructureSocket>();
            }

            return socket;
        }

        public T getSocket<T>() where T : ISocket
        {
            T matchingSocket = default(T);

            foreach (StructureSocket socket in StructureSockets)
            {
                matchingSocket = socket.GetComponentInChildren<T>();

                if (matchingSocket != null) break;
            }

            return matchingSocket;
        }

        public List<T> getSockets<T>() where T : ISocket
        {
            List<T> matchingSockets = new List<T>();

            foreach (StructureSocket socket in StructureSockets)
            {
                T matchingSocket = socket.GetComponentInChildren<T>();

                if (matchingSocket != null)
                {
                    matchingSockets.Add(matchingSocket);
                }
            }

            return matchingSockets;
        }

        public List<StructureSocket> getAllSockets()
        {
            return StructureSockets;
        }

        public GameObject getSocketGameObject<T>() where T : ISocket
        {
            foreach (StructureSocket socket in StructureSockets)
            {
                if (socket is T)
                {
                    return socket.gameObject;
                }
            }

            return null;
        }

        public List<GameObject> getSocketsGameObjects<T>() where T : ISocket
        {
            List<GameObject> matchingSocketObjs = new List<GameObject>();

            foreach (StructureSocket socket in StructureSockets)
            {
                if (socket is T)
                {
                    matchingSocketObjs.Add(socket.gameObject);
                }
            }

            return matchingSocketObjs;
        }

        public List<GameObject> getAllSocketGameObjects()
        {
            List<GameObject> socketObjs = new List<GameObject>();

            foreach (StructureSocket socket in StructureSockets)
            {
                socketObjs.Add(socket.gameObject);
            }

            return socketObjs;
        }
        #endregion

        /* module getters */
        #region module getters	
        public Module getModuleInSocket(string socketLabel)
        {
            StructureSocket structureSocket = findSocketByName(socketLabel);

            if (structureSocket != null)
            {
                Module module = structureSocket.InstalledModule;

                if (module is Weapon) return null;
                else return structureSocket.InstalledModule;
            }

            return null;
        }

        public T getModuleInSocket<T>(string socketLabel) where T : IModule
        {
            T module = default(T);

            StructureSocket structureSocket = findSocketByName(socketLabel);

            if (structureSocket != null)
            {
                module = structureSocket.InstalledModule.GetComponentInChildren<T>();
            }

            return module;
        }

        public T getModule<T>() where T : IModule
        {
            T matchingModule = default(T);

            foreach (Module module in Modules)
            {
                matchingModule = module.GetComponentInChildren<T>();

                if (matchingModule != null)
                {
                    break;
                }
            }

            return matchingModule;
        }

        public List<T> getModules<T>() where T : IModule
        {
            List<T> matchingModules = new List<T>();

            foreach (Module module in Modules)
            {
                T matchingModule = module.GetComponentInChildren<T>();

                if (matchingModule != null)
                {
                    matchingModules.Add(matchingModule);
                }
            }

            return matchingModules;
        }

        public List<Module> getAllModules()
        {
            return Modules;
        }

        public GameObject getModuleGameObject<T>() where T : IModule
        {
            foreach (Module module in Modules)
            {
                if (module is T)
                {
                    return module.gameObject;
                }
            }

            return null;
        }

        public List<GameObject> getModuleGameObjects<T>() where T : IModule
        {
            List<GameObject> matchingModObjs = new List<GameObject>();

            foreach (Module module in Modules)
            {
                if (module is T)
                {
                    matchingModObjs.Add(module.gameObject);
                }
            }

            return matchingModObjs;
        }

        public List<GameObject> getAllModuleGameObjects()
        {
            List<GameObject> modObjs = new List<GameObject>();

            foreach (Module module in Modules)
            {
                modObjs.Add(module.gameObject);
            }

            return modObjs;
        }
        #endregion

        /* weapon getters */
        #region weapon getters

        public Weapon getWeaponInSocket(string socketLabel)
        {
            StructureSocket structureSocket = findSocketByName(socketLabel);

            if (structureSocket != null)
            {
                return structureSocket.InstalledModule as Weapon;
            }

            return null;
        }

        public T getWeaponInSocket<T>(string socketLabel) where T : IWeapon
        {
            T weapon = default(T);

            StructureSocket structureSocket = findSocketByName(socketLabel);

            if (structureSocket != null)
            {
                weapon = structureSocket.InstalledModule.GetComponentInChildren<T>();
            }

            return weapon;
        }

        public T getWeapon<T>() where T : IWeapon
        {
            T matchingWeapon = default(T);

            foreach (Weapon weapon in Weapons)
            {
                matchingWeapon = weapon.GetComponentInChildren<T>();

                if (matchingWeapon != null) break;
            }

            return matchingWeapon;
        }

        public List<T> getWeapons<T>() where T : IWeapon
        {
            List<T> matchingWeapons = new List<T>();

            foreach (Weapon weapon in Weapons)
            {
                T matchingWeapon = weapon.GetComponentInChildren<T>();

                if (matchingWeapon != null)
                {
                    matchingWeapons.Add(matchingWeapon);
                }
            }

            return matchingWeapons;
        }

        public List<Weapon> getWeapons()
        {
            return Weapons;
        }

        public GameObject getWeaponGameObject<T>() where T : IWeapon
        {
            foreach (Weapon weapon in Weapons)
            {
                if (weapon is T)
                {
                    return weapon.gameObject;
                }
            }

            return null;
        }

        public List<GameObject> getWeaponGameObjects<T>() where T : IWeapon
        {
            List<GameObject> matchingWeapObjs = new List<GameObject>();

            foreach (Weapon weapon in Weapons)
            {
                if (weapon is T)
                {
                    matchingWeapObjs.Add(weapon.gameObject);
                }
            }

            return matchingWeapObjs;
        }

        public List<GameObject> getAllWeaponGameObjects()
        {
            List<GameObject> weapObjs = new List<GameObject>();

            foreach (Weapon weapon in Weapons)
            {
                weapObjs.Add(weapon.gameObject);
            }

            return weapObjs;
        }
        #endregion
        #endregion

        #region fitting
        #region device fitting
        public Device addDevice(Device device, Transform systemsParent)
        {
            D.log("Fitting", "Installing " + device.name + " into structure " + gameObject.name);

            device.setStructure(this);

            device.init();
            Devices.Add(device);

            D.log("Structure", device.DeviceData.Type + ":" + device.DeviceData.SubType + " attached to structure " + gameObject.name);

            return device;
        }

        public Device addDevice(string resourcePath, Transform systemsParent)
        {
            Device devicePrefab = Resources.Load<Device>("Devices/" + resourcePath);

            if (devicePrefab == null)
            {
                D.warn("Device: {0}", "Cannot find a device prefab in any Resources/Devices folder with path: " + resourcePath);
                return null;
            }

            Device device = Instantiate(devicePrefab) as Device;

            if (device != null)
            {
                device.DeviceData.ResourcePath = resourcePath;
                device.name = device.name.Remove(device.name.Length - 7);

                if (addDevice(device, systemsParent) != null)
                {
                    device.gameObject.transform.SetParent(systemsParent);
                }
            }
            else
            {
                D.warn("Fitting: {0}", "Cannot instantiate from a device prefab with path: Resources/Devices/" + resourcePath);
            }

            return device;
        }

        public Device addDevice(GameObject devicePrefab, Transform systemsParent)
        {
            if (devicePrefab == null)
            {
                D.warn("Device: {0}", "The device prefab you are attempting to add is null");
                return null;
            }

            GameObject deviceGO = Instantiate(devicePrefab) as GameObject;

            if (deviceGO != null)
            {
                deviceGO.name = deviceGO.name.Remove(deviceGO.name.Length - 7);
                Device device = deviceGO.GetComponent<Device>();

                if (device != null)
                {
                    D.log("Fitting", "Installing " + device.name + " into structure " + gameObject.name);

                    device.setStructure(this);
                    device.gameObject.transform.SetParent(systemsParent);

                    device.init();
                    Devices.Add(device);

                    D.log("Structure", device.DeviceData.Type + ":" + device.DeviceData.SubType + " attached to structure " + gameObject.name);

                    return device;
                }
                else
                {
                    D.warn("Fitting: {0}", "Could not install device " + device.DeviceData.Type);

                    Destroy(deviceGO);
                }
            }
            else
            {
                D.warn("Fitting: {0}", "Cannot instantiate from a device prefab with name " + devicePrefab.name);
            }

            return null;
        }
        #endregion

        #region socket fitting
        public List<string> getSocketNames()
        {
            List<string> socketNames = new List<string>();

            foreach (StructureSocket structureSocket in StructureSockets)
            {
                socketNames.Add(structureSocket.label);
            }

            return socketNames;
        }

        public List<string> getModuleSocketNames()
        {
            List<string> socketNames = new List<string>();

            foreach (StructureSocket structureSocket in StructureSockets)
            {
                WeaponSocket weaponSocket = structureSocket as WeaponSocket;

                // if not a weapon socket, must be a module socket
                if (weaponSocket == null)
                {
                    socketNames.Add(structureSocket.label);
                }
            }

            return socketNames;
        }

        public List<string> getWeaponSocketNames()
        {
            List<string> socketNames = new List<string>();

            foreach (StructureSocket structureSocket in StructureSockets)
            {
                WeaponSocket weaponSocket = structureSocket as WeaponSocket;

                if (weaponSocket != null)
                {
                    socketNames.Add(weaponSocket.label);
                }
            }

            return socketNames;
        }

        public GameObject findSocketGOByName(string name)
        {
            foreach (StructureSocket structureSocket in StructureSockets)
            {
                if (structureSocket != null && structureSocket.name.ToUpper().Equals(name.ToUpper()))
                {
                    return structureSocket.gameObject;
                }
            }

            return null;
        }

        public StructureSocket findSocketByName(string name)
        {
            foreach (StructureSocket structureSocket in StructureSockets)
            {
                if (structureSocket != null && structureSocket.name.ToUpper().Equals(name.ToUpper()))
                {
                    return structureSocket;
                }
            }

            return null;
        }

        public bool addSocket(StructureSocket structureSocket)
        {
            if (findSocketByName(structureSocket.label) != null)
            {
                D.warn("Fitting: {0}", "Cannot fit another socket with the name " + structureSocket.label + " to the structure " + Name);
                return false;
            }

            if (structureSocket != null)
            {
                D.log("Fitting", "Installing socket " + structureSocket.name + " on " + gameObject.name + " at position: " + structureSocket.transform.position.ToString("0.0000000"));

                if (structureSize >= structureSocket.minStructureSize && structureSize <= structureSocket.maxStructureSize)
                {
                    structureSocket.init();
                    StructureSockets.Add(structureSocket);
                    return true;
                }
                else
                {
                    D.warn("Fitting: {0}", structureSocket.label + " will not fit onto " + name + " due to having an incompatible socket size");
                    return false;
                }
            }
            else
            {
                D.warn("Socket: {0}", "Structure socket is null");
                return false;
            }
        }

        public GameObject addSocket(string resourcePath, string socketName, Vector2 position, float? rotation = null)
        {
            GameObject socketPrefab = Resources.Load<GameObject>("Sockets/" + resourcePath);

            if (socketPrefab == null)
            {
                D.warn("Socket: {0}", "Cannot find a socket prefab in any Resources/Sockets folder with path: " + resourcePath);
                return null;
            }

            GameObject structureSocketGO = Instantiate(socketPrefab) as GameObject;
            StructureSocket structureSocket = structureSocketGO.GetComponent<StructureSocket>();

            if (structureSocketGO != null && structureSocket != null)
            {
                structureSocketGO.name = socketName;

                // reset this parent in the custom builder if desired
                structureSocketGO.transform.parent = gameObject.transform;
                structureSocketGO.transform.localPosition = position;

                if (addSocket(structureSocket) == true)
                {
                    if (rotation != null)
                    {
                        structureSocket.transform.rotation = Quaternion.Euler(0, 0, rotation.GetValueOrDefault());
                    }

                    return structureSocketGO;
                }
                else
                {
                    Destroy(structureSocketGO);
                    return null;
                }
            }
            else
            {
                if (structureSocketGO == null)
                {
                    D.warn("Socket: {0}", "Cannot instantiate from a socket prefab with path: Resources/Sockets/" + resourcePath);
                }
                else
                {
                    D.warn("Socket: {0}", "Missing StructureSocket script on socket prefab " + structureSocketGO.name);
                }

                return null;
            }
        }
        #endregion

        #region module fitting
        public bool addModule(StructureSocket structureSocket, Module module)
        {
            if (structureSocket == null)
            {
                D.warn("Fitting: {0}", "Cannot add a module to a socket set to null");
                return false;
            }

            if (structureSocket.InstalledModule != null)
            {
                D.warn("Fitting: {0}", "Socket " + structureSocket.label + " already has module " + structureSocket.InstalledModule.GetType() + " : " + structureSocket.InstalledModule.DeviceData.SubType + " installed in it");
                return false;
            }

            if (module != null)
            {
                D.log("Fitting", "Installing module " + module.name + " onto structure " + gameObject.name);

                module.setStructure(this);
                module.init();

                if (structureSocket.install(module.gameObject) == true)
                {
                    Modules.Add(module);

                    D.log("Structure", module.DeviceData.Type + ":" + module.DeviceData.SubType + " attached to socket " + structureSocket.label + " on structure " + gameObject.name);

                    return true;
                }
                else
                {
                    D.warn("Fitting: {0}", "Could not install module " + module.DeviceData.Type + " into socket " + structureSocket.label);
                    return false;
                }
            }
            else
            {
                D.warn("Module: {0}", "No module to install into socket " + structureSocket.gameObject.name);
                return false;
            }
        }

        public bool addModule(GameObject structureSocketGO, Module module)
        {
            if (structureSocketGO == null)
            {
                D.warn("Fitting: {0}", "Cannot install module " + module.DeviceData.Type + " as the socket is set to null");
                return false;
            }

            StructureSocket structureSocket = structureSocketGO.GetComponent<StructureSocket>();

            if (structureSocket != null)
            {
                return addModule(structureSocket, module);
            }

            return false;
        }

        public Module addModule(string socketLabel, string resourcePath)
        {
            GameObject structureSocketGO = findSocketGOByName(socketLabel);

            if (structureSocketGO != null)
            {
                return addModule(structureSocketGO, resourcePath);
            }

            D.warn("Socket: {0}", "No structure socket named " + socketLabel + " to install module into. Check you have created the required socket prefab and built a socket from it before attempting to add a module to it.");

            return null;
        }

        public Module addModule(string socketLabel, GameObject modulePrefab)
        {
            if (modulePrefab == null)
            {
                D.warn("Module: {0}", "The module prefab you are attempting to add to socket " + socketLabel + " is null");
                return null;
            }

            GameObject structureSocketGO = findSocketGOByName(socketLabel);

            if (structureSocketGO != null)
            {
                StructureSocket structureSocket = structureSocketGO.GetComponent<StructureSocket>();

                if (structureSocket != null)
                {
                    if (structureSocket.InstalledModule != null)
                    {
                        D.warn("Fitting: {0}", "Socket " + structureSocket.label + " already has module " + structureSocket.InstalledModule.GetType() + " : " + structureSocket.InstalledModule.DeviceData.SubType + " installed in it");
                        return null;
                    }

                    GameObject moduleGO = Instantiate(modulePrefab) as GameObject;

                    if (moduleGO != null)
                    {
                        moduleGO.name = moduleGO.name.Remove(moduleGO.name.Length - 7);
                        Module module = moduleGO.GetComponent<Module>();

                        if (module != null)
                        {
                            D.log("Fitting", "Installing module " + module.name + " onto structure " + gameObject.name);

                            module.setStructure(this);
                            module.init();

                            if (structureSocket.install(module.gameObject) == true)
                            {
                                Modules.Add(module);

                                D.log("Structure", module.DeviceData.Type + ":" + module.DeviceData.SubType + " attached to socket " + structureSocket.label + " on structure " + gameObject.name);

                                return module;
                            }
                            else
                            {
                                D.warn("Fitting: {0}", "Could not install module " + module.DeviceData.Type + " into socket " + structureSocket.label);

                                Destroy(moduleGO);

                                return null;
                            }
                        }
                        else
                        {
                            D.warn("Module: {0}", "No module to install into socket " + socketLabel);
                        }
                    }
                    else
                    {
                        D.warn("Fitting: {0}", "Cannot instantiate from a module prefab with name " + modulePrefab.name);
                    }
                }
                else
                {
                    D.warn("Socket: {0}", "No structure socket attached to " + structureSocketGO.name);
                }
            }
            else
            {
                D.warn("Socket: {0}", "No structure socket GameObject to install module into. Check you have created the required socket prefab and built a socket from it before attempting to add a module to it.");
            }

            return null;
        }

        public Module addModule(GameObject structureSocketGO, string resourcePath)
        {
            GameObject modulePrefab = Resources.Load<GameObject>("Modules/" + resourcePath);

            if (modulePrefab == null)
            {
                D.warn("Module: {0}", "Cannot find a module prefab in any Resources/Modules folder with path: " + resourcePath);
                return null;
            }

            GameObject moduleGO = Instantiate(modulePrefab) as GameObject;

            if (moduleGO != null)
            {
                moduleGO.name = moduleGO.name.Remove(moduleGO.name.Length - 7);
                Module module = moduleGO.GetComponent<Module>();
                module.DeviceData.ResourcePath = resourcePath;

                if (addModule(structureSocketGO, module) == false)
                {
                    Destroy(moduleGO);
                    return null;
                }
            }
            else
            {
                D.warn("Fitting: {0}", "Cannot instantiate from a module prefab with path: Resources/Modules/" + resourcePath);
            }

            return moduleGO.GetComponent<Module>();
        }
        #endregion

        #region weapon fitting
        public bool addWeapon(StructureSocket structureSocket, Weapon weapon)
        {
            if (structureSocket == null)
            {
                D.warn("Fitting: {0}", "Cannot add a weapon to a socket set to null");
                return false;
            }

            if (structureSocket.InstalledModule != null)
            {
                D.warn("Fitting: {0}", "Socket " + structureSocket.label + " already has weapon " + structureSocket.InstalledModule.DeviceData.Type + " : " + structureSocket.InstalledModule.DeviceData.SubType + " installed in it");
                return false;
            }

            if (weapon != null)
            {
                D.log("Fitting", "Installing weapon " + weapon.name + " onto structure " + gameObject.name);

                weapon.setStructure(this);
                weapon.init();

                if (structureSocket.install(weapon.gameObject) == true)
                {
                    Weapons.Add(weapon);

                    D.log("Structure", weapon.DeviceData.Type + ":" + weapon.DeviceData.SubType + " attached to socket " + structureSocket.label + " on structure " + gameObject.name);

                    return true;
                }
                else
                {
                    D.warn("Fitting: {0}", "Could not install weapon " + weapon.DeviceData.Type + " into socket " + structureSocket.label);
                    return false;
                }
            }
            else
            {
                D.warn("Weapon: {0}", "No weapon to install into socket " + structureSocket.gameObject.name);
                return false;
            }
        }

        public bool addWeapon(GameObject structureSocketGO, Weapon weapon)
        {
            if (structureSocketGO == null)
            {
                D.warn("Fitting: {0}", "Cannot install weapon " + weapon.DeviceData.Type + " as the socket is set to null");
                return false;
            }

            StructureSocket structureSocket = structureSocketGO.GetComponent<StructureSocket>();

            if (structureSocket != null)
            {
                return addWeapon(structureSocket, weapon);
            }

            return false;
        }

        public Weapon addWeapon(string socketLabel, string resourcePath)
        {
            GameObject structureSocketGO = findSocketGOByName(socketLabel);

            if (structureSocketGO != null)
            {
                return addWeapon(structureSocketGO, resourcePath);
            }

            D.warn("Socket: {0}", "No structure socket named " + socketLabel + " to install weapon into. Check you have created the required socket prefab and built a socket from it before attempting to add a weapon to it.");

            return null;
        }

        public Weapon addWeapon(string socketLabel, GameObject weaponPrefab)
        {
            if (weaponPrefab == null)
            {
                D.warn("Weapon: {0}", "The weapon prefab you are attempting to add to socket " + socketLabel + " is null");
                return null;
            }

            GameObject structureSocketGO = findSocketGOByName(socketLabel);

            if (structureSocketGO != null)
            {
                StructureSocket structureSocket = structureSocketGO.GetComponent<StructureSocket>();

                if (structureSocket != null)
                {
                    if (structureSocket.InstalledModule != null)
                    {
                        D.warn("Fitting: {0}", "Socket " + structureSocket.label + " already has weapon " + structureSocket.InstalledModule.GetType() + " : " + structureSocket.InstalledModule.DeviceData.SubType + " installed in it");
                        return null;
                    }

                    GameObject weaponGO = Instantiate(weaponPrefab) as GameObject;

                    if (weaponGO != null)
                    {
                        weaponGO.name = weaponGO.name.Remove(weaponGO.name.Length - 7);
                        Weapon weapon = weaponGO.GetComponent<Weapon>();

                        if (weapon != null)
                        {
                            D.log("Fitting", "Installing weapon " + weapon.name + " onto structure " + gameObject.name);

                            weapon.setStructure(this);
                            weapon.init();

                            if (structureSocket.install(weapon.gameObject) == true)
                            {
                                Weapons.Add(weapon);

                                D.log("Structure", weapon.DeviceData.Type + ":" + weapon.DeviceData.SubType + " attached to socket " + structureSocket.label + " on structure " + gameObject.name);

                                return weapon;
                            }
                            else
                            {
                                D.warn("Fitting: {0}", "Could not install weapon " + weapon.DeviceData.Type + " into socket " + structureSocket.label);

                                Destroy(weaponGO);

                                return null;
                            }
                        }
                        else
                        {
                            D.warn("Weapon: {0}", "No weapon to install into socket " + socketLabel);
                        }
                    }
                    else
                    {
                        D.warn("Fitting: {0}", "Cannot instantiate from a weapon prefab with name " + weaponPrefab.name);
                    }
                }
                else
                {
                    D.warn("Socket: {0}", "No structure socket attached to " + structureSocketGO.name);
                }
            }
            else
            {
                D.warn("Socket: {0}", "No structure socket GameObject to install weapon into. Check you have created the required socket prefab and built a socket from it before attempting to add a weapon to it.");
            }

            return null;
        }

        public Weapon addWeapon(GameObject structureSocketGO, string resourcePath)
        {
            GameObject weaponPrefab = Resources.Load<GameObject>("Weapons/" + resourcePath);

            if (weaponPrefab == null)
            {
                D.warn("Weapon: {0}", "Cannot find a weapon prefab in any Resources/Weapons folder with path: " + resourcePath);
                return null;
            }

            GameObject weaponGO = Instantiate(weaponPrefab) as GameObject;

            if (weaponGO != null)
            {
                weaponGO.name = weaponGO.name.Remove(weaponGO.name.Length - 7);
                Weapon weapon = weaponGO.GetComponent<Weapon>();

                if (addWeapon(structureSocketGO, weapon) == false)
                {
                    Destroy(weaponGO);
                    return null;
                }
            }
            else
            {
                D.warn("Fitting: {0}", "Cannot instantiate from a weapon prefab with path: Resources/Weapons/" + resourcePath);
            }

            return weaponGO.GetComponent<Weapon>();
        }
        #endregion
        #endregion

        #region Controller

        public void detachController()
        {
            Controller.booted = false;
            Controller.enabled = false;
            Controller = null;
        }

        public void attachController(StructureController controller)
        {
            Controller = controller;

            if (Gamemode.addController(this) == true)
            {
                D.log("Structure", "Controller attached to structure " + gameObject.name);
            }
            else
            {
                D.warn("Structure: {0}", "Could not add controller to structure " + gameObject.name);
            }
        }
        #endregion

        #region Hull, Shields & Damage
        public void recalculateShields()
        {
            ShieldStrength = 0;

            foreach (IShieldGenerator shield in shields)
            {
                MaxShieldStrength += shield.ShieldGeneratorData.MaxCharge;

                if (shield.isShieldUp() == true)
                {
                    ShieldStrength += shield.CurrentCharge;
                }
            }

            MaxShieldStrength = ShieldStrength;
        }

        public void increaseHullStrength(float amount)
        {
            if (HullStrength < MaxHullStrength && Destroyed == false)
            {
                HullStrength = Mathf.Min(HullStrength + amount, MaxHullStrength);

                if (HullStrength > notifyHullLowPercent * MaxHullStrength && notifiedHullLow == true)
                {
                    notifiedHullLow = false;
                    notifiedHullBreached = false;
                }
                else if (HullStrength > notifyHullBreachPercent * MaxHullStrength && notifiedHullBreached == true)
                {
                    notifiedHullBreached = false;
                }
            }
        }

        public virtual bool takeDamage(GameObject collidedObject, float damage, IWeapon weapon, (GameObject structure, GameObject system)? target, Projectile projectile = null)
        {
            // check flags to quickly return (TODO - should I call an event to register a hit regardless?)
            if ((Gamemode != null && Gamemode.damageOn == false) || Destroyed == true || CanBeDamaged == false || damage == 0)
            {
                return false;
            }

            Weapon weaponOriginator = weapon as Weapon;

            // TODO - change to an event dispatcher?
            if (weapon != null && weaponOriginator.WeaponData.CanReveal == true)
            {
                // if a cloaked ship takes any damage, its cloaking device deactivates
                ICloakingDevice cloakingDevice = getModule<ICloakingDevice>() as ICloakingDevice;

                if (cloakingDevice != null && cloakingDevice.isCloakActive() == true)
                {
                    cloakingDevice.deactivateCloakImmediately();
                }
            }

            LastHitBy = null;

            // D.log("Structure", gameObject.name + " has " + damage + " damage incoming");

            #region Damage handling
            if (damage != Mathf.Infinity)
            {
                if (weapon != null)
                {
                    #region Damage modifiers
                    // calculate damage modifier for the weapon
                    IDamageModifier weaponMod = weapon as IDamageModifier;

                    if (weaponMod != null)
                    {
                        damage = weaponMod.damageModifier(collidedObject, damage, weaponOriginator, target, projectile);
                    }

                    // calculate damage modifier for the projectile (if this hit the structure)
                    if (projectile != null)
                    {
                        IDamageModifier projectileMod = projectile as IDamageModifier;

                        if (projectileMod != null)
                        {
                            damage = projectileMod.damageModifier(collidedObject, damage, weaponOriginator, target, projectile);
                        }
                    }
                    #endregion

                    // D.log("Structure", gameObject.name + " has " + damage + " modified damage incoming");

                    #region Stat handling
                    if (Stats != null)
                    {
                        Stats.damageThisLife += damage;
                        Stats.totalDamageTaken += damage;
                    }

                    // who hit the structure?
                    LastHitBy = weapon.getStructure();

                    if (weaponOriginator != null)
                    {
                        Structure damageCauser = weaponOriginator.getStructure();

                        // call the TakenAnyDamage event on this structure
                        Call_TakenAnyDamage(this, new DamageEventArgs(damage, this, damageCauser));

                        // call the InstigatedAnyDamage event on the weapon owner's AI controller (if present)
                        if (damageCauser != null)
                        {
                            damageCauser.Call_InstigatedAnyDamage(this, new DamageEventArgs(damage, this, damageCauser));
                        }
                    }

                    // record damage for attacker if it stores stats
                    Structure attackerStructure = weapon.getStructure();

                    if (attackerStructure.Stats != null)
                    {
                        attackerStructure.Stats.totalDamageInflicted += damage;
                    }

                    // record modified damage for this hit and update weapon owner's damage entry in assist list
                    int i = 0;
                    bool foundEntry = false;
                    float prevDamage = 0;

                    if (Stats != null)
                    {
                        Stats.actualDamageThisLife += damage;

                        foreach ((Structure structure, float damage) entry in Stats.assistList)
                        {
                            if (entry.structure == LastHitBy)
                            {
                                prevDamage = entry.damage;
                                foundEntry = true;
                                break;
                            }

                            i++;
                        }

                        if (foundEntry == false)
                        {
                            Stats.assistList.Add((LastHitBy, damage));
                        }
                        else
                        {
                            Stats.assistList.RemoveAt(i);
                            Stats.assistList.Add((LastHitBy, prevDamage + damage));
                        }
                    }
                    #endregion
                }

                // store current damage for structure to absorb
                float remainingDamage = damage;

                // assume all damage goes to the shields first
                float shieldDamage = damage;

                // set bleed damage to 0
                float bleedDamage = 0;

                try
                {
                    // if ship has shields and they are up, let shields absorb as much damage as possible and determine remaining damage 
                    if (shields.Count > 0)
                    {
                        AllShieldsFailed = true;
                        MaxShieldStrength = 0;
                        ShieldStrength = 0;

                        foreach (IShieldGenerator shield in shields)
                        {
                            MaxShieldStrength += shield.ShieldGeneratorData.MaxCharge;

                            if (shield.isShieldUp() == true)
                            {
                                AllShieldsFailed = false;

                                // get current shield strength
                                float shieldStrength = shield.CurrentCharge;

                                ShieldStrength += shieldStrength;

                                // if we have bleed damage from a previous shield to resolve then add it to the remaining damage
                                if (bleedDamage > 0)
                                {
                                    remainingDamage += bleedDamage;
                                    bleedDamage = 0;
                                }

                                // continue loop if no more damage to absorb
                                if (remainingDamage == 0) continue;

                                // get current shield percentage
                                float shieldPercentage = shieldStrength / shield.ShieldGeneratorData.MaxCharge;

                                // determine shield reduction and bleed through								
                                if (shieldPercentage < shield.ShieldGeneratorData.WeakFraction)
                                {
                                    if (target != null && target.GetValueOrDefault().system == null)
                                    {
                                        bleedDamage = shield.ShieldGeneratorData.BleedFraction * damage;
                                        damage = shield.OneMinusBleedFraction * damage;
                                    }
                                    else
                                    {
                                        // TODO - change this * 2.0f or whatever for a weapon precision fraction/factor (equivalent to reducing the aspect ratio of the weapon / concentrated firepower)
                                        float precisionBleedFraction = Mathf.Clamp01(shield.ShieldGeneratorData.BleedFraction * 2.0f);

                                        bleedDamage = precisionBleedFraction * damage;
                                        damage = (1.0f - precisionBleedFraction) * damage;
                                    }
                                }

                                if (damage > shieldStrength)
                                {
                                    shield.failed();

                                    remainingDamage -= shieldStrength;
                                    remainingDamage = Mathf.Max(remainingDamage, 0);
                                }
                                else
                                {
                                    shield.decreaseCharge(damage);
                                    remainingDamage = 0;
                                }

                                shield.hit(damage / shieldStrength);
                            }
                        }

                        if (Stats != null)
                        {
                            Stats.totalShieldDamageTaken += (shieldDamage - (remainingDamage + bleedDamage));
                        }

                        // D.log("Structure", gameObject.name + " has taken " + totalShieldDamageTaken + " total shield damage ");
                    }
                }
                catch (Exception e3)
                {
                    // D.log ("Exception", "Shield damage exception: " + e3.ToString());
                }

                // D.log("Structure", gameObject.name + " has taken " + (remainingDamage + bleedDamage) + " structure damage");

                // TODO - note that if a module takes damage then we might want to damage the hull if the module is destroyed
                if (target == null) // Area of Effect damage etc.
                {
                    HullStrength -= (remainingDamage + bleedDamage);

                    if (Stats != null)
                    {
                        Stats.totalHullDamageTaken += (remainingDamage + bleedDamage);
                    }
                }
                else
                {
                    GameObject targetSystem = target.GetValueOrDefault().system;

                    if (targetSystem == null)
                    {
                        HullStrength -= (remainingDamage + bleedDamage);

                        if (Stats != null)
                        {
                            Stats.totalHullDamageTaken += (remainingDamage + bleedDamage);
                        }
                    }
                    else
                    {
                        Module module = targetSystem.GetComponent<Module>();

                        if (module != null)
                        {
                            damageReport = module.decreaseArmour(remainingDamage + bleedDamage);

                            // second part of the damage report is whether any damage has occured to the structure due to the module exploding
                            HullStrength -= damageReport.damageOnDestroy;

                            if (Stats != null)
                            {
                                Stats.totalArmourDamageTaken += (remainingDamage + bleedDamage);
                            }

                            if (damageReport.destroyed == true)
                            {
                                // TODO - strictly this is really module destroyed event so think of moving it for all damage to the targeted module
                                Call_ModuleDamaged(this, new ModuleDamageEventArgs(LastHitBy, this, targetSystem.GetComponent<Module>(), remainingDamage + bleedDamage, true));

                                if (weapon != null)
                                {
                                    Structure attackerStructure = weapon.getStructure();

                                    if (attackerStructure.Stats != null)
                                    {
                                        attackerStructure.Stats.numModulesDestroyed++;
                                    }

                                    Gamemode.Gui.setMessage(gameObject.name + " lost its " + targetSystem.GetComponent<Module>().DeviceData.Type.ToLower() + " to " + LastHitBy.name);
                                }
                                else
                                {
                                    Gamemode.Gui.setMessage(gameObject.name + " lost its " + targetSystem.GetComponent<Module>().DeviceData.Type.ToLower());
                                }

                                Call_TargetDestroyed(this, new TargetDestroyedEventArgs(this, targetSystem.GetComponent<Module>(), LastHitBy, weaponOriginator));
                            }
                            else
                            {
                                Call_ModuleDamaged(this, new ModuleDamageEventArgs(LastHitBy, this, targetSystem.GetComponent<Module>(), remainingDamage + bleedDamage, false));
                            }
                        }
                    }
                }
            }
            else
            {
                HullStrength -= damage;
            }

            // hull messages
            if (HullStrength != Mathf.Infinity)
            {
                if (HullStrength <= notifyHullBreachPercent * MaxHullStrength && notifiedHullBreached == false)
                {
                    Gamemode.Gui.setMessage(gameObject.name + " hull breached");
                    notifiedHullBreached = true;
                }
                else if (HullStrength <= notifyHullLowPercent * MaxHullStrength && notifiedHullLow == false)
                {
                    Gamemode.Gui.setMessage(gameObject.name + " hull integrity low");
                    notifiedHullLow = true;
                }
            }
            #endregion

            #region Structure destruction
            if (HullStrength <= 0)
            {
                // D.log("Structure", gameObject.name + " hull has failed!");

                HullStrength = 0;

                AIStateController stateController = Controller as AIStateController;

                if (stateController != null)
                {
                    stateController.DebugState = false;
                }

                if (weapon != null)
                {
                    GameManager.Instance.MainCamera.setLastKillBy(LastHitBy.transform);

                    if (LastHitBy.Command != null)
                    {
                        Gamemode.Gui.setMessage(gameObject.name + " has been destroyed by " + LastHitBy.Command.rankData.abbreviation + " " + LastHitBy.Command.label + " in " + LastHitBy.gameObject.name + "!");
                    }
                }
                else
                {
                    Gamemode.Gui.setMessage(gameObject.name + " has been destroyed");
                }

                #region VFX and UI

                // update the health bar
                recalculateHealthBar();

                // disable the shield mesh (if present)
                if (shields.Count > 0)
                {
                    // just use the reference to the first shield for disabling
                    shields[0].failed();
                }

                // randomly spawn module explosions 
                for (int moduleIndex = 0; moduleIndex < StructureSockets.Count; moduleIndex++)
                {
                    if (UnityEngine.Random.value > 0.5f)
                    {
                        if (StructureSockets[moduleIndex].InstalledModule != null)
                        {
                            StructureSockets[moduleIndex].InstalledModule.explode(2 * ((int)(structureSize) + 1));
                        }
                    }
                }

                // spawn structure explosion
                if (explosionInitial != null)
                {
                    GameObject clonedExplosion = explosionInitial.Spawn(transform);

                    explosionVFXController = clonedExplosion.GetComponent<ExplosionVFXController>();

                    // make any changes to the explosion here (if any)
                    explosionVFXController.setSortingLayerOrder(transform, -1);
                }

                // change structure labels to red				
                if (FactionLabel != null)
                {
                    FactionLabel.GetComponent<FactionLabel>().SetLabelColour(Color.red);
                }

                if (NameLabel != null)
                {
                    NameLabel.GetComponent<NameLabel>().SetLabelColour(Color.red);
                }
                #endregion

                // call the despawn event dispatcher (removes fitting vfxs etc.)
                Call_Despawn(this, new DespawnEventArgs(this));

                // set destroyed flag
                Destroyed = true;

                // call the NotifyKiller event on the damage causer's structure
                if (LastHitBy != null)
                {
//                    if (target != null)
//                    {
                        LastHitBy.Call_NotifyKiller(this, new TargetDestroyedEventArgs(this, null, LastHitBy, weaponOriginator));
//                    }
                }

                // call the NotifyKilled and TargetDestroyed events on this structure
                Call_NotifyKilled(this, new TargetDestroyedEventArgs(this, null, LastHitBy, weaponOriginator));
                Call_TargetDestroyed(this, new TargetDestroyedEventArgs(this, null, LastHitBy, weaponOriginator));

                // stop the controller if present
                if (Controller != null)
                {
                    // D.log("Controller", "Controller for " + gameObject.name + " has gone offline!");
                    Controller.booted = false;
                    //Controller.StopCoroutine(Controller.update());
                    Controller.stop();
                }

                #region Destruction stat handling
                // update attacker stats
                if (weapon != null && LastHitBy.Stats != null)
                {
                    LastHitBy.Stats.numKills++;
                }

                // update structure stats
                if (Stats != null)
                {
                    Stats.numDeaths++;

                    foreach ((Structure structure, float damage) entry in Stats.assistList)
                    {
                        if (entry.damage / Stats.actualDamageThisLife >= 0.2f)
                        {
                            Call_NotifyAssister(this, new AssistEventArgs(entry.structure, this, entry.damage / Stats.actualDamageThisLife));

                            // record assits in structure Stats
                            entry.structure.Stats.numAssists++;
                        }
                    }

                    // zero out damage this life
                    Stats.damageThisLife = 0;
                    Stats.actualDamageThisLife = 0;

                    Stats.survivalTimes.Add((float)(AliveTimer.Elapsed.TotalSeconds));
                }

                // add alive time to current list of survival times
                Call_SurvivalTimeUpdated(this, new SurvivalTimeEventArgs(this, (float)(AliveTimer.Elapsed.TotalSeconds), false));

                // calculates the current average survivial time
                calculateAverageSurvivalTime(false);

                // stop the alive timer until respawn
                stopSurvivalClock();

                // reset alive timer
                resetSurvivalClock();
                #endregion

                // set flag for systems initiated
                SystemsInitiated = false;

                return true;
            }
            #endregion

            return false;
        }
        #endregion

        #region power readout
        public string getPowerRequirements()
        {
            powerInfo.Length = 0;
            coresInfo.Length = 0;
            devicesInfo.Length = 0;
            modulesInfo.Length = 0;

            int powerPercent;
            string powerActual;

            // get general powergrid info
            if (powergrid != null)
            {
                powerPercent = (int)(powergrid.getCurrentPower() * 100 / powergrid.getMaxPower());
                powerActual = (int)(powergrid.getCurrentPower()) + " / " + (int)(powergrid.getMaxPower());
            }
            else
            {
                powerPercent = 0;
                powerActual = "0 / 0";
            }

            // get powercore generation info
            float powerTotalInst = 0;
            float powerTotalPerSec = 0;

            foreach (PowerCore powercore in powergenerators)
            {
                if (powercore != null)
                {
                    coresInfo.Append("\n");
                    coresInfo.Append(powercore.GetType());
                    coresInfo.Append(": ");

                    if (powercore.DeviceData.ActiveOn)
                    {
                        coresInfo.Append((powercore.PowerCoreData.PowerGeneration * Time.deltaTime).ToString("F2"));
                        powerTotalInst += (powercore.PowerCoreData.PowerGeneration * Time.deltaTime);
                        powerTotalPerSec += powercore.PowerCoreData.PowerGeneration;
                    }
                    else
                    {
                        coresInfo.Append("0");
                    }

                    coresInfo.Append(" : ");
                    coresInfo.Append((int)(powercore.PowerCoreData.PowerGeneration));
                }
            }

            string coresTotals = "\n\nCore Total: " + powerTotalInst.ToString("F2") + " : " + powerTotalPerSec.ToString("F2");

            // get device power info
            float devicesTotalInst = 0;
            float devicesTotalPerSec = 0;

            int slotID = 0;

            foreach (Device device in Devices)
            {
                if (device != null)
                {
                    devicesInfo.Append("\nSlot ");
                    devicesInfo.Append(slotID);
                    devicesInfo.Append(": ");
                    devicesInfo.Append(device.DeviceData.Type);
                    devicesInfo.Append(": ");

                    if (device.DeviceData.ActiveOn)
                    {
                        devicesInfo.Append((device.DeviceData.RequiredPower * Time.deltaTime).ToString("F2"));
                        devicesTotalInst += (device.DeviceData.RequiredPower * Time.deltaTime);
                        devicesTotalPerSec += device.DeviceData.RequiredPower;
                    }
                    else
                    {
                        devicesInfo.Append("0");
                    }

                    devicesInfo.Append(" : ");
                    devicesInfo.Append((int)(device.DeviceData.RequiredPower));
                }

                slotID++;
            }

            string devicesTotals = "\n\nDevice Total: " + devicesTotalInst.ToString("F2") + " : " + devicesTotalPerSec.ToString("F2");

            // get module power info
            float modulesTotalInst = 0;
            float modulesTotalPerSec = 0;

            slotID = 0;

            foreach (StructureSocket structureSocket in StructureSockets)
            {
                if (structureSocket.InstalledModule != null)
                {
                    modulesInfo.Append("\nSlot ");
                    modulesInfo.Append(slotID);
                    modulesInfo.Append(" ");
                    modulesInfo.Append(structureSocket.label);
                    modulesInfo.Append(": ");
                    modulesInfo.Append(structureSocket.InstalledModule.DeviceData.Type);
                    modulesInfo.Append(": ");

                    if (structureSocket.InstalledModule.DeviceData.ActiveOn)
                    {
                        modulesInfo.Append((structureSocket.InstalledModule.DeviceData.RequiredPower * Time.deltaTime).ToString("F2"));
                        modulesTotalInst += (structureSocket.InstalledModule.DeviceData.RequiredPower * Time.deltaTime);
                        modulesTotalPerSec += structureSocket.InstalledModule.DeviceData.RequiredPower;
                    }
                    else
                    {
                        modulesInfo.Append("0");
                    }

                    modulesInfo.Append(" : ");
                    modulesInfo.Append((int)(structureSocket.InstalledModule.DeviceData.RequiredPower));

                    slotID++;
                }
            }

            string modulesTotals = "\n\nModule Total: " + modulesTotalInst.ToString("F2") + " : " + modulesTotalPerSec.ToString("F2");

            string powerStability;

            float powerReqPerSec = devicesTotalPerSec + modulesTotalPerSec;
            float powerDiff = powerTotalPerSec - powerReqPerSec;

            if (powerTotalPerSec >= powerReqPerSec)
            {
                powerStability = "Stable +" + powerDiff.ToString("F2");
            }
            else
            {
                powerStability = "Loss " + powerDiff.ToString("F2") + " (" + (powergrid.getCurrentPower() / powerDiff).ToString("F2") + "s)";
            }

            //powerInfo.Append(gameObject.name);
            powerInfo.Append("\nPower : ");
            powerInfo.Append(powerActual);
            powerInfo.Append(" : ");
            powerInfo.Append(powerPercent);
            powerInfo.Append("%");
            powerInfo.Append("\nPower generation: ");
            powerInfo.Append(powerStability);
            powerInfo.Append(coresTotals);
            //powerInfo.Append(coresInfo);
            powerInfo.Append("\nTotal Power Required: ");
            powerInfo.Append((devicesTotalInst + modulesTotalInst).ToString("F2"));
            powerInfo.Append(" : ");
            powerInfo.Append(powerReqPerSec.ToString("F2"));
            powerInfo.Append(devicesTotals);
            //powerInfo.Append(devicesInfo);
            powerInfo.Append(modulesTotals);
            //powerInfo.Append(modulesInfo);

            return powerInfo.ToString();
        }
        #endregion

        #region Survival Clock
        public void calculateAverageSurvivalTime(bool combatFinished)
        {
            if (Stats != null)
            {
                // calculate current average survival time
                float totalSurvivalTimes = 0;

                foreach (float survivalTime in Stats.survivalTimes)
                {
                    totalSurvivalTimes += survivalTime;
                }

                // note: add one to numDeaths for accuracy since still alive at end of combat round
                if (combatFinished == false)
                {
                    Stats.averageSurvivalTime = totalSurvivalTimes / Stats.numDeaths;
                }
                else
                {
                    Stats.averageSurvivalTime = totalSurvivalTimes / (Stats.numDeaths + 1);
                }
            }
        }
        public void startSurvivalClock()
        {
            // D.log("Structure", "Starting alive timer for " + gameObject.name);
            AliveTimer.Start();
        }

        public void stopSurvivalClock()
        {
            // D.log("Structure", "Stopping alive timer for " + gameObject.name);
            AliveTimer.Stop();
        }

        public void resetSurvivalClock()
        {
            // D.log("Structure", "Resetting alive timer for " + gameObject.name);
            AliveTimer.Reset();
        }
        #endregion

        public virtual void Update()
        {
            if (GameManager.Instance.getSuspended() == false)
            {
                if (Destroyed == true)
                {
                }
                else
                {
                    // check if outside the boundary (is false if controller is invulnerable to boundary rules)
                    if (OutsideBoundary == true)
                    {
                        BoundaryTimer += Time.deltaTime;

                        if (BoundaryTimer >= MaxBoundaryTime)
                        {
                            Gamemode.Gui.setMessage(gameObject.name + " has violated the arena boundary rules");

                            takeDamage(this.gameObject, Mathf.Infinity, null, (gameObject, null));

                            OutsideBoundary = false;
                        }
                    }

                    if (StructureRenderer != null)
                    {
                        structureBounds = StructureRenderer.sprite.bounds;
                    }

                    if (ShieldRenderer != null)
                    {
                        shieldBounds = ShieldRenderer.bounds;
                    }

                    if (scrollingTextTrans != null)
                    {
                        if (scrollingText.StructureBoundsOffset != BoundsOffset.None)
                        {
                            Vector2 structureBoundsOffset = Vector2.zero;

                            if ((scrollingText.StructureBoundsOffset & BoundsOffset.PosX) != 0)
                            {
                                structureBoundsOffset.x += (shieldBounds.extents.x + scrollingText.CanvasExtents.x);
                            }
                            else if ((scrollingText.StructureBoundsOffset & BoundsOffset.NegX) != 0)
                            {
                                structureBoundsOffset.x -= (shieldBounds.extents.x + scrollingText.CanvasExtents.x);
                            }

                            if ((scrollingText.StructureBoundsOffset & BoundsOffset.PosY) != 0)
                            {
                                structureBoundsOffset.y += (shieldBounds.extents.y + scrollingText.CanvasExtents.y);
                            }
                            else if ((scrollingText.StructureBoundsOffset & BoundsOffset.NegY) != 0)
                            {
                                structureBoundsOffset.y -= (shieldBounds.extents.y + scrollingText.CanvasExtents.y);
                            }

                            scrollingTextTrans.position = new Vector3(transform.position.x + structureBoundsOffset.x + scrollingText.Offset.x, transform.position.y + structureBoundsOffset.y + scrollingText.Offset.y, 0);
                        }
                        else
                        {
                            scrollingTextTrans.position = new Vector3(transform.position.x + scrollingText.Offset.x, transform.position.y + scrollingText.Offset.y, 0);
                        }

                        scrollingTextTrans.rotation = Quaternion.identity;
                    }

                    if (healthBarTrans != null)
                    {
                        healthBarTrans.position = new Vector3(transform.position.x, transform.position.y + shieldBounds.extents.y + 15, 0);
                        healthBarTrans.rotation = Quaternion.identity;
                    }

                    if (FactionLabel != null)
                    {
                        FactionLabel.transform.rotation = factionLabelRotation;
                        FactionLabel.transform.position = new Vector3(transform.position.x + factionLabelOffset.x, transform.position.y + factionLabelOffset.y - shieldBounds.extents.y - 50, transform.position.z + factionLabelOffset.z + factionTextMesh.offsetZ);
                    }

                    if (NameLabel != null)
                    {
                        NameLabel.transform.rotation = nameLabelRotation;
                        NameLabel.transform.position = new Vector3(transform.position.x + nameLabelOffset.x, transform.position.y + nameLabelOffset.y - shieldBounds.extents.y, transform.position.z + nameLabelOffset.z + nameTextMesh.offsetZ);
                    }

                    // do structure fitting updates here!!!
                    if (initialLifeSpan > 0)
                    {
                        if (AliveTimer.Elapsed.TotalSeconds >= initialLifeSpan)
                        {
                            // remove this GameObject (TODO: cause it an infinite amount of damage instead?)
                            GameManager.Instance.removeStructure(this);
                            Destroy(gameObject);
                            return;
                        }
                    }

                    if (SystemsInitiated)
                    {
                        if (BuffManager != null)
                        {
                            BuffManager.update();
                        }

                        ShieldStrength = 0;
                        MaxShieldStrength = 0;

                        foreach (Device device in Devices)
                        {
                            if (device != null)
                            {
                                device.update();
                            }
                        }

                        foreach (StructureSocket structureSocket in StructureSockets)
                        {
                            structureSocket.update();

                            if (structureSocket.InstalledModule != null)
                            {
                                structureSocket.InstalledModule.update();
                            }
                        }

                        recalculateHealthBar();
                    }
                }
            }
        }

        #region debugging
        public void debugMaximise(object sender, DebugEventArgs args)
        {
            HullStrength = MaxHullStrength;
            Gamemode.Gui.setMessage("DEBUG: hull restored");
        }

        public void debugMinimise(object sender, DebugEventArgs args)
        {
            HullStrength = 0;
            Gamemode.Gui.setMessage("DEBUG: hull depleted");
        }

        public void debugIncrease(object sender, DebugEventArgs args, int amount)
        {
            HullStrength = Mathf.Clamp(HullStrength + amount, 0, MaxHullStrength);
            Gamemode.Gui.setMessage("DEBUG: hull strength increased by " + amount);
        }

        public void debugDecrease(object sender, DebugEventArgs args, int amount)
        {
            HullStrength = Mathf.Clamp(HullStrength - amount, 0, MaxHullStrength);

            if (HullStrength == 0) takeDamage(this.gameObject, Mathf.Infinity, null, null);

            Gamemode.Gui.setMessage("DEBUG: hull strength decreased by " + amount);
        }

        public void debugExplode(object sender, DebugEventArgs args)
        {
            takeDamage(this.gameObject, Mathf.Infinity, null, null);
            Gamemode.Gui.setMessage("DEBUG: structure self-destructed");
        }

        public void debugActivate(object sender, DebugEventArgs args)
        {
            // raise shields (if present)
            foreach (IShieldGenerator shieldGenerator in shields)
            {
                shieldGenerator.raiseShield();
            }

            // D.log("Controller", "AI for " + gameObject.name + " has gone offline!");
            Controller.booted = true;

            SystemsInitiated = true;

            enabled = true;
            Gamemode.Gui.setMessage("DEBUG: structure activated");
        }

        public void debugDeactivate(object sender, DebugEventArgs args)
        {
            // disable the shield mesh (if present)
            if (shields.Count > 0)
            {
                // just use the reference to the first shield for disabling
                shields[0].failed();
            }

            // D.log("Controller", "AI for " + gameObject.name + " has gone offline!");
            Controller.booted = false;
            //Controller.StopCoroutine(Controller.update());
            Controller.stop();

            SystemsInitiated = false;

            enabled = false;
            Gamemode.Gui.setMessage("DEBUG: structure deactivated");
        }

        public IEnumerator debugSelfDestruct(object sender, DebugEventArgs args)
        {
            yield return new WaitForSeconds(0);

            takeDamage(this.gameObject, Mathf.Infinity, null, null);

            Gamemode.Gui.setMessage("DEBUG: structure has self-destructed!");
        }
        #endregion

        #region event dispatchers
        ////////////////////////////////////
        /*
			Structure event dispatchers
		*/
        ////////////////////////////////////		

        public void Call_ActivateUltimate(object sender, UltimateEventArgs args)
        {
            if (ActivateUltimate != null && thermalcontrol.isOverheated() == true)
            {
                ActivateUltimate(sender, args);

                GameMode.Call_ActivateUltimate(sender, args);
            }
        }

        public void Call_DockInitiatorDocked(object sender, DockingPortEventArgs args)
        {
            if (DockInitiatorDocked != null)
            {
                DockInitiatorDocked(sender, args);

                GameMode.Call_DockInitiatorDocked(sender, args);
            }
        }

        public void Call_DockReceiverDocked(object sender, DockingPortEventArgs args)
        {
            if (DockReceiverDocked != null)
            {
                DockReceiverDocked(sender, args);

                GameMode.Call_DockReceiverDocked(sender, args);
            }
        }

        public void Call_DockInitiatorUndocked(object sender, DockingPortEventArgs args)
        {
            if (DockInitiatorUndocked != null)
            {
                DockInitiatorUndocked(sender, args);

                GameMode.Call_DockInitiatorUndocked(sender, args);
            }
        }

        public void Call_DockReceiverUndocked(object sender, DockingPortEventArgs args)
        {
            if (DockReceiverUndocked != null)
            {
                DockReceiverUndocked(sender, args);

                GameMode.Call_DockReceiverUndocked(sender, args);
            }
        }

        public void Call_LandingInitiatorLanded(object sender, HangarEventArgs args)
        {
            if (LandingInitiatorLanded != null)
            {
                LandingInitiatorLanded(sender, args);

                GameMode.Call_LandingInitiatorLanded(sender, args);
            }
        }

        public void Call_LandingReceiverLanded(object sender, HangarEventArgs args)
        {
            if (LandingReceiverLanded != null)
            {
                LandingReceiverLanded(sender, args);

                GameMode.Call_LandingReceiverLanded(sender, args);
            }
        }

        public void Call_LaunchInitiatorLaunched(object sender, HangarEventArgs args)
        {
            if (LaunchInitiatorLaunched != null)
            {
                LaunchInitiatorLaunched(sender, args);

                GameMode.Call_LaunchInitiatorLaunched(sender, args);
            }
        }

        public void Call_LaunchReceiverLaunched(object sender, HangarEventArgs args)
        {
            if (LaunchReceiverLaunched != null)
            {
                LaunchReceiverLaunched(sender, args);

                GameMode.Call_LaunchReceiverLaunched(sender, args);
            }
        }

        public void Call_InBounds(object sender, BoundaryEventArgs args)
        {
            if (InBounds != null)
            {
                InBounds(sender, args);
            }
        }

        public void Call_OutOfBounds(object sender, BoundaryEventArgs args)
        {
            if (OutOfBounds != null)
            {
                OutOfBounds(sender, args);
            }
        }

        public void Call_InstigatedAnyDamage(object sender, DamageEventArgs args)
        {
            if (InstigatedAnyDamage != null)
            {
                InstigatedAnyDamage(sender, args);
            }
        }

        public void Call_TakenAnyDamage(object sender, DamageEventArgs args)
        {
            if (TakenAnyDamage != null)
            {
                TakenAnyDamage(sender, args);
            }
        }

        public void Call_NotifyAssister(object sender, AssistEventArgs args)
        {
            if (NotifyAssister != null)
            {
                NotifyAssister(sender, args);
            }
        }

        public void Call_NotifyKilled(object sender, TargetDestroyedEventArgs args)
        {
            if (NotifyKilled != null)
            {
                NotifyKilled(sender, args);
            }
        }

        public void Call_NotifyKiller(object sender, TargetDestroyedEventArgs args)
        {
            if (NotifyKiller != null)
            {
                NotifyKiller(sender, args);
            }
        }

        public void Call_ModuleDamaged(object sender, ModuleDamageEventArgs args)
        {
            if (ModuleDamaged != null)
            {
                ModuleDamaged(sender, args);

                GameMode.Call_ModuleDestroyed(sender, args);
            }
        }

        public void Call_TargetDestroyed(object sender, TargetDestroyedEventArgs args)
        {
            if (TargetDestroyed != null)
            {
                TargetDestroyed(sender, args);
            }
        }

        public void Call_SurvivalTimeUpdated(object sender, SurvivalTimeEventArgs args)
        {
            if (SurvivalTimeUpdated != null)
            {
                SurvivalTimeUpdated(sender, args);
            }
        }
        #endregion

        #region event handlers
        ///////////////////////////////////////////
        /*
			Structure event handlers
		*/
        ///////////////////////////////////////////		

        protected virtual void OnSpawn(object sender, SpawnEventArgs args)
        {
            /*
            D.log ("Controller", this.name + " has been spawned");
            */
        }

        protected virtual void OnDespawn(object sender, DespawnEventArgs args)
        {
            /*
            D.log ("Controller", this.name + " has been despawned");
            */

            foreach (Device device in Devices)
            {
                List<IVisualEffect> vfxs = device.getVFXs();

                foreach (IVisualEffect vfx in vfxs)
                {
                    vfx.stopVFX();
                }
            }

            foreach (Module module in Modules)
            {
                List<IVisualEffect> vfxs = module.getVFXs();

                foreach (IVisualEffect vfx in vfxs)
                {
                    vfx.stopVFX();
                }
            }

            foreach (Weapon weapon in Weapons)
            {
                List<IVisualEffect> vfxs = weapon.getVFXs();

                foreach (IVisualEffect vfx in vfxs)
                {
                    vfx.stopVFX();
                }
            }
        }

        protected virtual void OnRespawn(object sender, RespawnEventArgs args)
        {
            /*
            D.log ("Controller", this.name + " has been respawned");
            */

            AIStateController stateController = Controller as AIStateController;

            if (stateController != null)
            {
                stateController.DebugState = true;
            }
        }

        protected virtual void OnHasRespawned(object sender, RespawnEventArgs args)
        {
            /*
            D.log ("Controller", this.name + " has been respawned");
            */

            GameManager.Instance.addStructure(this);
        }

        protected virtual void OnDestruct(object sender, DestroyEventArgs args)
        {
            /*
            D.log ("Controller", this.name + " has been destroyed");
            */

            GameManager.Instance.removeStructure(this);
        }

        protected virtual void Structure_UltimateActivated(object sender, UltimateEventArgs args)
        {
            /*
            D.log ("Controller", this.name + " has activated their ultimate ability");
            */
        }

        protected virtual void Structure_DockInitiatorDocked(object sender, DockingPortEventArgs args)
        {
            /*
            if (args.port != null && args.ship != null && args.portStructure != null)
            {
                D.log ("Controller", args.ship.name + " has docked at " + args.port.name + " on " + args.portStructure.name);
            }
            */
        }

        protected virtual void Structure_DockReceiverDocked(object sender, DockingPortEventArgs args)
        {
            /*
            if (args.port != null && args.ship != null && args.portStructure != null)
            {
                D.log ("Controller", args.ship.name + " has undocked from " + args.port.name + " on " + args.portStructure.name);
            }
            */
        }

        protected virtual void Structure_DockInitiatorUndocked(object sender, DockingPortEventArgs args)
        {
            /*
            if (args.port != null && args.ship != null && args.portStructure != null)
            {
                D.log ("Controller", args.ship.name + " has docked at " + args.port.name + " on " + args.portStructure.name);
            }
            */
        }

        protected virtual void Structure_DockReceiverUndocked(object sender, DockingPortEventArgs args)
        {
            /*
            if (args.port != null && args.ship != null && args.portStructure != null)
            {
                D.log ("Controller", args.ship.name + " has undocked from " + args.port.name + " on " + args.portStructure.name);
            }
            */
        }

        protected virtual void Structure_LandingInitiatorLanded(object sender, HangarEventArgs args)
        {
            /*
            if (args.port != null && args.ship != null && args.portStructure != null)
            {
                D.log ("Controller", args.ship.name + " has docked at " + args.port.name + " on " + args.portStructure.name);
            }
            */
        }

        protected virtual void Structure_LandingReceiverLanded(object sender, HangarEventArgs args)
        {
            /*
            if (args.port != null && args.ship != null && args.portStructure != null)
            {
                D.log ("Controller", args.ship.name + " has undocked from " + args.port.name + " on " + args.portStructure.name);
            }
            */
        }

        protected virtual void Structure_LaunchInitiatorLaunched(object sender, HangarEventArgs args)
        {
            /*
            if (args.port != null && args.ship != null && args.portStructure != null)
            {
                D.log ("Controller", args.ship.name + " has docked at " + args.port.name + " on " + args.portStructure.name);
            }
            */
        }

        protected virtual void Structure_LaunchReceiverLaunched(object sender, HangarEventArgs args)
        {
            /*
            if (args.port != null && args.ship != null && args.portStructure != null)
            {
                D.log ("Controller", args.ship.name + " has undocked from " + args.port.name + " on " + args.portStructure.name);
            }
            */
        }

        protected virtual void Structure_InBounds(object sender, BoundaryEventArgs args)
        {
            if (args.structure != null)
            {
                //D.log ("Controller", args.structure.name + " has violated the boundary rules");

                args.structure.OutsideBoundary = false;
                args.structure.BoundaryTimer = 0;
            }
        }

        protected virtual void Structure_OutOfBounds(object sender, BoundaryEventArgs args)
        {
            if (args.structure != null)
            {
                //D.log ("Controller", args.structure.name + " has violated the boundary rules");

                args.structure.OutsideBoundary = true;
            }
        }

        protected virtual void Structure_InstigatedAnyDamage(object sender, DamageEventArgs args)
        {
            /*
            if (args.damageCauser != null)
            {
                if (args.damagedStructure != null)
                {
                    D.log ("Controller", args.damageCauser.name + " caused " + args.damage + " damage to " + args.damagedStructure.name);
                }
                else
                {
                    D.log ("Controller", args.damageCauser.name + " caused " + args.damage + " damage");
                }
            }
            else
            {
                if (args.damagedStructure != null)
                {
                    D.log ("Controller", name + " caused " + args.damage + " damage to " + args.damagedStructure.name);
                }
                else
                {
                    D.log ("Controller", name + " caused " + args.damage + " damage to another structure");
                }
            }
            */
        }

        protected virtual void Structure_TakenAnyDamage(object sender, DamageEventArgs args)
        {
            /*
            if (args.damagedStructure != null)
            {
                if (args.damageCauser != null)
                {
                    D.log ("Controller", args.damagedStructure.name + " received " + args.damage + " damage from " + args.damageCauser.name);
                }
                else
                {
                    D.log ("Controller", args.damagedStructure.name + " received " + args.damage + " damage");
                }
            }
            else
            {
                if (args.damageCauser != null)
                {
                    D.log ("Controller", name + " received " + args.damage + " damage from " + args.damageCauser.name);
                }
                else
                {
                    // D.log ("Controller", structure.name + " received " + args.damage + " damage");
                }
            }
            */
        }

        protected virtual void Structure_NotifyKiller(object sender, TargetDestroyedEventArgs args)
        {
            /*
            if (args.attacker != null)
            {
                if (args.structureAttacked != null)
                {
                    D.log ("Controller", args.attacker.Name + " has destroyed " + args.structureAttacked.Name);
                }
                else
                {
                    D.log ("Controller", args.attacker.Name + " has destroyed a structure");
                }
            }
            else
            {
                if (args.structureAttacked != null)
                {
                    D.log ("Controller", name + " has destroyed " + args.structureAttacked.Name);
                }
                else
                {
                    D.log ("Controller", name + " has destroyed a structure");
                }
            }
            */
        }

        protected virtual void Structure_NotifyKilled(object sender, TargetDestroyedEventArgs args)
        {
            /*
            if (args.structureAttacked != null)
            {
                if (args.attacker != null)
                {
                    D.log ("Controller", args.structureAttacked.Name + " has been destroyed by " + args.attacker.Name);
                }
                else
                {
                    D.log ("Controller", args.structureAttacked.Name + " has been destroyed");
                }
            }
            */
            #endregion
        }
    }
}