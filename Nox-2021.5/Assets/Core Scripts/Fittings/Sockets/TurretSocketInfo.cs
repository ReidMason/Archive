using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NoxCore.Fittings.Modules;
using NoxCore.Placeables;

namespace NoxCore.Fittings.Sockets
{
    public class TurretSocketInfo : StructureSocketInfo
    {
        public bool fixedFiringArc;
        public float fireArcHalf;

        protected static void setSocketInfo(TurretSocketInfo turretSocketInfo, StructureSocketInfo structureSocketInfo)
        {
            turretSocketInfo.parent = structureSocketInfo.parent;
            turretSocketInfo.label = structureSocketInfo.label;
            turretSocketInfo.position = structureSocketInfo.position;
            turretSocketInfo.rotation = structureSocketInfo.rotation;
            turretSocketInfo.allowedFittedTypes = new List<string>();
            turretSocketInfo.allowedFittedTypes = structureSocketInfo.allowedFittedTypes;
            turretSocketInfo.minTechLevel = structureSocketInfo.minTechLevel;
            turretSocketInfo.maxTechLevel = structureSocketInfo.maxTechLevel;
            turretSocketInfo.minStructureSize = structureSocketInfo.minStructureSize;
            turretSocketInfo.maxStructureSize = structureSocketInfo.maxStructureSize;
        }

        public static TurretSocketInfo CopyToTurretSocketInfo(StructureSocketInfo socketInfo)
        {
            TurretSocketInfo temp = ScriptableObject.CreateInstance<TurretSocketInfo>();

            setSocketInfo(temp, socketInfo);

            return temp;
        }
    }
}