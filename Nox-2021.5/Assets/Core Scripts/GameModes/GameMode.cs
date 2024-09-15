using System.Collections.Generic;

using UnityEngine;

using NoxCore.Controllers;
using NoxCore.Cameras;
using NoxCore.Data;
using NoxCore.Debugs;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Fittings.Weapons;
using NoxCore.GUIs;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Utilities;

namespace NoxCore.GameModes
{
	#region standard gameplay event argument classes
	////////////////////////////////////
	/*
		Standard game mode specific event arguments
	*/
	////////////////////////////////////

	public class CameraEventArgs : EventArgs
	{
        public bool suppressMessage;

        public CameraEventArgs(bool suppressMessage = false)
        {
            this.suppressMessage = suppressMessage;
        }
    }
	
	public class DebugEventArgs : EventArgs
	{
		public bool cursorActive;
		
		public DebugEventArgs(bool cursorActive)
		{
			this.cursorActive = cursorActive;
		}
	}

    public class InputEventArgs : EventArgs
    {}

    public class GameEventArgs : EventArgs
	{}
	#endregion	
	
	public abstract class GameMode : MonoBehaviour, ICollisionManager
	{
		// all match states
		public enum MatchState
		{
			ENTERINGSCENE,		// initial state. GameObjects not ticking. Scene not fully initialiased.
			WAITINGTOSTART,		// next state. HandleMatchIsWaitingToStart is called when entering. GameObjects are ticking but players not spawned in. Next state when ReadyToStartMatch returns true or player hits StartMatch
			INPROGRESS,			// HandleMatchHasStarted is called when entering which calls BeginPlay (what, if anything, in Unity?) on all GameObjects. Next state if ReadyToEndMatch returns true or player hits EndMatch
			WAITINGPOSTMATCH,	// HandleMatchHasEnded is called when entering. GameObjects are still ticking but new players not accepted. Transitions when scene transfer starts.
			LEAVINGSCENE,		// last state. HandleLeavingScene is called when entering. Stay in this state while transferring to a new scene
			ABORTEDMATCH		// failure state started from AbortedMatch. Set only if their is an unrecoverable error.
		}

		#region standard game mode event definitions
		///////////////////////////////////////////
		/*
			Standard game mode events
		*/
		///////////////////////////////////////////	

        #region camera mode broadcast events
        public delegate void CameraEventDispatcher(object sender, CameraEventArgs cameraEventArgs);
		public static event CameraEventDispatcher CameraMode_Free;
		public static event CameraEventDispatcher CameraMode_UserSelected;
		#endregion

		#region debug mode broadcast events
		public delegate void DebugEventDispatcher(object sender, DebugEventArgs debugEventArgs);		
		public static event DebugEventDispatcher DebugMode_Toggle;
		public static event DebugEventDispatcher DebugMode_NextChangeAmount;
		public static event DebugEventDispatcher DebugMode_PrevChangeAmount;
		public static event DebugEventDispatcher DebugMode_CursorUp;				
		public static event DebugEventDispatcher DebugMode_CursorDown;
        public static event DebugEventDispatcher DebugMode_Squadron;
		public static event DebugEventDispatcher DebugMode_Wing;
		public static event DebugEventDispatcher DebugMode_Fleet;
		public static event DebugEventDispatcher DebugMode_Hull;		
		public static event DebugEventDispatcher DebugMode_Devices;
		public static event DebugEventDispatcher DebugMode_Modules;
		public static event DebugEventDispatcher DebugMode_Weapons;
		public static event DebugEventDispatcher DebugMode_Ordnances;		
		public static event DebugEventDispatcher DebugMode_Shields;
		public static event DebugEventDispatcher DebugMode_Engines;		
		//public static event DebugEventDispatcher DebugMode_Cloakers;
		public static event DebugEventDispatcher DebugMode_Maximise;
		public static event DebugEventDispatcher DebugMode_Minimise;		
		public static event DebugEventDispatcher DebugMode_Decrease;
		public static event DebugEventDispatcher DebugMode_Increase;
		public static event DebugEventDispatcher DebugMode_Explode;
		public static event DebugEventDispatcher DebugMode_Activate;
		public static event DebugEventDispatcher DebugMode_Deactivate;
		public static event DebugEventDispatcher DebugMode_Increment;
		public static event DebugEventDispatcher DebugMode_Decrement;
		public static event DebugEventDispatcher DebugMode_Damage;
		public static event DebugEventDispatcher DebugMode_SelfDestruct;
		public static event DebugEventDispatcher DebugMode_Raise;		
		public static event DebugEventDispatcher DebugMode_Lower;
		public static event DebugEventDispatcher DebugMode_Fail;
		/*public static event DebugEventDispatcher DebugMode_Cloak;		
		public static event DebugEventDispatcher DebugMode_Decloak;*/
		#endregion

		#region standard gameplay events
		// spawning, respawning, despawning, warping (ind and squad, wing, fleet), destroyed, mod destroyed, ultimate activation
		public delegate void SpawnEventDispatcher(object sender, SpawnEventArgs args);
		public static event SpawnEventDispatcher Spawn;

		public delegate void DespawnEventDispatcher(object sender, DespawnEventArgs args);
		public static event DespawnEventDispatcher Despawn;

		public delegate void RespawnEventDispatcher(object sender, RespawnEventArgs args);
		public static event RespawnEventDispatcher Respawn;
		public static event RespawnEventDispatcher HasRespawned;

		public delegate void DestroyEventDispatcher(object sender, DestroyEventArgs args);
		public static event DestroyEventDispatcher Destruct;

		public delegate void ModuleDestroyedEventDispatcher(object sender, ModuleDamageEventArgs args);
		public static event ModuleDestroyedEventDispatcher ModuleDestroyed;

		public delegate void WarpEventDispatcher(object sender, WarpEventArgs args);
		public static event WarpEventDispatcher WarpGateActivated;

		public delegate void ShipWarpedEventDispatcher(object sender, ShipWarpedEventArgs args);
		public static event ShipWarpedEventDispatcher ShipHasWarpedIn;
		public static event ShipWarpedEventDispatcher ShipHasWarpedOut;

		public delegate void SquadronWarpEventDispatcher(object sender, SquadronWarpedEventArgs args);
		public static event SquadronWarpEventDispatcher SquadronHasWarpedIn;
		public static event SquadronWarpEventDispatcher SquadronHasWarpedOut;

		public delegate void WingWarpEventDispatcher(object sender, WingWarpedEventArgs args);
		public static event WingWarpEventDispatcher WingHasWarpedIn;
		public static event WingWarpEventDispatcher WingHasWarpedOut;

		public delegate void FleetWarpEventDispatcher(object sender, FleetWarpedEventArgs args);
		public static event FleetWarpEventDispatcher FleetHasWarpedIn;
		public static event FleetWarpEventDispatcher FleetHasWarpedOut;

		public delegate void ActivateUltimateEventDispatcher(object sender, UltimateEventArgs args);
		public static event ActivateUltimateEventDispatcher ActivateUltimate;

		public delegate void DockingPortEventDispatcher(object sender, DockingPortEventArgs args);
		public static event DockingPortEventDispatcher DockInitiatorDocked;
		public static event DockingPortEventDispatcher DockReceiverDocked;
		public static event DockingPortEventDispatcher DockInitiatorUndocked;
		public static event DockingPortEventDispatcher DockReceiverUndocked;

		public delegate void HangarEventDispatcher(object sender, HangarEventArgs args);
		public static event HangarEventDispatcher LandingInitiatorLanded;
		public static event HangarEventDispatcher LandingReceiverLanded;
		public static event HangarEventDispatcher LaunchInitiatorLaunched;
		public static event HangarEventDispatcher LaunchReceiverLaunched;

        #endregion

        #endregion

        #region main game mode data
        protected bool _abort;
        public bool abort { get { return _abort; } }

        // standard camera mode variables
		protected BaseCameraEnum cameraMode;
		public BaseCameraEnum CameraMode { get { return cameraMode; } }
		
		// standard debug mode variables
		protected bool _toggleDebugMode;        
		public bool toggleDebugMode { get { return _toggleDebugMode; } set { _toggleDebugMode = value; } }
		protected BaseDebugEnum debugMode;

		protected bool playerControllerPresent;

		protected bool cursorActive;

        // FPS counter display
        protected FPSDisplay fpsDisplay;
        protected GameObject fpsPanelGO;

		[Tooltip("Used to modify values in debug mode")] public List<int> changeAmounts = new List<int>();
		protected int amountIndex;
		protected int currentDevice = -1;
		protected int currentModule = -1;
		protected int currentWeapon = -1;
		protected int currentShield = -1;
		protected int currentEngine = -1;
		//protected int currentCloak = -1;
		
		// standard game mode references & variables
		protected NoxGUI _Gui;
		public NoxGUI Gui { get { return _Gui; } set { _Gui = value; } }
		
		protected TopDown_Camera _Cam;
		public TopDown_Camera Cam { get { return _Cam; } set { _Cam = value; } }
		
		public bool generateResultsFile;
		public bool shouldSpawnAtStartSpot;
		public string scenarioFolder;
        public List<string> scenarioSubFolders = new List<string>();
        public string resultsFolder;
        public int maxPlayers;
        public int numPlayers;
		public int maxAIs;
		public int numAIs;
        public float minDespawnTime, maxDespawnTime;
        public float minRespawnTime, maxRespawnTime;

        public MatchState matchState;
		
		public bool damageOn;
		public float minDespawnDelay;
		public float minRespawnDelay;

        public float aiInitStartDelayRange;
        public bool randomAIStartDelay;
		
		protected List<Structure> agents = new List<Structure>();

        public abstract LayerMask? getCollisionMask(string collisionMaskName);

		[Header("Debug Keys")]
		[Tooltip("Toggle the debug mode on (note: must be tracking a structure to use debugging)")] public KeyCode debugModeToggle = KeyCode.BackQuote;
		[Tooltip("Affect the tracked structure's hull")] public KeyCode selectHull = KeyCode.H;
		[Tooltip("Affect the tracked structure's shields")] public KeyCode selectShields = KeyCode.S;
		[Tooltip("Affect the tracked structure's engines")] public KeyCode selectEngines = KeyCode.E;
		[Tooltip("Cause infinite damage to the structure or curosr selected object(s)")] public KeyCode explode = KeyCode.X;
		//[Tooltip("Trigger the structure's ultimate ability (if able)")] public KeyCode triggerUltimateAbility = KeyCode.Q;
		[Tooltip("Cause damage to the structure based on change amounts")] public KeyCode debugDamage = KeyCode.Space;
		[Tooltip("Affect the tracked structure's devices")] public KeyCode selectDevices = KeyCode.D;
		[Tooltip("Affect the tracked structure's modules")] public KeyCode selectModules = KeyCode.M;
		[Tooltip("Affect the tracked structure's weapons")] public KeyCode selectWeapons = KeyCode.W;
		[Tooltip("Affect the selected weapon's ordnance ")] public KeyCode selectOrdnances = KeyCode.O;
		//[Tooltip("Affect the tracked structure's cloaking device")] public KeyCode selectCloakers = KeyCode.C;
		[Tooltip("Affect the tracked ship's squadron")] public KeyCode selectSquadron = KeyCode.Alpha1;
		[Tooltip("Affect the tracked ship's wing")] public KeyCode selectWing = KeyCode.Alpha2;
		[Tooltip("Affect the tracked ship's fleet")] public KeyCode selectFleet = KeyCode.Alpha3;

