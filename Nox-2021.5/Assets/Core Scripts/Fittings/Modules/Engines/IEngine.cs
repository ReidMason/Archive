using NoxCore.Data.Fittings;
using NoxCore.Debugs;

namespace NoxCore.Fittings.Modules
{
	public interface IEngine : IModule, ISystemDebuggable
	{
		EngineData EngineData { get; set; }

		float getMaxSpeed();
        void setMaxSpeed(float maxSpeed);
		void resetEngineTrails();
		void restartEngineTrails();

/*
		void setSilentRunningFactor(float factor);
		void engageSilentRunning();
		void disengageSilentRunning();
*/
	}
}