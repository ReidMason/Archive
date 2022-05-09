using UnityEngine;
using System.Collections.Generic;

using NoxCore.Helm;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Utilities;

namespace NoxCore.Builders
{
    public class ShipBuilder : StructureBuilder
    {
        public bool overrideIsSquadronData;
        public int overrideNumShips;
        public List<Vector2> overrideFormationPositions = new List<Vector2>();
        public List<string> overridePilotNames = new List<string>();

        public int overrideFleetDataID;
        public int overrideWingDataID;
        public int overrideSquadronDataID;

        public float overrideThrottle;

        void Awake()
        {
            setBuildType(BuildType.SHIP);
            overrideThrottle = 1.0f;        // assume full throttle on spawn in
        }

        public override void initialise(Structure shipStructure)
        {
            base.initialise(shipStructure);

            //(shipStructure as Ship).setFleetDataInfo(overrideFleetDataID, overrideWingDataID, overrideSquadronDataID);
        }

        public void changeSquadronDataSize()
        {
            overrideFormationPositions.Resize(overrideNumShips);
            overridePilotNames.Resize(overrideNumShips, "");

            for(int i = 0; i < overridePilotNames.Count; i++)
            {
                if (overridePilotNames[i].Length == 0) overridePilotNames[i] = overrideStructureCommand.label + " " + (i+1);
            }
        }

        /*
        public override Structure setupStructure(GameObject go)
        {
            Ship shipStructure = go.GetComponent<Ship>();

            if (overrideStructureType != null && overrideStructureType != shipStructure.GetType())
            {
                // remove existing structure from new GameObject (note: not the prefab)
                if (shipStructure != null) Destroy(shipStructure);

                // add overridden structure
                shipStructure = go.AddComponent(overrideStructureType) as Ship;
            }

            if (shipStructure == null)
            {
                D.log("Structure", "No structure component added to the prefab at: " + prefabPath);
                Destroy(go);
                return null;
            }
            else
            {
                initialise(shipStructure);
            }

            return shipStructure;
        }
        */
        public virtual HelmController setupHelm(GameObject go, Ship shipStructure)
        {
            HelmController helm = null;

            helm = go.AddComponent<HelmController>();

            if (helm != null)
            {
                shipStructure.attachHelmController(helm);

                helm.desiredThrottle = overrideThrottle;

                helm.init();
            }

            return helm;
        }
    }
}