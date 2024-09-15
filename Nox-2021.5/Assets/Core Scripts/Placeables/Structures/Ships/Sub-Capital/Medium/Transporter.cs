using UnityEngine;

using NoxCore.Data.Placeables;

namespace NoxCore.Placeables.Ships
{
    public abstract class Transporter : Ship
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            // set ship classification properties
            Classification = ShipClassification.TRANSPORTER;
            structureSize = StructureSize.MEDIUM;

            base.init(noxObjectData);
            /*
            StructureInfo.aspectRadius = 18.0f;

            MaxForce = 65000;
            MaxTurnRate = 30.0f * Mathf.Deg2Rad;    // turn rate (degrees per second) converted into radians

            StructureInfo.mass = 5000;*/
        }
    }
}