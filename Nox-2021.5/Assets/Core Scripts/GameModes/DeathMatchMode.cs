using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

using NoxCore.Controllers;
using NoxCore.Cameras;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Weapons;
using NoxCore.GUIs;
using NoxCore.Helm;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Utilities;

namespace NoxCore.GameModes
{
	public class DeathMatchEventArgs : GameEventArgs
	{
		public Structure structureAttacked;
		public Module moduleAttacked;
		public Structure attacker;
		
		public DeathMatchEventArgs(Structure structureAttacked, Module moduleAttacked, Structure attacker)
		{
			this.structureAttacked = structureAttacked;
			this.moduleAttacked = moduleAttacked;
			this.attacker = attacker;
		}
	}	
	
	public class DeathMatchMode : GameMode
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

        // custom game mode references & variables
        private Timer matchTimer;
        public int maxTime;
        private ArenaBarrier arena;

        [Range(0.0f, 1.0f)]
        public float spawnRadiusFraction;

        // custom camera variables
        public int currentShip;

        // getters/setters	

        #region custom game mode rules
        #region custom game mode camera methods		
        #endregion

        ////////////////////////////////////
        /*
			Custom game mode events
		*/
        ////////////////////////////////////

        public delegate void DeathMatchEventDispatcher(object sender, DeathMatchEventArgs args);
		public static event DeathMatchEventDispatcher TargetDestroyed;

        #region collision settings
        public LayerMask shipCollisionMask;
        public LayerMask structureCollisionMask;
        public LayerMask projectileCollisionMask;
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
			if (matchTimer.getTime() >= matchTimer.maxTime)
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
            collisionMaskName = collisionMaskName.ToUpper();

