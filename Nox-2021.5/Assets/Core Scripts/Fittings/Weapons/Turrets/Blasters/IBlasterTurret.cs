using NoxCore.Data.Fittings;

namespace NoxCore.Fittings.Weapons
{
    public interface IBlasterTurret : IRotatingTurret, ITargetable
    {
        BlasterTurretData BlasterTurretData { get; set; }
    }
}