using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;

using NoxCore.Controllers;
using NoxCore.Cameras;
using NoxCore.Data;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Weapons;
using NoxCore.GUIs;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Stats;
using NoxCore.Utilities;
using NoxCore.Builders;


namespace NoxCore.GameModes
{
    public class SquadvsSquadEventArgs : GameEventArgs
    {
        public Structure structureAttacked;
        public Module moduleAttacked;
        public Structure attacker;

        public SquadvsSquadEventArgs(Structure structureAttacked, Module moduleAttacked, Structure attacker)
        {
            this.structureAttacked = structureAttacked;
            this.moduleAttacked = moduleAttacked;
            this.attacker = attacker;
        }
    }

    public class SquadvsSquadMode : GameMode
    {
        #region custom game mode events
        ////////////////////////////////////
        /*
			Custom game mode events
		*/
        ////////////////////////////////////

        #region camera mode events
        public static event CameraEventDispatcher CameraMode_NextStation;
        public static event CameraEventDispatcher CameraMode_PrevStation;
        #endregion

        #region debug mode events
        #endregion
        #endregion

        private SquadvsSquadGUI gui;

        public bool skipIntro;

        public List<FactionData> factions = new List<FactionData>();

        // custom game mode references & variables
        private Timer matchTimer;
        public int maxTime;
        private ArenaBarrier arena;
        protected float maxBoundaryTime;
        public bool stationDestroyed;

        public float stationVulnerabilityTime;
        protected bool stationsVulnerable;

        protected List<Station> stations = new List<Station>();

        private Dictionary<int, float> factionSpawnRots;

        public Vector2 initSpawnRadialOffset;

        [Range(0.0f, 1.0f)]
        public float spawnRadiusFraction;

        [Range(0, 1)]
        public float spawnInSpeedFraction;

        public List<CombatStats> combateerStats;
        public Dictionary<string, SquadCombatStats> squadStats;

        public List<Texture2D> insignias, miniInsignias;

        // custom camera variables
        public int currentShip, currentStructure;

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

        public delegate void SquadvsSquadEventDispatcher(object sender, TeamDeathMatchEventArgs args);
        public static event SquadvsSquadEventDispatcher TargetDestroyed;

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
            if (matchTimer.getTime() >= maxTime)
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

        public static Texture2D loadPNG(FileInfo fileInfo)
        {
            Texture2D tex = null;
            byte[] fileData;

            if (fileInfo.Exists == true)
            {
                fileData = File.ReadAllBytes(fileInfo.FullName);
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);                // note: this will auto-resize the texture dimensions.
            }
            return tex;
        }

