using NoxCore.Buffs;

namespace NoxCore.Data
{
    public interface IBuffData
    {
        BuffType BuffType { get; set; }
        int MaxStack { get; set; }
        float Amount { get; set; }
        bool Percent { get; set; }
        float Duration { get; set; }
    }
}