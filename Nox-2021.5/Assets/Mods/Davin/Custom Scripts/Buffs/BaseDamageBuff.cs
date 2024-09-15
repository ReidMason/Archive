using System.Collections.Generic;
using System.Linq;

using NoxCore.Buffs;
using NoxCore.Fittings.Weapons;

namespace Davin.Buffs
{
    public class BaseDamageBuff : Buff
    {
        List<IWeapon> weapons;

        public BaseDamageBuff(List<IWeapon> weapons, BuffType buffType, int maxStack, float amount, bool percent, float duration) : base(buffType, maxStack, amount, percent, duration)
        {
            this.weapons = weapons;
        }

        public override void applyBuff()
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                weapons[i].WeaponData.BaseDamage = (int)calculateBuff(weapons[i].WeaponData.BaseDamage);
            }
        }

        public override void unapplyBuff()
        {
            for (int i = 0; i < weapons.Count; i++)
            {
                weapons[i].WeaponData.BaseDamage = (int)calculateDebuff(weapons[i].WeaponData.BaseDamage);
            }
        }
    }
}