using UnityEngine;

using NoxCore.Data.Fittings;

namespace NoxCore.Fittings.Weapons
{
    public interface IRotatingTurret : IWeapon, IRotate
    {
        RotatingTurretData RotatingTurretData { get; set; }

        Vector2? TargetPosition { get; set; }
    }
}
