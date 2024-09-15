using UnityEngine;
using System.Collections.Generic;

using NoxCore.Data;
using NoxCore.Managers;
using NoxCore.Placeables;

namespace NoxCore.Stats
{
	public class FactionCombatStats
	{
		public int factionID;
		public string factionName;
		public uint factionTotalCost;
		public List<Structure> factionStructures;
		protected List<float> survivalTimes = new List<float>();

		public float factionKills;
		public int factionAssists;
		public float factionDeaths;
		public int factionModulesDestroyed;
        public int factionModulesLost;
        public float factionDamageCaused;
		public float factionDamageTaken;
        public float factionMassKilled;
        public float factionMassLost;
        public float factionKillParticipation;

        public float factionKAD;
        public float factionDAM;
        public float factionMAS;
        public float factionAST;

        public float scaledTotalScore;

		public static int maxFactionNameLength;
		
		public FactionCombatStats(Structure structure)
		{
            if (structure != null)
            {
                factionID = structure.Faction.ID;
                factionName = structure.Faction.label;

                factionTotalCost += structure.TotalCost;

                factionStructures = new List<Structure>();
                factionStructures.Add(structure);

                if (structure.Faction.label.Length > FactionCombatStats.maxFactionNameLength)
                {
                    FactionCombatStats.maxFactionNameLength = Mathf.Min(20, structure.Faction.label.Length);
                }
            }
            else
            {
                factionID = -9999;
                factionName = "ZERO";
            }
		}
		
		public void updateFactionAverageSurvivalTime(SurvivalTimeEventArgs args)
		{
			survivalTimes.Add(args.updatedSurvivalTime);
		
			// calculate current average survival time
			float totalSurvivalTimes = 0;
			
			foreach(float survivalTime in survivalTimes)
			{
				totalSurvivalTimes += survivalTime;
			}

            FactionData faction = FactionManager.Instance.findFaction(args.updatedStructure.Faction.ID);

            if (faction != null)
            {
                // note: add one to numDeaths for accuracy since still alive at end of combat round
                if (args.combatFinished == false)
                {
                    factionAST = (totalSurvivalTimes / factionDeaths) / faction.FriendlyStructures.Count;
                }
                else
                {
                    factionAST = (totalSurvivalTimes / (factionDeaths + 1)) / faction.FriendlyStructures.Count;
                }
            }
		}
	}
}