		[Tooltip("Toggle the debug cursor on (note: the cursor is used to move through a group of similar objects on the structure e.g. devices)")] public KeyCode toggleCursorActive = KeyCode.C;
		[Tooltip("Move the cursor is to the next object in the group")] public KeyCode debugCursorUp = KeyCode.PageUp;
		[Tooltip("Move the cursor is to the previous object in the group")] public KeyCode debugCursorDown = KeyCode.PageDown;

		[Tooltip("Use the next change amount")] public KeyCode nextChangeAmount = KeyCode.RightBracket;
		[Tooltip("Use the previous change amount")] public KeyCode prevChangeAmount = KeyCode.LeftBracket;

		[Tooltip("Maximise the chosen module's armour or weapon's ammo")] public KeyCode maximise = KeyCode.Insert;
		[Tooltip("Minimise the chosen module's armour or weapon's ammo")] public KeyCode minimise = KeyCode.Delete;
		[Tooltip("Increment the shield's charge, module's armour or weapon's ammo by the current change mount")] public KeyCode increase = KeyCode.Equals;
		[Tooltip("Decrease the shield's charge, module's armour or weapon's ammo by the current change mount")] public KeyCode decrease = KeyCode.Minus;

		[Tooltip("Activate the chosen system")] public KeyCode activate = KeyCode.Home;
		[Tooltip("Deactivate the chosen system")] public KeyCode deactivate = KeyCode.End;

		#endregion

		#region game mode getters/setters
		// standard getters/setters
		public MatchState getMatchState()
		{
			return matchState;
		}	

		public string getScenarioFolder()
		{
			return scenarioFolder;
		}
        #endregion

        #region standard game mode rules
        #region agent management methods
        public Structure getAgent(int agentID)
		{
			if (agentID < 0)
			{
				D.error("GameMode: {0}", "Attempted to return a structure using an invalid agent ID: " + agentID);
			}
			else
			{		
				if (agentID < agents.Count)
				{
					return agents[agentID];
				}
			}
			
			return null;		
		}
		
		public int getAgentID(Structure structure)
		{
			if (structure == null)
			{
				D.error("GameMode", "Attempted to return an agent ID using a structure reference that is null");
			}
			else
			{
				for (int i = 0; i < agents.Count; i++)
				{
					if (agents[i] == structure)
					{
						return i;
					}
				}
			}
			
			return -1;
		}
		
		public Controller getAgentController(int agentID)
		{
			if (agentID < 0 || agentID >= agents.Count)
			{
				D.error("GameMode: {0}", "Attempted to return an AI controller using an invalid agent ID: " + agentID);
			}
			else
			{		
				return agents[agentID].Controller;
			}
			
			return null;
		}

        public bool removeController(Structure structure)
        {
            if (structure.Controller != null)
            {
                AIController aiController = structure.Controller as AIController;

                if (aiController != null)
                {
                    numAIs--;

                    D.log("Content", "AI controller removed from " + structure.name);
                }
                else
                {
                    numPlayers--;

                    D.log("Content", "Player controller removed from " + structure.name);
                }

                return true;
            }

            return false;
        }

		public bool addController(Structure structure)
		{
			if (structure == null)			
			{
				D.error("GameMode: {0}", "Attempted to return an agent ID using a structure reference that is null");
				return false;
			}

            AIController aiController = structure.Controller as AIController;

            if (aiController != null)
            {
                if (numAIs < maxAIs)
                {
                    if (structure.tag == "Ship" || structure.tag == "Structure")
                    {
                        agents.Add(structure);
                        numAIs++;
                        D.log("Content", "Added AI controlled structure to game mode");
                        return true;
                    }

                    return true;
                }
                else
                {
                    D.warn("Content: {0}", "Maximum number of AIs reached for this game mode");
                    return false;
                }
            }
            else
            {
                if (numPlayers < maxPlayers)
                {
                    if (structure.tag == "Ship" || structure.tag == "Structure")
                    {
                        agents.Add(structure);
                        numPlayers++;
                        D.log("Content", "Added player controlled structure to game mode");
                        return true;
                    }

                    return true;
                }
                else
                {
                    D.warn("Content: {0}", "Maximum number of players reached for this game mode");
                    return false;
                }
            }
        }	
		#endregion

		#region camera methods				
		public void setCameraMode(BaseCameraEnum mode)
		{
			cameraMode = mode;
		}
		
		public virtual void setInitialCameraTarget()
		{
			if (Cam.followTarget != null)
			{
				Cam.transform.position = new Vector3(Cam.followTarget.position.x + Cam.targetOffset.x, Cam.followTarget.position.y + Cam.targetOffset.y, Cam.followTarget.position.z + Cam.targetOffset.z);				
				
				setCameraMode(BaseCameraEnum.TRACK_SELECTED);
			}
			else if(Cam.acquireCameraTarget != "")
			{
				GameObject targetGO = GameObject.Find(Cam.acquireCameraTarget);
				
				if (targetGO != null)
				{
					Cam.setFollowTarget(targetGO.transform);
					
					Cam.transform.position = new Vector3(Cam.followTarget.position.x + Cam.targetOffset.x, Cam.followTarget.position.y + Cam.targetOffset.y, Cam.followTarget.position.z + Cam.targetOffset.z);
					
					setCameraMode(BaseCameraEnum.TRACK_SELECTED);						
				}
				else
				{
					if (Cam.acquireCameraTarget.Length > 0)
					{
						D.warn("Camera: {0}", "Cannot find initial camera target: " + Cam.acquireCameraTarget);
					}
					else
					{
						Cam.transform.position = new Vector3(transform.position.x, Cam.targetOffset.y, transform.position.z);
					}

					setCameraMode(BaseCameraEnum.FREE);					
				}
			}
			else
			{		
				Cam.transform.position = new Vector3(0, Cam.targetOffset.y, 0);
				setCameraMode(BaseCameraEnum.FREE);
			}					
		}
		#endregion

		#region debugging methods
        public virtual void reportAtCursor()
        {
			if (Cam.followTarget != null)
			{
				Structure structure = Cam.followTarget.GetComponent<Structure>();

				if (debugMode == BaseDebugEnum.DEVICES)
				{
					if (currentDevice == -1)
					{
						if (structure.Devices.Count > 0) currentDevice = 0;
					}

					if (currentDevice != -1)
					{
						Gui.setMessage("DEBUG: device cursor set to index " + currentDevice + ": " + structure.Devices[currentDevice].DeviceData.Type);
					}
				}
				else if (debugMode == BaseDebugEnum.MODULES)
				{
					if (currentModule == -1)
					{
						if (structure.Modules.Count > 0) currentModule = 0;
					}

					if (currentModule != -1)
					{
						Gui.setMessage("DEBUG: module cursor set to index " + currentModule + ": " + structure.Modules[currentModule].DeviceData.Type + ": " + structure.Modules[currentModule].getSocket().name);
					}
				}
				else if (debugMode == BaseDebugEnum.WEAPONS)
				{
					if (currentWeapon == -1)
					{
						if (structure.Weapons.Count > 0) currentWeapon = 0;
					}

					if (currentWeapon != -1)
					{
						Gui.setMessage("DEBUG: module cursor set to index " + currentWeapon + ": " + structure.Weapons[currentWeapon].DeviceData.Type + ": " + structure.Weapons[currentWeapon].getSocket().name);
					}
				}
				else if (debugMode == BaseDebugEnum.SHIELDS)
				{
					if (currentShield == -1)
					{
						if (structure.shields.Count > 0) currentShield = 0;
					}

					if (currentShield != -1)
					{
						Gui.setMessage("DEBUG: shield cursor set to index " + currentShield + ": " + structure.shields[currentShield].DeviceData.Type + ": " + structure.shields[currentShield].getSocket().name);
					}
				}
				else if (debugMode == BaseDebugEnum.ENGINES)
				{
					Ship ship = structure as Ship;

					if (ship != null)
					{
						if (currentEngine == -1 && ship.engines.Count > 0)
						{
							currentEngine = 0;
						}

						if (currentEngine != -1)
						{
							Gui.setMessage("DEBUG: engine cursor set to index " + currentEngine + ": " + ship.engines[currentEngine].DeviceData.Type);
						}
					}
				}/*
				else if (debugMode == BaseDebugEnum.CLOAKERS)
				{
					ICloakingDevice[] cloakers = structure.getModules<ICloakingDevice>().ToArray();

					if (currentCloak == -1)
					{
						if (cloakers.Length > 0) currentCloak = 0;
					}

					if (currentCloak == -1)
					{
						Gui.setMessage("DEBUG: cloak cursor set to index " + currentCloak + ": " + cloakers[currentCloak].DeviceData.Type);
					}
				}*/
			}
        }

