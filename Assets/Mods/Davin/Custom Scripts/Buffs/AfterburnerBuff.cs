using System.Collections.Generic;
using System.Linq;

using NoxCore.Buffs;
using NoxCore.Data;
using NoxCore.Data.Placeables;
using NoxCore.Fittings.Modules;

namespace Davin.Buffs
{
    public class AfterburnerBuff : Buff
    {
        List<IEngine> engines;

        public AfterburnerBuff(List<IEngine> engines, BuffData buffData) : base(buffData)
        {
            this.engines = engines;
        }

        public override void applyBuff()
        {
            for (int i = 0; i < engines.Count; i++)
            {
                engines[i].EngineData.MaxSpeed = calculateBuff(engines[i].EngineData.MaxSpeed);
            }

            if (engines.Count > 0)
            {
                ShipData shipData = engines[0].getStructure().StructureData as ShipData;

                shipData.SpeedLimiter = calculateBuff(shipData.SpeedLimiter);
            }
        }

        public override void unapplyBuff()
        {
            for (int i = 0; i < engines.Count; i++)
            {
                engines[i].EngineData.MaxSpeed = calculateDebuff(engines[i].EngineData.MaxSpeed);
            }

            if (engines.Count > 0)
            {
                ShipData shipData = engines[0].getStructure().StructureData as ShipData;

                shipData.SpeedLimiter = calculateDebuff(shipData.SpeedLimiter);
            }
        }
    }
}