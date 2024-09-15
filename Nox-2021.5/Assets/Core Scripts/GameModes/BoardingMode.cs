using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

using NoxCore.Controllers;
using NoxCore.Cameras;
using NoxCore.Data;
using NoxCore.Fittings.Sockets;
using NoxCore.GUIs;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Stats;
using NoxCore.Utilities;

using Davin.Controllers;
using Davin.GameModes;

namespace NoxCore.GameModes
{
    public class BoardingMode : GameMode
    {
        public class BoardingEventArgs : EventArgs
        {
            public Structure structure;

            public BoardingEventArgs(Structure structure)
            {
                this.structure = structure;
            }
        }

        #region custom game mode events
        ////////////////////////////////////
        /*
			Custom game mode events
		*/
        ////////////////////////////////////
        public delegate void WarpEventDispatcher(object sender, WarpEventArgs args);
        public static event WarpEventDispatcher AssaultFleetDataShipWarpIn;

        public delegate void BoardingEventDispatcher(object sender, BoardingEventArgs args);
        public static event BoardingEventDispatcher BoardingTargetDocked;
        public static event BoardingEventDispatcher BoardingTargetBoarded;

        #region camera mode events
        public static event CameraEventDispatcher CameraMode_NextStation;
        public static event CameraEventDispatcher CameraMode_PrevStation;
        #endregion

        #region debug mode events
        #endregion
        #endregion

        private TimedBoardingGUI gui;

        // custom game mode variables and references
        Timer matchTimer;

        public Structure boardingTarget;
        public Ship boardingShip;

        public int maxTime;

        public List<CombatStats> combateerStats;

        public List<WarpInInfo> warpInInfos = new List<WarpInInfo>();

        public List<Ship> inactiveShips = new List<Ship>();

        public float boardingTime;
        protected float boardingTimer;
        protected bool markedTargetDocked;
        protected bool markedTargetBoarded;
        protected float targetDockedTime;
        protected float targetBoardedTime;

        [Range(0, 1)]
        public float spawnInSpeedFraction;

        protected bool firstWarpIn = true;

        // custom camera variables
        public int currentShip, currentStructure;

        #region getters/setters	
        public float getBoardingTimer()
        {
            return boardingTimer;
        }
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
            if (matchTimer.getTime() >= matchTimer.maxTime || markedTargetBoarded == true || (boardingShip != null && boardingShip.Destroyed == true))
            {
                return true;
            }

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
            AssaultFleetDataShipWarpIn += BoardingMode_WarpIn;
            #endregion

            #region subscribe to custom camera mode events
            CameraMode_NextStation += BoardingMode_CameraMode_NextStation;
            CameraMode_PrevStation += BoardingMode_CameraMode_PrevStation;
            #endregion

            #region subscribe to custom debug mode events
            #endregion

            boardingTarget = FindObjectOfType<Station>();

            boardingTarget.DockReceiverDocked += BoardingMode_BoardingTargetDocked;
            BoardingMode.BoardingTargetBoarded += BoardingMode_BoardingTargetBoarded;

            // set reference to custom game mode GUI
            Gui = GameObject.Find("UI Manager").GetComponent<TimedBoardingGUI>();

            if (Gui == null)
            {
                D.error("GUI: {0}", "Cannot find the custom GUI component under the UI Manager GameObject");
                GameEventManager.Call_AbortedMatch(this);
                return;
            }

            gui = Gui as TimedBoardingGUI;