		public virtual void moveCursor(bool down)
		{
			if (Cam.followTarget != null)
			{
				Structure structure = Cam.followTarget.GetComponent<Structure>();

				if (debugMode == BaseDebugEnum.DEVICES)
				{
					if (currentDevice == -1)
					{
						if (structure.Devices.Count > 0) currentDevice = 0;
					}

					if (down == false)
					{
						currentDevice--;

						if (currentDevice < 0) currentDevice = structure.Devices.Count - 1;
					}
					else
					{
						currentDevice++;

						if (currentDevice >= structure.Devices.Count) currentDevice = 0;
					}
				}
				else if (debugMode == BaseDebugEnum.MODULES)
				{
					if (down == false)
					{
						currentModule--;

						if (currentModule < 0) currentModule = structure.Modules.Count - 1;
					}
					else
					{
						currentModule++;

						if (currentModule >= structure.Modules.Count) currentModule = 0;
					}
				}
				else if (debugMode == BaseDebugEnum.WEAPONS)
				{
					if (down == false)
					{
						currentWeapon--;

						if (currentWeapon < 0) currentWeapon = structure.Weapons.Count - 1;
					}
					else
					{
						currentWeapon++;

						if (currentWeapon >= structure.Weapons.Count) currentWeapon = 0;
					}
				}
				else if (debugMode == BaseDebugEnum.SHIELDS)
				{
					if (down == false)
					{
						currentShield--;

						if (currentShield < 0) currentShield = structure.shields.Count - 1;
					}
					else
					{
						currentShield++;

						if (currentShield >= structure.shields.Count) currentShield = 0;
					}
				}
				else if (debugMode == BaseDebugEnum.ENGINES)
				{
					Ship ship = structure as Ship;

					if (ship != null)
					{
						if (down == false)
						{
							currentEngine--;

							if (currentEngine < 0) currentEngine = ship.engines.Count - 1;
						}
						else
						{
							currentEngine++;

							if (currentEngine >= ship.engines.Count) currentEngine = 0;
						}
					}
				}/*
				else if (debugMode == BaseDebugEnum.CLOAKERS)
				{
					ICloakingDevice[] cloakers = structure.getModules<ICloakingDevice>().ToArray();

					if (down == false)
					{
						currentCloak--;

						if (currentCloak < 0) currentCloak = cloakers.Length - 1;
					}
					else
					{
						currentCloak++;

						if (currentCloak >= cloakers.Length) currentCloak = 0;
					}
				}
				*/
				reportAtCursor();
			}
        }
		
		public virtual void checkDebugCommand()
		{
			if (Cam.followTarget != null)
			{
                if (Input.GetKeyDown(nextChangeAmount))
                {
                    if (changeAmounts.Count > 0)
                    {
                        Call_DebugMode_NextChangeAmount(this, null);
                    }
                    else
                    {
                        D.warn("GameMode: {0}", "No debug change amounts set in custom game mode");
                    }
                }
				else if (Input.GetKeyDown(prevChangeAmount))
				{
					if (changeAmounts.Count > 0)
					{
						Call_DebugMode_PrevChangeAmount(this, null);
					}
					else
					{
						D.warn("GameMode: {0}", "No debug change amounts set in custom game mode");
					}
				}

				if (Input.GetKeyDown(debugCursorUp))
				{
					Call_DebugMode_CursorUp(this, null);
				}
				else if (Input.GetKeyDown(debugCursorDown))
				{
					Call_DebugMode_CursorDown(this, null);
				}
				
				if (cursorActive == false)
				{				
					if (Input.GetKeyDown(maximise))
					{
						Call_DebugMode_Maximise(this, new DebugEventArgs(false));
					}
					else if (Input.GetKeyDown(minimise))
					{
						Call_DebugMode_Minimise(this, new DebugEventArgs(false));
					}
					
					if (Input.GetKeyDown(increase))
					{
						Call_DebugMode_Increase(this, new DebugEventArgs(false));
					}
					else if(Input.GetKeyDown(decrease))
					{
						Call_DebugMode_Decrease(this, new DebugEventArgs(false));
					}
					
					if (Input.GetKeyDown(explode))
					{
						Call_DebugMode_Explode(this, new DebugEventArgs(false));
					}
					
					if (Input.GetKeyDown(activate))
					{
						Call_DebugMode_Activate(this, new DebugEventArgs(false));
					}
					else if(Input.GetKeyDown(deactivate))
					{
						Call_DebugMode_Deactivate(this, new DebugEventArgs(false));
					}					
				}
				else
				{
					if (Input.GetKeyDown(maximise))
					{
						Call_DebugMode_Maximise(this, new DebugEventArgs(true));
					}
					else if (Input.GetKeyDown(minimise))
					{
						Call_DebugMode_Minimise(this, new DebugEventArgs(true));
					}
					
					if (Input.GetKeyDown(increase))
					{
						Call_DebugMode_Increase(this, new DebugEventArgs(true));
					}
					else if(Input.GetKeyDown(decrease))
					{
						Call_DebugMode_Decrease(this, new DebugEventArgs(true));
					}
					
					if (Input.GetKeyDown(explode))
					{
						Call_DebugMode_Explode(this, new DebugEventArgs(true));
					}
					
					if (Input.GetKeyDown(activate))
					{
						Call_DebugMode_Activate(this, new DebugEventArgs(true));
					}
					else if(Input.GetKeyDown(deactivate))
					{
						Call_DebugMode_Deactivate(this, new DebugEventArgs(true));
					}											
				}				
			}           
		}
		#endregion

		///////////////////////////////////////////
		/*
			abstract methods for match state
		*/		
		///////////////////////////////////////////				
		
		protected abstract bool ReadyToStartMatch();
		protected abstract bool ReadyToEndMatch();
		
		///////////////////////////////////////////
		/*
			other abstract/virtual methods
		*/		
		///////////////////////////////////////////			

		// TODO - should have a set match state method for direct manipulation of match state?
		
		public virtual void init()
		{
			Debug.Log("GameMode - init");

			// set reference to main camera
			Cam = GameObject.Find("Main Camera").GetComponent<TopDown_Camera>();
            Cam.reset();

            fpsPanelGO = GameObject.Find("Managers/UI Manager/Canvas/FPS Panel");

            if (fpsPanelGO != null)
            {
                fpsDisplay = fpsPanelGO.GetComponent<FPSDisplay>();
            }
		}

        public virtual void initController(Structure structure)
        {}

        public virtual void positionStructure(Structure structureGO)
        {}
		
		public virtual float getDespawnTime(StructureController controller, Structure structure)
		{
			// get this info from custom game mode, or the AIController (if present) otherwise from each individual structure
			if (controller != null)
			{
				return controller.getDespawnTime();
			}
			else
			{
				return structure.StructureData.DespawnTime;
			}
		}		
		
		public virtual float getRespawnTime(StructureController controller, Structure structure)
		{
            // get this info from custom game mode, or the AIController (if present) otherwise from each individual structure
            if (controller != null)
			{
				return controller.getRespawnTime();
			}
			else
			{
				return structure.StructureData.RespawnTime;
			}
		}

		public virtual bool canRespawn(StructureController controller, Structure structure)
		{
		    // get this info from the AIController (if present) otherwise from each individual structure
		    if (controller != null)
		    {
			    return controller.canRespawn();
		    }
		    else
		    {
			    return structure.CanRespawn;
		    }
		}

        public virtual Vector2 getSpawnPosition() { return Vector3.zero; }

        public virtual float getSpawnRotation()
        {
            Vector2 randRot = UnityEngine.Random.insideUnitCircle;

            float bearing = (Mathf.Atan2(-randRot.y, randRot.x) * Mathf.Rad2Deg) + 90;

            if (bearing < 0) bearing += 360;

            return bearing;
        }
        #endregion

        #region Unity events/messages

        protected virtual void OnEnable()
        {
			#region subscribe to standard match events
			GameEventManager.EnteringScene += GameEventManager_EnteringScene;
			GameEventManager.MatchIsWaitingToStart += GameEventManager_MatchIsWaitingToStart;
			GameEventManager.MatchHasStarted += GameEventManager_MatchHasStarted;
			GameEventManager.MatchHasEnded += GameEventManager_MatchHasEnded;
			GameEventManager.LeavingScene += GameEventManager_LeavingScene;
			GameEventManager.AbortedMatch += GameEventManager_AbortedMatch;
			#endregion

			#region subscribe to standard camera mode events
			CameraMode_Free += GameMode_CameraMode_Free;
			CameraMode_UserSelected += GameMode_CameraMode_UserSelected;
			#endregion

			#region subscribe to standard debug mode events
			DebugMode_Toggle += GameMode_DebugMode_Toggle;
			DebugMode_Squadron += GameMode_DebugMode_Squadron;
			DebugMode_Wing += GameMode_DebugMode_Wing;
			DebugMode_Fleet += GameMode_DebugMode_Fleet;
			DebugMode_Hull += GameMode_DebugMode_Hull;
			DebugMode_Devices += GameMode_DebugMode_Devices;
			DebugMode_Modules += GameMode_DebugMode_Modules;
			DebugMode_Weapons += GameMode_DebugMode_Weapons;
			DebugMode_Ordnances += GameMode_DebugMode_Ordnances;
			DebugMode_Shields += GameMode_DebugMode_Shields;
			DebugMode_Engines += GameMode_DebugMode_Engines;
			//DebugMode_Cloakers += GameMode_DebugMode_Cloakers;

			DebugMode_NextChangeAmount += GameMode_DebugMode_NextChangeAmount;
			DebugMode_PrevChangeAmount += GameMode_DebugMode_PrevChangeAmount;
			DebugMode_CursorUp += GameMode_DebugMode_CursorUp;
			DebugMode_CursorDown += GameMode_DebugMode_CursorDown;
			DebugMode_Maximise += GameMode_DebugMode_Maximise;
			DebugMode_Minimise += GameMode_DebugMode_Minimise;
			DebugMode_Increase += GameMode_DebugMode_Increase;
			DebugMode_Decrease += GameMode_DebugMode_Decrease;
			DebugMode_Explode += GameMode_DebugMode_Explode;
			DebugMode_Activate += GameMode_DebugMode_Activate;
			DebugMode_Deactivate += GameMode_DebugMode_Deactivate;
			DebugMode_Increment += GameMode_DebugMode_Increment;
			DebugMode_Decrement += GameMode_DebugMode_Decrement;
			DebugMode_Damage += GameMode_DebugMode_Damage;
			DebugMode_SelfDestruct += GameMode_DebugMode_SelfDestruct;
			DebugMode_Raise += GameMode_DebugMode_Raise;
			DebugMode_Lower += GameMode_DebugMode_Lower;
			DebugMode_Fail += GameMode_DebugMode_Fail;
			/*DebugMode_Cloak += GameMode_DebugMode_Cloak;
			DebugMode_Decloak += GameMode_DebugMode_Decloak;*/
			#endregion
        }

