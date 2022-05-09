using UnityEngine;
using System.Collections.Generic;

using NoxCore.Data.Fittings;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Sockets;

namespace NoxCore.Fittings.Modules
{
	public interface IModule : IDevice
	{
        ModuleData ModuleData { get; set; }

        float Armour { get; set; }
        Vector2 getPosition();
        StructureSocket getSocket();
		List<string> getSocketTypes();
		void resetArmour();
        float getArmour();
        void setArmour(float amount);
		void increaseArmour(float amount);
		(bool destroyed, float damageOnDestroy) decreaseArmour(float amount);
        float getMaxArmour();
        void setMaxArmour(float amount);
        void explode(int repeatedNumExplosions = 0);
	}
}