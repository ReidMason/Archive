using UnityEngine;

using NoxCore.Fittings.Modules;

namespace NoxCore.Fittings.Sockets
{
	public class ModuleSocket : StructureSocket
	{		
		protected GameObject _ModuleGO;
		public GameObject ModuleGO { get { return _ModuleGO; } set { _ModuleGO = value; } }
						
		public override bool install(GameObject moduleGO)
		{
			Module module = moduleGO.GetComponent<Module>();

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
                    // D.log("Fitting", "Setting module in socket to: " + module.name);
                    ModuleGO = moduleGO;

                    InstalledModule = module;

                    // attach module to socket
                    moduleGO.transform.parent = gameObject.transform;
                    moduleGO.transform.position = gameObject.transform.position;

                    return true;
                }
            }
								
			D.warn("Fitting: {0}", "Module has type(s): " + string.Join(", ", module.getSocketTypes().ToArray()) + " but socket can only be fitted with modules with any type from: " + string.Join(", ", allowedFittedTypes.ToArray()));

            return false;
		}
	}
}