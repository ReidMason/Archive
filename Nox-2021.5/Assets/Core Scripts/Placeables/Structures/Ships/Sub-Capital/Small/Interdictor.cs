using UnityEngine;
using System.Collections;

using NoxCore.Data.Placeables;
using NoxCore.Placeables;

namespace NoxCore.Placeables.Ships
{
    public abstract class Interdictor : Ship
    {        
        public override void init(NoxObjectData noxObjectData = null)
        {
            // set ship classification properties
            Classification = ShipClassification.INTERDICTOR;
            structureSize = StructureSize.SMALL;

            base.init(noxObjectData);
            /*
            StructureInfo.aspectRadius = 12.0f;

            MaxForce = 45000;
            MaxTurnRate = 15.0f * Mathf.Deg2Rad;    // turn rate (degrees per second) converted into radians

            StructureInfo.mass = 2200;*/
        }
    }
}