        protected virtual void OnDisable()
        {
			#region unsubscribe from standard match events
			GameEventManager.EnteringScene -= GameEventManager_EnteringScene;
			GameEventManager.MatchIsWaitingToStart -= GameEventManager_MatchIsWaitingToStart;
			GameEventManager.MatchHasStarted -= GameEventManager_MatchHasStarted;
			GameEventManager.MatchHasEnded -= GameEventManager_MatchHasEnded;
			GameEventManager.LeavingScene -= GameEventManager_LeavingScene;
			GameEventManager.AbortedMatch -= GameEventManager_AbortedMatch;
			#endregion

			#region unsubscribe from standard camera mode events
			CameraMode_Free -= GameMode_CameraMode_Free;
			CameraMode_UserSelected -= GameMode_CameraMode_UserSelected;
			#endregion

			#region unsubscribe from standard debug mode events
			DebugMode_Toggle -= GameMode_DebugMode_Toggle;
			DebugMode_Squadron -= GameMode_DebugMode_Squadron;
			DebugMode_Squadron -= GameMode_DebugMode_Wing;
			DebugMode_Squadron -= GameMode_DebugMode_Fleet;
			DebugMode_Hull -= GameMode_DebugMode_Hull;
			DebugMode_Devices -= GameMode_DebugMode_Devices;
			DebugMode_Modules -= GameMode_DebugMode_Modules;
			DebugMode_Weapons -= GameMode_DebugMode_Weapons;
			DebugMode_Ordnances -= GameMode_DebugMode_Ordnances;
			DebugMode_Shields -= GameMode_DebugMode_Shields;
			DebugMode_Engines -= GameMode_DebugMode_Engines;
			//DebugMode_Cloakers -= GameMode_DebugMode_Cloakers;

			DebugMode_NextChangeAmount -= GameMode_DebugMode_NextChangeAmount;
			DebugMode_PrevChangeAmount -= GameMode_DebugMode_PrevChangeAmount;
			DebugMode_CursorUp -= GameMode_DebugMode_CursorUp;
			DebugMode_CursorDown -= GameMode_DebugMode_CursorDown;
			DebugMode_Maximise -= GameMode_DebugMode_Maximise;
			DebugMode_Minimise -= GameMode_DebugMode_Minimise;
			DebugMode_Increase -= GameMode_DebugMode_Increase;
			DebugMode_Decrease -= GameMode_DebugMode_Decrease;
			DebugMode_Explode -= GameMode_DebugMode_Explode;
			DebugMode_Activate -= GameMode_DebugMode_Activate;
			DebugMode_Deactivate -= GameMode_DebugMode_Deactivate;
			DebugMode_Increment -= GameMode_DebugMode_Increment;
			DebugMode_Decrement -= GameMode_DebugMode_Decrement;
			DebugMode_Damage -= GameMode_DebugMode_Damage;
			DebugMode_SelfDestruct -= GameMode_DebugMode_SelfDestruct;
			DebugMode_Raise -= GameMode_DebugMode_Raise;
			DebugMode_Lower -= GameMode_DebugMode_Lower;
			DebugMode_Fail -= GameMode_DebugMode_Fail;
			/*DebugMode_Cloak -= GameMode_DebugMode_Cloak;
			DebugMode_Decloak -= GameMode_DebugMode_Decloak;*/
			#endregion
        }

        public virtual void Update()
		{
			#region standard key input
			if (Input.GetKeyDown(KeyCode.Escape))
			{
				GameEventManager.Call_AbortedMatch(this);
				enabled = false;
                return;
			}

            #endregion

            #region standard camera key input	
            // check for standard camera key input		
            if (Input.GetKeyDown(KeyCode.F1)) 
			{
				Call_CameraMode_Free(this, null);
			}
			
			if (Input.GetKeyDown(KeyCode.F2)) 
			{
				Call_CameraMode_UserSelected(this, null);
			}  

            if (playerControllerPresent == false && cameraMode == BaseCameraEnum.TRACK_SELECTED)
			{
				if (Input.GetMouseButtonDown(0))
				{
					Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

					RaycastHit2D[] hit2Ds = Physics2D.GetRayIntersectionAll(ray, Mathf.Infinity, 1 << LayerMask.NameToLayer("Structure") | 1 << LayerMask.NameToLayer("Ship"));

					if (hit2Ds.Length > 0)
					{
						GameManager.Instance.MainCamera.setFollowTarget(hit2Ds[hit2Ds.Length - 1].transform);
					}
					else
					{
						RaycastHit hit;

						if (Physics.Raycast(ray, out hit))
						{
							if (hit.transform.tag != "Celestial" && hit.transform.gameObject.layer == LayerMask.NameToLayer("Shield"))
							{
								GameManager.Instance.MainCamera.setFollowTarget(hit.transform.parent);
							}
						}
					}
				}
			}
			#endregion
			
			#region standard debug key input
			
			// check for standard debug input
			if (Input.GetKeyDown(KeyCode.BackQuote))
			{
				toggleDebugMode = !toggleDebugMode;

				if (toggleDebugMode) Gui.setMessage("Debug Mode ON");
				else Gui.setMessage("Debug Mode OFF");
			}

			if (toggleDebugMode && Cam.followTarget != null)
			{
				if (Input.GetKeyDown(toggleCursorActive))
                {
					cursorActive = !cursorActive;

					if (toggleDebugMode) Gui.setMessage("Debug Cursor ON");
					else Gui.setMessage("Debug Cursor OFF");
				}

				if (Input.GetKeyDown(selectSquadron))
                {
                    Call_DebugMode_Squadron(this, null);
                }

				if (Input.GetKeyDown(selectWing))
				{
					Call_DebugMode_Wing(this, null);
				}

				if (Input.GetKeyDown(selectFleet))
				{
					Call_DebugMode_Fleet(this, null);
				}
				if (Input.GetKeyDown(selectHull))
				{
                    Call_DebugMode_Hull(this, null);
				}			
				
				if (Input.GetKeyDown(debugDamage))
				{
                    Call_DebugMode_Damage(this, null);
				}

				if (Input.GetKeyDown(explode))
				{
					Call_DebugMode_SelfDestruct(this, null);
				}

				if (Input.GetKeyDown(selectShields))
				{
                    Call_DebugMode_Shields(this, null);
				}
				
				if (Input.GetKeyDown(selectEngines))
				{
                    Call_DebugMode_Engines(this, null);
				}                                
				
				if (Input.GetKeyDown(selectDevices))
				{
                    Call_DebugMode_Devices(this, null);
				}            
				
				if (Input.GetKeyDown(selectModules))
				{
                    Call_DebugMode_Modules(this, null);
				}            
				
				if (Input.GetKeyDown(selectWeapons))
				{
                    Call_DebugMode_Weapons(this, null);
				}				
				
				if (Input.GetKeyDown(selectOrdnances))
				{
                    Call_DebugMode_Ordnances(this, null);
				}            
				/*
				if (Input.GetKeyDown(selectCloakers))
				{
                    Call_DebugMode_Cloakers(this, null);
				}				
				*/
				checkDebugCommand();
			}
			#endregion

			if (matchState == MatchState.INPROGRESS)
			{
				if (ReadyToEndMatch() == true)
				{
					GameEventManager.Call_MatchHasEnded(this);
				}
			}
		}
        #endregion

        #region event dispatchers

        #region standard input event dispatchers
        ////////////////////////////////////
        /*
			Event dispatchers for the standard game mode
		*/
        ////////////////////////////////////	
        #endregion

        #region standard camera mode event dispatchers
        ////////////////////////////////////
        /*
			Event dispatchers for the standard camera modes
		*/
        ////////////////////////////////////	

        public static void Call_CameraMode_Free(object sender, CameraEventArgs args)
		{			
			if (CameraMode_Free != null)
			{
				CameraMode_Free(sender, args);
			}
		}	
		
		public static void Call_CameraMode_UserSelected(object sender, CameraEventArgs args)
		{			
			if (CameraMode_UserSelected != null)
			{
				CameraMode_UserSelected(sender, args);
			}
		}
		#endregion

		#region standard debug mode event dispatchers
		////////////////////////////////////
		/*
			Event dispatchers for the standard debug modes
		*/
		////////////////////////////////////		

		public static void Call_DebugMode_Toggle(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Toggle != null)
			{
				DebugMode_Toggle(sender, args);
			}
		}		
		
		public static void Call_DebugMode_NextChangeAmount(object sender, DebugEventArgs args)
		{			
			if (DebugMode_NextChangeAmount != null)
			{
                DebugMode_NextChangeAmount(sender, args);
			}
		}

		public static void Call_DebugMode_PrevChangeAmount(object sender, DebugEventArgs args)
		{
			if (DebugMode_PrevChangeAmount != null)
			{
				DebugMode_PrevChangeAmount(sender, args);
			}
		}

		public static void Call_DebugMode_CursorUp(object sender, DebugEventArgs args)
		{			
			if (DebugMode_CursorUp != null)
			{
				DebugMode_CursorUp(sender, args);
			}
		}						
		
		public static void Call_DebugMode_CursorDown(object sender, DebugEventArgs args)
		{			
			if (DebugMode_CursorDown != null)
			{
				DebugMode_CursorDown(sender, args);
			}
		}

        public static void Call_DebugMode_Squadron(object sender, DebugEventArgs args)
        {
            if (DebugMode_Squadron != null)
            {
                DebugMode_Squadron(sender, args);
            }
        }

		public static void Call_DebugMode_Wing(object sender, DebugEventArgs args)
		{
			if (DebugMode_Wing != null)
			{
				DebugMode_Wing(sender, args);
			}
		}

		public static void Call_DebugMode_Fleet(object sender, DebugEventArgs args)
		{
			if (DebugMode_Fleet != null)
			{
				DebugMode_Fleet(sender, args);
			}
		}

