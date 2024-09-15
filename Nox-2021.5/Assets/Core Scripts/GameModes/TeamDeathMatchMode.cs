using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;

using UnityEngine;

using NoxCore.Builders;
using NoxCore.Controllers;
using NoxCore.Cameras;
using NoxCore.Data;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Fittings.Weapons;
using NoxCore.GUIs;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Stats;
using NoxCore.Utilities;
using NoxCore.Rules;

namespace NoxCore.GameModes
{
    public class TeamDeathMatchEventArgs : GameEventArgs
    {
        public Structure structureAttacked;
        public Module moduleAttacked;
        public Structure attacker;

        public TeamDeathMatchEventArgs(Structure structureAttacked, Module moduleAttacked, Structure attacker)
        {
            this.structureAttacked = structureAttacked;
            this.moduleAttacked = moduleAttacked;
            this.attacker = attacker;
        }
    }

    public class TeamDeathMatchMode : GameMode
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
        private TeamDeathMatchGUI gui;
        private Timer matchTimer;
        public int maxTime;
        private ArenaBarrier arena;
        protected float maxBoundaryTime;

        [Range(0.0f, 1.0f)]
        public float spawnRadiusFraction;

        [Range(0, 1)]
        public float spawnInSpeedFraction;

        public bool warpIn;

        public float ASTPercent = 25;
        public float KADPercent = 30;
        public float MASPercent = 25;
        public float DAMPercent = 20;

        public List<CombatStats> combateerStats;
        public Dictionary<string, FactionCombatStats> factionStats;
        public Dictionary<string, FactionCombatStats> combatStatsSorted;
        public Dictionary<string, FactionCombatStatsZScored> factionStatsZScored;
        public Dictionary<string, FactionCombatStatsWeighted> factionStatsWeighted;
        public Dictionary<string, FactionCombatStatsWeighted> factionStatsSorted;

        public List<Texture2D> miniInsignias;

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

        public delegate void TeamDeathMatchEventDispatcher(object sender, TeamDeathMatchEventArgs args);
        public static event TeamDeathMatchEventDispatcher TargetDestroyed;

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
            List<Ship> inactiveShips = GameObject.Find("Placeables").FindInactive<Ship>();

            foreach (Structure structure in inactiveShips)
            {
                structure.Controller.GeneratesStats = true;

                if (structure.Stats == null)
                {
                    structure.Stats = structure.gameObject.AddComponent<StructureStats>();
                }

                if (structure.Stats != null)
                {
                    structure.Stats.structure = structure;

                    structure.InstigatedAnyDamage += TeamDeathMatch_InstigatedAnyDamage;
                    structure.TakenAnyDamage += TeamDeathMatch_TakenAnyDamage;
                    structure.NotifyKiller += TeamDeathMatch_NotifyKiller;
                    structure.NotifyKilled += TeamDeathMatch_NotifyKilled;
                    structure.NotifyAssister += TeamDeathMatch_NotifyAssister;
                    structure.ModuleDamaged += TeamDeathMatch_ModuleDamaged;
                    structure.SurvivalTimeUpdated += TeamDeathMatch_SurvivalTimeUpdated;
                    structure.Spawn += TeamDeathMatch_Spawn;
                    structure.Respawn += TeamDeathMatch_Respawn;
                }
            }
            #endregion

            #region subscribe to custom camera mode events
            #endregion

            #region subscribe to custom debug mode events
            #endregion

            // set reference to custom game mode GUI
            Gui = GameObject.Find("UI Manager").GetComponent<TeamDeathMatchGUI>();

            if (Gui == null)
            {
                D.error("GUI: {0}", "Cannot find the custom GUI component under the UI Manager GameObject");
                GameEventManager.Call_AbortedMatch(this);
                return;
            }

            gui = Gui as TeamDeathMatchGUI;

            if (gui == null)
            {
                D.error("GUI", "Cannot cast supplied GUI component to the required TeamDeathMatchGUI");
                return;
            }

