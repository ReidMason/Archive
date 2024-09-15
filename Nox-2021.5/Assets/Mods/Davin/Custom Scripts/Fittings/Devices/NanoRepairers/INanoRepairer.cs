using NoxCore.Data.Fittings;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;

namespace Davin.Fittings.Devices
{
	public interface INanoRepairer : IDevice
	{
        NanoRepairerData NanoRepairerData { get; set; }

        Module PrevRepairTarget { get; }
        Module RepairTarget { get; set; }

        void setRepairTarget(Module module);
	}
}