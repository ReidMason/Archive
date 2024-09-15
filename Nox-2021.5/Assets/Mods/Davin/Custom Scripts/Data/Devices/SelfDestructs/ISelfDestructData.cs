using UnityEngine;

using NoxCore.Data.Fittings;
using NoxCore.Fittings.Weapons;

namespace Davin.Data.Fittings
{
    public interface ISelfDestructData : IDeviceData
    {
        Explosion Explosion { get; set; }
        float RadialDamage { get; set; }
    }
}