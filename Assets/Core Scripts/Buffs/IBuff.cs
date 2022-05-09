using UnityEngine;
using System.Collections;

namespace NoxCore.Buffs
{
    public interface IBuff
    {
        void setStack(int stack);
        void incrementStack();
        void decrementStack();
        void applyBuff();
        void unapplyBuff();
        void update();
    }
}