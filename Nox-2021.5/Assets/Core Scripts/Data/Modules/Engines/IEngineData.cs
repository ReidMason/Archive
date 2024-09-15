using UnityEngine;

namespace NoxCore.Data.Fittings
{
    public interface IEngineData : IModuleData
    {
        float MaxSpeed { get; set; }
        float MaxOverheatedSpeed { get; set; }
        bool UseCustomExhaustColours { get; set; }
        Gradient ExhaustColourGradient { get; set; }
    }
}