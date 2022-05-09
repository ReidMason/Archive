namespace NoxCore.Fittings.Weapons
{
    public interface IUnguidedLauncher : ILauncher
    {
        bool AllowFiring { get; set; }
    }
}