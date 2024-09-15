using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

using NoxCore.Controllers;
using NoxCore.Cameras;
using NoxCore.GameModes;
using NoxCore.GUIs;
using NoxCore.Helm;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Stats;
using NoxCore.Utilities;

namespace Davin.GameModes
{
    public class FleetReviewMode : GameMode
    {
        #region custom game mode events
        ////////////////////////////////////
        /*
			Custom game mode events
		*/
        ////////////////////////////////////

        #region camera mode events
        #endregion

        #region debug mode events
        #endregion
        #endregion

        private BasicGUI gui;

        // custom game mode variables and references
        public List<CombatStats> combateerStats;

        [Range(0, 1)]
        public float spawnInSpeedFraction;

        public int currentShip;

        // custom camera variables

        #region getters/setters	
        #endregion

        #region custom game mode rules
        #region custom game mode camera methods		
        #endregion

        #region collision settings
        [SerializeField]
        private StringLayerMaskDictionary collisionMaskStore = StringLayerMaskDictionary.New<StringLayerMaskDictionary>();
        private Dictionary<string, LayerMask> stringLayerMasks
        {
            get { return collisionMaskStore.dictionary; }
        }
        #endregion

        ///////////////////////////////////////////
        /*
			implemented abstract methods for match state
		*/
        ///////////////////////////////////////////				

        protected override bool ReadyToStartMatch()
        {
            if (Application.isEditor == true)
            {
                return true;
            }
            else if (numAIs > 0)
            {
                return true;
            }

            return false;
        }

        protected override bool ReadyToEndMatch()
        {
            return false;
        }

        ///////////////////////////////////////////
        /*
			other abstract/virtual methods
		*/
        ///////////////////////////////////////////			

        public override LayerMask? getCollisionMask(string collisionMaskName)
        {
            LayerMask storedCollisionMask;

            collisionMaskName = collisionMaskName.ToUpper();

            if (collisionMaskStore.dictionary.ContainsKey(collisionMaskName))
            {
                collisionMaskStore.dictionary.TryGetValue(collisionMaskName, out storedCollisionMask);
                return storedCollisionMask;
            }
            else
            {
                return null;
            }
        }

        public override void init()
        {
            base.init();

            // set collision matrix based on collision layer masks
            Physics2D.SetLayerCollisionMask(LayerMask.NameToLayer("Ship"), getCollisionMask("SHIP").GetValueOrDefault());
            Physics2D.SetLayerCollisionMask(LayerMask.NameToLayer("Structure"), getCollisionMask("STRUCTURE").GetValueOrDefault());
            Physics2D.SetLayerCollisionMask(LayerMask.NameToLayer("Projectile"), getCollisionMask("PROJECTILE").GetValueOrDefault());

            #region subscribe to custom game mode events
            #endregion

            #region subscribe to custom camera mode events
            #endregion

            #region subscribe to custom debug mode events
            #endregion

            // set reference to custom game mode GUI
            Gui = GameObject.Find("UI Manager").GetComponent<BasicGUI>();

            if (Gui == null)
            {
                D.error("GUI: {0}", "Cannot find the custom GUI component under the UI Manager GameObject");
                GameEventManager.Call_AbortedMatch(this);
                return;
            }

            gui = Gui as BasicGUI;

            if (gui == null)
            {
                D.error("GUI", "Cannot cast supplied GUI component to the required BasicGUI");
                return;
            }

            gui.enabled = false;
        }

        public override void initController(Structure structure)
        {
            if (structure != null)
            {
                structure.enabled = true;

                StructureController controller = structure.GetComponent<StructureController>();
                
                if (controller != null)
                {
                    controller.start();
                }

                structure.StructureCollider.enabled = true;

                if (structure != null && structure.Stats != null)
                {
                    structure.startSurvivalClock();
                }

                Ship ship = structure as Ship;

                if (ship != null)
                {
                    ship.Bearing = -ship.transform.rotation.eulerAngles.z;

                    if (ship.Bearing < 0) ship.Bearing += 360;

                    float theta = 90 - ship.Bearing;

                    if (theta < 0) theta += 360;

                    float x = Mathf.Cos(theta * Mathf.Deg2Rad);
                    float y = Mathf.Sin(theta * Mathf.Deg2Rad);

                    ship.Heading = new Vector2(x, y);

                    ship.setSpawnInSpeed(spawnInSpeedFraction);
                }
            }
        }

