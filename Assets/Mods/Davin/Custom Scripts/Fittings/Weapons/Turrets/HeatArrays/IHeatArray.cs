using NoxCore.Fittings.Weapons;

using Davin.Data.Fittings;

namespace Davin.Fittings.Weapons
{
    public interface IHeatArray : IWeapon
    {
        HeatArrayData HeatArrayData { get; set; }
    }
}