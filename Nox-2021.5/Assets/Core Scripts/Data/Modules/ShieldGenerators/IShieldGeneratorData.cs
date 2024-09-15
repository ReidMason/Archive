namespace NoxCore.Data.Fittings
{
    public interface IShieldGeneratorData : IModuleData
    {
        float ShieldDelay { get; }
        float WeakFraction { get; }
        float BleedFraction { get; }
        float MinCharge { get; }
        float MaxCharge { get; }
        float RechargeRate { get; }
    }
}