using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using NoxCore.Buffs;
using NoxCore.Utilities;

namespace NoxCore.Managers
{
    public class BuffManager : MonoBehaviour, IBuffManager
    {
        protected List<Buff> buffs = new List<Buff>();
        public List<Buff> Buffs { get { return buffs; } }

        //public List<BuffInfo> buffInfos = new List<BuffInfo>();

        [ShowOnly]
        public int numBuffsActive;

        [ShowOnly]
        public int buffChainLength;

        /*
        public void SetBuffInfos()
        {
            buffInfos.Clear();

            foreach (Buff buff in buffs)
                buffInfos.Add(buff.ToInfo());
        }
        */
        protected int getBuffLocation(Buff buff)
        {
            // matches buff on type, duration, amount and percent
            for (int buffLoc = 0; buffLoc < buffs.Count; buffLoc++)
            {
                if (buffs[buffLoc].GetType() == buff.GetType() && buffs[buffLoc].Duration == buff.Duration && buffs[buffLoc].Amount == buff.Amount && buffs[buffLoc].Percent == buff.Percent)
                {
                    return buffLoc;
                }
            }

            return -1;
        }

        public Buff addBuff(Buff buffToAdd)
        {
            int buffLoc = getBuffLocation(buffToAdd);

            Type buffType = buffToAdd.GetType();

            if (buffLoc != -1)
            {
                Buff buff = buffs[buffLoc];

                int stack = buff.getStack();

                if (stack < buff.MaxStack)
                {
                    D.log("Logic", "Found existing buff of type " + buffType.ToString() + " and incrementing its stack");

                    // unapply buffs from current buff location
                    unapplyBuffsFromIndex(buffLoc);

                    // add to buff effect multiplier (the stack)
                    buff.incrementStack();

                    // reset the stack timer (note: could make this optional for a given buff type)
                    buff.resetTimer();

                    // reapply buffs from buff location
                    applyBuffsFromIndex(buffLoc);

                    buff.stacked.Invoke();

                    numBuffsActive++;

                    return buff;
                }
                else
                {
                    D.log("Logic", "Found existing buff of type " + buffType.ToString() + " but reached maximum stack size for this buff");

                    buff.resetTimer();

                    return buff;
                }
            }
            else
            {
                D.log("Logic", "Adding new buff of type " + buffType.ToString() + " to " + gameObject.name);

                // set the stack to 1
                buffToAdd.incrementStack();

                // apply the new buff effect
                buffToAdd.applyBuff();

                // add new buff to buffs
                buffs.Add(buffToAdd);

                if (buffToAdd.added == null)
                {
                    buffToAdd.added = new UnityEvent();
                }

                if (buffToAdd.stacked == null)
                {
                    buffToAdd.stacked = new UnityEvent();
                }

                if (buffToAdd.destacked == null)
                {
                    buffToAdd.destacked = new UnityEvent();
                }

                if (buffToAdd.removed == null)
                {
                    buffToAdd.removed = new UnityEvent();
                }

                buffToAdd.added.Invoke();

                numBuffsActive++;

                buffChainLength = buffs.Count;

                return buffToAdd;
            }
        }

        public Buff DecrementStack(Buff buffToDecrement)
        {
            int buffLoc = getBuffLocation(buffToDecrement);

            if (buffLoc != -1 && buffToDecrement.getStack() > 1)
            {
                buffToDecrement.decrementStack();

                numBuffsActive--;
            }

            return buffToDecrement;
        }

        public void removeBuff(Buff buffToRemove, int buffLoc)
        {
            if (buffLoc == -1)
            {
                buffLoc = getBuffLocation(buffToRemove);
            }

            Type buffType = buffToRemove.GetType();

            if (buffLoc != -1)
            {
                D.log("Logic", "Found existing buff of type " + buffType.ToString() + " and removing it");

                int stack = buffToRemove.getStack();

                // unapply buffs from current buffs
                unapplyBuffsFromIndex(buffLoc, true);

                // TODO - either do this is destacked by the buff duration and call the destacked event
                buffToRemove.removed.Invoke();

                // remove expired buff
                buffs.RemoveAt(buffLoc);

                // reapply following buffs
                applyBuffsFromIndex(buffLoc);

                numBuffsActive -= stack;
                buffChainLength = buffs.Count;
            }            
        }

        protected void applyBuffsFromIndex(int index)
        {
            for (int buffLoc = index; buffLoc < buffs.Count; buffLoc++)
            {
                Buff buff = buffs[buffLoc];

                int stack = buff.getStack();

                buff.setStack(1);

                for (int s = 1; s <= stack; s++)
                {
                    buff.applyBuff();
                    buff.incrementStack();
                }

                buff.setStack(stack);
            }
        }

        protected void unapplyBuffsFromIndex(int index, bool expired = false)
        {
            for (int buffLoc = index; buffLoc < buffs.Count; buffLoc++)
            {
                Buff buff = buffs[buffLoc];

                int stack = buff.getStack();

                for (int s = stack; s > 0; s--)
                {
                    buff.unapplyBuff();
                    buff.decrementStack();
                }

                buff.setStack(stack);
            }
        }

        public void update()
        {
            for (int buffLoc = buffs.Count-1; buffLoc >= 0; buffLoc--)
            {
                Buff buff = buffs[buffLoc];

                // update buff timer for standard buffs only
                if (buff.BuffType == BuffType.STANDARD)
                {
                    buff.update();
                }

                // check if buff is now deactivated and remove it from list if buffs (note: permanent buffs cannot be removed by design)
                if (buff.Activated == false && buff.BuffType != BuffType.PERMANENT)
                {
                    removeBuff(buff, buffLoc);
                }
            }
        }
    }
}