using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NoxCore.Fittings.Modules;
using NoxCore.Placeables;

namespace NoxCore.Fittings.Sockets
{
    public class StructureSocketInfo : ScriptableObject
    {
        public Transform parent;
        public string resourcePath;
        public string label;
        public List<string> allowedFittedTypes = new List<string>();
        public byte minTechLevel, maxTechLevel;
        public StructureSize minStructureSize, maxStructureSize;
        public Vector2 position;
        public float? rotation;

        public virtual bool canInstall(Module module)
        {
            // only fit module into the socket based on tech level
            if (module.DeviceData.TechLevel < minTechLevel || module.DeviceData.TechLevel > maxTechLevel)
            {
                D.warn("Fitting: {0}", "Module has an invalid tech level for the socket. Module is: " + module.DeviceData.TechLevel + " and socket is Min: " + minTechLevel + " and Max: " + maxTechLevel);
                return false;
            }

            foreach (string allowedFittedType in allowedFittedTypes)
            {
                // only fit correct module for the socket based on socket type
                if (module.getSocketTypes().Contains(allowedFittedType))
                {
                    return true;
                }
            }

            D.warn("Fitting: {0}", "Module has type(s): " + string.Join(", ", module.getSocketTypes().ToArray()) + " but socket can only be fitted with modules with any type from: " + string.Join(", ", allowedFittedTypes.ToArray()));

            return false;
        }
    }
}