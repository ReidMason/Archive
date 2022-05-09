using NoxCore.Data;
using NoxCore.Data.Fittings;

namespace Davin.Data.Fittings
{
    public interface IAfterburnerData : IDeviceData
    {
        CooldownBuffData CooldownBuff { get; set; }
    }
}