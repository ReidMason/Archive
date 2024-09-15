using NoxCore.Data.Fittings;

namespace NoxCore.Fittings.Weapons
{
    public interface ILaserTurret : IRotatingTurret, ITargetable
    {
        LaserTurretData LaserTurretData { get; set; }
    }
}