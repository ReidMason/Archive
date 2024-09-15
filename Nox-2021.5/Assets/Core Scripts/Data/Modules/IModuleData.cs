using UnityEngine;

namespace NoxCore.Data.Fittings
{
    public interface IModuleData : IDeviceData
    {
        float AspectRadius { get; set; }
        float MaxArmour { get; set; }
        GameObject Explosion { get; set; }
        float DamageOnDestroy { get; set; }
    }
}