            //miniInsignias = new List<Texture2D>();
            /*
            string scenarioFolderRoot = Path.Combine(Application.dataPath, scenarioFolder);

            foreach (string scenarioSubFolder in scenarioSubFolders)
            {
                string subScenarioFolderRoot = Path.Combine(scenarioFolderRoot, scenarioSubFolder);
                string miniInsigniaPath = Path.Combine(subScenarioFolderRoot, "Insignia-small.png");

                System.IO.FileInfo squadInsigniaPath = new FileInfo(miniInsigniaPath);

                Texture2D squadInsignia = loadPNG(squadInsigniaPath);

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

            gui.enabled = false;
        }

        public override Vector2 getSpawnPosition()
        {
            Vector2 randPos = UnityEngine.Random.insideUnitCircle * ArenaBarrier.radius * spawnRadiusFraction;

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

        public override void initController(Structure structure)
        {
            if (structure != null)
            {
                structure.enabled = true;

                structure.CanBeDamaged = true;

                StructureController controller = structure.GetComponent<StructureController>();
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
                    ship.WarpInOnRespawn = true;

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
            }
        }

        public override void positionStructure(Structure structure)
        {
            bool infoSet = false;
            Vector2 spawnSpot = Vector2.zero;
            Quaternion spawnRotation = Quaternion.identity;

            Ship spawningShip = structure as Ship;

            if (spawningShip != null)
            {
                FactionData faction = spawningShip.Faction;

                SquadronData squadron = null;

                // check if ship is part of a squadron and ensure it cannot respawn by itself
                if (spawningShip.Classification == ShipClassification.FIGHTER || spawningShip.Classification == ShipClassification.BOMBER)
                {
                    spawningShip.CanRespawn = false;

                    squadron = faction.FleetManager.findSquadronData(spawningShip.FleetData.ID, spawningShip.WingData.ID, spawningShip.SquadronData.ID);

                    if (squadron != null)
                    {
                        squadron.CanRespawn = true;
                    }
                }

                if (squadron != null)
                {
                    if (spawningShip.SquadronData.ID > 1)
                    {
                        GameObject leaderShip = squadron.getLeader();

                        if (leaderShip != null)
                        {
                            IFormationFly formationController = spawningShip.Controller as IFormationFly;

                            float startRotation = leaderShip.transform.rotation.eulerAngles.z;
                            spawnSpot = (Vector2)leaderShip.transform.position + formationController.getFormationOffset().Rotate(startRotation);
                            spawnRotation = leaderShip.transform.rotation;

                            infoSet = true;
                        }
                    }
                }

                if (infoSet == false)
                {
                    Structure otherStructure = null;

                    // spawn near to closest intact faction structure or randomly

                    // get a list of all structures in the faction in the scene
                    List<Structure> factionStructures = faction.FriendlyStructures.ToList<Structure>();

                    // note: this will give no structures initially as all ships may be warping in at start
                    if (factionStructures.Count > 0)
                    {
                        // remove all structures not yet spawned in
                        factionStructures.RemoveAll(s => s.spawnedIn == false);

                        // remove all destroyed structures
                        factionStructures.RemoveAll(s => s.Destroyed == true);
                    }
                    else
                    {
                        // find active ship in scene that hasn't yet warped in and been added to the GameManager's list of ships
                        List<Ship> warpShips = GameObject.FindObjectsOfType<Ship>().ToList<Ship>();

                        foreach (Ship ship in warpShips)
                        {
                            // add any ship in our faction which has spawned in (or is warping in)
                            if (ship.Faction.ID == structure.Faction.ID && ship.spawnedIn == true) factionStructures.Add(ship);
                        }
                    }

                    // remove any active ships outside the arena
                    factionStructures.RemoveAll(s => s.transform.position.magnitude > ArenaBarrier.radius);

                    // sort remaining faction structures by distance
                    factionStructures.Sort(delegate (Structure a, Structure b)
                    {
                        float sqrLen1 = (spawningShip.transform.position - a.transform.position).sqrMagnitude;
                        float sqrLen2 = (spawningShip.transform.position - b.transform.position).sqrMagnitude;

                        return sqrLen1.CompareTo(sqrLen2);
                    });

                    if (factionStructures.Count > 0)
                    {
                        otherStructure = factionStructures[0];
                    }

                    if (otherStructure == null)
                    {
                        spawnSpot = getSpawnPosition();
                        spawnRotation = Quaternion.Euler(0, 0, getSpawnRotation());
                    }
                    else
                    {
                        Vector2 otherStructurePosition = otherStructure.transform.position;
                        spawnRotation = otherStructure.transform.rotation;

                        Vector2 offsetPosition = UnityEngine.Random.insideUnitCircle.normalized;

                        spawnSpot = otherStructurePosition + (offsetPosition * ((otherStructure.StructureData.Mass + spawningShip.StructureData.Mass) / 20.0f));

                        // flip the offset position if the respawnSpot is outside the arena
                        if (spawnSpot.magnitude >= ArenaBarrier.radius)
                        {
                            spawnSpot = otherStructurePosition - (offsetPosition * ((otherStructure.StructureData.Mass + spawningShip.StructureData.Mass) / 20.0f));
                        }
                    }
                }

                // locate spawning structure (in case coords are needed by other spawning in structures to offset against)
                spawningShip.transform.position = spawnSpot;
                spawningShip.transform.rotation = spawnRotation;

                spawningShip.Bearing = -spawnRotation.eulerAngles.z;

                if (spawningShip.Bearing < 0) spawningShip.Bearing += 360;

                float theta = 90 - spawningShip.Bearing;

                if (theta < 0) theta += 360;

                float x = Mathf.Cos(theta * Mathf.Deg2Rad);
                float y = Mathf.Sin(theta * Mathf.Deg2Rad);

                spawningShip.Heading = new Vector2(x, y);

                if (structure.Controller != null)
                {
                    structure.Controller.setInitialLocationAndRotation(spawnSpot, spawnRotation);
                }
                else
                {
                    D.warn("GameMode: {0}", structure.name + " has no Controller");
                }
            }
        }

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

        private static double erf(double x)
        {
            //A&S formula 7.1.26
            double a1 = 0.254829592;
            double a2 = -0.284496736;
            double a3 = 1.421413741;
            double a4 = -1.453152027;
            double a5 = 1.061405429;
            double p = 0.3275911;
            x = Math.Abs(x);
            double t = 1 / (1 + p * x);
            //Direct calculation using formula 7.1.26 is absolutely correct
            //But calculation of nth order polynomial takes O(n^2) operations
            //return 1 - (a1 * t + a2 * t * t + a3 * t * t * t + a4 * t * t * t * t + a5 * t * t * t * t * t) * Math.Exp(-1 * x * x);

            //Horner's method, takes O(n) operations for nth order polynomial
            return 1 - ((((((a5 * t + a4) * t) + a3) * t + a2) * t) + a1) * t * Math.Exp(-1 * x * x);
        }

        public static double NORMSDIST(double z)
        {
            double sign = 1;
            if (z < 0) sign = -1;
            return 0.5 * (1.0 + sign * erf(Math.Abs(z) / Math.Sqrt(2)));
        }

        protected float calculateZScore(float value, float mean, float stdDev)
        {
            if (stdDev == 0) return 0;
            else  return (float)NORMSDIST((value - mean) / stdDev);
        }

        protected void calculateMatchStats()
        {
            float killsMean, deathsMean, assistsMean, modsKilledMean, modsLostMean, massKilledMean, massLostMean, damageCausedMean, damageTakenMean, killParticipationMean, averageSurvivalTimeMean, KADMean, DAMMean, MASMean;
            float killsStdDev, deathsStdDev, assistsStdDev, modsKilledStdDev, modsLostStdDev, massKilledStdDev, massLostStdDev, damageCausedStdDev, damageTakenStdDev, killParticipationStdDev, averageSurvivalTimeStdDev, KADStdDev, DAMStdDev, MASStdDev;

            float sum, diff, sumSquares, variance;

            FactionCombatStats zero = new FactionCombatStats(null);
            zero.factionAST = 0;
            factionStats.Add("ZERO", zero);

            int numFactions = factionStats.Count;

            // calculate KAD, DAM and MAS
            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                factionStat.Value.factionKAD = factionStat.Value.factionKills + (0.2f * factionStat.Value.factionAssists) - factionStat.Value.factionDeaths;
                factionStat.Value.factionDAM = (factionStat.Value.factionDamageCaused * factionStat.Value.factionKills) / ((factionStat.Value.factionDamageTaken+1) * (factionStat.Value.factionDeaths+1));

                if (factionStat.Value.factionMassLost == 0)
                {
                    factionStat.Value.factionMAS = factionStat.Value.factionMassKilled / (factionStat.Value.factionMassLost + 1);
                }
                else
                {
                    factionStat.Value.factionMAS = factionStat.Value.factionMassKilled / (factionStat.Value.factionMassLost);
                }
            }

            #region stat means and standard deviations
            // calculate Kills history stat
            sum = 0;
            sumSquares = 0;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                sum += factionStat.Value.factionKills;
            }

            killsMean = sum / numFactions;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                diff = factionStat.Value.factionKills - killsMean;
                sumSquares += Mathf.Pow(diff, 2);
            }

            variance = sumSquares / (numFactions-1);

            killsStdDev = Mathf.Sqrt(variance);

            // calculate Assists history stat
            sum = 0;
            sumSquares = 0;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                sum += factionStat.Value.factionAssists;
            }

            assistsMean = sum / factionStats.Count;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                diff = factionStat.Value.factionAssists - assistsMean;
                sumSquares += Mathf.Pow(diff, 2);
            }

            variance = sumSquares / (factionStats.Count - 1);

            assistsStdDev = Mathf.Sqrt(variance);

            // calculate Deaths history stat
            sum = 0;
            sumSquares = 0;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                sum += factionStat.Value.factionDeaths;
            }

            deathsMean = sum / factionStats.Count;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                diff = factionStat.Value.factionDeaths - deathsMean;
                sumSquares += Mathf.Pow(diff, 2);
            }

            variance = sumSquares / (factionStats.Count - 1);

            deathsStdDev = Mathf.Sqrt(variance);

            // calculate ModsKilled history stat
            sum = 0;
            sumSquares = 0;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                sum += factionStat.Value.factionModulesDestroyed;
            }

            modsKilledMean = sum / numFactions;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                diff = factionStat.Value.factionModulesDestroyed - modsKilledMean;
                sumSquares += Mathf.Pow(diff, 2);
            }

            variance = sumSquares / (numFactions - 1);

            modsKilledStdDev = Mathf.Sqrt(variance);

            // calculate ModsLost history stat
            sum = 0;
            sumSquares = 0;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                sum += factionStat.Value.factionModulesLost;
            }

            modsLostMean = sum / numFactions;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                diff = factionStat.Value.factionModulesLost - modsLostMean;
                sumSquares += Mathf.Pow(diff, 2);
            }

            variance = sumSquares / (numFactions - 1);

            modsLostStdDev = Mathf.Sqrt(variance);

            // calculate MassKilled history stat
            sum = 0;
            sumSquares = 0;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                sum += factionStat.Value.factionMassKilled;
            }

            massKilledMean = sum / numFactions;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                diff = factionStat.Value.factionMassKilled - massKilledMean;
                sumSquares += Mathf.Pow(diff, 2);
            }

            variance = sumSquares / (numFactions - 1);

            massKilledStdDev = Mathf.Sqrt(variance);

            // calculate MassLost history stat
            sum = 0;
            sumSquares = 0;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                sum += factionStat.Value.factionMassLost;
            }

            massLostMean = sum / numFactions;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                diff = factionStat.Value.factionMassLost - massLostMean;
                sumSquares += Mathf.Pow(diff, 2);
            }

            variance = sumSquares / (numFactions - 1);

            massLostStdDev = Mathf.Sqrt(variance);

            // calculate DamageCaused history stat
            sum = 0;
            sumSquares = 0;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                sum += factionStat.Value.factionDamageCaused;
            }

            damageCausedMean = sum / numFactions;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                diff = factionStat.Value.factionDamageCaused - damageCausedMean;
                sumSquares += Mathf.Pow(diff, 2);
            }

            variance = sumSquares / (numFactions - 1);

            damageCausedStdDev = Mathf.Sqrt(variance);

            // calculate DamageTaken history stat
            sum = 0;
            sumSquares = 0;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                sum += factionStat.Value.factionDamageTaken;
            }

            damageTakenMean = sum / numFactions;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                diff = factionStat.Value.factionDamageTaken - damageTakenMean;
                sumSquares += Mathf.Pow(diff, 2);
            }

            variance = sumSquares / (numFactions - 1);

            damageTakenStdDev = Mathf.Sqrt(variance);

            // calculate KillParticipation history stat
            sum = 0;
            sumSquares = 0;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                sum += factionStat.Value.factionKillParticipation;
            }

            killParticipationMean = sum / numFactions;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                diff = factionStat.Value.factionKillParticipation - killParticipationMean;
                sumSquares += Mathf.Pow(diff, 2);
            }

            variance = sumSquares / (numFactions - 1);

            killParticipationStdDev = Mathf.Sqrt(variance);

            // calculate AverageSurvivalTime history stat

            sum = 0;
            sumSquares = 0;

            float lowestAST = Mathf.Infinity;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                if (factionStat.Key == "ZERO") continue;

                if (factionStat.Value.factionAST < lowestAST) lowestAST = factionStat.Value.factionAST;

                sum += factionStat.Value.factionAST;
            }

            averageSurvivalTimeMean = sum / numFactions;

            zero.factionAST = lowestAST;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                diff = factionStat.Value.factionAST - averageSurvivalTimeMean;
                sumSquares += Mathf.Pow(diff, 2);
            }

            variance = sumSquares / (numFactions - 1);

            averageSurvivalTimeStdDev = Mathf.Sqrt(variance);

            // calculate KAD history stat
            sum = 0;
            sumSquares = 0;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                sum += factionStat.Value.factionKAD;
            }

            KADMean = sum / numFactions;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                diff = factionStat.Value.factionKAD - KADMean;
                sumSquares += Mathf.Pow(diff, 2);
            }

            variance = sumSquares / (numFactions - 1);

            KADStdDev = Mathf.Sqrt(variance);

            // calculate DAM history stat
            sum = 0;
            sumSquares = 0;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                sum += factionStat.Value.factionDAM;
            }

            DAMMean = sum / numFactions;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                diff = factionStat.Value.factionDAM - DAMMean;
                sumSquares += Mathf.Pow(diff, 2);
            }

            variance = sumSquares / (numFactions - 1);

            DAMStdDev = Mathf.Sqrt(variance);

            // calculate MAS history stat
            sum = 0;
            sumSquares = 0;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                sum += factionStat.Value.factionMAS;
            }

            MASMean = sum / numFactions;

            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                diff = factionStat.Value.factionMAS - MASMean;
                sumSquares += Mathf.Pow(diff, 2);
            }

            variance = sumSquares / (numFactions - 1);

            MASStdDev = Mathf.Sqrt(variance);
            #endregion

            // calculate z-scores and weighted stats for all combat stats
            foreach (KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                FactionCombatStatsZScored zStat = new FactionCombatStatsZScored(factionStat.Value);

                zStat.factionKillsZ = calculateZScore(factionStat.Value.factionKills, killsMean, killsStdDev);
                zStat.factionAssistsZ = calculateZScore(factionStat.Value.factionAssists, assistsMean, assistsStdDev);
                zStat.factionDeathsZ = calculateZScore(factionStat.Value.factionDeaths, deathsMean, deathsStdDev);
                zStat.factionModulesDestroyedZ = calculateZScore(factionStat.Value.factionModulesDestroyed, modsKilledMean, modsKilledStdDev);
                zStat.factionModulesLostZ = calculateZScore(factionStat.Value.factionModulesLost, modsLostMean, modsLostStdDev);
                zStat.factionMassKilledZ = calculateZScore(factionStat.Value.factionMassKilled, massKilledMean, massKilledStdDev);
                zStat.factionMassLostZ = calculateZScore(factionStat.Value.factionMassLost, massLostMean, massLostStdDev);
                zStat.factionDamageCausedZ = calculateZScore(factionStat.Value.factionDamageCaused, damageCausedMean, damageCausedStdDev);
                zStat.factionDamageTakenZ = calculateZScore(factionStat.Value.factionDamageTaken, damageTakenMean, damageTakenStdDev);
                zStat.factionKillParticipationZ = calculateZScore(factionStat.Value.factionKillParticipation, killParticipationMean, killParticipationStdDev);
                zStat.factionAverageSurvivalTimeZ = calculateZScore(factionStat.Value.factionAST, averageSurvivalTimeMean, averageSurvivalTimeStdDev);
                zStat.factionKADZ = calculateZScore(factionStat.Value.factionKAD, KADMean, KADStdDev);
                zStat.factionDAMZ = calculateZScore(factionStat.Value.factionDAM, DAMMean, DAMStdDev);
                zStat.factionMASZ = calculateZScore(factionStat.Value.factionMAS, MASMean, MASStdDev);

                factionStatsZScored.Add(factionStat.Key, zStat);

                FactionCombatStatsWeighted weightedStat = new FactionCombatStatsWeighted(zStat);
                weightedStat.factionAverageSurvivalTimePC = ASTPercent * zStat.factionAverageSurvivalTimeZ;
                weightedStat.factionKADPC = KADPercent * zStat.factionKADZ;
                weightedStat.factionMASRatioPC = MASPercent * zStat.factionMASZ;
                weightedStat.factionDAMRatioPC = DAMPercent * zStat.factionDAMZ;

                weightedStat.calculateTotalScore();                

                factionStatsWeighted.Add(factionStat.Key, weightedStat);                
            }
            
            FactionCombatStatsWeighted zeroPC;
            factionStatsWeighted.TryGetValue("ZERO", out zeroPC);

            if (zeroPC != null)
            {
                foreach (KeyValuePair<string, FactionCombatStatsWeighted> factionWeightedStat in factionStatsWeighted)
                {
                    factionWeightedStat.Value.factionAverageSurvivalTimePC += zeroPC.factionAverageSurvivalTimePC;
                    factionWeightedStat.Value.factionKADPC += zeroPC.factionKADPC;
                    factionWeightedStat.Value.factionMASRatioPC += zeroPC.factionMASRatioPC;
                    factionWeightedStat.Value.factionDAMRatioPC += zeroPC.factionDAMRatioPC;
                    factionWeightedStat.Value.totalScore += zeroPC.totalScore;
                }
            }

            float maxKADPC = -Mathf.Infinity;
            float maxMASPC = -Mathf.Infinity;
            float maxDAMPC = -Mathf.Infinity;
            float maxASTPC = -Mathf.Infinity;
            float maxTotalScore = -Mathf.Infinity;

            foreach (KeyValuePair<string, FactionCombatStatsWeighted> factionWeightedStat in factionStatsWeighted)
            {
                if (factionWeightedStat.Value.factionKADPC > maxKADPC) maxKADPC = factionWeightedStat.Value.factionKADPC;
                if (factionWeightedStat.Value.factionMASRatioPC > maxMASPC) maxMASPC = factionWeightedStat.Value.factionMASRatioPC;
                if (factionWeightedStat.Value.factionDAMRatioPC > maxDAMPC) maxDAMPC = factionWeightedStat.Value.factionDAMRatioPC;
                if (factionWeightedStat.Value.factionAverageSurvivalTimePC > maxASTPC) maxASTPC = factionWeightedStat.Value.factionAverageSurvivalTimePC;
                if (factionWeightedStat.Value.totalScore > maxTotalScore) maxTotalScore = factionWeightedStat.Value.totalScore;
            }

            foreach (KeyValuePair<string, FactionCombatStatsWeighted> factionWeightedStat in factionStatsWeighted)
            {
                factionWeightedStat.Value.scaleFactionKADPC(KADPercent, maxKADPC);
                factionWeightedStat.Value.scaleFactionMASRatioPC(MASPercent, maxMASPC);
                factionWeightedStat.Value.scaleFactionDAMRatioPC(DAMPercent, maxDAMPC);
                factionWeightedStat.Value.scaleFactionAverageSurvivalTimePC(ASTPercent, maxASTPC);
                factionWeightedStat.Value.scaleTotalScore(maxTotalScore);
            }

            foreach(KeyValuePair<string, FactionCombatStats> factionStat in factionStats)
            {
                FactionCombatStatsWeighted weightedStats;

                factionStatsWeighted.TryGetValue(factionStat.Key, out weightedStats);

                factionStat.Value.scaledTotalScore = weightedStats.scaledTotalScore;
            }

            combatStatsSorted = factionStats.OrderByDescending(x => x.Value.scaledTotalScore).Take(numFactions).ToDictionary(x => x.Key, x => x.Value);
            factionStatsSorted = factionStatsWeighted.OrderByDescending(x => x.Value.scaledTotalScore).Take(numFactions).ToDictionary(x => x.Key, x => x.Value);
        }

        protected void generateResults()
        {
            string classnameStr = this.GetType().Name;

            Dictionary<string, FactionCombatStats> factionStats = (Gui as TeamDeathMatchGUI).getTDMFactions();

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

            foreach(KeyValuePair<string, FactionCombatStats> entry in factionStats)
            {
                sw.WriteLine("\nFaction (" + entry.Value.factionID + "): " + entry.Value.factionName + "\n");
                sw.WriteLine("Total Cost: " + entry.Value.factionTotalCost);
                sw.WriteLine("Average Survival Time: " + entry.Value.factionAST);
                sw.WriteLine("Kills: " + entry.Value.factionKills);
                sw.WriteLine("Deaths: " + entry.Value.factionDeaths);
                sw.WriteLine("Assists: " + entry.Value.factionAssists);
                sw.WriteLine("Modules Destroyed: " + entry.Value.factionModulesDestroyed);
                sw.WriteLine("Modules Lost: " + entry.Value.factionModulesLost);
                sw.WriteLine("Mass Killed: " + entry.Value.factionMassKilled);
                sw.WriteLine("Mass Lost: " + entry.Value.factionMassLost);
                sw.WriteLine("Damage Caused: " + entry.Value.factionDamageCaused);
                sw.WriteLine("Damage Taken: " + entry.Value.factionDamageTaken);

                sw.WriteLine("KAD: " + entry.Value.factionKAD);
                sw.WriteLine("DAM: " + entry.Value.factionDAM);
                sw.WriteLine("MAS: " + entry.Value.factionMAS);

                FactionCombatStatsZScored entryZ;

                factionStatsZScored.TryGetValue(entry.Key, out entryZ);

                if (entryZ != null)
                {
                    sw.WriteLine("-----------Z-SCORES---------");
                    sw.WriteLine("Average Survival Time: " + entryZ.factionAverageSurvivalTimeZ);
                    sw.WriteLine("Kills: " + entryZ.factionKillsZ);
                    sw.WriteLine("Deaths: " + entryZ.factionDeathsZ);
                    sw.WriteLine("Assists: " + entryZ.factionAssistsZ);
                    sw.WriteLine("Modules Destroyed: " + entryZ.factionModulesDestroyedZ);
                    sw.WriteLine("Modules Lost: " + entryZ.factionModulesLostZ);
                    sw.WriteLine("Mass Killed: " + entryZ.factionMassKilledZ);
                    sw.WriteLine("Mass Lost: " + entryZ.factionMassLostZ);
                    sw.WriteLine("Damage Caused: " + entryZ.factionDamageCausedZ);
                    sw.WriteLine("Damage Taken: " + entryZ.factionDamageTakenZ);

                    sw.WriteLine("KAD: " + entryZ.factionKADZ);
                    sw.WriteLine("DAM: " + entryZ.factionDAMZ);
                    sw.WriteLine("MAS: " + entryZ.factionMASZ);

                    FactionCombatStatsWeighted entryPC;

                    factionStatsWeighted.TryGetValue(entry.Key, out entryPC);

                    if (entryPC != null)
                    {
                        sw.WriteLine("--------WEIGHTED--------");
                        sw.WriteLine("Average Survival Time PC: " + entryPC.factionAverageSurvivalTimePC);
                        sw.WriteLine("KAD: " + entryPC.factionKADPC);
                        sw.WriteLine("DAM: " + entryPC.factionDAMRatioPC);
                        sw.WriteLine("MAS: " + entryPC.factionMASRatioPC);

                        sw.WriteLine("--------TOTALS--------");
                        sw.WriteLine("Total: " + entryPC.totalScore);
                        sw.WriteLine("Scaled Total: " + entryPC.scaledTotalScore);
                    }
                }
            }

            sw.WriteLine("-----------ALL COMBATEERS---------------");

            foreach (CombatStats stat in combateerStats)
            {
                sw.WriteLine("Ship: " + stat.structure.name);

                if (stat.structure.Command != null)
                {
                    sw.WriteLine("Commanded By: " + stat.structure.Command.rankData.abbreviation + " " + stat.structure.Command.label);
                }

                sw.WriteLine("Faction: " + stat.structure.Faction.label);

                if (stat.structure.Stats != null)
                {
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

            combateerStats = new List<CombatStats>();
            factionStats = new Dictionary<string, FactionCombatStats>();
            factionStatsZScored = new Dictionary<string, FactionCombatStatsZScored>();
            factionStatsWeighted = new Dictionary<string, FactionCombatStatsWeighted>();

            List<Ship> inactiveShips = GameObject.Find("Placeables").FindInactive<Ship>();

            foreach (Structure structure in inactiveShips)
            {
                Ship ship = structure as Ship;

                positionStructure(structure);

                ship.Call_WarpIn(this, new WarpEventArgs(ship.gameObject, getScenarioFolder(), null, ship.transform.position, ship.transform.rotation.eulerAngles.z));

                if (ship.Classification == ShipClassification.FIGHTER || ship.Classification == ShipClassification.BOMBER)
                {
                    ship.NotifyKilled += OnTinyKilled;
                }

                structure.spawnedIn = true;
            }

            // attempt to start the match
            if (ReadyToStartMatch() == true)
            {
                // initialise the GUI here
                if (Gui != null)
                {
                    Gui.init();
                }

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

            List<Structure> structures = GameManager.Instance.getAllStructures();

            foreach (Structure structure in structures)
            {
                initController(structure);

                Ship ship = structure as Ship;

                if (ship != null)
                {
                    if (ship.structureSize == StructureSize.TINY)
                    {
                        BasicSquadronAI squadronController = ship.GetComponent<BasicSquadronAI>();

                        if (squadronController != null)
                        {
                            squadronController.initController();
                        }
                    }
                }
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

            TeamDeathMatchGUI TDMGui = Gui as TeamDeathMatchGUI;

            TDMGui.leftPanel.SetActive(false);
            TDMGui.rightPanel.SetActive(false);

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

        public virtual void TeamDeathMatch_Spawn(object sender, SpawnEventArgs args)
        {
            Structure structure = args.spawnedStructure;

            combateerStats.Add(new CombatStats(structure.Stats));

            FactionCombatStats factionStat;

            if (factionStats.ContainsKey(structure.Faction.label) == false)
            {
                factionStat = new FactionCombatStats(structure); // note: this adds all structure stats to the faction
                factionStats.Add(structure.Faction.label, factionStat);
            }
            else
            {
                factionStats.TryGetValue(structure.Faction.label, out factionStat);

                if (factionStat != null)
                {
                    factionStat.factionStructures.Add(structure);
                    factionStat.factionTotalCost += structure.TotalCost;
                }
            }
        }

        public virtual void TeamDeathMatch_Respawn(object sender, RespawnEventArgs args)
        {
            if (args.respawnedStructure != null)
            {
                positionStructure(args.respawnedStructure);
            }
        }

        public virtual void TeamDeathMatch_InstigatedAnyDamage(object sender, DamageEventArgs args)
        {
            if (args.damageCauser != null)
            {
                int statsIndex = combateerStats.FindIndex(s => s.structure == args.damageCauser);

                if (statsIndex >= 0)
                {
                    combateerStats[statsIndex].damageCaused += args.damage;

                    FactionCombatStats factionStat;
                    factionStats.TryGetValue(args.damageCauser.Faction.label, out factionStat);

                    if (factionStat != null)
                    {
                        factionStat.factionDamageCaused += args.damage;
                    }
                }
            }
        }

        public virtual void TeamDeathMatch_TakenAnyDamage(object sender, DamageEventArgs args)
        {
            if (args.damagedStructure != null)
            {
                int statsIndex = combateerStats.FindIndex(s => s.structure == args.damagedStructure);

                if (statsIndex >= 0)
                {
                    combateerStats[statsIndex].damageTaken += args.damage;

                    FactionCombatStats factionStat;
                    factionStats.TryGetValue(args.damagedStructure.Faction.label, out factionStat);

                    if (factionStat != null)
                    {
                        factionStat.factionDamageTaken += args.damage;
                    }
                }
            }
        }

        public virtual void TeamDeathMatch_NotifyKiller(object sender, TargetDestroyedEventArgs args)
        {
            if (args.attacker != null)
            {
                int statsIndex = combateerStats.FindIndex(s => s.structure == args.attacker);

                if (statsIndex >= 0)
                {
                    combateerStats[statsIndex].kills++;
                    combateerStats[statsIndex].massKill += args.structureAttacked.MaxHullStrength;

                    FactionCombatStats factionStat;
                    factionStats.TryGetValue(args.attacker.Faction.label, out factionStat);

                    if (factionStat != null)
                    {
                        if (args.structureAttacked.structureSize == StructureSize.TINY)
                        {
                            factionStat.factionKills += 0.2f;
                        }
                        else { factionStat.factionKills++; }

                        factionStat.factionMassKilled += args.structureAttacked.MaxHullStrength;
                    }
                }
            }
        }

        public virtual void TeamDeathMatch_NotifyAssister(object sender, AssistEventArgs args)
        {
            if (args.assister != null)
            {
                int statsIndex = combateerStats.FindIndex(s => s.structure == args.assister);

                if (statsIndex >= 0)
                {
                    combateerStats[statsIndex].assists++;

                    FactionCombatStats factionStat;
                    factionStats.TryGetValue(args.assister.Faction.label, out factionStat);

                    if (factionStat != null)
                    {
                        factionStat.factionAssists++;
                        factionStat.factionKillParticipation += args.killParticipation;
                    }
                }
            }
        }

        public virtual void TeamDeathMatch_NotifyKilled(object sender, TargetDestroyedEventArgs args)
        {
            if (args.structureAttacked != null)
            {
                int statsIndex = combateerStats.FindIndex(s => s.structure == args.structureAttacked);

                if (statsIndex >= 0)
                {
                    combateerStats[statsIndex].deaths++;
                    combateerStats[statsIndex].massLoss += args.structureAttacked.MaxHullStrength;

                    FactionCombatStats factionStat;
                    factionStats.TryGetValue(args.structureAttacked.Faction.label, out factionStat);

                    if (factionStat != null)
                    {
                        if (args.structureAttacked.structureSize == StructureSize.TINY)
                        {
                            factionStat.factionDeaths += 0.2f;
                        }
                        else { factionStat.factionDeaths++; }
                        
                        factionStat.factionMassLost += args.structureAttacked.MaxHullStrength;
                    }
                }
            }
        }

        public virtual void TeamDeathMatch_ModuleDamaged(object sender, ModuleDamageEventArgs args)
        {
            if (args.moduleHit != null && args.moduleAttacker != null)
            {
                if (args.destroyed == true)
                {
                    int statsIndex = combateerStats.FindIndex(s => s.structure == args.moduleAttacker);

                    if (statsIndex >= 0)
                    {
                        combateerStats[statsIndex].modulesDestroyed++;

                        FactionCombatStats factionStat;
                        factionStats.TryGetValue(args.moduleAttacker.Faction.label, out factionStat);

                        if (factionStat != null)
                        {
                            factionStat.factionModulesDestroyed++;
                            factionStat.factionMassKilled += args.moduleHit.ModuleData.MaxArmour;
                        }

                        factionStats.TryGetValue(args.moduleOwner.Faction.label, out factionStat);

                        if (factionStat != null)
                        {
                            factionStat.factionModulesLost++;
                            factionStat.factionMassLost += args.moduleHit.ModuleData.MaxArmour;
                        }
                    }
                }
            }
        }

        public virtual void TeamDeathMatch_SurvivalTimeUpdated(object sender, SurvivalTimeEventArgs args)
        {
            if (args.updatedStructure != null)
            {
                int statsIndex = combateerStats.FindIndex(s => s.structure == args.updatedStructure);

                if (statsIndex >= 0)
                {
                    combateerStats[statsIndex].averageSurvivalTime = args.updatedSurvivalTime;

                    FactionCombatStats factionStat;
                    factionStats.TryGetValue(args.updatedStructure.Faction.label, out factionStat);

                    if (factionStat != null)
                    {
                        factionStat.updateFactionAverageSurvivalTime(args);
                    }
                }
            }
        }

        public void OnTinyKilled(object sender, TargetDestroyedEventArgs args)
        {
            Ship tiny = args.structureAttacked as Ship;

            if (tiny != null)
            {
                FactionData faction = FactionManager.Instance.findFaction(tiny.Faction.ID);
                /*
                List<Ship> ships = faction.FleetManager.getAllShips();

                foreach (Ship ship in ships)
                {
                    if (ship.Classification == ShipClassification.CARRIER)
                    {
                        IHangar hangar = ship.getSocket<IHangar>() as IHangar;

                        if (hangar != null)
                        {
                            hangar.addShip(tiny);
                            break;
                        }
                    }
                }
                */

                BasicSquadronAI squadronAI = tiny.GetComponent<BasicSquadronAI>();

                if (squadronAI != null)
                {
                    IHangar hangar = squadronAI.getHangar();

                    // TODO - should not need this check
                    if (!hangar.getShipsInHangar().Contains(tiny))
                    {
                        // add ship to hangar
                        hangar.addShip(tiny);

                        // check if the carrier is dead
                        if (squadronAI.getHangarStructure().Destroyed == false)
                        {
                            SquadronData squadron = faction.FleetManager.findSquadronData(tiny.FleetData.ID, tiny.WingData.ID, tiny.SquadronData.ID);

                            // respawn the squadron from the hangar
                            if (squadron != null && squadron.MembersAlive == 0 && squadron.CanRespawn == true)
                            {
                                foreach (Ship ship in squadron.ships)
                                {
                                    if (hangar.requestLaunch(ship))
                                    {
                                        squadron.MembersAlive++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        #endregion        
    }
}