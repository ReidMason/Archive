using NoxCore.Fittings.Weapons;

using Davin.Data.Fittings;

namespace Davin.Fittings.Weapons
{
    public interface IRapidFireBlasterTurret : IBlasterTurret
    {
        RapidFireBlasterTurretData RapidFireBlasterTurretData { get; set; }
    }
}