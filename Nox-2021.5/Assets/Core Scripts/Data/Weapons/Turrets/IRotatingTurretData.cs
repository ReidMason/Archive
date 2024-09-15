namespace NoxCore.Data.Fittings
{
    public interface IRotatingTurretData : IWeaponData
    {
        float SlewSpeed { get; }
        float TrackingAngle { get; }
        bool TransversalDamage { get; }
    }
}