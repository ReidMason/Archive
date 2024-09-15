using UnityEngine;
using System.Collections;

using NoxCore.Data.Placeables;
using NoxCore.Placeables;

namespace NoxCore.Placeables.Ships
{
    public abstract class Battleship : Ship
    {        
        public override void init(NoxObjectData noxObjectData = null)
        {
            // set ship classification properties
            Classification = ShipClassification.BATTLESHIP;
            structureSize = StructureSize.LARGE;

            base.init(noxObjectData);
            /*
            StructureInfo.aspectRadius = 30.0f;

            MaxForce = 90000;
            MaxTurnRate = 17.5f * Mathf.Deg2Rad;    // turn rate (degrees per second) converted into radians

            StructureInfo.mass = 6000;*/
        }
    }
}