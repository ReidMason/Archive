using UnityEngine;
using System.Collections;

using NoxCore.Data.Placeables;
using NoxCore.Placeables;

namespace NoxCore.Placeables.Ships
{
	public abstract class Corvette : Ship 
	{        
        public override void init(NoxObjectData noxObjectData = null)
        {
            // set ship classification properties
            Classification = ShipClassification.CORVETTE;
            structureSize = StructureSize.SMALL;

            base.init(noxObjectData);

            //StructureData.aspectRadius = 7.0f;

            //MaxForce = 75000;
            //MaxTurnRate = 40.0f * Mathf.Deg2Rad;    // turn rate (degrees per second) converted into radians

            //StructureInfo.mass = 1400;
        }
    }
}