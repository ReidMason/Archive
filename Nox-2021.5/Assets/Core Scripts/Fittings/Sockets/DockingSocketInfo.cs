using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NoxCore.Placeables;

namespace NoxCore.Fittings.Sockets
{
    public class DockingSocketInfo : StructureSocketInfo
    {
        public StructureSize minDockingSize, maxDockingSize;
        public Vector2 approachVector;
        public float maxDockingSpeed;
        public float tetherDistance;

        [Range(0,180)]
        public float approachArc;

        public float dockingTime;
        public float undockingTime;

        protected static void setSocketInfo(DockingSocketInfo dockingSocketInfo, StructureSocketInfo structureSocketInfo)
        {
            dockingSocketInfo.parent = structureSocketInfo.parent;
            dockingSocketInfo.label = structureSocketInfo.label;
            dockingSocketInfo.position = structureSocketInfo.position;
            dockingSocketInfo.rotation = structureSocketInfo.rotation;
            dockingSocketInfo.allowedFittedTypes = new List<string>();
            dockingSocketInfo.allowedFittedTypes = structureSocketInfo.allowedFittedTypes;
            dockingSocketInfo.minTechLevel = structureSocketInfo.minTechLevel;
            dockingSocketInfo.maxTechLevel = structureSocketInfo.maxTechLevel;
            dockingSocketInfo.minStructureSize = structureSocketInfo.minStructureSize;
            dockingSocketInfo.maxStructureSize = structureSocketInfo.maxStructureSize;
        }

        public static DockingSocketInfo CopyToDockingSocketInfo(StructureSocketInfo socketInfo)
        {
            DockingSocketInfo temp = ScriptableObject.CreateInstance<DockingSocketInfo>();

            setSocketInfo(temp, socketInfo);

            return temp;
        }
    }
}