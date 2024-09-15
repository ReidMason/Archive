using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NoxCore.Data.Placeables;

namespace NoxCore.Placeables
{
    public class BeaconStation : Station
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            // change any default values in the parent class here

            if (noxObjectData != null)
            {
                base.init(Instantiate(noxObjectData));
            }
            else
            {
                base.init();
            }
        }
    }
}