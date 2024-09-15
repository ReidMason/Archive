using System.Collections;
using System.Collections.Generic;
using System.Linq;

using NoxCore.Buffs;
using NoxCore.Fittings.Weapons;

namespace Davin.Buffs
{
    public class RotatingTurretBuff : Buff
    {
        List<RotatingTurret> turrets;

        public RotatingTurretBuff(List<RotatingTurret> turrets, BuffType buffType, int maxStack, float amount, bool percent, float duration) : base(buffType, maxStack, amount, percent, duration)
        {
            this.turrets = turrets;
        }

        public override void applyBuff()
        {
            for (int i = 0; i < turrets.Count; i++)
            {
                turrets[i].RotatingTurretData.SlewSpeed = calculateBuff(turrets[i].RotatingTurretData.SlewSpeed);
            }
        }

        public override void unapplyBuff()
        {
            for (int i = 0; i < turrets.Count; i++)
            {
                turrets[i].RotatingTurretData.SlewSpeed = calculateDebuff(turrets[i].RotatingTurretData.SlewSpeed);
            }
        }
    }
}