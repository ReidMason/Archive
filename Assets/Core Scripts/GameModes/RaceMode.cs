using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;

using NoxCore.Controllers;
using NoxCore.Cameras;
using NoxCore.GUIs;
using NoxCore.Helm;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Stats;
using NoxCore.Utilities;
using Fungus;

namespace NoxCore.GameModes
{
	public class RaceMode : GameMode 
	{
		public class RaceModeEventArgs : EventArgs
		{
			public Structure structure;
			public float raceTime;
			
			public RaceModeEventArgs(Structure structure, float raceTime)
			{
				this.structure = structure;
				this.raceTime = raceTime;
			}
		}
		
		// custom game mode references
		protected Timer raceTimer;
		protected Text countdownText;
		protected AudioSource audioSource;

		// custom game mode variables
		protected static int numRacers;
		protected static int numRacersFinished;
		protected List<(GameObject navPoint, int id)> raceGatesAndIDs;

		[Range (0, 1)]
        public float spawnInSpeedFraction;
		public int maxLaps;
		public int maxTime;
        public int initalCountdownFontSize;
        public int countdownFontShrinkRate;
        public AudioClip[] countdownClips = new AudioClip[4];

        protected Transform startPoint;

        protected int currentShip;
		protected GameObject currentLeadRacer;
		protected GameObject currentLastRacer;
		
		////////////////////////////////////
		/*
			Custom game mode events
		*/
		////////////////////////////////////		
		
		public delegate void RaceModeEventDispatcher(object sender, RaceModeEventArgs args);
		public static event RaceModeEventDispatcher RacerFinished;
		public static event RaceModeEventDispatcher NewLeadRacer;
		public static event RaceModeEventDispatcher NewLastRacer;

        public delegate void RaceModeInputEventDispatcher(object sender, InputEventArgs args);
        public static event RaceModeInputEventDispatcher TogglePositionIDs;
        public static event RaceModeInputEventDispatcher ToggleRacerIDs;

        public static event CameraEventDispatcher CameraMode_PreviousRacer;
		public static event CameraEventDispatcher CameraMode_NextRacer;
		public static event CameraEventDispatcher CameraMode_LeadRacer;
		public static event CameraEventDispatcher CameraMode_LastRacer;

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
			else if (numAIs + numPlayers > 0)
			{
				return true;
			}			
			
			return false;
		}
		
