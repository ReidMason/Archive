using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NoxCore.Buffs;
using NoxCore.Fittings.Modules;
using NoxCore.Placeables;

namespace Davin.Buffs
{
    public class MaxHullBuff : Buff
    {
        Structure structure;

        public MaxHullBuff(Structure structure, BuffType buffType, int maxStack, float amount, bool percent, float duration) : base(buffType, maxStack, amount, percent, duration)
        {
            this.structure = structure;
        }

        public override void applyBuff()
        {
            // note: MaxArmour is a property so must use this other polymorphic method to calculate the buffed value
            structure.MaxHullStrength = calculateBuff(structure.MaxHullStrength);
        }

        public override void unapplyBuff()
        {
            // note: MaxArmour is a property so must use this other polymorphic method to calculate the buffed value
            structure.MaxHullStrength = calculateDebuff(structure.MaxHullStrength);
        }
    }
}