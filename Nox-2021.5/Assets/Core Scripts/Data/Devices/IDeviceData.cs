using UnityEngine;

namespace NoxCore.Data.Fittings
{
    public interface IDeviceData
    {
        KeyCode DebugKey { get; set; }
        string ResourcePath { get; set; }
        string Type { get; set; }
        string SubType { get; set; }
        byte TechLevel { get; set; }
        byte Priority { get; set; }
        uint Cost { get; set; }
        float RequiredPower { get; set; }
        float ActiveHeat { get; set; }
        float EMField { get; set; }
        float ActivatingDelay { get; set; }
        float DeactivatingDelay { get; set; }
        bool ActiveOn { get; set; }
        bool ActiveOnSpawn { get; set; }
    }
}