using UnityEngine;

namespace NoxCore.Data
{
    [CreateAssetMenu(fileName = "CommanderData Details", menuName = "ScriptableObjects/CommanderData")]
    
    public class CommanderData : ScriptableObject
    {
        public Texture2D portrait;
        public string label;
        public RankData rankData;
        public uint credits;
    }
}