using System.Collections.Generic;
using System;
using System.IO;

using UnityEngine;

using NoxCore.Cameras;
using NoxCore.Controllers;
using NoxCore.Data;
using NoxCore.Fittings.Modules;
using NoxCore.GUIs;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Utilities;

namespace NoxCore.GameModes
{
    public class KobayashiMaruMode : GameMode
    {
        public class KobayashiMaruEventArgs : GameEventArgs
        {
            public Structure structureAttacked;
            public Module moduleAttacked;
            public Structure attacker;

            public KobayashiMaruEventArgs(Structure structureAttacked, Module moduleAttacked, Structure attacker)
            {
                this.structureAttacked = structureAttacked;
                this.moduleAttacked = moduleAttacked;
                this.attacker = attacker;
            }
        }

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

        // custom game mode references & variables
        private Timer matchTimer;
        public int maxTime;
        private ArenaBarrier arena;

        [Range(0.0f, 1.0f)]
        public float spawnRadiusFraction;

        [Range(0, 1)]
        public float spawnInSpeedFraction;

        //private int waveNum;
        public string enemyFactionName;
        public Ship trappedShip;

        // custom camera variables
        public int currentShip;

        // getters/setters	

        #region custom game mode rules
        #region custom game mode camera methods		
        #endregion

        [SerializeField]
        private StringLayerMaskDictionary collisionMaskStore = StringLayerMaskDictionary.New<StringLayerMaskDictionary>();
        private Dictionary<string, LayerMask> stringLayerMasks
        {
            get { return collisionMaskStore.dictionary; }
        }

        ////////////////////////////////////
        /*
			Custom game mode events
		*/
        ////////////////////////////////////

        //public delegate void TeamDeathMatchEventDispatcher(object sender, TeamDeathMatchEventArgs args);
        //public static event TeamDeathMatchEventDispatcher TargetDestroyed;

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
            if (trappedShip.Destroyed == true)
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
            #endregion

            #region subscribe to custom camera mode events
            #endregion

            #region subscribe to custom debug mode events
            #endregion

            // set reference to custom game mode GUI
            Gui = GameObject.Find("UI Manager").GetComponent<KobayashiMaruGUI>();

            if (Gui == null)
            {
                D.error("GUI: {0}", "Cannot find the custom GUI component under the UI Manager GameObject");
                GameEventManager.Call_AbortedMatch(this);
                return;
            }

            Gui.enabled = false;
        }

        public override Vector2 getSpawnPosition()
        {
            Vector2 randPos = UnityEngine.Random.insideUnitCircle * arena.getRadius() * spawnRadiusFraction;

            return new Vector2(randPos.x, randPos.y);
        }

        public override float getSpawnRotation()
        {
            Vector2 randRot = UnityEngine.Random.insideUnitCircle;

            float bearing = (Mathf.Atan2(-randRot.y, randRot.x) * Mathf.Rad2Deg) + 90;

            if (bearing < 0) bearing += 360;

            return bearing;
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

            //Structure structure = null;

            StreamWriter sw = new StreamWriter(Path.Combine(resultsPath, resultsFile + ".txt"));

            sw.WriteLine("Timestamp: " + DateTime.Now.ToString("yyyyMMddHHmmssffff") + "\n");

            sw.WriteLine("Ship: " + trappedShip.gameObject.name);
            sw.WriteLine("Survival Time: " + matchTimer.getTimeStr());

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

            //waveNum = 1;

            GameObject arenaBoundary = GameObject.Find("Arena Barrier");

            if (arenaBoundary == null)
            {
                D.error("Game-Logic", "Required ArenaBarrier GameObject not set for this game mode");
                GameEventManager.Call_AbortedMatch(this);
            }
            else
            {
                arena = arenaBoundary.GetComponent<ArenaBarrier>();

                if (arena == null)
                {
                    D.error("Game-Logic", "Arena Barrier GameObject does not contain an ArenaBarrier component required for this game mode");
                    GameEventManager.Call_AbortedMatch(this);
                    return;
                }
                else
                {
                    matchTimer = GameManager.Instance.GetComponent<Timer>();
                }
            }

            FactionData enemyFaction = FactionManager.Instance.findFaction(enemyFactionName);

            if (enemyFaction == null)
            {
                D.error("Game-Logic", "Could not find the enemy faction associated with the enemy faction name entered into the game mode (" + enemyFactionName + ")");
                GameEventManager.Call_AbortedMatch(this);
                return;
            }

            // set spawn locations and rotations for each structure
            List<Structure> structures = GameManager.Instance.getAllStructures();

            foreach (Structure structure in structures)
            {
                StructureController controller = structure.GetComponent<StructureController>();

                if (controller != null)
                {
                    controller.setInitialLocationAndRotation(controller.startSpot.GetValueOrDefault(), Quaternion.Euler(new Vector3(0, 0, controller.startRotation.GetValueOrDefault())));

                    structure.spawnedIn = true;
                }
                else
                {
                    D.warn("GameMode: {0}", structure.Name + " has no Controller");
                }
            }

            // attempt to start the match
            if (ReadyToStartMatch() == true)
            {
                // initialise the GUI
                Gui.init();

                // initialise the camera here
                Cam.init();

                setInitialCameraTarget();

                GameEventManager.Call_MatchHasStarted(this);
            }
        }

        protected override void GameEventManager_MatchHasStarted(object sender)
        {
            base.GameEventManager_MatchHasStarted(sender);

            // start all structure and ship GameObjects ticking

            Gui.setMessage("Imperative! This is the Kobayashi Maru, ...nineteen periods out of Altair Six.", Color.cyan);
            Gui.setMessage("We have struck a gravitic mine and have lost all power.", Color.cyan);
            Gui.setMessage("...Our hull is penetrated and we have sustained many casualties.", Color.cyan);
            Gui.setMessage("This is the " + trappedShip.name + ". Your message is breaking up. Can you give your coordinates? Repeat. This is the...");

            List<Structure> structures = GameManager.Instance.getAllStructures();

            foreach (Structure structure in structures)
            {
                structure.enabled = true;

                StructureController controller = structure.Controller;
                //controller.StartCoroutine(controller.update());
                controller.start();

                if (structure != null && structure.Stats != null)
                {
                    structure.startSurvivalClock();
                }

                Ship ship = structure as Ship;

                if (ship != null)
                {
                    ship.StructureCollider.enabled = true;

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

            matchTimer.startTimer();
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

            Gui.setMessage("Test ended!");

            List<Structure> structures = GameManager.Instance.getAllStructures();

            foreach (Structure structure in structures)
            {
                if (structure != null)
                {
                    structure.stopSurvivalClock();

                    structure.enabled = false;

                    StructureController controller = structure.Controller;
                    //StopCoroutine(controller.update());
                    controller.stop();

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

        public void KobayashiMaruMode_TargetDestroyed(object sender, KobayashiMaruEventArgs args)
        {
        }
        #endregion
    }
}