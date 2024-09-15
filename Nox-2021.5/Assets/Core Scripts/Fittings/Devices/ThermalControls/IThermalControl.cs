using UnityEngine;

using NoxCore.Data.Fittings;

namespace NoxCore.Fittings.Devices
{
    public interface IThermalControl : IDevice
    {
        ThermalControlData ThermalControlData { get; set; }
        float CurrentHeat { get; set; }
        bool Overheated { get; }

        float getHeatCapacity();
        float getCurrentHeat();
        void setCurrentHeat(float heat);
        string getHeatPercentage();
        bool isOverheated();
        void addHeat(Device device, float heat);
		void radiateHeat(float heat);	
	}
}