using UnityEngine;
using System.Collections;

using NoxCore.Data.Placeables;
using NoxCore.Placeables;

namespace NoxCore.Placeables.Ships
{
    public abstract class Cruiser : Ship
    {        
        public override void init(NoxObjectData noxObjectData = null)
        {
            // set ship classification properties
            Classification = ShipClassification.CRUISER;
            structureSize = StructureSize.MEDIUM;

            base.init(noxObjectData);
            /*
            StructureInfo.aspectRadius = 15.0f;

            MaxForce = 120000;
            MaxTurnRate = 35.0f * Mathf.Deg2Rad;    // turn rate (degrees per second) converted into radians

            StructureInfo.mass = 4000;*/
        }
        
    }
}