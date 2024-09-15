using UnityEngine;
using System.Collections;

using NoxCore.Data.Placeables;
using NoxCore.Placeables;

namespace NoxCore.Placeables.Ships
{
    public abstract class Scout : Ship
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            //set ship classification properties
            Classification = ShipClassification.RECON;
            structureSize = StructureSize.TINY;

            base.init(noxObjectData);
            /*
            StructureInfo.aspectRadius = 1.5f;

            MaxForce = 3000;
            MaxTurnRate = 65.0f * Mathf.Deg2Rad;    // turn rate (degrees per second) converted into radians

            StructureInfo.mass = 75;*/
        }
    }
}