        public override void positionStructure(Structure structureGO)
        {
            StructureController controller = structureGO.GetComponent<StructureController>();

            if (controller != null)
            {
                if (controller.GeneratesStats == true)
                {
                    if (controller.structure.Stats == null)
                    {
                        D.warn("Structure: {0}", controller.structure.Name + " either has not been constructed or does not have standard stats");
                    }
                    else
                    {
                        combateerStats.Add(new CombatStats(controller.structure.Stats));
                    }
                }

                if (controller.startSpot == null)
                {
                    controller.setInitialLocationAndRotation(getSpawnPosition(), Quaternion.Euler(new Vector3(0, 0, getSpawnRotation())));
                }
                else if (controller.startRotation == null)
                {
                    Vector2 startSpot = controller.startSpot.GetValueOrDefault();

                    controller.setInitialLocationAndRotation(startSpot, Quaternion.Euler(Vector2.zero));
                }
                else
                {
                    controller.setInitialLocationAndRotation(controller.startSpot.GetValueOrDefault(), Quaternion.Euler(new Vector3(0, 0, controller.startRotation.GetValueOrDefault())));
                }
            }
            else
            {
                D.warn("GameMode: {0}", structureGO.name + " has no Controller");
            }
        }

        public override void Update()
        {
            #region custom key input
            // check for custom key presses
            #endregion

            #region custom camera key input	
            // check for custom game mode camera key presses

            if (cameraMode == BaseCameraEnum.TRACK_SELECTED && Input.GetKeyDown(KeyCode.F3))
            {
                trackPreviousShip();
            }
            else if (cameraMode == BaseCameraEnum.TRACK_SELECTED && Input.GetKeyDown(KeyCode.F4))
            {
                trackNextShip();
            }
            #endregion

            #region custom debug key input
            // check for custom game mode debug key inputr
            #endregion

            if (matchState == MatchState.INPROGRESS)
            {
                if (ReadyToEndMatch() == true)
                {
                    GameEventManager.Call_MatchHasEnded(this);
                }
            }

            base.Update();
        }

        ///////////////////////////////////////////
        /*
			Other non-abstract/virtual methods
		*/
        ///////////////////////////////////////////	
        protected void trackPreviousShip()
        {
            List<Ship> shipList = GameManager.Instance.getShips();

            if (shipList.Count > 0)
            {
                currentShip--;

                if (currentShip < 0) currentShip = shipList.Count - 1;

                Cam.setFollowTarget(shipList[currentShip].transform);
            }
        }

        protected void trackNextShip()
        {
            List<Ship> shipList = GameManager.Instance.getShips();

            if (shipList.Count > 0)
            {
                currentShip++;

                if (currentShip == shipList.Count) currentShip = 0;

                Cam.setFollowTarget(shipList[currentShip].transform);
            }
        }

        #region match reporter methods
        protected void generateResults()
        {
            string classnameStr = this.GetType().Name;

            string resultsFile = classnameStr + "_" + DateTime.Now.ToString("yyyyMMddHHmmssffff");
            string resultsPath = Path.Combine(Application.dataPath, "../" + resultsFolder + "/" + classnameStr);

            if (File.Exists(resultsPath))
            {
                // D.warn ("Game-Rules", "A file with the same name as the GameMode's results folder+game mode already exists");
                resultsPath = Path.Combine(Application.dataPath, "../" + resultsFolder + "/" + classnameStr + "_" + Guid.NewGuid());
            }

            if (!Directory.Exists(resultsPath))
            {
                // note: technically this only creates the directory if it does not yet exist so the previous test is not really necessary
                // also: if the name of the folder matches a file then this will throw an exception hence previous check
                Directory.CreateDirectory(resultsPath);
            }

            StreamWriter sw = new StreamWriter(Path.Combine(resultsPath, resultsFile + ".txt"));

            sw.WriteLine("Timestamp: " + DateTime.Now.ToString("yyyyMMddHHmmssffff") + "\n");

            sw.Close();
        }
        #endregion
        #endregion