        public override void init()
        {
            base.init();

            // set collision matrix based on collision layer masks
            Physics2D.SetLayerCollisionMask(LayerMask.NameToLayer("Ship"), getCollisionMask("SHIP").GetValueOrDefault());
            Physics2D.SetLayerCollisionMask(LayerMask.NameToLayer("Structure"), getCollisionMask("STRUCTURE").GetValueOrDefault());
            Physics2D.SetLayerCollisionMask(LayerMask.NameToLayer("Projectile"), getCollisionMask("PROJECTILE").GetValueOrDefault());

            #region subscribe to custom game mode events
            List<Structure> structures = GameManager.Instance.getAllStructures();

            foreach (Structure structure in structures)
            {
              //  if (structure.Stats != null)
                {
                    structure.InstigatedAnyDamage += SquadvsSquad_InstigatedAnyDamage;
                    structure.TakenAnyDamage += SquadvsSquad_TakenAnyDamage;
                    structure.NotifyKiller += SquadvsSquad_NotifyKiller;
                    structure.NotifyKilled += SquadvsSquad_NotifyKilled;
                    structure.NotifyAssister += SquadvsSquad_NotifyAssister;
                    structure.ModuleDamaged += SquadvsSquad_ModuleDamaged;
                    structure.SurvivalTimeUpdated += SquadvsSquad_SurvivalTimeUpdated;
                    structure.Respawn += SquadvsSquad_Respawn;
                }
            }
            #endregion

            #region subscribe to custom camera mode events
            CameraMode_NextStation += SquadvsSquadMode_CameraMode_NextStation;
            CameraMode_PrevStation += SquadvsSquadMode_CameraMode_PrevStation;
            #endregion

            #region subscribe to custom debug mode events
            #endregion

            // set reference to custom game mode GUI
            Gui = GameObject.Find("UI Manager").GetComponent<SquadvsSquadGUI>();

            if (Gui == null)
            {
                D.error("GUI: {0}", "Cannot find the custom GUI component under the UI Manager GameObject");
                GameEventManager.Call_AbortedMatch(this);
                return;
            }

            gui = Gui as SquadvsSquadGUI;

            if (gui == null)
            {
                D.error("GUI", "Cannot cast supplied GUI component to the required SquadvsSquadGUI");
                return;
            }



           // string scenarioFolderRoot = Path.Combine(Application.dataPath, scenarioFolder);

          /*  foreach (string scenarioSubFolder in scenarioSubFolders)
            {
                string subScenarioFolderRoot = Path.Combine(scenarioFolderRoot, scenarioSubFolder);
                string insigniaPath = Path.Combine(subScenarioFolderRoot, "Insignia-large.png");
                string miniInsigniaPath = Path.Combine(subScenarioFolderRoot, "Insignia-small.png");

                System.IO.FileInfo squadInsigniaPath = new FileInfo(insigniaPath);

                Texture2D squadInsignia = loadPNG(squadInsigniaPath);

                if (squadInsignia != null)
                {
                    D.log("Content", "Insignia successfully loaded from path: " + squadInsigniaPath.FullName);
                    insignias.Add(squadInsignia);
                }
                else
                {
                    D.log("Content", "Could not read the squad insignia PNG file at: " + squadInsigniaPath.FullName);
                }

                squadInsigniaPath = new FileInfo(miniInsigniaPath);

                squadInsignia = loadPNG(squadInsigniaPath);

                if (squadInsignia != null)
                {
                    D.log("Content", "Insignia successfully loaded from path: " + squadInsigniaPath.FullName);
                    miniInsignias.Add(squadInsignia);
                }
                else
                {
                    D.log("Content", "Could not read the squad insignia PNG file at: " + squadInsigniaPath.FullName);
                }
            }
          */
            // NOTE: there are two default factions created in GameManager (Neutral is faction ID -1 and Unaffiliated is 0)
            if (FactionManager.Instance.Factions.Count < 2)
            {
                D.error("GameMode: {0}", "Incorrect number of factions. Need at least two factions");
                GameEventManager.Call_AbortedMatch(this);
                return;
            }


            insignias = new List<Texture2D>();
            miniInsignias = new List<Texture2D>();

            foreach(FactionData data in FactionManager.Instance.Factions)
            {
                insignias.Add(data.largeInsignia);
                miniInsignias.Add(data.smallInsignia);
            }

            // stores the rotation each faction has around a notional circle (all factions spawn in radially around the centre of the arena)
            factionSpawnRots = new Dictionary<int, float>();

            // quick array to store the rotations available around the notional arena
            float[] factionRots;
            int numFactions = FactionManager.Instance.Factions.Count;

            factionRots = new float[numFactions];

            float rotInterval = 360 / numFactions;

            for (int factionNum = 0; factionNum < factionRots.Length; factionNum++)
            {
                factionRots[factionNum] = rotInterval * factionNum;
            }

            int factionsAssigned = 0;
            bool switchedSides = false;

            // randomly assign each faction's rotation around the arena circle
            foreach (FactionData faction in FactionManager.Instance.Factions)
            {
                if (faction.label.Equals("Neutral") || faction.label.Equals("Unaffiliated")) continue;

                FleetDataManager fleetManager = faction.FleetManager;

                int randNum;

                while (true)
                {
                    randNum = UnityEngine.Random.Range(0, numFactions);

                    if (!factionSpawnRots.ContainsValue(factionRots[randNum]))
                    {
                        break;
                    }
                }

                if (randNum != factionsAssigned) switchedSides = true;

                factionSpawnRots.Add(faction.ID, factionRots[randNum]);

                factionsAssigned++;
            }

            gui.switchedSides = switchedSides;

            gui.enabled = false;
        }

