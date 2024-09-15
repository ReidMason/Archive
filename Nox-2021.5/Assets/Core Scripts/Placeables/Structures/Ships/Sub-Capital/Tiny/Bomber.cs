using UnityEngine;
using System.Collections;

using NoxCore.Data.Placeables;
using NoxCore.Placeables;

namespace NoxCore.Placeables.Ships
{
    public abstract class Bomber : Ship
    {        
        public override void init(NoxObjectData noxObjectData = null)
        {
            // set ship classification properties
            Classification = ShipClassification.BOMBER;
            structureSize = StructureSize.TINY;

            base.init(noxObjectData);
            /*
            StructureInfo.aspectRadius = 2.5f;

            MaxForce = 40000;
            MaxTurnRate = 35.0f * Mathf.Deg2Rad;    // turn rate (degrees per second) converted into radians

            StructureInfo.mass = 200;*/
        }
    }
}