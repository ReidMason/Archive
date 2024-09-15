using UnityEngine;
using System.Collections.Generic;

using NoxCore.Data;
using NoxCore.Placeables;

namespace NoxCore.Managers
{
    public class FactionManager : MonoBehaviour
    {
        [SerializeField] private List<FactionData> _factions = new List<FactionData>();
        public List<FactionData> Factions { get { return _factions; } }

        /// <summary>
        ///   Provide singleton support for this class.
        ///   The script must still be attached to a game object, but this will allow it to be called
        ///   from anywhere without specifically identifying that game object.
        /// </summary>
        private static FactionManager instance;
        public static FactionManager Instance { get { return instance; } set { instance = value; } }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;              
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        void init()
        {
            DontDestroyOnLoad(gameObject);
        }

        public bool addFaction(FactionData faction)
        {
            if (!Factions.Exists(x => x.ID == faction.ID) && !Factions.Exists(x => x.label == faction.label) && !Factions.Exists(x => x.abbreviation == faction.abbreviation))
            {
                Factions.Add(faction);

                faction.Stations = new List<Structure>();
                faction.FleetManager = new FleetDataManager();

                return true;
            }

            return false;
        }

        public bool removeFaction(int id)
        {
            return Factions.RemoveAll(faction => faction.ID == id) > 0;
        }

        public bool removeFaction(string label)
        {
            return Factions.RemoveAll(faction => faction.label == label) > 0;
        }

        public FactionData findFaction(int factionID)
        {
            return Factions.Find(x => x.ID == factionID);
        }

        public FactionData findFaction(string factionName)
        {
            return Factions.Find(x => x.name == factionName);
        }
    }
}
