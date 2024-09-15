using UnityEngine;

using NoxCore.Data.Placeables;

namespace NoxCore.Placeables.Ships
{
    public abstract class Frigate : Ship
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            // set ship classification properties
            Classification = ShipClassification.FRIGATE;
            structureSize = StructureSize.SMALL;

            base.init(noxObjectData);

            //MaxForce = 60000;
            //MaxTurnRate = 30.0f * Mathf.Deg2Rad;    // turn rate (degrees per second) converted into radians

            //StructureInfo.mass = 1700;
        }
    }
}