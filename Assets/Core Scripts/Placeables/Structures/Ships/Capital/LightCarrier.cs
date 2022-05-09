using UnityEngine;
using System.Collections;

using NoxCore.Data.Placeables;
using NoxCore.Placeables;

namespace NoxCore.Placeables.Ships
{
    public abstract class LightCarrier : Ship
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            // set ship classification properties
            Classification = ShipClassification.CARRIER;
            structureSize = StructureSize.MASSIVE;

            base.init(noxObjectData);
            /*
            StructureInfo.aspectRadius = 50.0f;

            MaxForce = 120000;
            MaxTurnRate = 20.0f * Mathf.Deg2Rad;    // turn rate (degrees per second) converted into radians

            StructureInfo.mass = 10000;*/
        }
    }
}