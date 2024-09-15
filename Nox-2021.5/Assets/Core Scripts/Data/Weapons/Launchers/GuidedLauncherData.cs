using UnityEngine;

using System;

namespace NoxCore.Data.Fittings
{
    [CreateAssetMenu(fileName = "GuidedLauncherData_", menuName = "ScriptableObjects/Fittings/Weapons/GuidedLauncher")]
    public class GuidedLauncherData : ProjectileLauncherData, IGuidedLauncherData
    {
        [Header("Guided Launcher")]

        public float __lockTime;
        [NonSerialized] protected float _lockTime;
        public float LockTime { get { return _lockTime; } set { _lockTime = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            LockTime = __lockTime;
        }
    }
}