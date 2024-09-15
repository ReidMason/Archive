using UnityEngine;

using NoxCore.Data.Fittings;

namespace NoxCore.Fittings.Devices
{
	public interface IPowerCore : IDevice
	{
		PowerCoreData PowerCoreData { get; set; }

		// void ejectCore();	
	}
}