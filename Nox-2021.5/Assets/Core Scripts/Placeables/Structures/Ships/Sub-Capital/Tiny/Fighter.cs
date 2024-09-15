using UnityEngine;
using System.Collections;

using NoxCore.Data.Placeables;
using NoxCore.Placeables;

namespace NoxCore.Placeables.Ships
{
    public abstract class Fighter : Ship
    {        
        public override void init(NoxObjectData noxObjectData = null)
        {
            // set ship classification properties
            Classification = ShipClassification.FIGHTER;
            structureSize = StructureSize.TINY;

            base.init(noxObjectData);
            /*
            StructureInfo.aspectRadius = 1.0f;

            MaxForce = 60000;
            MaxTurnRate = 55.0f * Mathf.Deg2Rad;    // turn rate (degrees per second) converted into radians

            StructureInfo.mass = 100;*/
        }
    }
}