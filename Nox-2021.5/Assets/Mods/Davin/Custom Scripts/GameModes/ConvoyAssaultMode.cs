using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

using NoxCore.Builders;
using NoxCore.Cameras;
using NoxCore.Controllers;
using NoxCore.Data;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Weapons;
using NoxCore.GameModes;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Managers;
using NoxCore.Stats;
using NoxCore.Utilities;

using Davin.GUIs;

namespace Davin.GameModes
{
    [Serializable]
    public struct WarpInInfo
    {
        public float warpInTime;
        public FactionData faction;
        public FleetData fleet;
        public WingData wing;
        public SquadronData squadron;
    }

    public class ConvoyAssaultMode : GameMode
    {
        #region custom game mode events
        ////////////////////////////////////
        /*
			Custom game mode events
		*/
        ////////////////////////////////////

        public delegate void WarpEventDispatcher(object sender, WarpEventArgs args);
        public static event WarpEventDispatcher ConvoyShipWarpIn;
        public static event WarpEventDispatcher ConvoyShipWarpOut;        

        #region camera mode events
        #endregion

        #region debug mode events
        #endregion
        #endregion

        private ConvoyAssaultGUI gui;

        // custom game mode variables and references
        private Timer matchTimer;
        public int maxTime;

        [Range(0, 1)]
        public float spawnInSpeedFraction;

        public List<CombatStats> combateerStats;

        public List<WarpInInfo> warpInInfos = new List<WarpInInfo>();

        public List<Ship> inactiveShips = new List<Ship>();

        protected int maxNumTransporters;
        protected int numTransportersDisabled;
        protected int numTransportersDestroyed;
        protected int numTransportersWarpedOut;

        // custom camera variables
        public int currentShip;

        // getters/setters	

        #region custom game mode rules
        [SerializeField]
        private StringLayerMaskDictionary collisionMaskStore = StringLayerMaskDictionary.New<StringLayerMaskDictionary>();
        private Dictionary<string, LayerMask> stringLayerMasks
        {
            get { return collisionMaskStore.dictionary; }
        }

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
            else if (numAIs + numPlayers > 0)
            {
                return true;
            }

            return false;
        }

