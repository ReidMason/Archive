using NoxCore.Data.Fittings;

namespace NoxCore.Fittings.Weapons
{
    public interface IPlasmaCannon : IRotatingTurret, ITargetable
    {
        PlasmaCannonData PlasmaCannonData { get; set; }
    }
}