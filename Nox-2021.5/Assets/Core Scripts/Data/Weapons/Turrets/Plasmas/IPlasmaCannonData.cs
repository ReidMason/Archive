namespace NoxCore.Data.Fittings
{
    public interface IPlasmaCannonData : IWeaponData
    {
        float EffectDuration { get; }
        float ShieldDamageModifier { get; }
        float HullDamageModifier { get; }
    }
}