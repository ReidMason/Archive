using UnityEngine;

using NoxCore.Data.Placeables;

namespace NoxCore.Placeables.Ships
{
    public abstract class Shuttle : Ship
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            // set ship classification properties
            Classification = ShipClassification.SHUTTLE;
            structureSize = StructureSize.SMALL;

            base.init(noxObjectData);
            /*
            StructureInfo.aspectRadius = 6.0f;

            MaxForce = 85000;
            MaxTurnRate = 55.0f * Mathf.Deg2Rad;    // turn rate (degrees per second) converted into radians

            StructureInfo.mass = 900;*/
        }
    }
}