using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

using NoxCore.Controllers;
using NoxCore.Data;
using NoxCore.Effects;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.GameModes;
using NoxCore.GUIs;
using NoxCore.Helm;
using NoxCore.Managers;
using NoxCore.Utilities;
using NoxCore.Utilities.Geometry;
using NoxCore.Data.Placeables;

namespace NoxCore.Placeables.Ships
{
    #region Enums
    public enum ShipClassification
	{
		FIGHTER,
		BOMBER,
		FIGHTERBOMBER,
		RECON,
		SHUTTLE,
		TRANSPORTER,
		SUPPORT,
		INTERDICTOR,
		CORVETTE,
		FRIGATE,
		INTERCEPTOR,
		CRUISER,
		DESTROYER,
		BATTLECRUISER,
		BATTLESHIP,
		DREADNOUGHT,
		CARRIER,
		SUPERCARRIER,
		EXPERIMENTAL
	};

    public enum ShipState { UNKNOWN, WARPING, FLYING, LANDING, LANDED, DOCKING, DOCKED, UNDOCKING }

    #endregion

    #region ship event argument classes
    public class ShipWarpedEventArgs : EventArgs
    {
        public GameObject shipGO;
        public bool warpedIn;

        public ShipWarpedEventArgs(GameObject shipGO, bool warpedIn)
        {
            this.shipGO = shipGO;
            this.warpedIn = warpedIn;
        }
    }

    public class SquadronWarpedEventArgs : EventArgs
    {
        public string squadronName;
        public bool warpedIn;

        public SquadronWarpedEventArgs(string squadronName, bool warpedIn)
        {
            this.squadronName = squadronName;
            this.warpedIn = warpedIn;
        }
    }

    public class WingWarpedEventArgs : EventArgs
    {
        public string wingName;
        public bool warpedIn;

        public WingWarpedEventArgs(string wingName, bool warpedIn)
        {
            this.wingName = wingName;
            this.warpedIn = warpedIn;
        }
    }

    public class FleetWarpedEventArgs : EventArgs
    {
        public string fleetName;
        public bool warpedIn;

