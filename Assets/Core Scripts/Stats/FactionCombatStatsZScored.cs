namespace NoxCore.Stats
{
    public class FactionCombatStatsZScored
    {
        public int factionID;
        public string factionName;
        public float factionAverageSurvivalTimeZ;
        public float factionKillsZ;
        public float factionAssistsZ;
        public float factionDeathsZ;
        public float factionModulesDestroyedZ;
        public float factionModulesLostZ;
        public float factionDamageCausedZ;
        public float factionDamageTakenZ;
        public float factionMassKilledZ;
        public float factionMassLostZ;
        public float factionKillParticipationZ;

        public float factionKADZ;
        public float factionMASZ;
        public float factionDAMZ;

        public FactionCombatStatsZScored(FactionCombatStats factionCombatStats)
        {
            factionID = factionCombatStats.factionID;
            factionName = factionCombatStats.factionName;
        }
    }
}

