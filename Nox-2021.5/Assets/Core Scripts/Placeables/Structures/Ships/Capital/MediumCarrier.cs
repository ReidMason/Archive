using UnityEngine;
using System.Collections;

using NoxCore.Data.Placeables;
using NoxCore.Placeables;

namespace NoxCore.Placeables.Ships
{
    public abstract class MediumCarrier : Ship
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            // set ship classification properties
            Classification = ShipClassification.CARRIER;
            structureSize = StructureSize.MASSIVE;

            base.init(noxObjectData);
            /*
            StructureInfo.aspectRadius = 75.0f;

            MaxForce = 135000;
            MaxTurnRate = 15.0f * Mathf.Deg2Rad;    // turn rate (degrees per second) converted into radians

            StructureInfo.mass = 12500;*/
        }        
    }
}