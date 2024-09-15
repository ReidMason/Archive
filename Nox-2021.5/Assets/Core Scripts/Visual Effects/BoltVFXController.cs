using UnityEngine;
using System.Collections;

using NoxCore.Fittings.Weapons;

namespace NoxCore.Effects
{
    public class BoltVFXController : VFXController
    {
        public void setInitialBearing(float bearing)
        {
            foreach (ParticleSystem vfx in vfxs)
            {
                if (vfx != null)
                {
                    var main = vfx.main;
                    main.startRotation = bearing;
                }
            }
        }
    }
}
