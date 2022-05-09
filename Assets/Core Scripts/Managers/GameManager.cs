using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System;
using System.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

using NoxCore.Cameras;
using NoxCore.GameModes;
using NoxCore.Data;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Utilities;

namespace NoxCore.Managers
{
	public class GameManager : MonoBehaviour
	{
		protected GameMode _Gamemode;
		public GameMode Gamemode { get { return _Gamemode; } set { _Gamemode = value; } }

		protected TopDown_Camera _mainCamera;
		public TopDown_Camera MainCamera 
		{
			get
			{
				if (_mainCamera != null) return _mainCamera;
				else
				{
					MainCamera = FindObjectOfType<TopDown_Camera>();
					MainCamera.reset();

					return _mainCamera;
				}
			}
			set { _mainCamera = value; }
		}

		public float deltaTime;
		protected bool suspend;

		protected Collection<int> sortLayerCounter;
		public Collection<int> SortLayerCounter { get { return sortLayerCounter; } set { sortLayerCounter = value; } }

		protected List<Ship> shipList = new List<Ship>();
		protected List<Structure> structureList = new List<Structure>();
		protected List<Structure> allStructuresList = new List<Structure>();

		protected Transform placeablesParent;
		[SerializeField] public Transform PlaceablesParent { get; set; }

		protected Transform effectsParent;
		[SerializeField] public Transform EffectsParent { get; set; }

		protected Transform projectilesParent;
		[SerializeField] public Transform ProjectilesParent { get; set; }

		/// <summary>
		///   Provide singleton support for this class.
		///   The script must still be attached to a game object, but this will allow it to be called
		///   from anywhere without specifically identifying that game object.
		/// </summary>
		private static GameManager instance;		
		public static GameManager Instance {  get { return instance; } set { instance = value; } }

		public bool campaignMode;
		protected bool factionsInitialised;

		// Use this for initialization
		public void Start()
		{
			if (Instance != null) return;

			Debug.Log("GameManager - Start");

			Instance = this;

			SortLayerCounter = new Collection<int>();

			for (int i = 0; i < Enum.GetNames(typeof(StructureSize)).Length; i++)
			{
				SortLayerCounter.Add(0);
			}

			if (campaignMode == false)
			{
				initScene(SceneManager.GetActiveScene());
			}
		}

		protected void initFactions()
        {
			// blank all FactionData structure lists (TODO: check editor-only or does this happen in the build too? Don't believe it does)
			foreach (FactionData faction in FactionManager.Instance.Factions)
			{
				faction.Stations.Clear();
				faction.FriendlyStructures.Clear();
				faction.NeutralStructures.Clear();
				faction.EnemyStructures.Clear();
			}

			factionsInitialised = true;
		}

		public void initScene(Scene activeScene)
        {
            Debug.Log("Game manager reset called");

			IEnumerable<Transform> allTransforms = DataStructureUtilityMethods.GetAllTransformsInScene(activeScene);

			GameObject placeablesGO = activeScene.Find("Placeables", allTransforms);

			if (placeablesGO != null) PlaceablesParent = placeablesGO.transform;

			GameObject effectsGO = activeScene.Find("Effects", allTransforms);

			if (effectsGO != null) EffectsParent = effectsGO.transform;

			GameObject projectilesGO = activeScene.Find("Projectiles", allTransforms);

			if (projectilesGO != null) ProjectilesParent = projectilesGO.transform;

			GameObject camGO = activeScene.Find("Main Camera", allTransforms);

			if (camGO != null) MainCamera = camGO.GetComponent<TopDown_Camera>();

			#region game mode initialisation

			GameObject gameModeGO = null;

			if (campaignMode == true)
			{
				gameModeGO = activeScene.Find("Game Mode", allTransforms);
			}
			else
            {
				// for non-campaigns, the GameManager holds the GameMode component
				gameModeGO = gameObject;
			}

			if (gameModeGO != null)	Gamemode = gameModeGO.GetComponent<GameMode>();

			if (Gamemode == null)
			{
				D.error("Content: {0}", "No custom GameMode class attached to the GameManager");

				// call the AbortedMatch event dispatcher on the GameEventManager
				GameEventManager.Call_AbortedMatch(this);
				return;
			}

			Debug.Log("Game Manager has a reference to the level's game mode");

			// call the EnteringScene event dispatcher on the GameEventManager
			GameEventManager.Call_EnteringScene(this);

            #endregion

            #region pre-placed structure initialisation

            D.log("Content", "Initialising all pre-placed structures...");

			Debug.Log("Initialising all pre-placed structures...");

			allStructuresList.Clear();
			shipList.Clear();
			structureList.Clear();

			List<Structure> structures = activeScene.FindObjectsOfType<Structure>(allTransforms);

			List<Structure> inactiveStructures = PlaceablesParent.gameObject.FindInactive<Structure>();

			foreach (Structure structure in structures)
			{
				if (structure.SystemsInitiated == true) continue;

				Debug.Log("Initialising structure: " + structure.name);

				// initialise structure				
				structure.init();

				if (!inactiveStructures.Contains(structure))
				{
					Debug.Log("Spawning structure: " + structure.name);

					// call spawn
					structure.spawn();

					if (structure.Controller != null)
					{
						Debug.Log("Booting controller for " + structure.name);

						Ship ship = structure as Ship;

						if (ship != null)
						{
							ship.Controller.boot(structure, ship.Helm);
						}
						else
						{
							structure.Controller.boot(structure);
						}
					}
				}
				else
				{
					Ship ship = structure as Ship;

					if (ship != null)
                    {
						ship.OnEnable();
                    }
					else
                    {
						structure.OnEnable();
                    }

					structure.gameObject.SetActive(false);

					if (campaignMode == false)
					{
						structure.Faction.EnemyStructures.Clear();
						structure.Faction.FriendlyStructures.Clear();
						structure.Faction.NeutralStructures.Clear();
						structure.Faction.Stations.Clear();
					}
				}
			}

			if (factionsInitialised == false)
            {
				initFactions();
            }

			// post-initialisation setup for faction info
			foreach (Structure structure in structures)
			{
				addStructure(structure);
			}
			#endregion

			D.log("Content", "Number of active ships in system: " + shipList.Count);
			D.log("Content", "Number of active stations in system: " + structureList.Count);
			D.log("Content", "Total number of active structures in system: " + allStructuresList.Count);
			D.log("Content", "Number of inactive structures in systems: " + inactiveStructures.Count);

			D.log("Content", "Initialising game mode: " + Gamemode.GetType().Name);

			Debug.Log("GameManager - ready to initialise game mode");

			Gamemode.init();

			if (Gamemode.abort == false)
			{
				Debug.Log("Game manager - calling MatchIsWaitingToStart");
				GameEventManager.Call_MatchIsWaitingToStart(this);
			}
			else
			{
				D.log("Match was aborted.");
			}

		}

