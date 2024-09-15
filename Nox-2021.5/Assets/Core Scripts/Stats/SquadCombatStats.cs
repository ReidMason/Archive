using UnityEngine;
using System.Collections.Generic;

using NoxCore.Data;
using NoxCore.Managers;
using NoxCore.Placeables;

namespace NoxCore.Stats
{
    public class SquadCombatStats
    {
        public int squadID;
        public string squadName;
        public uint squadTotalCost;
        public List<Structure> squadStructures;
        protected List<float> survivalTimes = new List<float>();
        public float squadAverageSurvivalTime;
        public int squadKills;
        public int squadAssists;
        public int squadDeaths;
        public int squadModulesDestroyed;
        public int squadModulesLost;
        public float squadDamageCaused;
        public float squadDamageTaken;
        public float squadMassKilled;
        public float squadMassLost;
        public float squadKillParticipation;

        public bool killedStation;

        public float squadKAD;
        public float squadDAM;
        public float squadMAS;

        public float stationKillTime;
        public int numRespawns;
        public float firstKillTime;

        public static int maxSquadNameLength;

        public SquadCombatStats(Structure structure)
        {
            if (structure != null)
            {
                squadID = structure.Faction.ID;
                squadName = structure.Faction.label;

                squadTotalCost += structure.TotalCost;

                squadStructures = new List<Structure>();
                squadStructures.Add(structure);

                if (structure.Faction.label.Length > SquadCombatStats.maxSquadNameLength)
                {
                    SquadCombatStats.maxSquadNameLength = Mathf.Min(20, structure.Faction.label.Length);
                }
            }
            else
            {
                squadID = -9999;
                squadName = "ZERO";
            }
        }

        public void updateSquadAverageSurvivalTime(SurvivalTimeEventArgs args)
        {
            survivalTimes.Add(args.updatedSurvivalTime);

            // calculate current average survival time
            float totalSurvivalTimes = 0;

            foreach (float survivalTime in survivalTimes)
            {
                totalSurvivalTimes += survivalTime;
            }

            FactionData squad = FactionManager.Instance.findFaction(args.updatedStructure.Faction.ID);

            if (squad != null)
            {
                // note: add one to numDeaths for accuracy since still alive at end of combat round
                if (args.combatFinished == false)
                {
                    squadAverageSurvivalTime = (totalSurvivalTimes / squadDeaths) / squad.FleetManager.getNumShips();
                }
                else
                {
                    squadAverageSurvivalTime = (totalSurvivalTimes / (squadDeaths + 1) / squad.FleetManager.getNumShips());
                }
            }
        }
    }
}

