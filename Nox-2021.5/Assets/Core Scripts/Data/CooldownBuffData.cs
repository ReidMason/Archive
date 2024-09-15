using System;

using UnityEngine;

namespace NoxCore.Data
{
    [CreateAssetMenu(fileName = "BuffData", menuName = "ScriptableObjects/Buffs/Cooldown")]
    public class CooldownBuffData : BuffData, ICooldownBuffData
    {
        public float __cooldown;
        [NonSerialized] protected float _cooldown;
        public float Cooldown { get { return _cooldown; } set { _cooldown = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            if (__cooldown > 0)
            {
                Cooldown = __cooldown;
            }
            else
            {
                Cooldown = 1.0f;
            }
        }
    }
}