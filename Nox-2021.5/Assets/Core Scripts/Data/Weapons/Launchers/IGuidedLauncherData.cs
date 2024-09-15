namespace NoxCore.Data.Fittings
{
    public interface IGuidedLauncherData : IProjectileLauncherData
    {
        float LockTime { get; set; }
    }
}