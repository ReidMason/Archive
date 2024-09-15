using UnityEngine;

using System.Collections.Generic;

using NoxCore.Managers;
using NoxCore.Placeables;

namespace NoxCore.Data
{
    [CreateAssetMenu(fileName = "Faction Details", menuName = "ScriptableObjects/Faction")]
    public class FactionData : ScriptableObject
    {
        public string label, abbreviation;
        public int ID;
        public Color primaryColour, secondaryColour;
        public Texture2D largeInsignia, smallInsignia;

        [SerializeField] protected List<Structure> _stations;
        public List<Structure> Stations { get { return _stations; } set { _stations = value; } }

        [SerializeField] protected FleetDataManager _fleetManager;
        public FleetDataManager FleetManager { get { return _fleetManager; } set { _fleetManager = value; } }

        // note - could have an army manager and/or naval too

        [SerializeField] protected List<Structure> _enemyStructures;
        public List<Structure> EnemyStructures { get { return _enemyStructures; } set { _enemyStructures = value; } }

        [SerializeField] protected List<Structure> _neutralStructures;
        public List<Structure> NeutralStructures { get { return _neutralStructures; } set { _neutralStructures = value; } }

        [SerializeField] protected List<Structure> _friendlyStructures;
        public List<Structure> FriendlyStructures { get { return _friendlyStructures; } set { _friendlyStructures = value; } }

        public void swapFaction(Structure structure, FactionData from, FactionData to)
        {
            from.FriendlyStructures.Remove(structure);
            from.EnemyStructures.Add(structure);

            to.FriendlyStructures.Add(structure);
            to.EnemyStructures.Remove(structure);
        }
    }
}