		public TopDown_Camera getMainCamera()
		{
			return Camera.main.GetComponent<TopDown_Camera>();
		}	

        public List<Ship> getShips()
        {
            return shipList;
        }

        public List<Structure> getStructures()
        {
            return structureList;
        }

        public List<Structure> getAllStructures()
        {
            return allStructuresList;
        }

        public void addStructure(Structure structure)
        {
			bool structureAdded = false;

			if (structure != null)
			{
				Ship ship = structure as Ship;

				if (ship != null)
				{
					// add ship to ship list if not inactive in scene or warping in at some point
					if (ship.shipState != ShipState.UNKNOWN && !shipList.Contains(ship))
					{
						shipList.Add(ship);
						structureAdded = true;
					}
				}
				else
				{
					if (!structureList.Contains(structure))
					{
						structureList.Add(structure);
						structureAdded = true;

						if (structure.Faction != null)
						{
							// add to faction's own list of stations?
							FactionData faction = FactionManager.Instance.findFaction(structure.Faction.ID);

							Station station = structure as Station;

							if (faction != null && station != null)
							{
								faction.Stations.Add(structure);
							}
						}
					}
				}

				if (!allStructuresList.Contains(structure) && structureAdded == true)
				{
					allStructuresList.Add(structure);

					// add structure to faction's info
					foreach (FactionData faction in FactionManager.Instance.Factions)
					{
						if (structure.Faction == null) continue;

						if (faction.ID == structure.Faction.ID)
						{
							// add to friendlies
							faction.FriendlyStructures.Add(structure);
						}
						else if (structure.Faction.ID <= 0)
						{
							// add to neutrals
							faction.NeutralStructures.Add(structure);
						}
						else
						{
							// add to enemies
							faction.EnemyStructures.Add(structure);
						}
					}
				}
			}
        }

        public bool removeStructure(Structure structure)
        {
			bool removed = false;

			if (structure != null)
			{
				Ship ship = structure as Ship;

				if (ship != null)
				{
					removed = shipList.Remove(ship);
				}
				else
				{
					removed = structureList.Remove(structure);

					// remove from faction's own list of station
					FactionData faction = FactionManager.Instance.findFaction(structure.Faction.ID);

					if (faction != null)
					{
						faction.Stations.Remove(structure);
					}
				}

				allStructuresList.Remove(structure);
	
				// remove structure from each faction's info
				foreach (FactionData faction in FactionManager.Instance.Factions)
				{
					if (faction.ID == structure.Faction.ID)
					{
						// remove from friendlies
						faction.FriendlyStructures.Remove(structure);
					}
					else if (structure.Faction.ID <= 0)
					{
						// remove from neutrals
						faction.NeutralStructures.Remove(structure);
					}
					else
					{
						// remove from enemies
						faction.EnemyStructures.Remove(structure);
					}
				}

			}

			return removed;
		}
		
		public void refreshStructureLists()
        {
			List<Structure> allStructures = GameManager.Instance.getAllStructures();

			for (int i = allStructures.Count - 1; i >= 0; i--)
			{
				if (allStructures[i] == null) allStructuresList.RemoveAt(i);
			}

			List<Structure> structures = GameManager.Instance.getStructures();

			for (int i = structures.Count - 1; i >= 0; i--)
			{
				if (structures[i] == null) structureList.RemoveAt(i);
			}

			List<Ship> ships = GameManager.Instance.getShips();

			for (int i = ships.Count - 1; i >= 0; i--)
			{
				if (ships[i] == null) shipList.RemoveAt(i);
			}
		}

		public bool getSuspended()
		{
			return suspend;
		}
		
		public void setSuspend(bool suspend)
		{
			this.suspend = suspend;

            if (suspend == false)
            {
                Time.timeScale = 0;
                Time.fixedDeltaTime = 0;

                D.log("GameLogic", "All game updates suspended");
            }
            else
            {

                D.log("GameLogic", "All game updates resumed");
            }
		}
		
		public string getNamespaceFromFile(System.IO.FileInfo file)
		{
			string line, nameSpace;
			
			// process a make file if present
			try
			{
				using(StreamReader reader = new StreamReader(file.FullName))
				{
					// read file
					while ((line = reader.ReadLine()) != null)
					{
						if (line.Contains("namespace"))
						{
							nameSpace = line.TrimStart();
							nameSpace = nameSpace.Substring(10).TrimEnd();
							return nameSpace;
						}
					}
				}
			}
			catch (IOException e)
			{
				D.log("Exception", e.Message);
			}
			
			return null;
		}

		// Update is called once per frame
		void Update () 
		{	
			deltaTime = Time.deltaTime;
		}
	}
}