        public FleetWarpedEventArgs(string fleetName, bool warpedIn)
        {
            this.fleetName = fleetName;
            this.warpedIn = warpedIn;
        }
    }
    #endregion

    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Outline))]
    public abstract class Ship : Structure, IShip
    {
        static float MINSPACEDRIFTSPEED = 5.0f;
        static float SPACEDRIFTDRAG = 50000.0f;
        static float DELAYEDFORCEDECREASEFACTOR = 0.99f;

        #region specific ship events
        public delegate void WarpEventDispatcher(object sender, WarpEventArgs args);
        public event WarpEventDispatcher WarpIn;
        public event WarpEventDispatcher HasWarpedIn;
        public event WarpEventDispatcher WarpOut;
        public event WarpEventDispatcher HasWarpedOut;

        public delegate void ShipHasWarpedEventDispatcher(object sender, ShipWarpedEventArgs args);
        public event ShipHasWarpedEventDispatcher ShipHasWarpedIn;
        public event ShipHasWarpedEventDispatcher ShipHasWarpedOut;

        public delegate void SquadronWarpEventDispatcher(object sender, SquadronWarpedEventArgs args);
        public event SquadronWarpEventDispatcher SquadronHasWarpedIn;
        public event SquadronWarpEventDispatcher SquadronHasWarpedOut;

        public delegate void WingWarpEventDispatcher(object sender, WingWarpedEventArgs args);
        public event WingWarpEventDispatcher WingHasWarpedIn;
        public event WingWarpEventDispatcher WingHasWarpedOut;

        public delegate void FleetWarpEventDispatcher(object sender, FleetWarpedEventArgs args);
        public event FleetWarpEventDispatcher FleetHasWarpedIn;
        public event FleetWarpEventDispatcher FleetHasWarpedOut;

        #endregion

        [Header("Ship Overrides")]
        [SerializeField] protected bool _WarpInOnRespawn;
        public bool WarpInOnRespawn { get { return _WarpInOnRespawn; } set { _WarpInOnRespawn = value; } }

        #region Variables
        protected ShipData _shipData;
        public ShipData ShipData { get { return _shipData; } set { _shipData = value; } }

        [Header("Ship Tactical")]
        [SerializeField]
        public ShipState shipState = ShipState.FLYING;

        [Header("Ship Command")]
        [SerializeField] protected FleetData _fleet;
        public FleetData FleetData {  get { return _fleet; } set { _fleet = value; } }

        [SerializeField] protected WingData _wing;
        public WingData WingData { get { return _wing; } set { _wing = value; } }

        [SerializeField] protected SquadronData _squadron;
        public SquadronData SquadronData {  get { return _squadron; } set { _squadron = value; } }

        protected ShipClassification _Classification;
        public ShipClassification Classification { get { return _Classification; } set { _Classification = value; } }

        protected HelmController _helm;
        public HelmController Helm { get { return _helm; } set { _helm = value; } }

        [Header("Helm")]
        public Vector2 Heading;
        public float Bearing;
        public float MaxSpeed;
        public float Speed;
        protected Vector2 acceleration;

        protected float maxForce;
        public float MaxForce { get { return maxForce; } set { maxForce = value; } }

        protected Vector2 delayedForce;

        [ShowOnly]
        protected float inertialRating;
        public float InertialRating { get { return inertialRating; } set { inertialRating = value; } }

        [ShowOnly]
        public float actualVelocity;

        [ShowOnly]
        public Vector2 TotalSteeringForce;

        [ShowOnly]
        public float TotalSteeringForceMag;

        // device shorthand
        public List<IEngine> engines;

        protected bool areEnginesDisabled;
        public bool AreEnginesDisabled {  get { return areEnginesDisabled; } set { areEnginesDisabled = value; } }

        [ShowOnly]
        public float maxDeltaV;

        public float maxDeltaThrottle;

        protected bool prevOutsideBoundary, outsideBoundary;

        protected float _BoundaryRadius;
        public float BoundaryRadius { get { return _BoundaryRadius; } set { _BoundaryRadius = value; } }

        protected float spawnInSpeedFraction;
        protected Vector2 prevVelocity;
        public Vector2 PrevVelocity {  get { return prevVelocity; } set { prevVelocity = value; } }

        [Header("Ship Tactical")]
        protected bool _silentRunning;
        public bool silentRunning { get { return _silentRunning; } set { _silentRunning = value; } }

        [Range(0, 1)]
        public float silentRunningFactor;

        [Header("Warp Effect")]
        public GameObject warpInPrefab;
        public GameObject warpOutPrefab;
        #endregion

        #region initialisation & delegate subscriptions
        public override void init(NoxObjectData noxObjectData = null)
        {
            if (noxObjectData == null)
            {
                if (noxObject2DData == null) return;

                ShipData = Instantiate(noxObject2DData as ShipData);

                base.init(ShipData);
            }
            else
            {
                ShipData = noxObjectData as ShipData;
                base.init(noxObjectData);
            }

            TotalCost = ShipData.HullCost;

            // set standard ship properties
            StructureRigidbody.angularDrag = 0;
            StructureRigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            StructureRigidbody.drag = 0;
            StructureRigidbody.gravityScale = 0;
            StructureRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
            StructureRigidbody.isKinematic = false;
            StructureRigidbody.useAutoMass = false;

            // attach HelmController
            Transform helmTrans = transform.Find("Helm");

            if (helmTrans != null)
            {
                HelmController helm = helmTrans.GetComponent<HelmController>();

                if (helm != null)
                {
                    attachHelmController(helm);
                }
            }

            if (gameObject.activeInHierarchy && shipState != ShipState.UNKNOWN)
            {
                shipState = ShipState.FLYING;
            }

            if (SquadronData != null)
            {
                SquadronData.incrementSquadron(FleetData, WingData);
            }
        }

        public override void OnEnable()
        {
            if (eventsSubscribedInto == false)
            {
                // subscribe ship delegates
                WarpIn += Ship_WarpIn;
                WarpOut += Ship_WarpOut;
                HasWarpedIn += Ship_HasWarpedIn;
                HasWarpedOut += Ship_HasWarpedOut;

                base.OnEnable();
            }
        }

        public override void OnDisable()
        {
            if (eventsSubscribedInto == true)
            {
                // unsubscribe ship delegates
                WarpIn -= Ship_WarpIn;
                WarpOut -= Ship_WarpOut;
                HasWarpedIn -= Ship_HasWarpedIn;
                HasWarpedOut -= Ship_HasWarpedOut;

                base.OnDisable();
            }
        }

        public void attachHelmController(HelmController helm)
        {
            if (helm != null)
            {
                Helm = helm;
                Helm.setHelmStructure(this);
                Helm.init();

                foreach(SteeringBehaviour behaviour in Helm.getAllBehaviours())
                {
                    behaviour.init();
                }
            }
            
            // D.log ("Structure", "Helm controller attached to structure");
        }

        protected void setBoundaryRadius(float radius)
        {
            BoundaryRadius = radius;
        }

        protected void checkBoundary()
        {
            GameObject arenaBoundary = GameObject.Find("Arena Barrier");

            if (arenaBoundary)
            {
                ICircle arena = arenaBoundary.GetComponent("ArenaBarrier") as ICircle;

                if (arena != null)
                {
                    setBoundaryRadius(arena.getRadius());
                }
            }
            else
            {
                setBoundaryRadius(Mathf.Infinity);
            }
        }

        protected Gradient setupExhaust(Color[] colours, float[] cTimes, float[] alphas, float[] aTimes)
        {
            Gradient g;
            GradientColorKey[] gck;
            GradientAlphaKey[] gak;
            g = new Gradient();
            gck = new GradientColorKey[colours.Length];

            for (int cIndex = 0; cIndex < colours.Length; cIndex++)
            {
                gck[cIndex].color = colours[cIndex];
                gck[cIndex].time = cTimes[cIndex];
            }

            gak = new GradientAlphaKey[alphas.Length];

            for (int aIndex = 0; aIndex < alphas.Length; aIndex++)
            {
                gak[aIndex].alpha = alphas[aIndex];
                gak[aIndex].time = aTimes[aIndex];
            }

            g.SetKeys(gck, gak);

            return g;
        }
        #endregion

        #region spawning/respawning
        public void setSpawnInSpeed(float spawnInSpeedFraction)
        {
            this.spawnInSpeedFraction = spawnInSpeedFraction;

            if (powergrid != null && engines.Count > 0)
            {
                Vector2 spawnVelocity = Heading * MaxSpeed * spawnInSpeedFraction;
                StructureRigidbody.velocity = spawnVelocity;
                prevVelocity = spawnVelocity;
            }
        }

        public void setFleetDataInfo(FleetData fleet, WingData wing, SquadronData squadron)
        {
            if (squadron == null) return;
            squadron.addShip(this);

            if (wing == null) return;
            wing.addSquadronData(squadron);

            if (fleet == null) return;
            fleet.addWingData(wing);

            FactionData faction = FactionManager.Instance.findFaction(Faction.ID);

            if (faction != null)
            {
                faction.FleetManager.addFleetData(fleet);
            }
        }

        protected void initEngines()
        {
            // initialise the engines list
            engines = new List<IEngine>();

            AreEnginesDisabled = false;

            foreach (StructureSocket structureSocket in StructureSockets)
            {
                if (structureSocket != null && structureSocket.InstalledModule != null)
                {
                    IEngine engine = structureSocket.InstalledModule as IEngine;

                    if (engine != null)
                    {
                        engines.Add(engine);

                        // set initial max speed in case of moving on start
                        if (engine.isActiveOn() == true && engine.isActiveOnSpawn() == true && engine.isDestroyed() == false)
                        {
                            MaxSpeed += engine.getMaxSpeed();
                        }
                    }
                }
            }
        }

        public override void spawn(bool spawnEnabled = false)
        {
            if (ShipData == false) return;

            initEngines();

            // call parent spawn
            base.spawn(spawnEnabled);

            // calculate inertial rating based on ship's mass
            InertialRating = 1 - (1.0f / (1 + (1.0f / (StructureData.Mass / 1000.0f))));

            // calculate max velocity change per tick
            maxDeltaV = InertialRating / Time.fixedDeltaTime;

            // calculate max throttle change per tick
            maxDeltaThrottle = maxDeltaV / 10000.0f;

            if (Helm != null)
            {
                Helm.throttle = Helm.desiredThrottle;
            }

            // subscribe to own events

            // check if within an arena boundary
            checkBoundary();

            setFleetDataInfo(FleetData, WingData, SquadronData);
        }

        public override void respawn()
        {
            if (CanRespawn == true)
            {
                // TODO - change this to check if ship is in a hangar
                if (Classification == ShipClassification.FIGHTER || Classification == ShipClassification.BOMBER)
                {
                    if (shipState == ShipState.LANDED) return;
                }

                if (WarpInOnRespawn == true)
                {
                    StartCoroutine(rewarpInSequence(new WarpEventArgs(gameObject, gameObject.scene.path, null, transform.position, -Bearing)));
                }
                else
                {
                    respawnInit();
                }
            }
        }

        void respawnInit()
        {
            StructureRigidbody.WakeUp();
            StructureRigidbody.freezeRotation = false;

            delayedForce = Vector2.zero;

            initEngines();

            base.respawn();

            setBearing();

            setSpawnInSpeed(spawnInSpeedFraction);
        }

        IEnumerator rewarpInSequence(WarpEventArgs args)
        {
            Call_WarpIn(this, args);

            yield return new WaitForSeconds(3.3f);

            respawnInit();
        }
        #endregion

        #region Silent Running
        public void setSilentRunningFactor(float factor)
        {
            silentRunningFactor = Mathf.Clamp01(factor);
        }        

        public void disengageSilentRunning()
        {
            if (silentRunning == true)
            {
                silentRunning = false;
            }
        }

        public void engageSilentRunning()
        {
            if (silentRunning == false)
            {
                silentRunning = true;
            }
        }
        #endregion

        #region Movement
        public void setBearing(float offset = 0)
        {
            Bearing = -transform.rotation.eulerAngles.z + offset;

            if (Bearing < 0) Bearing += 360;

            float theta = 90 - Bearing;

            if (theta < 0) theta += 360;

            float x = Mathf.Cos(theta * Mathf.Deg2Rad);
            float y = Mathf.Sin(theta * Mathf.Deg2Rad);

            Heading = new Vector2(x, y);
        }

        public float angleTo(Vector2 v1, Vector2 v2)
        {
            float cosTheta = Vector2.Dot(v1, v2) / (v1.magnitude * v2.magnitude);

            if (cosTheta > 1.0f) cosTheta = 1.0f;
            else if (cosTheta < -1.0) cosTheta = -1.0f;

            // Debug.Log ("Angle To: " + Mathf.Acos(cosTheta) * Mathf.Rad2Deg);

            return Mathf.Acos(cosTheta);
        }

        public Vector2 correctVelocity(Vector2 currentVelocity, Vector2 requestedVelocity, float maxTurn)
        {
            float newBearing;

            if (isTurningLeft(currentVelocity, requestedVelocity) == true)
            {
                newBearing = Bearing - maxTurn;
                // Debug.Log ("Turning left - current bearing: " + Bearing + "   new bearing: " + newBearing);
            }
            else
            {
                newBearing = Bearing + maxTurn;
                // Debug.Log ("Turning right - current bearing: " + Bearing + "   new bearing: " + newBearing);
            }

            newBearing *= Mathf.Deg2Rad;

            return new Vector2(requestedVelocity.magnitude * Mathf.Sin(newBearing), requestedVelocity.magnitude * Mathf.Cos(newBearing));
        }

        public bool isTurningLeft(Vector2 currentVelocity, Vector2 newVelocity)
        {
            if (currentVelocity.x * newVelocity.y - currentVelocity.y * newVelocity.x > 0) return true;
            else return false;
        }
        #endregion

        #region Warp
        IEnumerator warpInSequence(WarpEventArgs args)
        {
            // perform the warp in effect
            GameObject warpEffect = warpInPrefab.Spawn(GameManager.Instance.EffectsParent, args.warpPosition.GetValueOrDefault(), Quaternion.Euler(0, 0, args.warpRotation.GetValueOrDefault()));
            
            if (warpEffect != null)
            {
                StartCoroutine(DelayedRecycle(warpEffect, 4.8f));

                yield return new WaitForSeconds(3.3f);
            }

            transform.position = args.warpPosition.GetValueOrDefault();
            transform.rotation = Quaternion.Euler(0, 0, args.warpRotation.GetValueOrDefault());

            setBearing();

            args.warpShipGO.SetActive(true);
            args.warpShip.enabled = true;

            Call_HasWarpedIn(this, args);
            Call_ShipHasWarpedIn(this, new ShipWarpedEventArgs(args.warpShipGO, true));
        }

        IEnumerator warpOutSequence(WarpEventArgs args)
        {
            GameObject warpEffect = warpOutPrefab.Spawn(GameManager.Instance.EffectsParent, transform.position, transform.rotation);

            if (warpEffect != null)
            {
                StartCoroutine(DelayedRecycle(warpEffect, 4.8f));

                yield return new WaitForSeconds(3.3f);
            }

            Call_HasWarpedOut(this, args);
            Call_ShipHasWarpedOut(this, new ShipWarpedEventArgs(args.warpShipGO, false));

            if (args.warpToScenePath == null) Destroy(gameObject);
        }
        #endregion

        IEnumerator DelayedRecycle(GameObject go, float delay)
        {
            // TODO - note: should really get the delay from the effect
            yield return new WaitForSeconds(delay);

            // recycle the object if it hasn't already been removed from the scene (warp out etc.)
            if (go != null)
            {
                go.Recycle();
            }
        }

        public void addDelayedForce(Vector2 force)
        {
            delayedForce += force;
        }

        #region Update
        public override void Update()
        {
            if (Destroyed == false && SystemsInitiated == true)
            {
                // update heading and bearing if moving
                if (StructureRigidbody.velocity.magnitude > 0.01f)
                {
                    // Debug.Log ("Velocity: " + transform.rigidbody.velocity.x + ", " + transform.rigidbody.velocity.y + ", " + transform.rigidbody.velocity.z);
                    // Debug.Log ("Normalised velocity: " + normalisedVelocity.x + ", " + normalisedVelocity.y + ", " + normalisedVelocity.z);
                    // Debug.DrawLine(transform.position, transform.position + (transform.rigidbody.velocity.normalized * 50), Color.blue);

                    Bearing = (Mathf.Atan2(-StructureRigidbody.velocity.y, StructureRigidbody.velocity.x) * Mathf.Rad2Deg) + 90;

                    if (Bearing < 0) Bearing += 360;

                    Heading = StructureRigidbody.velocity.normalized;

                    // Debug.Log ("Heading: " + Heading + "   Bearing: " + Bearing);
                }

                // rotate ship according to bearing
                StructureRigidbody.MoveRotation(-Bearing);

                // set ship's MaxSpeed to 0 and let the sum of the ship's active engines set the max speed when they update in the main Structure class
                MaxSpeed = 0;
            }

            base.Update();
        }

        public void FixedUpdate()
        {
            if (Destroyed == true || SystemsInitiated == false) return;

            Vector2 newVelocity = Vector2.zero;
            float angleToDestination;
            TotalSteeringForce = Vector2.zero;
            bool drifting = false;

            // call parent structure to update
            if (GameManager.Instance.getSuspended() == false)
            {
                if (Destroyed == false && Helm != null && Controller != null && SystemsInitiated == true)
                {
                    // calculate the resultant steering force from all active steering behaviours
                    TotalSteeringForce = Helm.calculateSteeringForces();
                    TotalSteeringForceMag = TotalSteeringForce.magnitude;
                }
            }

            // calculate internal forces
            if (Helm != null)
            {
                // update actual throttle setting
                Helm.throttle = Mathf.Clamp(Helm.desiredThrottle, Helm.throttle - maxDeltaThrottle, Helm.throttle + maxDeltaThrottle);
                
                // space drift drag force
                if (TotalSteeringForceMag == 0)
                {
                    // if still drifting quickly, determine a slow down acceleration based on mass of ship and a fake drag value
                    if (StructureRigidbody.velocity.magnitude > Ship.MINSPACEDRIFTSPEED || delayedForce.magnitude > 0)
                    {
                        drifting = true;

                        acceleration = delayedForce + (-StructureRigidbody.velocity.normalized * (Ship.SPACEDRIFTDRAG / StructureData.Mass));
                        /*
                        if (acceleration.magnitude > StructureRigidbody.velocity.magnitude / Time.fixedDeltaTime)
                        {
                            // counter any residual velocity via a precise acceleration in the opposite direction
                            acceleration = -StructureRigidbody.velocity / Time.fixedDeltaTime;
                        }*/
                    }
                    else
                    {
                        // counter any residual velocity via a precise acceleration in the opposite direction
                        acceleration = -StructureRigidbody.velocity / Time.fixedDeltaTime;
                    }
                }
                else
                {
                    // determine acceleration (F=ma -> a = F/m)
                    acceleration = (TotalSteeringForce + delayedForce) / StructureData.Mass;
                }

                // calculate new velocity if any acceleration
                if (acceleration.magnitude > 0)
                {
                    // Debug.Log ("Acceleration: " + acceleration + "   mag: " + acceleration.magnitude);

                    // get desired change in velocity
                    newVelocity = StructureRigidbody.velocity + (acceleration * Time.fixedDeltaTime);

                    // SPEED LIMITER!!!
                    if (newVelocity.magnitude > ShipData.SpeedLimiter)
                    {
                        newVelocity = newVelocity.normalized * ShipData.SpeedLimiter;
                    }

                    // Debug.Log ("Pre Velocity: " + newVelocity.magnitude);

                    // correct force based on maximum turning rate (if not drifting)
                    if (newVelocity.magnitude > 0 && drifting == false && delayedForce.magnitude == 0)
                    {
                        float maxTurn = ShipData.MaxTurnRate * Time.deltaTime;

                        angleToDestination = Vector2.Angle(newVelocity, transform.up);

                        if (angleToDestination > maxTurn)
                        {
                            // Debug.Log ("Turn too great. Angle: " + angleToDestination * Mathf.Rad2Deg + "   Max possible: " + maxTurn * Mathf.Rad2Deg);
                            // Debug.Log ("Current velocity: " + transform.rigidbody.velocity + "   requested velocity: " + newVelocity);				
                            newVelocity = correctVelocity(StructureRigidbody.velocity, newVelocity, maxTurn);
                            // Debug.Log ("Corrected velocity: " + newVelocity);	
                        }
                    }

                    Vector2 desiredVelocity = newVelocity;

                    // limit ship speed to maximum velocity
                    if (desiredVelocity.magnitude > (MaxSpeed * Helm.throttle))
                    {
                        // Debug.Log ("Velocity too high: " + newVelocity + "   mag: " + newVelocity.magnitude);
                        desiredVelocity = desiredVelocity.normalized * (MaxSpeed * Helm.throttle);
                        // Debug.Log ("Velocity reduced to: " + newVelocity + "   mag: " + newVelocity.magnitude);
                    }

                    // don't clamp velocity based on max delta v if drifting
                    if (drifting == false)
                    {
                        newVelocity = newVelocity.normalized * Mathf.Clamp(desiredVelocity.magnitude, prevVelocity.magnitude - maxDeltaV, prevVelocity.magnitude + maxDeltaV);
                    }

                    // set new velocity and speed
                    Speed = newVelocity.magnitude;
                    StructureRigidbody.velocity = newVelocity;
                }
            }

            // Debug.Log("Total steering force: " + totalSteeringForce + "   magnitude: " + totalSteeringForce.magnitude);

            if (delayedForce.magnitude > 0)
            {
                if (delayedForce.magnitude < ShipData.MaxForce / 100.0f)
                {
                    delayedForce = Vector2.zero;
                }
                else
                {
                    delayedForce *= Ship.DELAYEDFORCEDECREASEFACTOR;
                }
            }

            prevVelocity = StructureRigidbody.velocity;
            actualVelocity = StructureRigidbody.velocity.magnitude;
        }
        #endregion

        #region event dispatchers
        ////////////////////////////////////
        /*
			Event dispatchers for all Structure events
		*/
        ////////////////////////////////////	

        public void Call_WarpIn(object sender, WarpEventArgs args)
        {
            if (WarpIn != null)
            {
                WarpIn(sender, args);
            }
        }

        public void Call_WarpOut(object sender, WarpEventArgs args)
        {
            if (WarpOut != null)
            {
                WarpOut(sender, args);
            }
        }

        public void Call_HasWarpedIn(object sender, WarpEventArgs args)
        {
            if (HasWarpedIn != null)
            {
                HasWarpedIn(sender, args);
            }
        }

        public void Call_HasWarpedOut(object sender, WarpEventArgs args)
        {
            if (HasWarpedOut != null)
            {
                HasWarpedOut(sender, args);
            }
        }

        public void Call_ShipHasWarpedIn(object sender, ShipWarpedEventArgs args)
        {
            if (ShipHasWarpedIn != null)
            {
                ShipHasWarpedIn(sender, args);
            }

            GameMode.Call_ShipHasWarpedIn(sender, args);
        }

        public void Call_ShipHasWarpedOut(object sender, ShipWarpedEventArgs args)
        {
            if (ShipHasWarpedOut != null)
            {
                ShipHasWarpedOut(sender, args);
            }

            GameMode.Call_ShipHasWarpedOut(sender, args);
        }

        public void Call_SquadronHasWarpedIn(object sender, SquadronWarpedEventArgs args)
        {
            if (SquadronHasWarpedIn != null)
            {
                SquadronHasWarpedIn(sender, args);
            }

            GameMode.Call_SquadronHasWarpedIn(sender, args);
        }

        public void Call_SquadronHasWarpedOut(object sender, SquadronWarpedEventArgs args)
        {
            if (SquadronHasWarpedOut != null)
            {
                SquadronHasWarpedOut(sender, args);
            }

            GameMode.Call_SquadronHasWarpedOut(sender, args);
        }

        public void Call_WingHasWarpedIn(object sender, WingWarpedEventArgs args)
        {
            if (WingHasWarpedIn != null)
            {
                WingHasWarpedIn(sender, args);
            }

            GameMode.Call_WingHasWarpedIn(sender, args);
        }

        public void Call_WingHasWarpedOut(object sender, WingWarpedEventArgs args)
        {
            if (WingHasWarpedOut != null)
            {
                WingHasWarpedOut(sender, args);
            }

            GameMode.Call_WingHasWarpedOut(sender, args);
        }

        public void Call_FleetHasWarpedIn(object sender, FleetWarpedEventArgs args)
        {
            if (FleetHasWarpedIn != null)
            {
                FleetHasWarpedIn(sender, args);
            }

            GameMode.Call_FleetHasWarpedIn(sender, args);
        }

        public void Call_FleetHasWarpedOut(object sender, FleetWarpedEventArgs args)
        {
            if (FleetHasWarpedOut != null)
            {
                FleetHasWarpedOut(sender, args);
            }

            GameMode.Call_FleetHasWarpedOut(sender, args);
        }

        #endregion

        #region event handlers
        ///////////////////////////////////////////
        /*
			Handlers for Structure events
		*/
        ///////////////////////////////////////////	
        protected virtual void Ship_WarpIn(object sender, WarpEventArgs args)
        {
             if (args != null)
            {               
                // for campaign modes, check if the ship was previously present in the scene and delete the duplicate that isn't warping back in
                if (GameManager.Instance.campaignMode == true)
                {
                    List<Ship> structuresInScene = SceneManager.GetActiveScene().FindObjectsOfType<Ship>();

                    for (int i = structuresInScene.Count - 1; i >= 0; i--)
                    {
                        UniqueId structureId = structuresInScene[i].GetComponent<UniqueId>();
                        UniqueId warpStructureId = args.warpShipGO.GetComponent<UniqueId>();

                        // same Unity object?
                        if (structureId != null && warpStructureId != null)
                        {
                            if (structureId.uniqueId.Equals(warpStructureId.uniqueId))
                            {
                                // non-warping ship?
                                if (structuresInScene[i].shipState != ShipState.WARPING)
                                {
                                    Destroy(structuresInScene[i].gameObject);
                                }
                            }
                        }
                    }
                }               

                // make sure the ship has been initialised
                if (args.warpShip.ObjectInitialised == false)
                {
                    args.warpShip.init();
                }

                if (engines != null)
                {
                    foreach (IEngine engine in engines)
                    {
                        TrailRenderer exhaust = engine.getGameObject().GetComponentInChildren<TrailRenderer>();

                        if (exhaust != null)
                        {
                            exhaust.Clear();
                        }
                    }
                }

                // if ship is present in scene but deactivated, make it active and spawn it in
                if (args.warpShipGO.activeInHierarchy == false)
                {
                    args.warpShipGO.SetActive(true);                 
                    args.warpShip.spawn();                    
                }
                
                // boot the ship's controller if not previously booted
                if (args.warpShip.Controller.booted == false)
                {
                    args.warpShip.Controller.boot(args.warpShip, args.warpShip.Helm);
                }
                
                hideObject();

                StructureRigidbody.constraints = RigidbodyConstraints2D.FreezeAll;

                // spawn warp in effect
                StartCoroutine(warpInSequence(args));

                if (args.warpFromScenePath != null && args.warpToScenePath != null)
                {
                    D.log("Controller", args.warpShipGO.name + " has warped from " + args.warpFromScenePath + " to " + args.warpToScenePath + " at position " + args.warpPosition + " rotation " + args.warpRotation);
                }
                else
                {
                    D.log("Controller", args.warpShipGO.name + " has warped to  " + args.warpPosition);
                }
            }
        }

        protected virtual void Ship_HasWarpedIn(object sender, WarpEventArgs args)
        {
            GameManager.Instance.refreshStructureLists();

            shipState = ShipState.FLYING;

            args.warpShip.spawnedIn = true;

            GameManager.Instance.addStructure(args.warpShipGO.GetComponent<Ship>());

            // set reference to game mode for warped to scene
            Gamemode = GameManager.Instance.Gamemode;

            showObject();

            // if the captain name should begin cycling then start the coroutine
            if (NoxGUI.Instance.showCaptains == true && NoxGUI.Instance.showNames == true)
            {
                NameLabel.startCyclingNamesCoroutine();
            }

            if (engines != null)
            {
                foreach (IEngine engine in engines)
                {
                    TrailRenderer exhaust = engine.getGameObject().GetComponentInChildren<TrailRenderer>();

                    if (exhaust != null)
                    {
                        exhaust.Clear();
                    }

                    foreach (IVisualEffect vfx in engine.getVFXs())
                    {
                        vfx.startVFX();
                    }
                }
            }

            StructureRigidbody.constraints = RigidbodyConstraints2D.None;

            checkBoundary();

            OutsideBoundary = false;
            BoundaryTimer = 0;

            Gamemode.initController(this);

            if (SquadronData == null) return;

            // check if all ships in the squad/squadron has warped in and call all squadron warpedin event
            if (SquadronData.NumSquadronMembers == SquadronData.MaxShipsInSquadron)
            {
                Call_SquadronHasWarpedIn(this, new SquadronWarpedEventArgs(SquadronData.label, true));
            }

            // check if all ships in the wing has warped in and call all wing warpedin event
            if (WingData.NumWingMembers == WingData.MaxShipsInWing)
            {
                Call_WingHasWarpedIn(this, new WingWarpedEventArgs(WingData.label, true));
            }

            // check if all ships in fleet has warped in and call all fleet warpedin event
            if (FleetData.NumFleetMembers == FleetData.MaxShipsInFleet)
            {
                Call_FleetHasWarpedIn(this, new FleetWarpedEventArgs(FleetData.label, true));
            }
        }

        protected virtual void Ship_WarpOut(object sender, WarpEventArgs args)
        {
            if (args.warpShipGO != null && GameManager.Instance.EffectsParent != null)
            {
                args.warpShip.shipState = ShipState.WARPING;

                foreach (IEngine engine in engines)
                {
                    foreach (IVisualEffect vfx in engine.getVFXs())
                    {
                        vfx.stopVFX();
                    }
                }

                StartCoroutine(warpOutSequence(args));

                if (args.warpFromScenePath != null && args.warpToScenePath != null && args.warpPosition != null && args.warpRotation != null)
                {
                    D.log("Controller", args.warpShipGO.name + " is warping out of " + args.warpFromScenePath + " to " + args.warpToScenePath + " at position " + args.warpPosition + " rotation " + args.warpRotation);
                }
                else
                {
                    D.log("Controller", args.warpShipGO.name + " is warping out");
                }
            }
        }

        protected virtual void Ship_HasWarpedOut(object sender, WarpEventArgs args)
        {
            shipState = ShipState.UNKNOWN;

            args.warpShip.spawnedIn = false;

            GameManager.Instance.removeStructure(args.warpShipGO.GetComponent<Ship>());

            hideObject();

            foreach (IEngine engine in engines)
            {
                engine.resetEngineTrails();
            }
        }

        protected override void Structure_DockInitiatorDocked(object sender, DockingPortEventArgs args)
        {
            // D.log("Structure", args.ship.gameObject.name + " has docked with " + args.portStructure.gameObject.name);

            StructureRigidbody.velocity = Vector2.zero;

            Helm.destination = null;
            Helm.RangeToDestination = 0;

            // turn off stuff

            foreach (IEngine engine in engines)
            {
                if (engine.isActiveOn() == true)
                {
                    engine.deactivate();
                }
            }

            Gamemode.Gui.setMessage(args.ship.gameObject.name + " has docked with " + args.portStructure.gameObject.name);
        }

        protected override void Structure_DockInitiatorUndocked(object sender, DockingPortEventArgs args)
        {
            // D.log("Structure", args.ship.gameObject.name + " has docked with " + args.portStructure.gameObject.name);

            Gamemode.Gui.setMessage(args.ship.gameObject.name + " has undocked from " + args.portStructure.gameObject.name);

            AIStateController aiStateController = Controller as AIStateController;

            if (aiStateController != null)
            {
                aiStateController.reset();
            }

            foreach (IEngine engine in engines)
            {
                if (engine.isActiveOn() == true)
                {
                    engine.activate();
                }
            }

            setBearing();
        }

        protected override void Structure_LandingInitiatorLanded(object sender, HangarEventArgs args)
        {
            Gamemode.Gui.setMessage(args.ship.gameObject.name + " has landed inside " + args.hangarStructure.gameObject.name);

            shipState = ShipState.LANDED;

            Helm.destination = null;
            Helm.RangeToDestination = 0;

            IFormationFly formationController = Controller as IFormationFly;

            if (formationController != null)
            {
                Ship newLeader = formationController.pickNewLeader();

                if (newLeader != null)
                {
                    AIStateController leaderController = newLeader.Controller as AIStateController;

                    if (leaderController != null)
                    {
                        leaderController.state = "LAND";
                        newLeader.shipState = ShipState.LANDING;
                    }
                }
            }
        }

        protected override void Structure_LaunchInitiatorLaunched(object sender, HangarEventArgs args)
        {
            Gamemode.Gui.setMessage(args.ship.gameObject.name + " has launched from " + args.hangarStructure.gameObject.name);

            // allow hot start of launched ship
            foreach (IEngine engine in engines)
            {
                if (engine.isActiveOn() == true)
                {
                    engine.setActiveOn(true);
                }
            }

            Helm.desiredThrottle = 1.0f;

            transform.forward = Quaternion.Euler(args.hangar.getApproachVector()) * transform.forward;

            setBearing();
            
            shipState = ShipState.FLYING;
        }
        #endregion
    }
}