            if (gui == null)
            {
                D.error("GUI", "Cannot cast supplied GUI component to the required TimedBoardingGUI");
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

                if (controller != null) controller.start();

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
                if (markedTargetDocked == true)
                {
                    boardingTimer += Time.deltaTime;

                    if (boardingTimer >= boardingTime)
                    {
                        boardingTimer = boardingTime;

                        Call_BoardingTargetBoarded(this, new BoardingEventArgs(boardingTarget));
                    }
                }

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

        public Ship getBoardingShip()
        {
            return boardingShip;
        }

        public Structure getBoardingTarget()
        {
            return boardingTarget;
        }

        protected void InitiateAssaultFleetDataWarpInSequence()
        {
            inactiveShips = GameObject.Find("Placeables").FindInactive<Ship>();

            foreach (WarpInInfo warpInInfo in warpInInfos)
            {
                StartCoroutine(warpInShips(warpInInfo));
            }
        }

        IEnumerator warpInShips(WarpInInfo warpInInfo)
        {
            List<Ship> shipsToWarpIn = new List<Ship>();

            foreach (Ship ship in inactiveShips)
            {
                // find all ships that match the warpInInfo
                if (ship.Faction == warpInInfo.faction && ship.FleetData == warpInInfo.fleet && ship.WingData == warpInInfo.wing && ship.SquadronData == warpInInfo.squadron)
                {
                    shipsToWarpIn.Add(ship);
                }
            }

            // delay until warp in time
            yield return new WaitForSeconds(warpInInfo.warpInTime);

            // spawn ships
            foreach (Ship shipInWarp in shipsToWarpIn)
            {
                Call_AssaultFleetShipWarpIn(this, new WarpEventArgs(shipInWarp.gameObject, gameObject.scene.path, null, shipInWarp.transform.position, shipInWarp.transform.rotation.eulerAngles.z));
            }

//            gui.displayNames();
 //           gui.displayFactions();
//            gui.setHealthBarMode(true);
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

            if (boardingShip.Destroyed == true)
            {
                sw.WriteLine("Boarding ship destroyed at " + Timer.formatTimer(matchTimer.getTime(), true));
            }
            else if (markedTargetDocked == true)
            {
                sw.WriteLine("Target docked with at " + Timer.formatTimer(targetDockedTime, true));

                if (markedTargetBoarded == true)
                {
                    sw.WriteLine("Target boarded at " + Timer.formatTimer(targetBoardedTime, true));
                }
            }
            else
            {
                sw.WriteLine("Mission failed at " + Timer.formatTimer(matchTimer.getTime(), true));
            }

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
        public static void Call_AssaultFleetShipWarpIn(object sender, WarpEventArgs args)
        {
            if (AssaultFleetDataShipWarpIn != null)
            {
                AssaultFleetDataShipWarpIn(sender, args);
            }
        }
        #endregion

        #region custom camera mode event handlers
        ///////////////////////////////////////////
        /*
			Handlers for all custom camera mode events go here
			Note: these could override those in the parent class
		*/
        ///////////////////////////////////////////	
        protected virtual void BoardingMode_CameraMode_PrevStation(object sender, CameraEventArgs args)
        {
            // D.log ("Event", "Camera mode set to FOLLOW PREV STATION");	
            setCameraMode(SquadvsSquadCameraEnum.FOLLOW_PREV_STATION);

            List<Structure> structures = GameManager.Instance.getStructures();

            if (structures.Count > 0)
            {
                currentStructure--;

                if (currentStructure < 0) currentStructure = structures.Count - 1;

                Cam.setFollowTarget(structures[currentStructure].transform);

                Gui.setMessage("Following " + Cam.followTarget.name);
            }
        }

        protected virtual void BoardingMode_CameraMode_NextStation(object sender, CameraEventArgs args)
        {
            // D.log ("Event", "Camera mode set to FOLLOW NEXT STATION");	
            setCameraMode(SquadvsSquadCameraEnum.FOLLOW_NEXT_STATION);

            List<Structure> structures = GameManager.Instance.getStructures();

            if (structures.Count > 0)
            {
                currentStructure++;

                if (currentStructure == structures.Count) currentStructure = 0;

                Cam.setFollowTarget(structures[currentStructure].transform);

                Gui.setMessage("Following " + Cam.followTarget.name);
            }
        }
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

            if (boardingShip != null)
            {
                Cam.setFollowTarget(boardingShip.transform);
            }

            matchTimer = GameManager.Instance.GetComponent<Timer>();
            matchTimer.maxTime = maxTime;

            if (matchTimer != null)
            {
                matchTimer.maxTime = maxTime;
            }
            else
            {
                D.warn("Content: {0}", "No Timer component attached to the Game Manager");
            }

            combateerStats = new List<CombatStats>();

            // set spawn locations and rotations for each structure
            List<Structure> structures = GameManager.Instance.getAllStructures();

            foreach (Structure structure in structures)
            {
                positionStructure(structure);

                structure.spawnedIn = true;
            }

            TimedBoardingGUI boardingGUI = Gui as TimedBoardingGUI;

            if (boardingGUI != null)
            {
                boardingGUI.setBoardingTarget(boardingTarget);
            }

            // attempt to start the match
            if (ReadyToStartMatch() == true)
            {
                InitiateAssaultFleetDataWarpInSequence();

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

            matchTimer.startTimer();

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

            matchTimer.stopTimer();

            if (matchTimer.getTime() > matchTimer.maxTime)
            {
                matchTimer.setTimer(matchTimer.maxTime, true);
            }

            if (markedTargetBoarded == false)
            {
                Gui.setMessage("Command to " + boardingShip.Command.rankData.abbreviation + " " + boardingShip.Command.label + ", you have failed the mission. Better luck next time.");
            }
            else
            {
                Gui.setMessage("Command to " + boardingShip.Command.rankData.abbreviation + " " + boardingShip.Command.label + ", you have completed the mission. Well done!");
            }

            IPlayMusic music = boardingShip.Controller as IPlayMusic;

            if (music != null)
            {
                music.stopMusic();
            }

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

        public static void Call_BoardingTargetDocked(object sender, BoardingEventArgs args)
        {
            if (BoardingTargetDocked != null)
            {
                BoardingTargetDocked(sender, args);
            }
        }

        public static void Call_BoardingTargetBoarded(object sender, BoardingEventArgs args)
        {
            if (BoardingTargetBoarded != null)
            {
                BoardingTargetBoarded(sender, args);
            }
        }
        #endregion

        #region custom game mode event handlers
        ///////////////////////////////////////////
        /*
			Handlers for all custom game mode events
		*/
        ///////////////////////////////////////////

        public virtual void BoardingMode_WarpIn(object sender, WarpEventArgs args)
        {
            if (args.warpShipGO != null)
            {
                if (firstWarpIn == true)
                {
                    firstWarpIn = false;
                    Cam.transform.position = new Vector3(-3500, 0, Cam.transform.position.z);
                }

                Ship ship = args.warpShipGO.GetComponent<Ship>();

                if (ship != null)
                {
                    ship.HasWarpedIn += BoardingMode_HasWarpedIn;

                    ship.Call_WarpIn(this, args);
                }
            }
        }

        public void BoardingMode_HasWarpedIn(object sender, WarpEventArgs args)
        {
            if (args.warpShipGO != null)
            {
                Ship warpShip = args.warpShipGO.GetComponent<Ship>();

                if (warpShip != null)
                {
                    warpShip.HasWarpedIn -= BoardingMode_HasWarpedIn;

                    if (args.warpShipGO.name == "Delta Cuckoo")
                    {
                        boardingShip = args.warpShipGO.GetComponent<Structure>() as Ship;

                        Gui.setMessage("Head to the target co-ordinates " + boardingShip.Command.rankData.abbreviation + " " + boardingShip.Command.label);

                        boardingShip.DockInitiatorDocked += BoardingMode_BoardingShipDocked;
                        boardingShip.DockInitiatorUndocked += BoardingMode_BoardingShipUndocked;

                        GameObject station = GameObject.Find("Vespiary MaxSec");

                        if (station != null)
                        {
                            Structure structure = station.GetComponent<Structure>();

                            if (structure != null)
                            {
                                VespiaryMaxSecAI controller = structure.Controller as VespiaryMaxSecAI;

                                if (controller != null)
                                {
                                    controller.setBoardingShip(args.warpShip);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void BoardingMode_BoardingTargetDocked(object sender, DockingPortEventArgs args)
        {
            if (args.portStructure == boardingTarget)
            {
                // D.log ("Event", markedTarget.name + " has been docked at");
                Gui.setMessage(boardingShip.Command.rankData.abbreviation + " " + boardingShip.Command.label + " to Command, the station has been docked with and is being boarded");

                targetDockedTime = matchTimer.getTime();

                markedTargetDocked = true;
            }
        }

        public void BoardingMode_BoardingTargetBoarded(object sender, BoardingEventArgs args)
        {
            if (args.structure == boardingTarget)
            {
                // D.log ("Event", markedTarget.name + " has been boarded");
                Gui.setMessage(boardingShip.Command.rankData.abbreviation + " " + boardingShip.Command.label + " to Command, the " + boardingTarget.Name + " has been boarded and the prison is now back under control");

                targetBoardedTime = matchTimer.getTime();

                markedTargetBoarded = true;
            }
        }

        public void BoardingMode_BoardingShipDocked(object sender, DockingPortEventArgs args)
        {
            if (args.ship == boardingShip && args.portStructure == boardingTarget)
            {
                Gui.setMessage(boardingShip.Command.rankData.abbreviation + " " + boardingShip.Command.label + " to Command, the " + boardingTarget.Name + " is being boarding ");

                Call_BoardingTargetDocked(sender, new BoardingEventArgs(args.portStructure));

                markedTargetDocked = true;
            }
        }

        public void BoardingMode_BoardingShipUndocked(object sender, DockingPortEventArgs args)
        {
            if (args.portStructure == boardingTarget)
            {
                markedTargetDocked = false;
                boardingTimer = 0;

                boardingShip = null;
            }
        }
        #endregion
    }
}