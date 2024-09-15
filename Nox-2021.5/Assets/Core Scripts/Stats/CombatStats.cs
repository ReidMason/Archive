using UnityEngine;

using NoxCore.Placeables;

namespace NoxCore.Stats
{
	public class CombatStats
	{
		public Structure structure;
		public float averageSurvivalTime;
		public int kills;
		public int assists;
		public int deaths;
		public int modulesDestroyed, modulesLost;
		public float damageCaused;
		public float damageTaken;
        public float massKill;
        public float massLoss;
		
		public static int maxCombateerNameLength = 20;
		
		public CombatStats(StructureStats structureStats)
		{
            if (structureStats != null)
            {
                this.structure = structureStats.structure;
                this.averageSurvivalTime = structureStats.averageSurvivalTime;
                this.kills = structureStats.numKills;
                this.assists = structureStats.numAssists;
                this.deaths = structureStats.numDeaths;
                this.modulesDestroyed = structureStats.numModulesDestroyed;
                this.damageCaused = structureStats.totalDamageInflicted;
                this.damageTaken = structureStats.totalDamageTaken;

                if (this.structure.name.Length > CombatStats.maxCombateerNameLength)
                {
                    CombatStats.maxCombateerNameLength = Mathf.Min(20, this.structure.name.Length);
                }
            }
		}
	}
}