        #region custom camera mode event dispatchers
        ////////////////////////////////////
        /*
			Event dispatchers for the custom camera modes
		*/
        ////////////////////////////////////
        #endregion

        #region custom debug mode event dispatchers
        ///////////////////////////////////////////
        /*
			Event dispatchers for the custom debug modes
		*/
        ///////////////////////////////////////////	
        #endregion

        #region custom game mode event dispatchers
        ///////////////////////////////////////////
        /*
			Event dispatchers for all custom game mode events
		*/
        ///////////////////////////////////////////
        #endregion

        #region custom camera mode event handlers
        ///////////////////////////////////////////
        /*
			Handlers for all custom camera mode events go here
			Note: these could override those in the parent class
		*/
        ///////////////////////////////////////////	
        #endregion

        #region custom debug mode event handlers
        ///////////////////////////////////////////
        /*
			Handlers for all custom debug mode events go here
			Note: these could override those in the parent class
		*/
        ///////////////////////////////////////////
        #endregion

        #region standard game event handlers
        ///////////////////////////////////////////
        /*
			Overridden handlers for all standard game events
		*/
        ///////////////////////////////////////////

        protected override void GameEventManager_MatchIsWaitingToStart(object sender)
        {
            base.GameEventManager_MatchIsWaitingToStart(sender);

            /*
				should add setup rules from here or can do some/all of it in the mod builder files for individual setup
			*/

            combateerStats = new List<CombatStats>();

            // set spawn locations and rotations for each structure
            List<Structure> structures = GameManager.Instance.getAllStructures();

            foreach (Structure structure in structures)
            {
                positionStructure(structure);

                structure.spawnedIn = true;

                Ship ship = structure as Ship;
            }

            // attempt to start the match
            if (ReadyToStartMatch() == true)
            {
                // initialise the GUI
                if (Gui != null)
                {
                    Gui.init();
                }

                GameEventManager.Call_MatchHasStarted(this);
            }
        }

        protected override void GameEventManager_MatchHasStarted(object sender)
        {
            base.GameEventManager_MatchHasStarted(sender);

            // start all structure and ship GameObjects ticking

            List<Structure> structures = GameManager.Instance.getAllStructures();

            foreach (Structure structure in structures)
            {
                initController(structure);
            }

            // initialise the camera here
            Cam.init();

            setInitialCameraTarget();
        }

        protected override void GameEventManager_MatchHasEnded(object sender)
        {
            base.GameEventManager_MatchHasEnded(sender);

            Time.timeScale = 0;
            Time.fixedDeltaTime = 0;

            GameManager.Instance.setSuspend(true);

            List<Structure> structures = GameManager.Instance.getAllStructures();

            foreach (Structure structure in structures)
            {
                if (structure != null)
                {
                    structure.stopSurvivalClock();

                    if (structure.Stats != null)
                    {
                        structure.Stats.survivalTimes.Add((float)(structure.AliveTimer.Elapsed.TotalSeconds));
                    }

                    structure.calculateAverageSurvivalTime(true);
                }
            }

            if (generateResultsFile == true)
            {
                generateResults();
            }
        }
        #endregion

        #region custom game mode event dispatchers
        ////////////////////////////////////
        /*
			Event dispatchers for the custom game mode
		*/
        ////////////////////////////////////	
        #endregion

        #region custom game mode event handlers
        ///////////////////////////////////////////
        /*
			Handlers for all custom game mode events
		*/
        ///////////////////////////////////////////
        #endregion
    }
}