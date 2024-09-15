using UnityEngine;

namespace NoxCore.Data.Fittings
{
    public interface IWeaponData : IModuleData
    {
        float MaxAmmo { get; set; }
        GameObject EffectPrefab { get; set; }
        float PowerPerShot { get; set; }
        float HeatPerShot { get; set; }
        float MinRange { get; set; }
        float MaxRange { get; set; }
        float BaseDamage { get; set; }
        float FireRate { get; set; }
        bool CanReveal { get; set; }
    }
}