using NoxCore.Fittings.Weapons;

using Davin.Data.Fittings;

namespace Davin.Fittings.Weapons
{
    public interface ISnowballThrower : IRotatingTurret
    {
        SnowballThrowerData SnowballThrowerData { get; set; }
    }
}