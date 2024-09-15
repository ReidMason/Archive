using UnityEngine;
using System.Collections.Generic;

using NoxCore.Data.Fittings;
using NoxCore.Effects;
using NoxCore.Placeables;

namespace NoxCore.Fittings.Devices
{
	public interface IDevice
	{
		DeviceData DeviceData { get; set; }

		void init(DeviceData deviceData = null);
        List<IVisualEffect> getVFXs();
		void reset();
		void destroy();
		bool isDestroyed();
        GameObject getGameObject();
		Structure getStructure();
		void setStructure(Structure structure);
		void postFitting();
		bool isActiveOn();
        bool isActiveOnSpawn();
        void setActiveOn(bool activeOn);
		void setActiveOnSpawn(bool activeOnSpawn);
		string getState();
		bool isFlippingActivation();
		void activate();
		void deactivate();
		float getRequiredPower();
		void update();
	}
}