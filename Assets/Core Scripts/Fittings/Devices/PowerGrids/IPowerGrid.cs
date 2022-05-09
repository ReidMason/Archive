using UnityEngine;

using NoxCore.Data.Fittings;

namespace NoxCore.Fittings.Devices
{
	public interface IPowerGrid : IDevice
	{
		PowerGridData PowerGridData { get; set; }

		float CurrentPower { get; set; }
        float getCurrentPower();
		float getMaxPower();
		float addPower(float power);
		float consumePower(float power);
		float consumeFixedPower(float power);
		float supplyPower(Device device);
	}
}