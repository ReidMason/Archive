using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using NoxCore.Utilities;
using NoxCore.Data;

namespace NoxCore.Buffs
{
    public enum BuffType { STANDARD, PASSIVE, PERMANENT };

    public abstract class Buff : IBuff
    {
        protected BuffType buffType;
        public BuffType BuffType {  get { return buffType; } }

        protected float amount;
        public float Amount { get { return amount; } }

        protected bool percent;
        public bool Percent { get { return percent; } }

        protected float duration;
        public float Duration { get { return duration; } }

        [Range (1,int.MaxValue)]
        protected int maxStack;
        public int MaxStack { get { return maxStack; } }

        [ShowOnly]
        protected int stack;
        protected float timer;

        protected bool activated;
        public bool Activated { get { return activated; } }

        public UnityEvent added;
        public UnityEvent stacked;
        public UnityEvent destacked;
        public UnityEvent removed;

        public Buff(BuffType buffType, int maxStack, float amount, bool percent, float duration)
        {
            this.buffType = buffType;
            this.maxStack = maxStack;
            this.amount = amount;
            this.percent = percent;
            this.duration = duration;

            stack = 0;
            timer = 0;
            activated = true;

            D.log("Logic", GetType().ToString() + " timer activated");
        }

        public Buff(BuffData buffData)
        {
            this.buffType = buffData.BuffType;
            this.maxStack = buffData.MaxStack;
            this.amount = buffData.Amount;
            this.percent = buffData.Percent;
            this.duration = buffData.Duration;

            stack = 0;
            timer = 0;
            activated = true;

            D.log("Logic", GetType().ToString() + " timer activated");
        }

        public int getStack()
        {
            return stack;
        }

        public void setStack(int stack)
        {
            this.stack = stack;
        }

        public void incrementStack()
        {
            stack++;
        }

        public void decrementStack()
        {
            stack--;
        }

        public void resetTimer()
        {
            timer = 0;
        }

        public void deactivate()
        {
            activated = false;
        }

        public abstract void applyBuff();
        public abstract void unapplyBuff();
        /*
        protected void calculateBuff(ref float stat)
        {
            if (percent == true)
            {
                stat *= 1 + ((amount - 100) / 100);
            }
            else
            {
                stat += stack * amount;
            }
        }
        */
        protected float calculateBuff(float stat)
        {
            if (percent == true)
            {
                stat *= 1 + ((amount - 100) / 100);
            }
            else
            {
                stat += stack * amount;
            }

            return stat;
        }
        /*
        protected void calculateDebuff(ref float stat)
        {
            if (percent == true)
            {
                stat /= 1 + ((amount - 100) / 100);
            }
            else
            {
                stat -= stack * amount;
            }
        }
        */
        protected float calculateDebuff(float stat)
        {
            if (percent == true)
            {
                stat /= 1 + ((amount - 100) / 100);
            }
            else
            {
                stat -= stack * amount;
            }

            return stat;
        }
        /*
        public BuffInfo ToInfo()
        {
            return new BuffInfo(GetType().ToString(), BuffType.ToString(), MaxStack, Amount, Percent, Duration, stack, Activated);
        }
        */
        public virtual void update()
        {
            timer += Time.deltaTime;

            if (timer >= duration)
            {
                activated = false;
                D.log("Logic", GetType().ToString() + " timer deactivated");
            }
        }
    }
}