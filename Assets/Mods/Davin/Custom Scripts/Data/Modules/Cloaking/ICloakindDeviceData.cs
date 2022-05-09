using NoxCore.Data;
using NoxCore.Data.Fittings;
using NoxCore.Placeables;

namespace Davin.Data.Fittings
{
    public interface ICloakingDeviceData : IModuleData
    {
        StructureSize MaxStructureSize { get; set; }
        float CloakDelay { get; set; }
        BuffData BuffData { get; set; }
    }
}