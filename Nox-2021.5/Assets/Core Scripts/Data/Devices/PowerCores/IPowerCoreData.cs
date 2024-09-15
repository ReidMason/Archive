namespace NoxCore.Data.Fittings
{
    public interface IPowerCoreData : IDeviceData
    {
        float PowerGeneration { get; set; }
    }
}