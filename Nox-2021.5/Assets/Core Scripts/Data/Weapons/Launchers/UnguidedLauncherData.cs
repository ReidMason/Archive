using UnityEngine;

using System;

namespace NoxCore.Data.Fittings
{
    [CreateAssetMenu(fileName = "UnguidedLauncherData_", menuName = "ScriptableObjects/Fittings/Weapons/UnguidedLauncher")]
    public class UnguidedLauncherData : ProjectileLauncherData, IUnguidedLauncherData
    {
        //[Header("Unguided Launcher")]

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();
        }
    }
}