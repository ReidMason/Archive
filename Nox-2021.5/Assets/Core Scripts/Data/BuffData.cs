using System;

using UnityEngine;

using NoxCore.Buffs;

namespace NoxCore.Data
{
    [CreateAssetMenu(fileName = "BuffData", menuName = "ScriptableObjects/Buffs/Basic")]
    public class BuffData : ScriptableObject, IBuffData, ISerializationCallbackReceiver
    {
        public BuffType __buffType;
        [NonSerialized] protected BuffType _buffType;
        public BuffType BuffType { get { return _buffType; } set { _buffType = value; } }

        public int __maxStack;
        [NonSerialized] protected int _maxStack;
        public int MaxStack { get { return _maxStack; } set { _maxStack = value; } }

        public float __amount;
        [NonSerialized] protected float _amount;
        public float Amount { get { return _amount; } set { _amount = value; } }

        public bool __percent;
        [NonSerialized] protected bool _percent;
        public bool Percent { get { return _percent; } set { _percent = value; } }

        public float __duration;
        [NonSerialized] protected float _duration;
        public float Duration { get { return _duration; } set { _duration = value; } }

        public virtual void OnAfterDeserialize()
        {
            BuffType = __buffType;

            if (__maxStack > 0)
            {
                MaxStack = __maxStack;
            }
            else
            {
                MaxStack = 1;
            }

            Amount = __amount;
            Percent = __percent;
            Duration = __duration;
        }

        public virtual void OnBeforeSerialize()
        { }
    }
}