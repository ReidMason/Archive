using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

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
	public class CadetBrawlMode : GameMode
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
		Timer matchTimer;
		ArenaBarrier arena;
		
		public int maxTime;

		[Range (0.0f, 1.0f)]
		public float spawnRadiusFraction;

		public List<CombatStats> combateerStats;
		public List<FactionCombatStats> factionStats;
		
		// custom camera variables
		public int currentShip;

		// getters/setters

		#region custom game mode rules
		#region custom game mode camera methods		
		protected void trackPreviousShip()
		{
            List<Ship> shipList = GameManager.Instance.getShips();
			
			if (shipList.Count > 0)
			{
				currentShip--;                  
				
				if (currentShip < 0) currentShip = shipList.Count-1;
				
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
        #endregion

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
            List<Structure> structures = GameManager.Instance.getAllStructures();

            foreach (Structure structure in structures)
            {
                if (structure.Stats != null)
                {
                    structure.InstigatedAnyDamage += Structure_InstigatedAnyDamage;
                    structure.TakenAnyDamage += Structure_TakenAnyDamage;
                    structure.NotifyKilled += Structure_NotifyKiller;
                    structure.NotifyKilled += Structure_NotifyKilled;
                    structure.NotifyAssister += Structure_NotifyAssister;
                    structure.ModuleDamaged += Structure_ModuleDamaged;
                    structure.SurvivalTimeUpdated += Structure_SurvivalTimeUpdated;
                }
            }
			#endregion

			#region subscribe to custom camera mode events
			#endregion

			#region subscribe to custom debug mode events
			#endregion
			
			// set reference to custom game mode GUI
			Gui = GameObject.Find("UI Manager").GetComponent<NoxTimedCombatBrawlGUI>();

            if (Gui == null)
            {
                D.error("GUI: {0}", "Cannot find the custom GUI component under the UI Manager GameObject");
                GameEventManager.Call_AbortedMatch(this);
                return;
            }

            Gui.init();
			Gui.enabled = false;
		}

		public override bool canRespawn(StructureController controller, Structure structure)
		{
			if (controller != null)
			{
                return controller.canRespawn();
			}
			else
			{
				return structure.CanRespawn;
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

		public new Vector3 getSpawnPosition()
		{
			Vector2 randPos = UnityEngine.Random.insideUnitCircle * arena.getRadius() * spawnRadiusFraction;
			
			return new Vector3(randPos.x, 0, randPos.y);
		}
		
		public new Vector3 getSpawnRotation()
		{
			Vector2 randRot = UnityEngine.Random.insideUnitCircle;
			
			float bearing = (Mathf.Atan2(-randRot.y, randRot.x) * Mathf.Rad2Deg) + 90;
			
			if (bearing < 0) bearing += 360;
			
			return new Vector3(0, bearing, 0);
		}	

		#region match reporter
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
				if (ship != null)
				{					
					sw.WriteLine("Ship: " + ship.name);
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
			
			GameObject arenaBoundary = GameObject.Find("Arena Barrier");
			
			if (arenaBoundary == null)
			{
				D.error("GameMode: {0}", "Required ArenaBarrier GameObject not set for this game mode");
				GameEventManager.Call_AbortedMatch(this);	
			}
			else			
			{
				arena = arenaBoundary.GetComponent<ArenaBarrier>();
				
				if (arena == null)
				{
					D.error("GameMode: {0}", "Arena Barrier GameObject does not contain an ArenaBarrier component required for this game mode");
					GameEventManager.Call_AbortedMatch(this);					
				}
				else
				{				
					matchTimer = GameManager.Instance.GetComponent<Timer>();
					matchTimer.maxTime = maxTime;				
				}
			}			
			
			combateerStats = new List<CombatStats>();
			factionStats = new List<FactionCombatStats>();

            // set spawn locations and rotations for each structure
            List<Structure> structures = GameManager.Instance.getStructures();
			
			foreach(Structure structure in structures)
			{
				AIController ai = structure.GetComponent<AIController>();
				
				if (ai != null)
				{
					if (ai.GeneratesStats == true)
					{
						combateerStats.Add (new CombatStats(ai.structure.Stats));
						
						if (factionStats.Exists (s => s.factionID == ai.structure.Faction.ID) == false)
						{
							factionStats.Add (new FactionCombatStats(ai.structure));
						}
						else
						{
							int statsIndex = factionStats.FindIndex(s => s.factionID == ai.structure.Faction.ID);
							
							factionStats[statsIndex].factionStructures.Add(ai.structure);
							factionStats[statsIndex].factionTotalCost += ai.structure.TotalCost;
						}
					}
					
					if (ai.startSpot == null)
					{
						ai.setInitialLocationAndRotation(getSpawnPosition(), Quaternion.Euler(getSpawnRotation()));
					}
					else if (ai.startRotation == null)
					{
						ai.setInitialLocationAndRotation(ai.startSpot.GetValueOrDefault(), Quaternion.Euler(getSpawnRotation()));
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

            // set spawn locations and rotations for each ship
            List<Ship> ships = GameManager.Instance.getShips();
			
			foreach(Ship ship in ships)
			{
				AIController ai = ship.GetComponent<AIController>();
				
				if (ai != null)
				{
					if (ai.GeneratesStats == true)
					{
						combateerStats.Add (new CombatStats(ship.Stats));
						
						if (factionStats.Exists (s => s.factionID == ship.Faction.ID) == false)
						{
							factionStats.Add (new FactionCombatStats(ship));
						}
						else
						{
							int statsIndex = factionStats.FindIndex(s => s.factionID == ship.Faction.ID);
							
							factionStats[statsIndex].factionStructures.Add(ship);
							factionStats[statsIndex].factionTotalCost += ship.TotalCost;
						}						
					}
					
					if (ai.startSpot == null)
					{
						ai.setInitialLocationAndRotation(getSpawnPosition(), Quaternion.Euler(getSpawnRotation()));
					}
					else if (ai.startRotation == null)
					{
						ai.setInitialLocationAndRotation(ai.startSpot.GetValueOrDefault(), Quaternion.Euler(getSpawnRotation()));
					}
					else
					{
                        ai.setInitialLocationAndRotation(ai.startSpot.GetValueOrDefault(), Quaternion.Euler(new Vector3(0, 0, ai.startRotation.GetValueOrDefault())));
					}

                    ship.spawnedIn = true;
                }
				else
				{
					D.warn("GameMode: {0}", ship.Name + " has no AIController");
				}		
			}					
			
			// attempt to start the match
			if (ReadyToStartMatch() == true)
			{
				// initialise the GUI here
				//((NoxTimedCombatBrawlGUI)gui).Init();
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
				if (structure != null && structure.Stats != null)
				{
					structure.startSurvivalClock();
				}
			}
		}	
		
		protected override void GameEventManager_MatchHasEnded(object sender)
		{						
			base.GameEventManager_MatchHasEnded(sender);
			
			matchTimer.stopTimer();
			
			matchTimer.setTimer(matchTimer.maxTime, true);

            List<Structure> structures = GameManager.Instance.getAllStructures();
			
			foreach(Structure structure in structures)
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

		#region custom game mode event handlers
		///////////////////////////////////////////
		/*
			Handlers for all custom game mode events
		*/		
		///////////////////////////////////////////

		public virtual void Structure_InstigatedAnyDamage(object sender, DamageEventArgs args)
		{
			if (args.damageCauser != null)
			{
				int statsIndex = combateerStats.FindIndex(s => s.structure == args.damageCauser);
				
				if (statsIndex >= 0)
				{					
					combateerStats[statsIndex].damageCaused += args.damage;
					
					statsIndex = factionStats.FindIndex(s => s.factionID == args.damageCauser.Faction.ID);
					
					if (statsIndex >= 0)
					{
						factionStats[statsIndex].factionDamageCaused += args.damage;
					}
				}
			}
		}
		
		public virtual void Structure_TakenAnyDamage(object sender, DamageEventArgs args)
		{
			if (args.damagedStructure != null)
			{
				int statsIndex = combateerStats.FindIndex(s => s.structure == args.damagedStructure);
				
				if (statsIndex >= 0)
				{					
					combateerStats[statsIndex].damageTaken += args.damage;
					
					statsIndex = factionStats.FindIndex(s => s.factionID == args.damagedStructure.Faction.ID);
					
					if (statsIndex >= 0)
					{
						factionStats[statsIndex].factionDamageTaken += args.damage;
					}					
				}
			}
		}
		
		public virtual void Structure_NotifyKiller(object sender, TargetDestroyedEventArgs args)
		{
			if (args.attacker != null)
			{
				int statsIndex = combateerStats.FindIndex(s => s.structure == args.attacker);
				
				if (statsIndex >= 0)
				{					
					combateerStats[statsIndex].kills++;
					
					statsIndex = factionStats.FindIndex(s => s.factionID == args.attacker.Faction.ID);
					
					if (statsIndex >= 0)
					{
						factionStats[statsIndex].factionKills++;
					}					
				}
			}
		}	
		
		public virtual void Structure_NotifyAssister(object sender, AssistEventArgs args)
		{
			if (args.assister != null)
			{
				int statsIndex = combateerStats.FindIndex(s => s.structure == args.assister);
				
				if (statsIndex >= 0)
				{					
					combateerStats[statsIndex].assists++;;
					
					statsIndex = factionStats.FindIndex(s => s.factionID == args.assister.Faction.ID);
					
					if (statsIndex >= 0)
					{
						factionStats[statsIndex].factionAssists++;
					}					
				}
			}
		}	
		
		public virtual void Structure_NotifyKilled(object sender, TargetDestroyedEventArgs args)
		{
			if (args.structureAttacked != null)
			{
				int statsIndex = combateerStats.FindIndex(s => s.structure == args.structureAttacked);
				
				if (statsIndex >= 0)
				{
					combateerStats[statsIndex].deaths++;
					
					statsIndex = factionStats.FindIndex(s => s.factionID == args.structureAttacked.Faction.ID);
					
					if (statsIndex >= 0)
					{
						factionStats[statsIndex].factionDeaths++;
					}					
				}
			}
		}
		
		public virtual void Structure_ModuleDamaged(object sender, ModuleDamageEventArgs args)
		{
			if (args.moduleHit != null && args.moduleAttacker != null)
			{
				if (args.destroyed == true)
				{
					int statsIndex = combateerStats.FindIndex(s => s.structure == args.moduleAttacker);
					
					if (statsIndex >= 0)
					{
						combateerStats[statsIndex].modulesDestroyed++;
						
						statsIndex = factionStats.FindIndex(s => s.factionID == args.moduleAttacker.Faction.ID);
						
						if (statsIndex >= 0)
						{
							factionStats[statsIndex].factionModulesDestroyed++;
						}					
					}
				}
			}
		}		
		
		public virtual void Structure_SurvivalTimeUpdated(object sender, SurvivalTimeEventArgs args)
		{
			if (args.updatedStructure != null)
			{
				int statsIndex = combateerStats.FindIndex(s => s.structure == args.updatedStructure);
				
				if (statsIndex >= 0)
				{
					combateerStats[statsIndex].averageSurvivalTime = args.updatedSurvivalTime;
					
					statsIndex = factionStats.FindIndex(s => s.factionID == args.updatedStructure.Faction.ID);
					
					if (statsIndex >= 0)
					{
						factionStats[statsIndex].updateFactionAverageSurvivalTime(args);
					}					
				}
			}
		}
		#endregion				
	}
}