        public Vector2 getSpawnPosition(int id)
        {
            Vector2 randPos = UnityEngine.Random.insideUnitCircle * (arena.getRadius() / 4);// * spawnRadiusFraction;
            if (id == 1)
                randPos.x -= (arena.getRadius() / 2);
            else
                randPos.x += (arena.getRadius() / 2);

            return new Vector2(randPos.x, randPos.y);
        }

        public override float getSpawnRotation()
        {
            Vector2 randRot = UnityEngine.Random.insideUnitCircle;

            float bearing = (Mathf.Atan2(-randRot.y, randRot.x) * Mathf.Rad2Deg) + 90;

            if (bearing < 0) bearing += 360;

            return bearing;
        }

        public override void initController(Structure structure)
        {
            if (structure != null)
            {
                structure.enabled = true;

                structure.CanBeDamaged = true;                

                StructureController controller = structure.GetComponent<StructureController>();
                //StartCoroutine(controller.update());
                controller.start();

                structure.StructureCollider.enabled = true;

                if (structure != null && structure.Stats != null)
                {
                    structure.startSurvivalClock();
                }

                Ship ship = structure as Ship;

                if (ship != null)
                {                    
                    ship.CanRespawn = true;
                    ship.StructureData.RespawnsAtStartSpot = true;
                    ship.Controller.invulnerableToArenaBoundary = false;

                    controller.structure.MaxBoundaryTime = maxBoundaryTime;

                    ship.Bearing = -ship.transform.rotation.eulerAngles.z;

                    if (ship.Bearing < 0) ship.Bearing += 360;

                    float theta = 90 - ship.Bearing;

                    if (theta < 0) theta += 360;

                    float x = Mathf.Cos(theta * Mathf.Deg2Rad);
                    float y = Mathf.Sin(theta * Mathf.Deg2Rad);

                    ship.Heading = new Vector2(x, y);

                    ship.setSpawnInSpeed(spawnInSpeedFraction);
                }
                else
                {
                    // all faction ships to respawn at this structure (TODO - this could be improved)
                    FactionData faction = FactionManager.Instance.findFaction(structure.Faction.ID);

                    foreach(Ship factionShip in faction.FleetManager.getAllShips())
                    {
                        factionShip.Controller.startSpot = structure.transform.position;
                    }
                }

                Station station = structure as Station;

                if (station != null)
                {
                    stations.Add(station);
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
                        structureGO.Stats = structureGO.gameObject.AddComponent<StructureStats>();
                        structureGO.Stats.structure = structureGO;
                    }
                   // else
                    {
                        combateerStats.Add(new CombatStats(controller.structure.Stats));

                        if (squadStats.ContainsKey(controller.structure.Faction.label) == false)
                        {
                            squadStats.Add(controller.structure.Faction.label, new SquadCombatStats(controller.structure));
                        }
                        else
                        {
                            SquadCombatStats squadStat;
                            squadStats.TryGetValue(controller.structure.Faction.label, out squadStat);

                            if (squadStat != null)
                            {
                                squadStat.squadStructures.Add(controller.structure);
                                squadStat.squadTotalCost += controller.structure.TotalCost;
                            }
                        }
                    }
                }

                if (controller.startSpot == null)
                {
                    controller.setInitialLocationAndRotation(getSpawnPosition(), Quaternion.Euler(new Vector3(0, 0, getSpawnRotation())));
                    if(FactionManager.Instance.Factions[0] == structureGO.Faction)
                        controller.transform.position = getSpawnPosition(1);
                    else
                        controller.transform.position = getSpawnPosition(2);

                    controller.transform.rotation = Quaternion.LookRotation(transform.forward, -controller.transform.position.normalized);
                }
                else if (controller.startRotation == null)
                {
                    Vector2 startSpot = controller.startSpot.GetValueOrDefault();

                    float posRot, bearRot;

                    factionSpawnRots.TryGetValue(structureGO.GetComponent<Structure>().Faction.ID, out posRot);

                    bearRot = (posRot - 90) * Mathf.Deg2Rad;
                    posRot *= Mathf.Deg2Rad;

                    // rotate around arena
                    startSpot = new Vector2(startSpot.x * Mathf.Cos(posRot) - startSpot.y * Mathf.Sin(posRot), startSpot.x * Mathf.Sin(posRot) + startSpot.y * Mathf.Cos(posRot));

                    // translate from centre
                    //startSpot = new Vector2(startSpot.x * Mathf.Sin(posRot), startSpot.y * Mathf.Cos(posRot));       

                    if (controller.structure is Ship)
                    {
                        if (startSpot.x > 0)
                        {
                            startSpot = new Vector2(startSpot.x + initSpawnRadialOffset.x, startSpot.y + initSpawnRadialOffset.y);
                        }
                        else
                        {
                            startSpot = new Vector2(startSpot.x - initSpawnRadialOffset.x, startSpot.y + initSpawnRadialOffset.y);
                        }
                    }

                    controller.setInitialLocationAndRotation(startSpot, Quaternion.Euler(new Vector3(0, 0, bearRot * Mathf.Rad2Deg)));
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

            if (stationsVulnerable == false)
            {
                if (matchTimer != null)
                {
                    if (matchTimer.getTime() >= stationVulnerabilityTime)
                    {
                        foreach (Station station in stations)
                        {
                            station.CanBeDamaged = true;
                        }

                        stationsVulnerable = true;
                    }
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

        protected void calculateMatchStats()
        {
            int numFactions = squadStats.Count;

            // calculate KAD, DAM and MAS
            foreach (KeyValuePair<string, SquadCombatStats> squadStat in squadStats)
            {
                squadStat.Value.squadKAD = squadStat.Value.squadKills + (0.2f * squadStat.Value.squadAssists) - squadStat.Value.squadDeaths;
                squadStat.Value.squadDAM = (squadStat.Value.squadDamageCaused * squadStat.Value.squadKills) / ((squadStat.Value.squadDamageTaken + 1) * (squadStat.Value.squadDeaths + 1));

                if (squadStat.Value.squadMassLost == 0)
                {
                    squadStat.Value.squadMAS = squadStat.Value.squadMassKilled / (squadStat.Value.squadMassLost + 1);
                }
                else
                {
                    squadStat.Value.squadMAS = squadStat.Value.squadMassKilled / (squadStat.Value.squadMassLost);
                }
            }
        }

        protected void generateResults()
        {
            string classnameStr = this.GetType().Name;

            Dictionary<string, SquadCombatStats> squadStats = (Gui as SquadvsSquadGUI).getSquads();

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

            foreach (KeyValuePair<string, SquadCombatStats> entry in squadStats)
            {
                sw.WriteLine("\nFaction (" + entry.Value.squadID + "): " + entry.Value.squadName + "\n");
                sw.WriteLine("Total Cost: " + entry.Value.squadTotalCost);

                sw.WriteLine("\nScore: " + entry.Value.squadMassKilled);

                sw.WriteLine("\nAverage Survival Time: " + entry.Value.squadAverageSurvivalTime);
                sw.WriteLine("Kills: " + entry.Value.squadKills);
                sw.WriteLine("Deaths: " + entry.Value.squadDeaths);
                sw.WriteLine("Assists: " + entry.Value.squadAssists);
                sw.WriteLine("Modules Destroyed: " + entry.Value.squadModulesDestroyed);
                sw.WriteLine("Modules Lost: " + entry.Value.squadModulesLost);
                sw.WriteLine("Mass Killed: " + entry.Value.squadMassKilled);
                sw.WriteLine("Mass Lost: " + entry.Value.squadMassLost);
                sw.WriteLine("Damage Caused: " + entry.Value.squadDamageCaused);
                sw.WriteLine("Damage Taken: " + entry.Value.squadDamageTaken);

                sw.WriteLine("KAD: " + entry.Value.squadKAD);
                sw.WriteLine("DAM: " + entry.Value.squadDAM);
                sw.WriteLine("MAS: " + entry.Value.squadMAS);
            }

            sw.WriteLine("-----------ALL COMBATEERS---------------");

            foreach (CombatStats stat in combateerStats)
            {
                sw.WriteLine("Ship: " + stat.structure.name);
                sw.WriteLine("Commanded By: " + stat.structure.Command.rankData.abbreviation + " " + stat.structure.Command.label);
                sw.WriteLine("Faction: " + stat.structure.Faction.label);

                if (stat.structure.Stats != null)
                {
                    sw.WriteLine("Score Contribution: " + (stat.massKill - stat.massLoss));

                    sw.WriteLine("Average Survival Time: " + Timer.formatTimer(stat.structure.Stats.averageSurvivalTime, true));
                    sw.WriteLine("Kills: " + stat.structure.Stats.numKills, false);
                    sw.WriteLine("Assists: " + stat.structure.Stats.numAssists);
                    sw.WriteLine("Deaths: " + stat.structure.Stats.numDeaths);
                    sw.WriteLine("Modules Destroyed: " + stat.structure.Stats.numModulesDestroyed);
                    sw.WriteLine("Modules Lost: " + stat.structure.Stats.numModulesLost);
                    sw.WriteLine("Mass Killed: " + stat.massKill);
                    sw.WriteLine("Mass Lost: " + stat.massLoss);
                    sw.WriteLine("Damage Caused: " + stat.structure.Stats.totalDamageInflicted);
                    sw.WriteLine("Damage Taken: " + stat.structure.Stats.totalDamageTaken);
                    sw.WriteLine("Hull Damage: " + stat.structure.Stats.totalHullDamageTaken);
                    sw.WriteLine("Shield Damage: " + stat.structure.Stats.totalShieldDamageTaken);
                    sw.WriteLine("Module Damage: " + stat.structure.Stats.totalArmourDamageTaken);
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

            matchTimer = GameManager.Instance.GetComponent<Timer>();
   
            if (matchTimer != null)
            {
                matchTimer.maxTime = maxTime;
            }
            else
            {
                D.warn("Content: {0}", "No Timer component attached to the Game Manager");
            }

            GameObject arenaBoundary = GameObject.Find("Circular Arena");

            if (arenaBoundary == null)
            {
                D.error("Content: {0}", "Required ArenaBarrier GameObject not set for this game mode");
                GameEventManager.Call_AbortedMatch(this);
            }
            else
            {
                
                arena = arenaBoundary.GetComponent<ArenaBuilder>().init();

                if (arena == null)
                {
                    D.error("Content: {0}", "Arena Barrier GameObject does not contain an ArenaBarrier component required for this game mode");
                    GameEventManager.Call_AbortedMatch(this);
                    return;
                }
                else
                {
                    maxBoundaryTime = 10.0f;
                }
            }

            combateerStats = new List<CombatStats>();
            squadStats = new Dictionary<string, SquadCombatStats>();

            // set spawn locations and rotations for each structure
            List<Structure> structures = GameManager.Instance.getAllStructures();

            foreach (Structure structure in structures)
            {
                // TODO - do I do this on other modes and should I be?
                structure.gameObject.SetActive(false);

                if (structure != null)
                {
                    positionStructure(structure);
                    structure.spawnedIn = true;
                }
            }

            // attempt to start the match
            if (ReadyToStartMatch() == true)
            {
                // initialise the GUI here
                if (Gui != null)
                {
                    Gui.init();
                }
            }

            D.log("Content", "Total number of factions: " + squadStats.Count);                  
        }

        public void triggerStartMatch()
        {
            List<Structure> structures = GameManager.Instance.getAllStructures();

            foreach (Structure structure in structures)
            {
                structure.gameObject.SetActive(true);
            }

            // initialise the camera here
            Cam.init();

            setInitialCameraTarget();

            GameEventManager.Call_MatchHasStarted(this);
        }

        protected override void GameEventManager_MatchHasStarted(object sender)
        {
            base.GameEventManager_MatchHasStarted(sender);

            // start all structure and ship GameObjects ticking

            List<Structure> structures = GameManager.Instance.getAllStructures();

            foreach (Structure structure in structures)
            {
                initController(structure);

                // set stations to be invulnerable initially
                Station station = structure as Station;

                if (station != null)
                {
                    station.CanBeDamaged = false;
                }
            }

            matchTimer.startTimer();
        }

        protected override void GameEventManager_MatchHasEnded(object sender)
        {
            base.GameEventManager_MatchHasEnded(sender);

            Gui.setMessage("Match ended!");

            // set match timer to either the maximum or minimum (because it might be slightly over or under) and stop the clock
            if (matchTimer.countdown == true)
            {
                matchTimer.setTimer(0, true);
            }
            else
            {
                matchTimer.setTimer(matchTimer.maxTime, true);
            }

            matchTimer.stopTimer();

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

            // calculate all match stats
            calculateMatchStats();

            if (generateResultsFile == true)
            {
                generateResults();
            }

            gui.showMatchReport();
        }
        #endregion

        ////////////////////////////////////
        /*
			Event dispatchers for the custom game mode
		*/
        ////////////////////////////////////	

        public static void Call_TargetDestroyed(object sender, TeamDeathMatchEventArgs args)
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
        public virtual void SquadvsSquad_Respawn(object sender, RespawnEventArgs args)
        {
            if (args.respawnedStructure != null)
            {
                int statsIndex = combateerStats.FindIndex(s => s.structure == args.respawnedStructure);

                if (statsIndex >= 0)
                {
                    SquadCombatStats squadStat;
                    squadStats.TryGetValue(args.respawnedStructure.Faction.label, out squadStat);

                    if (squadStat != null)
                    {
                        squadStat.numRespawns++;
                    }
                }

                args.respawnedStructure.StructureRigidbody.velocity = Vector2.zero;

                Ship respawnedShip = args.respawnedStructure as Ship;

                if (respawnedShip != null)
                {
                    respawnedShip.Bearing = respawnedShip.Controller.startRotation.GetValueOrDefault();
                    respawnedShip.StructureRigidbody.velocity = Vector2.zero;
                }
            }
        }

        public virtual void SquadvsSquad_InstigatedAnyDamage(object sender, DamageEventArgs args)
        {
            if (args.damageCauser != null)
            {
                int statsIndex = combateerStats.FindIndex(s => s.structure == args.damageCauser);

                if (statsIndex >= 0)
                {
                    combateerStats[statsIndex].damageCaused += args.damage;

                    SquadCombatStats squadStat;
                    squadStats.TryGetValue(args.damageCauser.Faction.label, out squadStat);

                    if (squadStat != null)
                    {
                        squadStat.squadDamageCaused += args.damage;
                    }
                }
            }
        }

        public virtual void SquadvsSquad_TakenAnyDamage(object sender, DamageEventArgs args)
        {
            if (args.damagedStructure != null)
            {
                int statsIndex = combateerStats.FindIndex(s => s.structure == args.damagedStructure);

                if (statsIndex >= 0)
                {
                    combateerStats[statsIndex].damageTaken += args.damage;

                    SquadCombatStats squadStat;
                    squadStats.TryGetValue(args.damagedStructure.Faction.label, out squadStat);

                    if (squadStat != null)
                    {
                        squadStat.squadDamageTaken += args.damage;
                    }
                }
            }
        }

        public virtual void SquadvsSquad_NotifyKiller(object sender, TargetDestroyedEventArgs args)
        {
            if (args.attacker != null)
            {
                int statsIndex = combateerStats.FindIndex(s => s.structure == args.attacker);

                if (statsIndex >= 0)
                {
                    SquadCombatStats squadStat;
                    squadStats.TryGetValue(args.attacker.Faction.label, out squadStat);

                    combateerStats[statsIndex].kills++;

                    combateerStats[statsIndex].massKill += args.structureAttacked.MaxHullStrength;

                    if (squadStat != null)
                    {
                        squadStat.squadKills++;

                        squadStat.squadMassKilled += args.structureAttacked.MaxHullStrength;

                        Ship ship = args.structureAttacked as Ship;

                        // station destroyed?
                        if (ship == null)
                        {
                            if (squadStat.stationKillTime == maxTime)
                            {
                                squadStat.stationKillTime = matchTimer.getTime();
                                squadStat.killedStation = true;
                            }
                        }

                        if (squadStat.firstKillTime == 0)
                        {
                            squadStat.firstKillTime = matchTimer.getTime();
                        }
                    }
                }
            }
        }

        public virtual void SquadvsSquad_NotifyAssister(object sender, AssistEventArgs args)
        {
            if (args.assister != null)
            {
                int statsIndex = combateerStats.FindIndex(s => s.structure == args.assister);

                if (statsIndex >= 0)
                {
                    combateerStats[statsIndex].assists++;

                    SquadCombatStats squadStat;
                    squadStats.TryGetValue(args.assister.Faction.label, out squadStat);

                    if (squadStat != null)
                    {
                        squadStat.squadAssists++;
                        squadStat.squadKillParticipation += args.killParticipation;
                    }
                }
            }
        }

        public virtual void SquadvsSquad_NotifyKilled(object sender, TargetDestroyedEventArgs args)
        {
            if (args.structureAttacked != null)
            {
                int statsIndex = combateerStats.FindIndex(s => s.structure == args.structureAttacked);

                if (statsIndex >= 0)
                {
                    combateerStats[statsIndex].deaths++;
                    combateerStats[statsIndex].massLoss += args.structureAttacked.MaxHullStrength;

                    SquadCombatStats squadStat;
                    squadStats.TryGetValue(args.structureAttacked.Faction.label, out squadStat);

                    if (squadStat != null)
                    {
                        squadStat.squadDeaths++;
                        squadStat.squadMassLost += args.structureAttacked.MaxHullStrength;

                        // killed by environment?
                        if (args.attacker == null)
                        {
                            foreach (KeyValuePair<string, SquadCombatStats> squad in squadStats)
                            {
                                // do something with entry.Value or entry.Key
                                if (squad.Key != args.structureAttacked.Faction.label)
                                {
                                    squad.Value.squadMassKilled += args.structureAttacked.MaxHullStrength;
                                }
                            }
                        }
                    }
                }
            }
        }

        public virtual void SquadvsSquad_ModuleDamaged(object sender, ModuleDamageEventArgs args)
        {
            if (args.moduleHit != null && args.moduleAttacker != null)
            {
                if (args.destroyed == true)
                {
                    int statsIndex = combateerStats.FindIndex(s => s.structure == args.moduleAttacker);

                    if (statsIndex >= 0)
                    {
                        combateerStats[statsIndex].modulesDestroyed++;

                        SquadCombatStats squadStat;
                        squadStats.TryGetValue(args.moduleAttacker.Faction.label, out squadStat);

                        if (squadStat != null)
                        {
                            squadStat.squadModulesDestroyed++;
                            squadStat.squadMassKilled += args.moduleHit.ModuleData.MaxArmour;
                        }

                        squadStats.TryGetValue(args.moduleOwner.Faction.label, out squadStat);

                        if (squadStat != null)
                        {
                            squadStat.squadModulesLost++;
                            squadStat.squadMassLost += args.moduleHit.ModuleData.MaxArmour;
                        }
                    }
                }
            }
        }

        public virtual void SquadvsSquad_SurvivalTimeUpdated(object sender, SurvivalTimeEventArgs args)
        {
            if (args.updatedStructure != null)
            {
                int statsIndex = combateerStats.FindIndex(s => s.structure == args.updatedStructure);

                if (statsIndex >= 0)
                {
                    combateerStats[statsIndex].averageSurvivalTime = args.updatedSurvivalTime;

                    SquadCombatStats squadStat;
                    squadStats.TryGetValue(args.updatedStructure.Faction.label, out squadStat);

                    if (squadStat != null)
                    {
                        squadStat.updateSquadAverageSurvivalTime(args);
                    }
                }
            }
        }

        protected virtual void SquadvsSquadMode_CameraMode_PrevStation(object sender, CameraEventArgs args)
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

        protected virtual void SquadvsSquadMode_CameraMode_NextStation(object sender, CameraEventArgs args)
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
    }
}