		protected override bool ReadyToEndMatch()
		{
			if (numRacersFinished == numRacers || raceTimer.getTime() >= raceTimer.maxTime)
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
		
		public override bool canRespawn(StructureController controller, Structure structure)
		{
			return false;
		}

        ///////////////////////////////////////////
        /*
			non-collision rules for custom game mode go here
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

            audioSource = Cam.GetComponent<AudioSource>();

            #region subscribe to custom game mode events
            RacerFinished += RaceMode_RacerFinished;
			NewLeadRacer += RaceMode_NewLeadRacer;
			NewLastRacer += RaceMode_NewLastRacer;

            TogglePositionIDs += RaceMode_TogglePositionIDs;
            ToggleRacerIDs += RaceMode_ToggleRacerIDs;
            #endregion

            #region subscribe to custom camera mode events
            CameraMode_PreviousRacer += RaceMode_CameraMode_PreviousRacer;
			CameraMode_NextRacer += RaceMode_CameraMode_NextRacer;
			CameraMode_LeadRacer += RaceMode_CameraMode_LeadRacer;
			CameraMode_LastRacer += RaceMode_CameraMode_LastRacer;
            #endregion

            #region subscribe to custom debug mode events
            #endregion

            // set reference to custom game mode GUI
            Gui = GameObject.Find("UI Manager").GetComponent<NoxRaceGUI>();

            if (Gui == null)
            {
                D.error("GUI: {0}", "Cannot find the custom GUI component under the UI Manager GameObject");
                GameEventManager.Call_AbortedMatch(this);
                return;
            }

            Gui.enabled = false;

            raceTimer = GameManager.Instance.GetComponent<Timer>();	
			raceTimer.maxTime = maxTime;
			
			countdownText = GameObject.Find("Countdown").GetComponent<Text>();

           raceGatesAndIDs = new List<(GameObject, int)>();

            GameObject [] raceGateGOs = GameObject.FindGameObjectsWithTag("NavPoint");

            foreach (GameObject raceGateGO in raceGateGOs)
            {
                RaceGate raceGate = raceGateGO.GetComponent<RaceGate>();

                raceGate.init();

                int number = raceGate.GateID;

                (GameObject navPoint, int id) raceGateAndID = (raceGateGO, raceGate.GateID);
                raceGatesAndIDs.Add(raceGateAndID);
            }

            raceGatesAndIDs.Sort((n1, n2) => n1.id.CompareTo(n2.id));

            startPoint = raceGatesAndIDs[0].navPoint.transform;
		}

        public Timer getTimer()
        {
            return raceTimer;
        }
		
		public override Vector2 getSpawnPosition()
		{
			return startPoint.position;
		}
		
		public override float getSpawnRotation()
		{
            float gateRotation = -startPoint.rotation.eulerAngles.z;

            if (gateRotation < 0) gateRotation += 360;

			return -gateRotation;
		}		
		
		///////////////////////////////////////////
		/*
			other methods
		*/		
		///////////////////////////////////////////
		
		public static int getNumRacers()
		{
			return numRacers;
		}
		
		protected void generateResults()
		{	
			List<FinishStats> endRacers = ((NoxRaceGUI)Gui).getEndRacers();
			
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
			
			Structure structure = null;

			using (StreamWriter sw = new StreamWriter(Path.Combine(resultsPath, resultsFile + ".txt")))
			{
				sw.WriteLine("Timestamp: " + DateTime.Now.ToString("yyyyMMddHHmmssffff") + "\n");

				int racePosition = 0;
				int numEqualRaceTimes = 0;
				string lastRaceTime = null;

				foreach (FinishStats racer in endRacers)
				{
					if (lastRaceTime != racer.raceTime)
					{
						racePosition += (numEqualRaceTimes + 1);
						numEqualRaceTimes = 0;
					}
					else
					{
						numEqualRaceTimes++;
					}

					structure = racer.racerTransform.gameObject.GetComponent<Structure>();
					sw.WriteLine("Ship: " + structure.name);
					sw.WriteLine("Commanded By: " + structure.Command.rankData.abbreviation + " " + structure.Command.label);
					sw.WriteLine("Faction: " + structure.Faction.label);
					sw.WriteLine("Race Position: " + racePosition);
					sw.WriteLine("Race Time: " + racer.raceTime);
					sw.WriteLine("\n\n");

					lastRaceTime = racer.raceTime;
				}

				List<RaceStats> curRacers = ((NoxRaceGUI)Gui).getCurrentRacers();

				int numEqualRemainingDistances = 0;
				float lastRemainingDistance = 0;

				foreach (RaceStats racer in curRacers)
				{
					if (lastRemainingDistance != racer.distanceRemaining)
					{
						racePosition += (numEqualRemainingDistances + 1);
						numEqualRemainingDistances = 0;
					}
					else
					{
						numEqualRemainingDistances++;
					}

					structure = racer.racerTransform.gameObject.GetComponent<Structure>();
					sw.WriteLine("Ship: " + structure.name);
					sw.WriteLine("Commanded By: " + structure.Command.rankData.abbreviation + " " + structure.Command.label);
					sw.WriteLine("Faction: " + structure.Faction.label);
					sw.WriteLine("Race Position: " + racePosition);
					sw.WriteLine("Remaining Distance: " + racer.distanceRemaining);
					sw.WriteLine("\n\n");

					lastRemainingDistance = racer.distanceRemaining;
				}

				sw.Close();
			}		
		}				
		
		public IEnumerator raceCountdown()
		{
			while(true)
			{
                yield return new WaitForSeconds(5);
                countdownText.fontSize = initalCountdownFontSize;
                countdownText.text = "3";
                if (countdownClips[0] != null) audioSource.PlayOneShot(countdownClips[0]);
				yield return new WaitForSeconds(1);
				countdownText.fontSize = initalCountdownFontSize;
				countdownText.text = "2";
                if (countdownClips[1] != null) audioSource.PlayOneShot(countdownClips[1]);
                yield return new WaitForSeconds(1);
				countdownText.fontSize = initalCountdownFontSize;
				countdownText.text = "1";
                if (countdownClips[2] != null) audioSource.PlayOneShot(countdownClips[2]);
                yield return new WaitForSeconds(1);
				countdownText.fontSize = initalCountdownFontSize;
				countdownText.text = "GO!";
                if (countdownClips[3] != null) audioSource.PlayOneShot(countdownClips[3]);
                GameEventManager.Call_MatchHasStarted(this);
				yield return new WaitForSeconds(1);
				countdownText.fontSize = initalCountdownFontSize;
				countdownText.text = "";
				break;
			}
        }
		
		new void Update()
		{
			base.Update();

            if (Input.GetKeyUp(KeyCode.L) == true)
            {
                Call_TogglePositionIDs(this, new InputEventArgs());
            }

            if (Input.GetKeyUp(KeyCode.I) == true)
            {
                Call_ToggleRacerIDs(this, new InputEventArgs());
            }

            if (Input.GetKeyDown(KeyCode.F3)) 
			{
				Call_CameraMode_PreviousRacer(this, null);
			}  
			
			if (Input.GetKeyDown(KeyCode.F4)) 
			{
				Call_CameraMode_NextRacer(this, null);
			}  
			
			if (Input.GetKeyDown(KeyCode.F5)) 
			{
				Call_CameraMode_LeadRacer(this, null);
			}  
			
			if (Input.GetKeyDown(KeyCode.F6)) 
			{
				Call_CameraMode_LastRacer(this, null);
			}

			if(Input.GetKeyDown(KeyCode.F9))
            {
				GameEventManager.Call_MatchHasEnded(this);
			}

            if (matchState == MatchState.WAITINGTOSTART)
			{
				countdownText.fontSize -= countdownFontShrinkRate;
			}
			
			if (matchState == MatchState.INPROGRESS && raceTimer.getTime() > 3)
			{
				// check for new leader/last racer and call events if any changes
				List<RaceStats> currentRacers = ((NoxRaceGUI)Gui).getCurrentRacers();
				
				if (currentRacers.Count > 0) 
				{
					if (currentLeadRacer != currentRacers[0].racerTransform.gameObject)
					{
						Call_NewLeadRacer(this, new RaceModeEventArgs(currentRacers[0].racerTransform.GetComponent<Ship>(), currentRacers[0].distanceRemaining));
					}
					
					if (currentLastRacer != currentRacers[currentRacers.Count-1].racerTransform.gameObject)
					{
						Call_NewLastRacer(this, new RaceModeEventArgs(currentRacers[currentRacers.Count-1].racerTransform.GetComponent<Ship>(), currentRacers[currentRacers.Count-1].distanceRemaining));
					}
				}
			}
		}
		
		///////////////////////////////////////////
		/*
			Overridden handlers for all standard game mode events
		*/		
		///////////////////////////////////////////
		
		protected override void GameEventManager_MatchIsWaitingToStart(object sender)
		{			
			// don't call base method here as you normally would (used for delaying start for countdown)
			// D.log ("Event", "Match is waiting to start");
			matchState = MatchState.WAITINGTOSTART;

			/*
				should add setup rules from here or can do some/all of it in the mod builder files for individual setup
			*/

			// get all ships (note: should all be racers)
			List<Ship> ships = GameManager.Instance.getShips();

			// set the number of racers
			RaceMode.numRacers = ships.Count;
			
			// set spawn locations and rotations for each structure			
			foreach(Ship ship in ships)
			{
				AIController ai = ship.Controller as AIController;
				
				if (ai != null)
				{
					ship.transform.position = getSpawnPosition();
					ship.transform.rotation = Quaternion.Euler(new Vector3(0, 0, getSpawnRotation()));

					ai.setInitialLocationAndRotation(ship.transform.position, Quaternion.Euler(new Vector3(0, 0, getSpawnRotation())));
				}
				else
				{
					D.warn("GameMode: {0}", ship.Name + " has no AIController");
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

                StartCoroutine(raceCountdown());
			}
		}
		
		protected override void GameEventManager_MatchHasStarted(object sender)
		{
			base.GameEventManager_MatchHasStarted(sender);
			
			// enable all structure
			
			List<Structure> structures = GameManager.Instance.getAllStructures();
			
			foreach(Structure structure in structures)
			{
				if (structure != null)
				{
					structure.enabled = true;

					// start ship controllers
					StructureController controller = structure.Controller;

					if (controller == true)
					{
						controller.start(UnityEngine.Random.Range(0, aiInitStartDelayRange));
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

            // start the race timer
            raceTimer.startTimer();	
		}	
		
		protected override void GameEventManager_MatchHasEnded(object sender)
		{
			// don't call parent here as you would normally so the racers can perform lazy turns
			// D.log ("Event", "Race has ended");
			matchState = MatchState.WAITINGPOSTMATCH;
			
			raceTimer.stopTimer();	
			
			if (generateResultsFile == true)
			{
				generateResults();
			}
		}
		
		////////////////////////////////////
		/*
			Event dispatchers for the custom game mode
		*/
		////////////////////////////////////		
		
		public static void Call_RacerFinished(object sender, RaceModeEventArgs args)
		{			
			if (RacerFinished != null)
			{
				RacerFinished(sender, args);
			}
		}		
		
		public static void Call_NewLeadRacer(object sender, RaceModeEventArgs args)
		{			
			if (NewLeadRacer != null)
			{
				NewLeadRacer(sender, args);
			}
		}		
		
		public static void Call_NewLastRacer(object sender, RaceModeEventArgs args)
		{			
			if (NewLastRacer != null)
			{
                NewLastRacer(sender, args);
			}
		}

        public static void Call_TogglePositionIDs(object sender, InputEventArgs args)
        {
            if (TogglePositionIDs != null)
            {
                TogglePositionIDs(sender, args);
            }
        }

        public static void Call_ToggleRacerIDs(object sender, InputEventArgs args)
        {
            if (ToggleRacerIDs != null)
            {
                ToggleRacerIDs(sender, args);
            }
        }

        public static void Call_CameraMode_PreviousRacer(object sender, CameraEventArgs args)
		{			
			if (CameraMode_PreviousRacer != null)
			{
				CameraMode_PreviousRacer(sender, args);
			}
		}	
		
		public static void Call_CameraMode_NextRacer(object sender, CameraEventArgs args)
		{			
			if (CameraMode_NextRacer != null)
			{
				CameraMode_NextRacer(sender, args);
			}
		}	
		
		public static void Call_CameraMode_LeadRacer(object sender, CameraEventArgs args)
		{			
			if (CameraMode_LeadRacer != null)
			{
				CameraMode_LeadRacer(sender, args);
			}
		}	
		
		public static void Call_CameraMode_LastRacer(object sender, CameraEventArgs args)
		{			
			if (CameraMode_LastRacer != null)
			{
				CameraMode_LastRacer(sender, args);
			}
		}	
		
        ///////////////////////////////////////////
        /*
			Handlers for the custom game mode events
		*/
        ///////////////////////////////////////////

        protected void RaceMode_RacerFinished(object sender, RaceModeEventArgs args)
		{
			numRacersFinished++;
            // D.log ("Event", "(caller:" + sender.ToString() + ") Racer " + args.structure.name + " finished in a time of: " + Timer.formatTimer(args.raceTime, true))

            string raceTimeStr = Timer.formatTimer(args.raceTime, true);

            NoxRaceGUI raceGUI = Gui as NoxRaceGUI;

            if (raceGUI != null)
            {
                raceGUI.racerFinished(args.structure.transform, raceTimeStr);
                //raceGUI.setMessage("Congratulations " + args.structure.name + "! You reached the finish line in : " + raceTimeStr);
            }
        }

        protected void RaceMode_NewLeadRacer(object sender, RaceModeEventArgs args)
        {
            // D.log ("Event", "(caller:" + sender.ToString() + ") Racer " + args.structure.name + " is the new lead racer!");

            currentLeadRacer = args.structure.gameObject;

            if (cameraMode == RaceCameraEnum.TRACK_RACE_LEADER)
            {
                Cam.setFollowTarget(currentLeadRacer.transform);
            }

            if (numRacersFinished == 0)
            {
                Gui.setMessage("Racer " + args.structure.name + " is the new lead racer!");
            }
		}	
		
		protected void RaceMode_NewLastRacer(object sender, RaceModeEventArgs args)
		{
			// D.log ("Event", "(caller:" + sender.ToString() + ") Racer " + args.structure.name + " is lagging behind!");
			
			currentLastRacer = args.structure.gameObject;
			
			if (cameraMode == RaceCameraEnum.TRACK_LAST_RACER)
			{
				Cam.setFollowTarget(currentLastRacer.transform);
			}
			
			Gui.setMessage("Racer " + args.structure.name + " is lagging behind!");
		}	
		
		protected virtual void RaceMode_CameraMode_PreviousRacer(object sender, CameraEventArgs args)
		{
			// D.log ("Event", "Camera mode set to FOLLOW PREVIOUS RACER");	
			setCameraMode(RaceCameraEnum.FOLLOW_PREVIOUS_RACER);
			
			List<Ship> ships = GameManager.Instance.getShips();
			
			if (ships.Count > 0)
			{
				currentShip--;                  
				
				if (currentShip < 0) currentShip = ships.Count-1;
				
				Cam.setFollowTarget(ships[currentShip].transform);
				
				Gui.setMessage("Following " + Cam.followTarget.name);
			}
		}
		
		protected virtual void RaceMode_CameraMode_NextRacer(object sender, CameraEventArgs args)
		{
			// D.log ("Event", "Camera mode set to FOLLOW NEXT RACER");	
			setCameraMode (RaceCameraEnum.FOLLOW_NEXT_RACER);
			
			List<Ship> ships = GameManager.Instance.getShips();
			
			if (ships.Count > 0)
			{
				currentShip++;
				
				if (currentShip == ships.Count) currentShip = 0;
				
				Cam.setFollowTarget(ships[currentShip].transform);
				
				Gui.setMessage("Following " + Cam.followTarget.name);
			}
		}
		
		protected virtual void RaceMode_CameraMode_LeadRacer(object sender, CameraEventArgs args)
		{
			// D.log ("Event", "Camera mode set to TRACK LEAD RACER");	
			setCameraMode(RaceCameraEnum.TRACK_RACE_LEADER);
			
			Gui.setMessage("Camera mode set to TRACK LEAD RACER");
			
			if (Cam.followTarget != currentLeadRacer.transform) 
			{
				Cam.setFollowTarget(currentLeadRacer.transform);
				
				Gui.setMessage("Following " + Cam.followTarget.name);
			}
		}	
		
		protected virtual void RaceMode_CameraMode_LastRacer(object sender, CameraEventArgs args)
		{
			// D.log ("Event", "Camera mode set to TRACK LAST RACER");	
			setCameraMode(RaceCameraEnum.TRACK_LAST_RACER);
			
			Gui.setMessage("Camera mode set to TRACK LAST RACER");
			
			if (Cam.followTarget != currentLastRacer.transform) 
			{
				Cam.followTarget = currentLastRacer.transform;
				Gui.setMessage("Following " + Cam.followTarget.name);
			}
		}

        protected virtual void RaceMode_TogglePositionIDs(object sender, InputEventArgs args)
        {
            // D.log ("Event", "Race mode toggling position IDs");	            
            NoxRaceGUI raceGUI = Gui as NoxRaceGUI;

            if (raceGUI != null)
            {
                Gui.setMessage("Race mode toggling position IDs");

                raceGUI.togglePositionIDs();
            }
        }

        protected virtual void RaceMode_ToggleRacerIDs(object sender, InputEventArgs args)
        {
            // D.log ("Event", "Race mode toggling racer IDs");	            
            NoxRaceGUI raceGUI = Gui as NoxRaceGUI;

            if (raceGUI != null)
            {
                Gui.setMessage("Race mode toggling racer IDs");

                raceGUI.toggleRacerIDs();
            }
        }
    }
}