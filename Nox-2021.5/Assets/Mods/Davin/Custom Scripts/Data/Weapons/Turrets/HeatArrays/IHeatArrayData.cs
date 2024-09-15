using NoxCore.Data.Fittings;

namespace Davin.Data.Fittings
{
    public interface IHeatArrayData : IRotatingTurretData
    {
        float EffectDuration { get; }
    }
}