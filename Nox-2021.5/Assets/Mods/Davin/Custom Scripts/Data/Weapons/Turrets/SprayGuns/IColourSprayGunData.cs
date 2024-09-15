using UnityEngine;

using NoxCore.Data.Fittings;

public interface IColourSprayGunData : IRotatingTurretData
{
    Color32 Colour { get; set; }
    float MinDropRadius { get; set; }
    float MaxDropRadius { get; set; }
    float Spread { get; set; }
}
