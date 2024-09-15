using UnityEngine;
using System.Collections;

using NoxCore.Data.Placeables;
using NoxCore.Placeables;

namespace NoxCore.Placeables.Ships
{
    public abstract class Battlecruiser : Ship
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            // set ship classification properties
            Classification = ShipClassification.BATTLECRUISER;
            structureSize = StructureSize.MEDIUM;

            base.init(noxObjectData);
            /*
            StructureInfo.aspectRadius = 20.0f;

            MaxForce = 60000;
            MaxTurnRate = 20.0f * Mathf.Deg2Rad;    // turn rate (degrees per second) converted into radians

            StructureInfo.mass = 4750*/
        }        
    }
}