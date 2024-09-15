using UnityEngine;

namespace NoxCore.Data.Placeables
{
    public interface IShipData : IStructureData
    {
        float SpeedLimiter { get; set; }
        bool AfterburnerCapable { get ; set; }
        bool WarpCapable { get; set; }
        uint NumEngineSockets { get; set; }
        float MaxForce { get; set; }
        float MaxTurnRate { get; set; }
    }
}