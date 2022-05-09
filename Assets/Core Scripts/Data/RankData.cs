using UnityEngine;

namespace NoxCore.Data
{
    [CreateAssetMenu(fileName = "RankData Details", menuName = "ScriptableObjects/RankData")]

    public class RankData : ScriptableObject
    {
        public string rank;
        public string abbreviation;
        public Texture2D insignia;
        public uint rankingCredits;
    }
}