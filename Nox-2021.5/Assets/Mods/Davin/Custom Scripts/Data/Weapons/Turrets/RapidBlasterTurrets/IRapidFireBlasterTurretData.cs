using NoxCore.Data.Fittings;

namespace Davin.Data.Fittings
{
    public interface IRapidFireBlasterTurretData : IBlasterTurretData
    {
        int BoltsToFire { get; set; }
    }
}