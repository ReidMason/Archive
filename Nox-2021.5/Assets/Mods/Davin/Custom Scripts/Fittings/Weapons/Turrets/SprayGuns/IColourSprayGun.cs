using NoxCore.Fittings.Weapons;

using Davin.Data.Fittings;

namespace Davin.Fittings.Weapons
{
    public interface IColourSprayGun : IRotatingTurret
    {
        ColourSprayGunData ColourSprayGunData { get; set; }
    }
}