		public static void Call_DebugMode_Hull(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Hull != null)
			{
				DebugMode_Hull(sender, args);
			}
		}	
		
		public static void Call_DebugMode_Damage(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Damage != null)
			{
				DebugMode_Damage(sender, args);
			}
		}
		
		public static void Call_DebugMode_Devices(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Devices != null)
			{
				DebugMode_Devices(sender, args);
			}
		}			
		
		public static void Call_DebugMode_Modules(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Modules != null)
			{
				DebugMode_Modules(sender, args);
			}
		}			
		
		public static void Call_DebugMode_Weapons(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Weapons != null)
			{
				DebugMode_Weapons(sender, args);
			}
		}			
		
		public static void Call_DebugMode_Ordnances(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Ordnances != null)
			{
				DebugMode_Ordnances(sender, args);
			}
		}			
		
		public static void Call_DebugMode_Shields(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Shields != null)
			{
				DebugMode_Shields(sender, args);
			}
		}			
		
		public static void Call_DebugMode_Engines(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Engines != null)
			{
				DebugMode_Engines(sender, args);
			}
		}	
		/*
		public static void Call_DebugMode_Cloakers(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Cloakers != null)
			{
				DebugMode_Cloakers(sender, args);
			}
		}					
		*/
		public static void Call_DebugMode_Maximise(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Maximise != null)
			{
				DebugMode_Maximise(sender, args);
			}
		}						
		
		public static void Call_DebugMode_Minimise(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Minimise != null)
			{
				DebugMode_Minimise(sender, args);
			}
		}						
		
		public static void Call_DebugMode_Increase(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Increase != null)
			{
				DebugMode_Increase(sender, args);
			}
		}						
		
		public static void Call_DebugMode_Decrease(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Decrease != null)
			{
				DebugMode_Decrease(sender, args);
			}
		}											
		
		public static void Call_DebugMode_Explode(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Explode != null)
			{
				DebugMode_Explode(sender, args);
			}
		}						
		
		public static void Call_DebugMode_Activate(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Activate != null)
			{
				DebugMode_Activate(sender, args);
			}
		}						
		
		public static void Call_DebugMode_Deactivate(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Deactivate != null)
			{
				DebugMode_Deactivate(sender, args);
			}
		}								
		
		public static void Call_DebugMode_Increment(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Increment != null)
			{
				DebugMode_Increment(sender, args);
			}
		}		
		
		public static void Call_DebugMode_Decrement(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Decrement != null)
			{
				DebugMode_Decrement(sender, args);
			}
		}
		
		public static void Call_DebugMode_SelfDestruct(object sender, DebugEventArgs args)
		{			
			if (DebugMode_SelfDestruct != null)
			{
				DebugMode_SelfDestruct(sender, args);
			}
		}		
		
		public static void Call_DebugMode_Raise(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Raise != null)
			{
				DebugMode_Raise(sender, args);
			}
		}		
		
		public static void Call_DebugMode_Lower(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Lower != null)
			{
				DebugMode_Lower(sender, args);
			}
		}		
		
		public static void Call_DebugMode_Fail(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Fail != null)
			{
				DebugMode_Fail(sender, args);
			}
		}
		/*
		public static void Call_DebugMode_Decloak(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Decloak != null)
			{
				DebugMode_Decloak(sender, args);
			}
		}			
		
		public static void Call_DebugMode_Cloak(object sender, DebugEventArgs args)
		{			
			if (DebugMode_Cloak != null)
			{
				DebugMode_Cloak(sender, args);
			}
		}*/
		#endregion

		#region standard gameplay event dispatchers

		public static void Call_Spawn(object sender, SpawnEventArgs args)
		{
			if (Spawn != null)
			{
				Spawn(sender, args);
			}
		}

		public static void Call_Despawn(object sender, DespawnEventArgs args)
		{
			if (Despawn != null)
			{
				Despawn(sender, args);
			}
		}

		public static void Call_Respawn(object sender, RespawnEventArgs args)
		{
			if (Respawn != null)
			{
				Respawn(sender, args);
			}
		}

		public static void Call_HasRespawned(object sender, RespawnEventArgs args)
		{
			if (HasRespawned != null)
			{
				HasRespawned(sender, args);
			}
		}

		public static void Call_Destruct(object sender, DestroyEventArgs args)
		{
			if (Destruct != null)
			{
				Destruct(sender, args);
			}
		}

		public static void Call_ModuleDestroyed(object sender, ModuleDamageEventArgs args)
		{
			if (ModuleDestroyed != null)
			{
				ModuleDestroyed(sender, args);
			}
		}

		public static void Call_WarpGateActivated(object sender, WarpEventArgs args)
		{
			if (WarpGateActivated != null)
			{
				WarpGateActivated(sender, args);
			}
		}

		public static void Call_ShipHasWarpedIn(object sender, ShipWarpedEventArgs args)
		{
			if (ShipHasWarpedIn != null)
			{
				ShipHasWarpedIn(sender, args);
			}
		}

		public static void Call_ShipHasWarpedOut(object sender, ShipWarpedEventArgs args)
		{
			if (ShipHasWarpedOut != null)
			{
				ShipHasWarpedOut(sender, args);
			}
		}


		public static void Call_SquadronHasWarpedIn(object sender, SquadronWarpedEventArgs args)
		{
			if (SquadronHasWarpedIn != null)
			{
				SquadronHasWarpedIn(sender, args);
			}
		}

		public static void Call_SquadronHasWarpedOut(object sender, SquadronWarpedEventArgs args)
		{
			if (SquadronHasWarpedOut != null)
			{
				SquadronHasWarpedOut(sender, args);
			}
		}

		public static void Call_WingHasWarpedIn(object sender, WingWarpedEventArgs args)
		{
			if (WingHasWarpedIn != null)
			{
				WingHasWarpedIn(sender, args);
			}
		}

		public static void Call_WingHasWarpedOut(object sender, WingWarpedEventArgs args)
		{
			if (WingHasWarpedOut != null)
			{
				WingHasWarpedOut(sender, args);
			}
		}

		public static void Call_FleetHasWarpedIn(object sender, FleetWarpedEventArgs args)
		{
			if (FleetHasWarpedIn != null)
			{
				FleetHasWarpedIn(sender, args);
			}
		}

		public static void Call_FleetHasWarpedOut(object sender, FleetWarpedEventArgs args)
		{
			if (FleetHasWarpedOut != null)
			{
				FleetHasWarpedOut(sender, args);
			}
		}

		public static void Call_ActivateUltimate(object sender, UltimateEventArgs args)
		{
			if (ActivateUltimate != null)
			{
				ActivateUltimate(sender, args);
			}
		}

		public static void Call_DockInitiatorDocked(object sender, DockingPortEventArgs args)
		{
			if (DockInitiatorDocked != null)
            {
				DockInitiatorDocked(sender, args);
            }
		}

		public static void Call_DockReceiverDocked(object sender, DockingPortEventArgs args)
		{
			if (DockReceiverDocked != null)
            {
				DockReceiverDocked(sender, args);
            }
		}

		public static void Call_DockInitiatorUndocked(object sender, DockingPortEventArgs args)
		{
			if (DockInitiatorUndocked != null)
            {
				DockInitiatorUndocked(sender, args);
            }
		}

		public static void Call_DockReceiverUndocked(object sender, DockingPortEventArgs args)
		{
			if (DockReceiverUndocked != null)
            {
				DockReceiverUndocked(sender, args);
            }
		}

		public static void Call_LandingInitiatorLanded(object sender, HangarEventArgs args)
		{
			if (LandingInitiatorLanded != null)
            {
				LandingInitiatorLanded(sender, args);
            }
		}

		public static void Call_LandingReceiverLanded(object sender, HangarEventArgs args)
		{
			if (LandingReceiverLanded != null)
            {
				LandingReceiverLanded(sender, args);
            }
		}

		public static void Call_LaunchInitiatorLaunched(object sender, HangarEventArgs args)
		{
			if (LaunchInitiatorLaunched != null)
            {
				LaunchInitiatorLaunched(sender, args);
            }
		}

		public static void Call_LaunchReceiverLaunched(object sender, HangarEventArgs args)
		{
			if(LaunchReceiverLaunched != null)
            {
				LaunchReceiverLaunched(sender, args);
            }
		}
		#endregion

		#endregion

		#region event handlers

		#region standard match event handlers
		///////////////////////////////////////////
		/*
			Handlers for all standard game events go here
			Note: these should all be overridden and called from the custom game mode class
		*/
		///////////////////////////////////////////

		protected virtual void GameEventManager_EnteringScene(object sender)
		{
			D.log("Event", "Entering scene");
			matchState = MatchState.ENTERINGSCENE;

			if (Gui != null && generateResultsFile == false)
			{
				Gui.setMessage("Testing mode active");
				Gui.setMessage("Set generateResultsFile on game mode component to true to create results file");
			}
		}

		protected virtual void GameEventManager_MatchIsWaitingToStart(object sender)
		{
			D.log("Event", "Match is waiting to start");
			matchState = MatchState.WAITINGTOSTART;

			// start all structure and ship GameObjects ticking (note: normally you'd start all GameObjects ticking here)

			List<Structure> structures = GameManager.Instance.getAllStructures();

			foreach (Structure structure in structures)
			{
				structure.enabled = true;
			}

			if (FindObjectOfType<PlayerController>() != null)
            {
				playerControllerPresent = true;
            }
		}

		protected virtual void GameEventManager_MatchHasStarted(object sender)
		{
			D.log("Event", "Match has started");
			matchState = MatchState.INPROGRESS;
		}

		protected virtual void GameEventManager_MatchHasEnded(object sender)
		{
			D.log("Event", "Match has ended");
			matchState = MatchState.WAITINGPOSTMATCH;

			// stop all structure, ship and projectile GameObjects ticking (note: normally you'd stop all GameObjects ticking here)

			List<Structure> structures = GameManager.Instance.getAllStructures();

			foreach (Structure structure in structures)
			{
				structure.enabled = false;
			}

			GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");

			foreach (GameObject projectile in projectiles)
			{
				projectile.GetComponent<Projectile>().enabled = false;
			}
		}

		protected virtual void GameEventManager_LeavingScene(object sender)
		{
			D.log("Event", "Leaving scene");
			matchState = MatchState.LEAVINGSCENE;

			//			CameraMode_Free -= GameMode_CameraMode_Free;
			//			CameraMode_UserSelected -= GameMode_CameraMode_UserSelected;

			Debug.Log("Game mode - LeavingScene");

			CameraMode_Free = null;
			CameraMode_UserSelected = null;

			GameEventManager.reset();
		}

		protected virtual void GameEventManager_AbortedMatch(object sender)
		{
			D.log("Event", "Aborted match");
			matchState = MatchState.ABORTEDMATCH;

			_abort = true;

			// note: normally you'd return the player to a menu screen etc. here but we will just quit instead			
			Application.Quit();
		}
		#endregion

		#region standard input event handlers
		///////////////////////////////////////////
		/*
			Handlers for all standard input events go here
			Note: these could be overridden in the custom game mode class and added to
		*/
		///////////////////////////////////////////		

		protected virtual void GameMode_ToggleNames(object sender, InputEventArgs args)
        {
            Gui.toggleNames();
        }

        protected virtual void GameMode_ToggleFactions(object sender, InputEventArgs args)
        {
            Gui.toggleFactions();
        }
        #endregion

        #region standard camera mode event handlers
        ///////////////////////////////////////////
        /*
			Handlers for all standard camera mode events go here
			Note: these could be overridden in the custom game mode class and added to
		*/
        ///////////////////////////////////////////		

        protected virtual void GameMode_CameraMode_Free(object sender, CameraEventArgs args)
		{
			// D.log ("Event", "Camera mode set to FREE");	
			setCameraMode(BaseCameraEnum.FREE);
			
			Cam.setFollowTarget(null);
			
			Gui.setMessage("Camera mode set to FREE");
		}		
		
		protected virtual void GameMode_CameraMode_UserSelected(object sender, CameraEventArgs args)
		{
			// D.log ("Event", "Camera mode set to TRACK SELECTED");	
			setCameraMode(BaseCameraEnum.TRACK_SELECTED);
			
			Gui.setMessage("Camera mode set to TRACK SELECTED");
		}	
		#endregion

		#region standard debug mode event handlers
		///////////////////////////////////////////
		/*
			Handlers for all standard debug events go here
			Note: these could be overridden in the custom game mode class and added to
		*/		
		///////////////////////////////////////////				

		protected virtual void GameMode_DebugMode_Toggle(object sender, DebugEventArgs args)
		{
			toggleDebugMode = !toggleDebugMode;
			
			if (toggleDebugMode) Gui.setMessage("Debug Mode ON");
			else Gui.setMessage("Debug Mode OFF");			
		}					
		
		protected virtual void GameMode_DebugMode_NextChangeAmount(object sender, DebugEventArgs args)
		{		
			if (changeAmounts.Count > 0)
			{
				amountIndex++;
				
				if (amountIndex >= changeAmounts.Count) amountIndex = 0;
				
				// D.log ("Event", "DEBUG: change amount now at " + changeAmounts[amountIndex]);	
				Gui.setMessage("Change amount to now " + changeAmounts[amountIndex]);
			}			
		}

		protected virtual void GameMode_DebugMode_PrevChangeAmount(object sender, DebugEventArgs args)
		{
			if (changeAmounts.Count > 0)
			{
				amountIndex--;

				if (amountIndex < 0) amountIndex = changeAmounts.Count-1;

				// D.log ("Event", "DEBUG: change amount now at " + changeAmounts[amountIndex]);	
				Gui.setMessage("Change amount to now " + changeAmounts[amountIndex]);
			}
		}

		protected virtual void GameMode_DebugMode_CursorUp(object sender, DebugEventArgs args)
		{
			moveCursor(false);
		}
		
		protected virtual void GameMode_DebugMode_CursorDown(object sender, DebugEventArgs args)									
		{
			moveCursor(true);
		}

        protected virtual void GameMode_DebugMode_Squadron(object sender, DebugEventArgs args)
        {
            // D.log ("Event", "Debug mode set to SQUADRON");			

            if (toggleDebugMode) Gui.setMessage("Debug mode set to SQUADRON");

            debugMode = BaseDebugEnum.SQUADRON;
        }

		protected virtual void GameMode_DebugMode_Wing(object sender, DebugEventArgs args)
		{
			// D.log ("Event", "Debug mode set to WING");			

			if (toggleDebugMode) Gui.setMessage("Debug mode set to WING");

			debugMode = BaseDebugEnum.WING;
		}

		protected virtual void GameMode_DebugMode_Fleet(object sender, DebugEventArgs args)
		{
			// D.log ("Event", "Debug mode set to FLEET");			

			if (toggleDebugMode) Gui.setMessage("Debug mode set to FLEET");

			debugMode = BaseDebugEnum.FLEET;
		}

		protected virtual void GameMode_DebugMode_Hull(object sender, DebugEventArgs args)
		{
            // D.log ("Event", "Debug mode set to HULL");			

            if (toggleDebugMode) Gui.setMessage("Debug mode set to HULL");
            
            debugMode = BaseDebugEnum.HULL;
		}	
		
		protected virtual void GameMode_DebugMode_Shields(object sender, DebugEventArgs args)
		{
            // D.log ("Event", "Debug mode set to SHIELDS");		

            if (toggleDebugMode) Gui.setMessage("Debug mode set to SHIELDS");

            reportAtCursor();

            debugMode = BaseDebugEnum.SHIELDS;
		}	
		
		protected virtual void GameMode_DebugMode_Engines(object sender, DebugEventArgs args)
		{
            // D.log ("Event", "Debug mode set to ENGINES");	

            if (toggleDebugMode) Gui.setMessage("Debug mode set to ENGINES");

            reportAtCursor();

            debugMode = BaseDebugEnum.ENGINES;
		}		
		
		protected virtual void GameMode_DebugMode_Devices(object sender, DebugEventArgs args)
		{
            // D.log ("Event", "Debug mode set to DEVICES");	

            if (toggleDebugMode) Gui.setMessage("Debug mode set to DEVICES");

            reportAtCursor();

            debugMode = BaseDebugEnum.DEVICES;
		}	
		
		protected virtual void GameMode_DebugMode_Modules(object sender, DebugEventArgs args)
		{
            // D.log ("Event", "Debug mode set to MODULES");	

            if (toggleDebugMode) Gui.setMessage("Debug mode set to MODULES");

            reportAtCursor();

            debugMode = BaseDebugEnum.MODULES;
		}	
		
		protected virtual void GameMode_DebugMode_Weapons(object sender, DebugEventArgs args)
		{
            // D.log ("Event", "Debug mode set to WEAPONS");	

            if (toggleDebugMode) Gui.setMessage("Debug mode set to WEAPONS");

            reportAtCursor();

            debugMode = BaseDebugEnum.WEAPONS;
		}	
		
		protected virtual void GameMode_DebugMode_Ordnances(object sender, DebugEventArgs args)
		{
            // D.log ("Event", "Debug mode set to ORDNANCES");		

            if (toggleDebugMode) Gui.setMessage("Debug mode set to ORDNANCES");

            reportAtCursor();

            debugMode = BaseDebugEnum.ORDNANCES;
		}	
		/*
		protected virtual void GameMode_DebugMode_Cloakers(object sender, DebugEventArgs args)
		{
            // D.log ("Event", "Debug mode set to CLOAKERS");			

            if (toggleDebugMode) Gui.setMessage("Debug mode set to CLOAKERS");

            reportAtCursor();

            debugMode = BaseDebugEnum.CLOAKERS;
		}			
		*/
		protected virtual void GameMode_DebugMode_Maximise(object sender, DebugEventArgs args)		
		{
			Structure structure = Cam.followTarget.GetComponent<Structure>();
			
			if (debugMode == BaseDebugEnum.HULL) structure.debugMaximise(sender, args);
			else if (debugMode == BaseDebugEnum.DEVICES)
			{			
				if (args.cursorActive == true) 
				{
					if (currentDevice == -1 && structure.Devices.Count > 0)
					{
						currentDevice = 0;
					}
					
					structure.Devices[currentDevice].debugMaximise(sender, args);
				}
				else
				{
					foreach(ISystemDebuggable device in structure.Devices)
					{
						device.debugMaximise(sender, args);
					}
				}
			}
			else if (debugMode == BaseDebugEnum.MODULES)
			{
				if (args.cursorActive == true) 
				{
					if (currentModule == -1 && structure.Modules.Count > 0)
					{
						currentModule = 0;
					}
					
					structure.Modules[currentModule].debugMaximise(sender, args);
				}
				else
				{
					foreach(ISystemDebuggable module in structure.Modules)
					{
						module.debugMaximise(sender, args);
					}
				}
			}
			else if (debugMode == BaseDebugEnum.WEAPONS)
			{
				if (args.cursorActive == true) 
				{
					if (currentWeapon == -1 && structure.Weapons.Count > 0)
					{
						currentWeapon = 0;
					}
					
					structure.Weapons[currentWeapon].debugMaximise(sender, args);
				}
				else
				{
					foreach(ISystemDebuggable weapon in structure.Weapons)
					{
						weapon.debugMaximise(sender, args);
					}
				}
			}			
			else if (debugMode == BaseDebugEnum.ORDNANCES)
			{		
				if (args.cursorActive == true)
				{
					if (currentWeapon == -1 && structure.Weapons.Count > 0)
					{
						currentWeapon = 0;
					}				
					
					if (structure.Weapons[currentWeapon].destroyed != true)
					{
						structure.Weapons[currentWeapon].debugMaximise(sender, args);
					}				
				}
				else
				{
					foreach(Weapon weapon in structure.Weapons)
					{						
						if (weapon.destroyed != true)
						{
							structure.Weapons[currentWeapon].debugMaximise(sender, args);
						}					
					}
				}
			}
		}
		
		protected virtual void GameMode_DebugMode_Minimise(object sender, DebugEventArgs args)		
		{
			Structure structure = Cam.followTarget.GetComponent<Structure>();
			
			if (debugMode == BaseDebugEnum.HULL) structure.debugMinimise(sender, args);
			else if (debugMode == BaseDebugEnum.DEVICES)
			{
				if (args.cursorActive == true) 
				{
					if (currentDevice == -1 && structure.Devices.Count > 0)
					{
						currentDevice = 0;
					}
					
					structure.Devices[currentDevice].debugMinimise(sender, args);
				}
				else
				{
					foreach(ISystemDebuggable device in structure.Devices)
					{
						device.debugMinimise(sender, args);
					}
				}
			}
			else if (debugMode == BaseDebugEnum.MODULES)
			{
				if (args.cursorActive == true) 
				{
					if (currentModule == -1 && structure.Modules.Count > 0)
					{
						currentModule = 0;
					}
					
					structure.Modules[currentModule].debugMinimise(sender, args);
				}
				else
				{
					foreach(ISystemDebuggable module in structure.Modules)
					{
						module.debugMinimise(sender, args);
					}
				}
			}
			else if (debugMode == BaseDebugEnum.WEAPONS)
			{
				if (args.cursorActive == true) 
				{
					if (currentWeapon == -1 && structure.Weapons.Count > 0)
					{
						currentWeapon = 0;
					}
					
					structure.Weapons[currentWeapon].debugMinimise(sender, args);
				}
				else
				{
					foreach(ISystemDebuggable weapon in structure.Weapons)
					{
						weapon.debugMinimise(sender, args);
					}
				}
			}
			else if (debugMode == BaseDebugEnum.ORDNANCES)
			{
				if (args.cursorActive == true)
				{
					if (currentWeapon == -1 && structure.Weapons.Count > 0)
					{
						currentWeapon = 0;
					}

					if (structure.Weapons[currentWeapon].destroyed != true)
					{
						structure.Weapons[currentWeapon].debugMinimise(sender, args);
					}				
				}
				else
				{
					foreach(Weapon weapon in structure.Weapons)
					{
						if (weapon.destroyed != true)
						{
							weapon.debugMinimise(sender, args);
						}					
					}
				}				
			}			
		}
		
		protected virtual void GameMode_DebugMode_Increase(object sender, DebugEventArgs args)		
		{
			Structure structure = Cam.followTarget.GetComponent<Structure>();
			
			if (debugMode == BaseDebugEnum.HULL) structure.debugIncrease(sender, args, changeAmounts[amountIndex]);
			else if (debugMode == BaseDebugEnum.SHIELDS)
			{
				if (args.cursorActive == true)
				{
					if (currentShield == -1 && structure.shields.Count > 0)
					{
						currentShield = 0;
					}

					structure.shields[currentShield].debugIncrease(sender, args, changeAmounts[amountIndex]);
				}
				else
				{
					foreach (IShieldDebuggable shield in structure.shields)
					{
						shield.debugIncrease(sender, args, changeAmounts[amountIndex]);
					}
				}
			}
			else if (debugMode == BaseDebugEnum.DEVICES)
			{
				if (args.cursorActive == true) 
				{
					if (currentDevice == -1 && structure.Devices.Count > 0)
					{
						currentDevice = 0;
					}
					
					structure.Devices[currentDevice].debugIncrease(sender, args, changeAmounts[amountIndex]);
				}
				else
				{
					foreach(ISystemDebuggable device in structure.Devices)
					{
						device.debugIncrease(sender, args, changeAmounts[amountIndex]);
					}
				}
			}
			else if (debugMode == BaseDebugEnum.MODULES)
			{
				if (args.cursorActive == true) 
				{
					if (currentModule == -1 && structure.Modules.Count > 0)
					{
						currentModule = 0;
					}
					
					structure.Modules[currentModule].debugIncrease(sender, args, changeAmounts[amountIndex]);
				}
				else
				{
					foreach(ISystemDebuggable module in structure.Modules)
					{
						module.debugIncrease(sender, args, changeAmounts[amountIndex]);
					}
				}
			}
			else if (debugMode == BaseDebugEnum.WEAPONS)
			{
				if (args.cursorActive == true) 
				{
					if (currentWeapon == -1 && structure.Weapons.Count > 0)
					{
						currentWeapon = 0;
					}
					
					structure.Weapons[currentWeapon].debugIncrease(sender, args, changeAmounts[amountIndex]);
				}
				else
				{
					foreach(ISystemDebuggable weapon in structure.Weapons)
					{
						weapon.debugIncrease(sender, args, changeAmounts[amountIndex]);
					}
				}
			}
			else if (debugMode == BaseDebugEnum.ORDNANCES)
			{
				if (args.cursorActive == true)
				{
					if (currentWeapon == -1 && structure.Weapons.Count > 0)
					{
						currentWeapon = 0;
					}

					if (structure.Weapons[currentWeapon].destroyed != true)
					{
						structure.Weapons[currentWeapon].debugIncrease(sender, args, changeAmounts[amountIndex]);
					}				
				}
				else
				{
					foreach(Weapon weapon in structure.Weapons)
					{
						if (weapon.destroyed != true)
						{
							weapon.debugIncrease(sender, args, changeAmounts[amountIndex]);
						}					
					}
				}	
			}	
		}
		
		protected virtual void GameMode_DebugMode_Decrease(object sender, DebugEventArgs args)		
		{
			Structure structure = Cam.followTarget.GetComponent<Structure>();

			if (debugMode == BaseDebugEnum.HULL) structure.debugDecrease(sender, args, changeAmounts[amountIndex]);
			else if (debugMode == BaseDebugEnum.SHIELDS)
			{
				if (args.cursorActive == true)
				{
					if (currentShield == -1 && structure.shields.Count > 0)
					{
						currentShield = 0;
					}

					structure.shields[currentShield].debugDecrease(sender, args, changeAmounts[amountIndex]);
				}
				else
				{
					foreach (IShieldDebuggable shield in structure.shields)
					{
						shield.debugDecrease(sender, args, changeAmounts[amountIndex]);
					}
				}
			}
			else if (debugMode == BaseDebugEnum.DEVICES)
			{
				if (args.cursorActive == true)
				{
					if (currentDevice == -1 && structure.Devices.Count > 0)
					{
						currentDevice = 0;
					}

					structure.Devices[currentDevice].debugDecrease(sender, args, changeAmounts[amountIndex]);
				}
				else
				{
					foreach (ISystemDebuggable device in structure.Devices)
					{
						device.debugDecrease(sender, args, changeAmounts[amountIndex]);
					}
				}
			}
			else if (debugMode == BaseDebugEnum.MODULES)
			{
				if (args.cursorActive == true)
				{
					if (currentModule == -1 && structure.Modules.Count > 0)
					{
						currentModule = 0;
					}

					structure.Modules[currentModule].debugDecrease(sender, args, changeAmounts[amountIndex]);
				}
				else
				{
					foreach (ISystemDebuggable module in structure.Modules)
					{
						module.debugDecrease(sender, args, changeAmounts[amountIndex]);
					}
				}
			}
			else if (debugMode == BaseDebugEnum.WEAPONS)
			{
				if (args.cursorActive == true)
				{
					if (currentWeapon == -1 && structure.Weapons.Count > 0)
					{
						currentWeapon = 0;
					}

					structure.Weapons[currentWeapon].debugDecrease(sender, args, changeAmounts[amountIndex]);
				}
				else
				{
					foreach (ISystemDebuggable weapon in structure.Weapons)
					{
						weapon.debugDecrease(sender, args, changeAmounts[amountIndex]);
					}
				}
			}
			else if (debugMode == BaseDebugEnum.ORDNANCES)
			{
				if (args.cursorActive == true)
				{
					if (currentWeapon == -1 && structure.Weapons.Count > 0)
					{
						currentWeapon = 0;
					}

					if (structure.Weapons[currentWeapon].destroyed != true)
					{
						structure.Weapons[currentWeapon].debugDecrease(sender, args, changeAmounts[amountIndex]);
					}
				}
				else
				{
					foreach (Weapon weapon in structure.Weapons)
					{
						if (weapon.destroyed != true)
						{
							weapon.debugDecrease(sender, args, changeAmounts[amountIndex]);
						}
					}
				}
			}
		}
		
		protected virtual void GameMode_DebugMode_Explode(object sender, DebugEventArgs args)		
		{
			Structure structure = Cam.followTarget.GetComponent<Structure>();

            if (debugMode == BaseDebugEnum.SQUADRON)
            {
				FactionData faction = FactionManager.Instance.findFaction(structure.Faction.ID);

                if (faction != null)
                {
                    Ship ship = structure as Ship;

                    if (ship != null)
                    {
                        SquadronData squadron = faction.FleetManager.findSquadronData(ship.FleetData.ID, ship.WingData.ID, ship.SquadronData.ID);

                        if (squadron != null)
                        {
                            foreach(Ship squadronShip in squadron.getAllShips())
                            {
                                squadronShip.debugExplode(sender, args);
                            }
                        }
                    }                    
                }
            }
            else if (debugMode == BaseDebugEnum.HULL)
            {
                structure.debugExplode(sender, args);
            }
            else if (debugMode == BaseDebugEnum.DEVICES)
            {
                if (args.cursorActive == true)
                {
                    if (currentDevice == -1 && structure.Devices.Count > 0)
                    {
                        currentDevice = 0;
                    }

                    structure.Devices[currentDevice].debugExplode(sender, args);
                }
                else
                {
                    foreach (ISystemDebuggable device in structure.Devices)
                    {
                        device.debugExplode(sender, args);
                    }
                }
            }
            else if (debugMode == BaseDebugEnum.MODULES)
            {
                if (args.cursorActive == true)
                {
                    if (currentModule == -1 && structure.Modules.Count > 0)
                    {
                        currentModule = 0;
                    }

                    structure.Modules[currentModule].debugExplode(sender, args);
                }
                else
                {
                    foreach (ISystemDebuggable module in structure.Modules)
                    {
                        module.debugExplode(sender, args);
                    }
                }
            }
            else if (debugMode == BaseDebugEnum.WEAPONS)
            {
                if (args.cursorActive == true)
                {
                    if (currentWeapon == -1 && structure.Weapons.Count > 0)
                    {
                        currentWeapon = 0;
                    }

                    structure.Weapons[currentWeapon].debugExplode(sender, args);
                }
                else
                {
                    foreach (ISystemDebuggable weapon in structure.Weapons)
                    {
                        weapon.debugExplode(sender, args);
                    }
                }
            }
            else if (debugMode == BaseDebugEnum.SHIELDS)
            {
                if (args.cursorActive == true)
                {
                    if (currentShield == -1 && structure.shields.Count > 0)
                    {
                        currentShield = 0;
                    }

                    structure.shields[currentShield].debugExplode(sender, args);
                }
                else
                {
                    foreach (IShieldDebuggable shield in structure.shields)
                    {
                        shield.debugExplode(sender, args);
                    }
                }
            }
            else if (debugMode == BaseDebugEnum.ENGINES)
            {
                Ship ship = structure as Ship;

                if (ship != null)
                {
					if (args.cursorActive == true)
                    {
                        if (currentEngine == -1 && ship.engines.Count > 0)
                        {
                            currentEngine = 0;
                        }

						ship.engines[currentEngine].debugExplode(sender, args);
                    }
                    else
                    {
                        foreach (ISystemDebuggable engine in ship.engines)
                        {
                            engine.debugExplode(sender, args);
                        }
                    }
                }
            }/*
            else if (debugMode == BaseDebugEnum.CLOAKERS)
            {
                ICloakingDevice[] cloakerDevices = structure.getModules<ICloakingDevice>().ToArray();

                if (args.cursorActive == true)
                {
                    if (currentCloak == -1 && cloakerDevices.Length > 0)
                    {
                        currentCloak = 0;
                    }

					cloakerDevices[currentCloak].debugExplode(sender, args);
                }
                else
                {
                    foreach (ICloakDebuggable cloak in cloakerDevices)
                    {
                        cloak.debugExplode(sender, args);
                    }
                }
            }*/
		}
		
		protected virtual void GameMode_DebugMode_Activate(object sender, DebugEventArgs args)		
		{
			Structure structure = Cam.followTarget.GetComponent<Structure>();
			
			if (debugMode == BaseDebugEnum.HULL) structure.debugActivate(sender, args);
			else if (debugMode == BaseDebugEnum.DEVICES)
			{
				if (args.cursorActive == true) 
				{
					if (currentDevice == -1 && structure.Devices.Count > 0)
					{
						currentDevice = 0;
					}
					
					structure.Devices[currentDevice].debugActivate(sender, args);
				}
				else
				{
					foreach(ISystemDebuggable device in structure.Devices)
					{
						device.debugActivate(sender, args);
					}
				}
			}
			else if (debugMode == BaseDebugEnum.MODULES)
			{
				if (args.cursorActive == true) 
				{
					if (currentModule == -1 && structure.Modules.Count > 0)
					{
						currentModule = 0;
					}
					
					structure.Modules[currentModule].debugActivate(sender, args);
				}
				else
				{
					foreach(ISystemDebuggable module in structure.Modules)
					{
						module.debugActivate(sender, args);
					}
				}
			}
			else if (debugMode == BaseDebugEnum.WEAPONS)
			{
				if (args.cursorActive == true) 
				{
					if (currentWeapon == -1 && structure.Weapons.Count > 0)
					{
						currentWeapon = 0;
					}
					
					structure.Weapons[currentWeapon].debugActivate(sender, args);
				}
				else
				{
					foreach(ISystemDebuggable weapon in structure.Weapons)
					{
						weapon.debugActivate(sender, args);
					}
				}
			}
			else if (debugMode == BaseDebugEnum.SHIELDS)
			{
				if (args.cursorActive == true) 
				{
					if (currentShield == -1 && structure.shields.Count > 0)
					{
						currentShield = 0;
					}
					
					structure.shields[currentShield].debugActivate(sender, args);
				}
				else
				{
					foreach(IShieldDebuggable shield in structure.shields)
					{
						shield.debugActivate(sender, args);
					}
				}
			}
			else if (debugMode == BaseDebugEnum.ENGINES)
			{
				Ship ship = structure as Ship;

				if (ship != null)
				{
					if (args.cursorActive == true) 
					{
						if (currentEngine == -1 && ship.engines.Count > 0)
						{
							currentEngine = 0;
						}
						
						ship.engines[currentEngine].debugActivate(sender, args);
					}
					else
					{
						foreach(ISystemDebuggable engine in ship.engines)
						{
							engine.debugActivate(sender, args);
						}
					}
				}
			}/*
			else if (debugMode == BaseDebugEnum.CLOAKERS)
			{
				ICloakingDevice [] cloakingDevices = structure.getModules<ICloakingDevice>().ToArray();
				
				if (args.cursorActive == true)
				{
					if (currentCloak == -1 && cloakingDevices.Length > 0)
					{
						currentCloak = 0;
					}

					cloakingDevices[currentCloak].debugActivate(sender, args);
				}
				else
				{				
					foreach(ICloakDebuggable cloak in cloakingDevices)
					{
						cloak.debugActivate(sender, args);
					}
				}
			}*/
		}
		
		protected virtual void GameMode_DebugMode_Deactivate(object sender, DebugEventArgs args)		
		{
			Structure structure = Cam.followTarget.GetComponent<Structure>();
			
			if (debugMode == BaseDebugEnum.HULL) structure.debugDeactivate(sender, args);
			else if (debugMode == BaseDebugEnum.DEVICES)
			{
				if (args.cursorActive == true) 
				{
					if (currentDevice == -1 && structure.Devices.Count > 0)
					{
						currentDevice = 0;
					}
					
					structure.Devices[currentDevice].debugDeactivate(sender, args);
				}
				else
				{
					foreach(ISystemDebuggable device in structure.Devices)
					{
						device.debugDeactivate(sender, args);
					}
				}
			}
			else if (debugMode == BaseDebugEnum.MODULES)
			{
				if (args.cursorActive == true) 
				{
					if (currentModule == -1 && structure.Modules.Count > 0)
					{
						currentModule = 0;
					}
					
					structure.Modules[currentModule].debugDeactivate(sender, args);
				}					
				else
				{
					foreach(ISystemDebuggable module in structure.Modules)
					{
						module.debugDeactivate(sender, args);
					}
				}
			}
			else if (debugMode == BaseDebugEnum.WEAPONS)
			{
				if (args.cursorActive == true) 
				{
					if (currentWeapon == -1 && structure.Weapons.Count > 0)
					{
						currentWeapon = 0;
					}
					
					structure.Weapons[currentWeapon].debugDeactivate(sender, args);
				}
				else
				{
					foreach(ISystemDebuggable weapon in structure.Weapons)
					{
						weapon.debugDeactivate(sender, args);
					}
				}
			}
			else if (debugMode == BaseDebugEnum.SHIELDS)
			{
				if (args.cursorActive == true) 
				{
					if (currentShield == -1 && structure.shields.Count > 0)
					{
						currentShield = 0;
					}
					
					structure.shields[currentShield].debugDeactivate(sender, args);
				}
				else
				{
					foreach(IShieldDebuggable shield in structure.shields)
					{
						shield.debugDeactivate(sender, args);
					}
				}
			}
			else if (debugMode == BaseDebugEnum.ENGINES)
			{
				Ship ship = structure as Ship;

				if (ship != null)
				{
					if (args.cursorActive == true) 
					{
						if (currentEngine == -1 && ship.engines.Count > 0)
						{
							currentEngine = 0;
						}
						
						ship.engines[currentEngine].debugDeactivate(sender, args);
					}
					else
					{
						foreach(ISystemDebuggable engine in ship.engines)
						{
							engine.debugDeactivate(sender, args);
						}
					}
				}
			}/*
			else if (debugMode == BaseDebugEnum.CLOAKERS)
			{
				ICloakingDevice[] cloakingDevices = structure.getModules<ICloakingDevice>().ToArray();
				
				if (args.cursorActive == true) 
				{
					if (currentCloak == -1 && cloakingDevices.Length > 0)
					{
						currentCloak = 0;
					}

					cloakingDevices[currentCloak].debugDeactivate(sender, args);
				}
				else
				{				
					foreach(ICloakDebuggable cloak in cloakingDevices)
					{
						cloak.debugDeactivate(sender, args);
					}
				}
			}*/
		}			
		
		protected virtual void GameMode_DebugMode_Damage(object sender, DebugEventArgs args)
		{
			Structure structure = Cam.followTarget.GetComponent<Structure>();
			
			if (structure != null)
			{
				structure.takeDamage(structure.gameObject, changeAmounts[amountIndex], null, null);
			}			
		}
		
		protected virtual void GameMode_DebugMode_SelfDestruct(object sender, DebugEventArgs args)		
		{
			if (debugMode == BaseDebugEnum.SQUADRON)
			{
				Ship ship = Cam.followTarget.GetComponent<Ship>();

				foreach (Ship squadronShip in ship.SquadronData.getAllShips())
                {
					IStructureDebuggable structure = squadronShip as IStructureDebuggable;

					structure.debugSelfDestruct(sender, args);
				}
			}
			else
			{
				IStructureDebuggable structure = Cam.followTarget.GetComponent<Structure>() as IStructureDebuggable;

				structure.debugSelfDestruct(sender, args);
			}
		}
		
		protected virtual void GameMode_DebugMode_Raise(object sender, DebugEventArgs args)		
		{
			Structure structure = Cam.followTarget.GetComponent<Structure>();
			
			if (debugMode == BaseDebugEnum.SHIELDS)
			{
				if (args.cursorActive == true) structure.shields[currentShield].debugRaise(sender, args);
				else
				{
					foreach(IShieldDebuggable shield in structure.shields)
					{
						shield.debugRaise(sender, args);
					}
				}
			}
		}
		
		protected virtual void GameMode_DebugMode_Lower(object sender, DebugEventArgs args)		
		{
			Structure structure = Cam.followTarget.GetComponent<Structure>();
			
			if (debugMode == BaseDebugEnum.SHIELDS)
			{
				if (args.cursorActive == true) structure.shields[currentShield].debugLower(sender, args);
				else
				{
					foreach(IShieldDebuggable shield in structure.shields)
					{
						shield.debugLower(sender, args);
					}
				}
			}			
		}
		
		protected virtual void GameMode_DebugMode_Fail(object sender, DebugEventArgs args)		
		{
			Structure structure = Cam.followTarget.GetComponent<Structure>();			
			
			if (debugMode == BaseDebugEnum.SHIELDS)
			{
				if (args.cursorActive == true) structure.shields[currentShield].debugFail(sender, args);
				else
				{
					foreach(IShieldDebuggable shield in structure.shields)
					{
						shield.debugFail(sender, args);
					}
				}
			}			
		}
		/*
		protected virtual void GameMode_DebugMode_Decloak(object sender, DebugEventArgs args)		
		{
			Structure structure = Cam.followTarget.GetComponent<Structure>();
			
			if (debugMode == BaseDebugEnum.CLOAKERS)
			{
				ICloakingDevice[] cloakingDevices = structure.getModules<ICloakingDevice>().ToArray();
				
				if (args.cursorActive == true) 
				{
					if (currentCloak == -1 && cloakingDevices.Length > 0)
					{
						currentCloak = 0;
					}

					cloakingDevices[currentCloak].debugDecloak(sender, args);
				}
				else
				{
					foreach(ICloakDebuggable cloak in cloakingDevices)
					{
						cloak.debugDecloak(sender, args);
					}
				}
			}		
		}			
		
		protected virtual void GameMode_DebugMode_Cloak(object sender, DebugEventArgs args)		
		{
			Structure structure = Cam.followTarget.GetComponent<Structure>();
			
			if (debugMode == BaseDebugEnum.CLOAKERS)
			{
				ICloakingDevice[] cloakingDevices = structure.getModules<ICloakingDevice>().ToArray();
				
				if (args.cursorActive == true)
				{
					if (currentCloak == -1 && cloakingDevices.Length > 0)
					{
						currentCloak = 0;
					}

					cloakingDevices[currentCloak].debugCloak (sender, args);
				}
				else
				{
					foreach(ICloakDebuggable cloak in cloakingDevices)
					{
						cloak.debugCloak(sender, args);
					}
				}
			}
		}	
		*/
		protected virtual void GameMode_DebugMode_Increment(object sender, DebugEventArgs args)		
		{
			Structure structure = Cam.followTarget.GetComponent<Structure>();
			
			if (debugMode == BaseDebugEnum.ORDNANCES)
			{
				if (args.cursorActive == true)
				{
					if (currentWeapon == -1 && structure.Weapons.Count > 0)
					{
						currentWeapon = 0;
					}
					
					if (structure.Weapons[currentWeapon].destroyed != true)
					{
						structure.Weapons[currentWeapon].debugAmmoIncrement(sender, args);
					}				
				}
				else
				{
					foreach(Weapon weapon in structure.Weapons)
					{
						if (weapon.destroyed != true)
						{
							weapon.debugAmmoIncrement(sender, args);
						}					
					}
				}
			}			
		}
		
		protected virtual void GameMode_DebugMode_Decrement(object sender, DebugEventArgs args)		
		{
			Structure structure = Cam.followTarget.GetComponent<Structure>();
			
			if (debugMode == BaseDebugEnum.ORDNANCES)
			{
				if (args.cursorActive == true)
				{
					if (currentWeapon == -1 && structure.Weapons.Count > 0)
					{
						currentWeapon = 0;
					}
					
					if (structure.Weapons[currentWeapon].destroyed != true)
					{
						structure.Weapons[currentWeapon].debugAmmoDecrement(sender, args);
					}				
				}
				else
				{
					foreach(Weapon weapon in structure.Weapons)
					{
						if (weapon.destroyed != true)
						{
							weapon.debugAmmoDecrement(sender, args);
						}					
					}
				}
			}			
		}
        #endregion

        #endregion
    }
}