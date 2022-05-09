using System.Collections.Generic;

using NoxCore.Buffs;
using NoxCore.Fittings.Modules;

namespace Davin.Buffs
{
    public class EngineMaxArmourBuff : Buff
    {
        List<IEngine> engines;

        public EngineMaxArmourBuff(List<IEngine> engines, BuffType buffType, int maxStack, float amount, bool percent, float duration) : base(buffType, maxStack, amount, percent, duration)
        {
            this.engines = engines;
        }

        public override void applyBuff()
        {
            for (int i = 0; i < engines.Count; i++)
            {                
                engines[i].ModuleData.MaxArmour = calculateBuff(engines[i].ModuleData.MaxArmour);
            }
        }

        public override void unapplyBuff()
        {
            for (int i = 0; i < engines.Count; i++)
            {
                engines[i].ModuleData.MaxArmour = calculateDebuff(engines[i].ModuleData.MaxArmour);
            }
        }
    }
}