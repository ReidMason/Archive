using UnityEngine;
using System.Collections;

using NoxCore.Data.Placeables;
using NoxCore.Placeables;

namespace NoxCore.Placeables.Ships
{
    public abstract class Dreadnought : Ship
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            //set ship classification properties
            Classification = ShipClassification.DREADNOUGHT;
            structureSize = StructureSize.LARGE;

            base.init(noxObjectData);
            /*
            StructureInfo.aspectRadius = 35.0f;

            MaxForce = 150000;
            MaxTurnRate = 10.0f * Mathf.Deg2Rad;    // turn rate (degrees per second) converted into radians

            StructureInfo.mass = 8000;*/
        }
    }
}