            switch (collisionMaskName)
            {
                case "SHIP": return shipCollisionMask;
                case "STRUCTURE": return structureCollisionMask;
                case "PROJECTILE": return projectileCollisionMask;
                default: return null;
            }
        }

        public override void init()
        {
            base.init();

            // set collision matrix based on collision layer masks
            Physics2D.SetLayerCollisionMask(LayerMask.NameToLayer("Ship"), shipCollisionMask);
            Physics2D.SetLayerCollisionMask(LayerMask.NameToLayer("Structure"), structureCollisionMask);
            Physics2D.SetLayerCollisionMask(LayerMask.NameToLayer("Projectile"), projectileCollisionMask);

            #region subscribe to custom game mode events
            #endregion

            #region subscribe to custom camera mode events
            #endregion

            #region subscribe to custom debug mode events
            #endregion

            // set reference to custom game mode GUI
            Gui = GameObject.Find("UI Manager").GetComponent<DeathMatchGUI>();

            if (Gui == null)
            {
                D.error("GUI: {0}", "Cannot find the custom GUI component under the UI Manager GameObject");
                GameEventManager.Call_AbortedMatch(this);
                return;
            }

            Gui.init();
            Gui.enabled = false;
        }

		public override Vector2 getSpawnPosition()
		{
			Vector2 randPos = UnityEngine.Random.insideUnitCircle * arena.getRadius() * spawnRadiusFraction;
			
			return new Vector2(randPos.x, randPos.y);
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
				resultsPath = Path.Combine(Application.dataPath, "../" + resultsFolder  + "/" + classnameStr + "_" + Guid.NewGuid());
			}
			
			if (!Directory.Exists(resultsPath))
			{
				// note: technically this only creates the directory if it does not yet exist so the previous test is not really necessary
				// also: if the name of the folder matches a file then this will throw an exception hence previous check
				Directory.CreateDirectory(resultsPath);
			}
			
			StreamWriter sw = new StreamWriter(Path.Combine(resultsPath, resultsFile + ".txt"));
			
			sw.WriteLine("Timestamp: " + DateTime.Now.ToString("yyyyMMddHHmmssffff") + "\n");			
			
			foreach(Ship ship in GameManager.Instance.getShips())
			{
				sw.WriteLine("Ship: " + ship.Name);
				sw.WriteLine("Commanded By: " + ship.Command.rankData.abbreviation + " " + ship.Command.label);
				sw.WriteLine("Faction: " + ship.Faction.label);

				if (ship.Stats != null)
				{
					sw.WriteLine("Average Survival Time: " + Timer.formatTimer(ship.Stats.averageSurvivalTime, true));
					sw.WriteLine("Kills: " + ship.Stats.numKills, false);
					sw.WriteLine("Assists: " + ship.Stats.numAssists);
					sw.WriteLine("Deaths: " + ship.Stats.numDeaths);
					sw.WriteLine("Modules Destroyed: " + ship.Stats.numModulesDestroyed);
					sw.WriteLine("Damage Caused: " + ship.Stats.totalDamageInflicted);
					sw.WriteLine("Damage Taken: " + ship.Stats.totalDamageTaken);
					sw.WriteLine("Hull Damage: " + ship.Stats.totalHullDamageTaken);
					sw.WriteLine("Shield Damage: " + ship.Stats.totalShieldDamageTaken);
					sw.WriteLine("Module Damage: " + ship.Stats.totalArmourDamageTaken);
				}

				sw.WriteLine("\n\n");
			}   
			
			sw.Close();			
		}
        #endregion
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

			GameObject arenaBoundary = GameObject.Find("Arena Barrier");
			
			if (arenaBoundary == null)
			{
				D.error("Content: {0}", "Required ArenaBarrier GameObject not set for this game mode");
				GameEventManager.Call_AbortedMatch(this);	
			}
			else			
			{
				arena = arenaBoundary.GetComponent<ArenaBarrier>();
				
				if (arena == null)
				{
					D.error("Content: {0}", "Arena Barrier GameObject does not contain an ArenaBarrier component required for this game mode");
					GameEventManager.Call_AbortedMatch(this);
                    return;			
				}
				else
				{				
					matchTimer = GameManager.Instance.GetComponent<Timer>();
					matchTimer.maxTime = maxTime;				
				}
			}

            // set spawn locations and rotations for each structure
            List<Structure> structures = GameManager.Instance.getAllStructures();
			
			foreach(Structure structure in structures)
			{
				AIController ai = structure.GetComponent<AIController>();
				
				if (ai != null)
				{
					if (ai.startSpot == null)
					{
						ai.setInitialLocationAndRotation(getSpawnPosition(), Quaternion.Euler(new Vector3(0, 0, getSpawnRotation())));
					}
					else if (ai.startRotation == null)
					{
						ai.setInitialLocationAndRotation(ai.startSpot.GetValueOrDefault(), Quaternion.Euler(new Vector3(0, 0, getSpawnRotation())));
					}
					else
					{
						ai.setInitialLocationAndRotation(ai.startSpot.GetValueOrDefault(), Quaternion.Euler(new Vector3(0, 0, ai.startRotation.GetValueOrDefault())));
					}

                    structure.spawnedIn = true;
                }
				else
				{
					D.warn("GameMode: {0}", structure.Name + " has no AIController");
				}		
			}		
					
			// attempt to start the match
			if (ReadyToStartMatch() == true)
			{
				// initialise the GUI here
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
			
			matchTimer.startTimer();

            List<Structure> structures = GameManager.Instance.getAllStructures();
			
			foreach(Structure structure in structures)
			{
				structure.startSurvivalClock();
			}							
		}
		
		protected override void GameEventManager_MatchHasEnded(object sender)
		{						
			base.GameEventManager_MatchHasEnded(sender);

            Gui.setMessage("Match ended!");

            // stop the match timer and set its value ot the maximum (because it might be slightly over)
            matchTimer.stopTimer();
			matchTimer.setTimer(matchTimer.maxTime, true);

            Time.timeScale = 0;
            Time.fixedDeltaTime = 0;

            GameManager.Instance.setSuspend(true);

            // stop the survivial clocks
            List<Structure> structures = GameManager.Instance.getAllStructures();
			
			foreach(Structure structure in structures)
			{
				structure.stopSurvivalClock();
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

            generateResults();
		}
        #endregion

        ////////////////////////////////////
        /*
			Event dispatchers for the custom game mode
		*/
        ////////////////////////////////////	

        public static void Call_TargetDestroyed(object sender, DeathMatchEventArgs args)
		{			
			if (TargetDestroyed != null)
			{
				TargetDestroyed(sender, args);
			}
		}

        #region custom game mode event handlers
        ///////////////////////////////////////////
        /*
			Handlers for all custom game mode events
		*/
        ///////////////////////////////////////////
        #endregion
    }
}
