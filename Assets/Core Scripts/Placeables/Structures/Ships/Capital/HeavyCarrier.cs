using UnityEngine;
using System.Collections;

using NoxCore.Data.Placeables;
using NoxCore.Placeables;

namespace NoxCore.Placeables.Ships
{
    public abstract class HeavyCarrier : Ship
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            // set ship classification properties
            Classification = ShipClassification.CARRIER;
            structureSize = StructureSize.MASSIVE;

            base.init(noxObjectData);
            /*
            StructureInfo.aspectRadius = 100.0f;

            MaxForce = 150000;
            MaxTurnRate = 10.0f * Mathf.Deg2Rad;    // turn rate (degrees per second) converted into radians

            StructureInfo.mass = 15000;*/
        }
    }
}