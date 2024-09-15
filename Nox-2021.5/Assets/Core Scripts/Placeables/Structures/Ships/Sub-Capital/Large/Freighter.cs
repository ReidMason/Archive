using UnityEngine;

using NoxCore.Data.Placeables;

namespace NoxCore.Placeables.Ships
{
    public abstract class Freighter : Ship
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            // set ship classification properties
            Classification = ShipClassification.TRANSPORTER;
            structureSize = StructureSize.LARGE;

            base.init(noxObjectData);
            /*
            StructureInfo.aspectRadius = 40.0f;

            MaxForce = 50000;
            MaxTurnRate = 8.5f * Mathf.Deg2Rad;    // turn rate (degrees per second) converted into radians

            StructureInfo.mass = 10000;*/
        }
    }
}