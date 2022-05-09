using UnityEngine;

using NoxCore.Placeables;

namespace NoxCore.Builders
{
    public class StationBuilder : StructureBuilder
    {
        void Awake()
        {
            setBuildType(BuildType.STATION);
        }
    }
}