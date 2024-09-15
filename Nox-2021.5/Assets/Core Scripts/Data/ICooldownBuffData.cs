using UnityEngine;

namespace NoxCore.Data
{
    public interface ICooldownBuffData : IBuffData
    {
        float Cooldown { get; set; }
    }
}
