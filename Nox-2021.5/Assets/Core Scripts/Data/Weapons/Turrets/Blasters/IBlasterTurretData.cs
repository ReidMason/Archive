namespace NoxCore.Data.Fittings
{
    public interface IBlasterTurretData : IRotatingTurretData
    {
        float ShieldDamageModifier { get; set; }
        float HullDamageModifier { get; set; }
    }
}