using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

using NoxCore.Builders;
using NoxCore.Controllers;
using NoxCore.Cameras;
using NoxCore.GUIs;
using NoxCore.Helm;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Weapons;
using NoxCore.Managers;
using NoxCore.Stats;
using NoxCore.Utilities;

namespace NoxCore.GameModes
{
    public class TargetRangeMode : GameMode
    {
        // custom game mode references & variables
        private Timer matchTimer;
        public int maxTime;
        private ArenaBarrier arena;
        protected float maxBoundaryTime;

        GameObject[] targets;

        [ShowOnly]
        public int maxTargets;

        [ShowOnly]
        public int targetsDestroyed;

        [Range(0, 1)]
        public float spawnInSpeedFraction;

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
            if (matchTimer.getTime() >= matchTimer.maxTime || targetsDestroyed == maxTargets)
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
            Gui = GameObject.Find("UI Manager").GetComponent<TimedTargetGUI>();

            if (Gui == null)
            {
                D.error("GUI: {0}", "Cannot find the custom GUI component under the UI Manager GameObject");
                GameEventManager.Call_AbortedMatch(this);
                return;
            }

            Gui.enabled = false;
        }

        public override void Update()
        {
            #region custom key input
            // check for custom key presses
            #endregion

            #region custom camera key input	
            // check for custom game mode camera key presses
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

            Structure structure = null;

            StreamWriter sw = new StreamWriter(Path.Combine(resultsPath, resultsFile + ".txt"));

            sw.WriteLine("Timestamp: " + DateTime.Now.ToString("yyyyMMddHHmmssffff") + "\n");

            List<Ship> ships = GameManager.Instance.getShips();

            structure = ships[0].GetComponent<Ship>();

            if (structure != null)
            {
                sw.WriteLine("Ship: " + structure.gameObject.name);
                sw.WriteLine("Num Targets Destroyed: " + targetsDestroyed);
                sw.WriteLine("Time: " + matchTimer.getTimeStr());
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

            targets = GameObject.FindGameObjectsWithTag("Target");
            maxTargets = targets.Length;

            foreach (GameObject target in targets)
            {
                Structure targetStructure = target.GetComponent<Structure>();
                targetStructure.enabled = true;
                targetStructure.NotifyKilled += TargetRangeMode_TargetDestroyed;
            }

            ArenaBuilder arenaBuilder = FindObjectOfType<ArenaBuilder>();

            if (arenaBuilder == null)
            {
                D.error("Content: {0}", "Required ArenaBarrier GameObject not set for this game mode");
                GameEventManager.Call_AbortedMatch(this);
            }
            else
            {
                arenaBuilder.init();
                maxBoundaryTime = 10.0f;
            }

            // set spawn locations and rotations for each structure
            List<Structure> structures = GameManager.Instance.getAllStructures();

            foreach (Structure structure in structures)
            {
                StructureController controller = structure.GetComponent<StructureController>();

                if (controller != null)
                {
                    if (controller.invulnerableToArenaBoundary == false)
                    {
                        controller.structure.MaxBoundaryTime = maxBoundaryTime;
                    }

                    controller.setInitialLocationAndRotation(controller.startSpot.GetValueOrDefault(), Quaternion.Euler(new Vector3(0, 0, controller.startRotation.GetValueOrDefault())));
                }
                else
                {
                    D.warn("GameMode: {0}", structure.name + " has no Controller");
                }

                structure.spawnedIn = true;
            }

            // attempt to start the match
            if (ReadyToStartMatch() == true)
            {
                // initialise the GUI
                Gui.init();

                // initialise the camera here
                if (Cam == null)
                {
                    Cam = GameObject.Find("Main Camera").GetComponent<TopDown_Camera>();
                    Cam.reset();
                }

                Cam.init();

                setInitialCameraTarget();

                GameEventManager.Call_MatchHasStarted(this);
            }
        }

        protected override void GameEventManager_MatchHasStarted(object sender)
        {
            base.GameEventManager_MatchHasStarted(sender);

            // start all structure and ship GameObjects ticking

            Gui.setMessage("Welcome to The Range cadet!");

            List<Structure> structures = GameManager.Instance.getAllStructures();

            foreach (Structure structure in structures)
            {
                if (structure != null)
                {
                    structure.enabled = true;

                    StructureController controller = structure.GetComponent<StructureController>();

                    if (controller != null)
                    {
                        controller.start();
                    }

                    if (structure.Stats != null)
                    {
                        structure.startSurvivalClock();
                    }

                    structure.StructureCollider.enabled = true;

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

        public void TargetRangeMode_TargetDestroyed(object sender, TargetDestroyedEventArgs args)
        {
            if (args.structureAttacked.tag == "Target")
            {
                // D.log ("Event", args.killedStructure.name + " has been destroyed");
                targetsDestroyed++;
            }
        }
        #endregion
    }
}
