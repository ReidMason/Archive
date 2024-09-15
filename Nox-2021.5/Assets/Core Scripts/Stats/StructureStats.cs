using System.Collections.Generic;

using NoxCore.Placeables;

namespace NoxCore.Stats
{
	public class StructureStats : Stats 
	{
		public Structure structure;

		public int numKills;
		public int numDeaths;
		public int numAssists;

		public float totalMassKilled, totalMassLost;

		public float damageThisLife, actualDamageThisLife;

		public int numModulesDestroyed, numModulesLost;

		public float totalHullDamageTaken, totalShieldDamageTaken, totalArmourDamageTaken;
		public float totalDamageInflicted, totalDamageTaken;
        public List<(Structure structure, float damage)> assistList = new List<(Structure, float)>();
		public float averageSurvivalTime;
		public List<float> survivalTimes = new List<float>();		
	}
}