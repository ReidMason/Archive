using System;

using UnityEngine;

using NoxCore.Data;
using NoxCore.Data.Fittings;
using NoxCore.Placeables;

namespace Davin.Data.Fittings
{
    [CreateAssetMenu(fileName = "CloakingDeviceData", menuName = "ScriptableObjects/Fittings/Modules/CloakingDevice")]
    public class CloakingDeviceData : ModuleData, ICloakingDeviceData
    {
        [Header("Cloaking Device")]

        public StructureSize __maxStructureSize;
        [NonSerialized] protected StructureSize _maxStructureSize;
        public StructureSize MaxStructureSize { get { return _maxStructureSize; } set { _maxStructureSize = value; } }

        public float __cloakDelay;
        [NonSerialized] protected float _cloakDelay;
        public float CloakDelay { get { return _cloakDelay; } set { _cloakDelay = value; } }

        public BuffData __buffData;
        [NonSerialized]
        protected BuffData _buffData;
        public BuffData BuffData { get { return _buffData; } set { _buffData = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            MaxStructureSize = __maxStructureSize;
            _cloakDelay = __cloakDelay;
            BuffData = __buffData;
        }
    }
}