namespace NoxCore.Stats
{
    public class FactionCombatStatsWeighted
    {
        public int factionID;
        public string factionName;
        public int ranking;

        public float factionKADPC;
        public float factionMASRatioPC;
        public float factionDAMRatioPC;
        public float factionAverageSurvivalTimePC;

        public float scaledFactionKADPC;
        public float scaledFactionMASRatioPC;
        public float scaledFactionDAMRatioPC;
        public float scaledFactionAverageSurvivalTimePC;

        public float totalScore;
        public float scaledTotalScore;

        public FactionCombatStatsWeighted(FactionCombatStatsZScored factionCombatStatsZScored)
        {
            factionID = factionCombatStatsZScored.factionID;
            factionName = factionCombatStatsZScored.factionName;
        }

        public void scaleFactionKADPC(float percent, float maxTotalScore)
        {
            scaledFactionKADPC = (factionKADPC / maxTotalScore) * percent;
        }

        public void scaleFactionMASRatioPC(float percent, float maxTotalScore)
        {
            scaledFactionMASRatioPC = (factionMASRatioPC / maxTotalScore) * percent;
        }

        public void scaleFactionDAMRatioPC(float percent, float maxTotalScore)
        {
            scaledFactionDAMRatioPC = (factionDAMRatioPC / maxTotalScore) * percent;
        }

        public void scaleFactionAverageSurvivalTimePC(float percent, float maxTotalScore)
        {
            scaledFactionAverageSurvivalTimePC = (factionAverageSurvivalTimePC / maxTotalScore) * percent;
        }

        public void calculateTotalScore()
        {
            totalScore = factionAverageSurvivalTimePC + factionKADPC + factionMASRatioPC + factionDAMRatioPC;
        }

        public void scaleTotalScore(float maxTotalScore)
        {
            scaledTotalScore = (totalScore / maxTotalScore) * 100;
        }
    }
}
