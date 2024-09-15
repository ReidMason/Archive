using UnityEngine;
using System.Collections;

using NoxCore.Data.Placeables;
using NoxCore.Placeables;

namespace NoxCore.Placeables.Ships
{
    public abstract class Interceptor : Ship
    {        
        public override void init(NoxObjectData noxObjectData = null)
        {
            // set ship classification properties
            Classification = ShipClassification.INTERCEPTOR;
            structureSize = StructureSize.SMALL;

            base.init(noxObjectData);
            /*
            StructureInfo.aspectRadius = 11.5f;

            MaxForce = 150000;
            MaxTurnRate = 55.0f * Mathf.Deg2Rad;    // turn rate (degrees per second) converted into radians

            StructureInfo.mass = 1750;*/
        }
    }
}