        protected override bool ReadyToEndMatch()
        {
            if (numTransportersDestroyed + numTransportersDisabled + numTransportersWarpedOut == maxNumTransporters)
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
            ConvoyShipWarpIn += ConvoyAssaultMode_WarpIn;
            #endregion

            #region subscribe to custom camera mode events
            if (cameraMode == BaseCameraEnum.TRACK_SELECTED && Input.GetKeyDown(KeyCode.F3))
            {
                trackPreviousShip();
            }
            else if (cameraMode == BaseCameraEnum.TRACK_SELECTED && Input.GetKeyDown(KeyCode.F4))
            {
                trackNextShip();
            }
            #endregion

            #region subscribe to custom debug mode events
            #endregion

            // set reference to custom game mode GUI
            Gui = GameObject.Find("UI Manager").GetComponent<ConvoyAssaultGUI>();

            if (Gui == null)
            {
                D.error("GUI: {0}", "Cannot find the custom GUI component under the UI Manager GameObject");
                GameEventManager.Call_AbortedMatch(this);
                return;
            }

            gui = Gui as ConvoyAssaultGUI;

            if (gui == null)
            {
                D.error("GUI", "Cannot cast supplied GUI component to the required ConvoyAssaultGUI");
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

                if (controller == true)
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
            // check for custom game mode debug key input
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

            gui.setHealthBarMode(true);
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

            gui.setHealthBarMode(true);
        }

        protected void InitiateConvoySequence()
        {
            inactiveShips = GameObject.Find("Placeables").FindInactive<Ship>();

            foreach (WarpInInfo warpInInfo in warpInInfos)
            {
                // find all ships that match the warpInInfo
                foreach(Ship ship in inactiveShips)
                {
                    if (ship.Faction == warpInInfo.faction && ship.FleetData == warpInInfo.fleet && ship.WingData == warpInInfo.wing && ship.SquadronData == warpInInfo.squadron)
                    {
                        maxNumTransporters++;
                    }
                }
            }

            gui.setConvoyTotal(maxNumTransporters);
            gui.setConvoyRemaining(maxNumTransporters);

            foreach (WarpInInfo warpInInfo in warpInInfos)
            {
                StartCoroutine(warpInShips(warpInInfo));
            }
        }

        IEnumerator warpInShips(WarpInInfo warpInInfo)
        {
            List<Ship> shipsToWarpIn = new List<Ship>();

            foreach(Ship ship in inactiveShips)
            {
                // find all ships that match the warpInInfo
                if (ship.Faction == warpInInfo.faction && ship.FleetData == warpInInfo.fleet && ship.WingData == warpInInfo.wing && ship.SquadronData == warpInInfo.squadron)
                {
                    shipsToWarpIn.Add(ship);
                }
            }

            // delay until warp in time
            yield return new WaitForSeconds(warpInInfo.warpInTime);

            // spawn convoy group
            foreach (Ship shipInWarp in shipsToWarpIn)
            {
                Call_ConvoyShipWarpIn(this, new WarpEventArgs(shipInWarp.gameObject, gameObject.scene.path, null, shipInWarp.transform.position, shipInWarp.transform.rotation.eulerAngles.z));

                // setup custom event handlers for convoy ship destroyed and module destroyed
                if (shipInWarp != null)
                {
                    shipInWarp.CanRespawn = false;
                    shipInWarp.ModuleDamaged += ConvoyAssaultMode_ModuleDamaged;
                    shipInWarp.NotifyKilled += ConvoyAssaultMode_NotifyKilled;
                }
            }

            gui.displayNames();
            gui.displayFactions();
            gui.setHealthBarMode(true);
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

            sw.WriteLine("Num Transporters Disabled: " + numTransportersDisabled);
            sw.WriteLine("Num Transporters Destroyed: " + numTransportersDestroyed);
            sw.WriteLine("Num Transporters Warped Out: " + numTransportersWarpedOut);

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
        public static void Call_ConvoyShipWarpIn(object sender, WarpEventArgs args)
        {
            if (ConvoyShipWarpIn != null)
            {
                ConvoyShipWarpIn(sender, args);
            }
        }

        public static void Call_ConvoyShipWarpOut(object sender, WarpEventArgs args)
        {
            if (ConvoyShipWarpOut != null)
            {
                ConvoyShipWarpOut(sender, args);
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

            matchTimer = GameManager.Instance.GetComponent<Timer>();

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
                if (structure != null)
                {
                    positionStructure(structure);
                    structure.spawnedIn = true;
                }
            }            

            // attempt to start the match
            if (ReadyToStartMatch() == true)
            {
                InitiateConvoySequence();

                // initialise the GUI here
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

            matchTimer.startTimer();
        }

        protected override void GameEventManager_MatchHasEnded(object sender)
        {
            base.GameEventManager_MatchHasEnded(sender);

            Gui.setMessage("Match ended!");

            Time.timeScale = 0;
            Time.fixedDeltaTime = 0;

            GameManager.Instance.setSuspend(true);

            // stop the survivial clocks
            List<Structure> structures = GameManager.Instance.getAllStructures();

            foreach (Structure structure in structures)
            {
                if (structure != null)
                {
                    structure.stopSurvivalClock();

                    if (structure.Stats != null)
                    {
                        if (structure.AliveTimer.Elapsed.TotalSeconds > matchTimer.maxTime)
                        {
                            structure.Call_SurvivalTimeUpdated(this, new SurvivalTimeEventArgs(structure, (float)matchTimer.maxTime, true));
                        }
                        else
                        {
                            structure.Call_SurvivalTimeUpdated(this, new SurvivalTimeEventArgs(structure, (float)(structure.AliveTimer.Elapsed.TotalSeconds), true));
                        }
                    }

                    structure.calculateAverageSurvivalTime(true);
                }
            }

            // remove all projectiles (note: could just disable them?)
            GameObject[] projectiles = GameObject.FindGameObjectsWithTag("Projectile");

            // remove projectiles
            foreach (GameObject projectile in projectiles)
            {
                Projectile proj = projectile.GetComponent<Projectile>();

                if (proj != null)
                {
                    proj.remove();
                }
            }

            if (generateResultsFile == true)
            {
                generateResults();
            }
        }
        #endregion

        #region custom game mode event handlers
        ///////////////////////////////////////////
        /*
			Handlers for all custom game mode events
		*/
        ///////////////////////////////////////////
        public  virtual void ConvoyAssaultMode_WarpIn(object sender, WarpEventArgs args)
        {
            if (args.warpShipGO != null)
            {
                Ship ship = args.warpShipGO.GetComponent<Ship>();

                if (ship != null)
                {
                    ship.HasWarpedIn += ConvoyAssaultMode_HasWarpedIn;

                    ship.Call_WarpIn(this, args);
                }
            }
        }

        public void ConvoyAssaultMode_HasWarpedIn(object sender, WarpEventArgs args)
        {
            if (args.warpShipGO != null)
            {
                Ship warpShip = args.warpShipGO.GetComponent<Ship>();

                if (warpShip != null)
                {
                    warpShip.HasWarpedIn -= ConvoyAssaultMode_HasWarpedIn;
                    warpShip.HasWarpedOut += ConvoyAssaultMode_HasWarpedOut;
                }
            }
        }

        public void ConvoyAssaultMode_HasWarpedOut(object sender, WarpEventArgs args)
        {
            if (args.warpShipGO != null)
            {
                Ship warpShip = args.warpShipGO.GetComponent<Ship>();

                if (warpShip != null)
                {
                    warpShip.HasWarpedOut -= ConvoyAssaultMode_HasWarpedOut;

                    numTransportersWarpedOut++;
                    gui.setConvoyWarpedOut(numTransportersWarpedOut);
                    gui.setConvoyRemaining(maxNumTransporters - numTransportersDestroyed - numTransportersDisabled - numTransportersWarpedOut);

                    // recheck the follow camera
                    if (cameraMode == BaseCameraEnum.TRACK_SELECTED && Cam.followTarget == warpShip)
                    {
                        Cam.setFollowTarget(null);
                    }
                }
            }
        }

        public virtual void ConvoyAssaultMode_ModuleDamaged(object sender, ModuleDamageEventArgs args)
        {
            if (args.moduleHit != null && args.moduleOwner != null)
            {
                if (args.destroyed == true)
                {
                    Ship ship = args.moduleOwner as Ship;

                    if (ship != null && ship.Classification == ShipClassification.TRANSPORTER)
                    {
                        IEngine engineDestroyed = args.moduleHit as IEngine;

                        if (engineDestroyed != null)
                        {
                            if (ship.AreEnginesDisabled == false) return;

                            ship.CanBeDamaged = false;

                            numTransportersDisabled++;
                            gui.setConvoyRemaining(maxNumTransporters - numTransportersDestroyed - numTransportersDisabled - numTransportersWarpedOut);
                            gui.setConvoyDisabled(numTransportersDisabled);
                        }
                    }
                }
            }
        }

        public virtual void ConvoyAssaultMode_NotifyKilled(object sender, TargetDestroyedEventArgs args)
        {
            if (args.structureAttacked != null)
            {
                Ship ship = args.structureAttacked as Ship;

                if (ship != null && ship.Classification == ShipClassification.TRANSPORTER)
                {                    
                    if (ship.AreEnginesDisabled == true)
                    {
                        numTransportersDisabled--;
                        gui.setConvoyDisabled(numTransportersDisabled);
                    }

                    numTransportersDestroyed++;
                    gui.setConvoyRemaining(maxNumTransporters - numTransportersDestroyed - numTransportersDisabled - numTransportersWarpedOut);
                    gui.setConvoyDestroyed(numTransportersDestroyed);
                }
            }
        }

        #endregion
    }
}
