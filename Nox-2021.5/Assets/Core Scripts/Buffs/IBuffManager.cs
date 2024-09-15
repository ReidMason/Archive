using NoxCore.Buffs;

namespace NoxCore.Managers
{ 
    public interface IBuffManager
    {
        Buff addBuff(Buff buffToAdd);
        void removeBuff(Buff buffToRemove, int buffLoc = -1);
        Buff DecrementStack(Buff buffToDecrement);
        void update();
    }
}
