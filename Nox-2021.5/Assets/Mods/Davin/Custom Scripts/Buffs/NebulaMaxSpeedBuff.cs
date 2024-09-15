using System.Collections.Generic;

using UnityEngine;

using NoxCore.Buffs;
using NoxCore.Data;
using NoxCore.Data.Placeables;
using NoxCore.Fittings.Modules;
using NoxCore.Managers;
using NoxCore.Placeables.Ships;

namespace Davin.Buffs
{
    public class NebulaMaxSpeedBuff : Buff
    {
        List<IEngine> engines;

        public NebulaMaxSpeedBuff(List<IEngine> engines, BuffData buffData) : base(buffData)
        {
            this.engines = engines;
        }

        public NebulaMaxSpeedBuff(List<IEngine> engines, BuffType buffType, int maxStack, float amount, bool percent, float duration) : base(buffType, maxStack, amount, percent, duration)
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

            foreach(Ship ship in GameManager.Instance.getShips())
            {
                if (!ship.gameObject.activeInHierarchy) continue;
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

            foreach (Ship ship in GameManager.Instance.getShips())
            {
                if (!ship.gameObject.activeInHierarchy) continue;
            }
        }
    }
}