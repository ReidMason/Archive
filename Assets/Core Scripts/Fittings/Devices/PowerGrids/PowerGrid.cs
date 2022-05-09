using UnityEngine;

using System;

using NoxCore.Utilities;
using NoxCore.Data.Fittings;

namespace NoxCore.Fittings.Devices
{
	public class PowerGrid : Device, IPowerGrid
	{
        [Header("Power Grid")]

		public PowerGridData __powerGridData;
		[NonSerialized]
		protected PowerGridData _powerGridData;
		public PowerGridData PowerGridData { get { return _powerGridData; } set { _powerGridData = value; } }

		[SerializeField]
		[ShowOnly]
		protected float _currentPower;
		public float CurrentPower { get { return _currentPower; } set { _currentPower = value; } }

        public override void init(DeviceData deviceData = null)
		{
			if (deviceData == null)
			{
				PowerGridData = Instantiate(__powerGridData);
				base.init(PowerGridData);
			}
			else
			{
				PowerGridData = deviceData as PowerGridData;
				base.init(deviceData);
			}			
			
			CurrentPower = PowerGridData.MaxPower;
		}
		
		public override void reset()
		{
			base.reset();
			CurrentPower = PowerGridData.MaxPower;
		}
		
		public float getCurrentPower()
		{
			return CurrentPower;
		}	
		
		public float getMaxPower()
		{
			return PowerGridData.MaxPower;
		}	
		
		public float addPower(float power)
		{
			CurrentPower += power;
				
			if (CurrentPower > PowerGridData.MaxPower)
			{
				CurrentPower = PowerGridData.MaxPower;
			}
				
			return power;
		}
		
		public float consumePower(float power)
		{
			if (isActiveOn() == true && isFlippingActivation() == false)
			{
				CurrentPower -= power;
				
				if (CurrentPower < 0)
				{
					power += CurrentPower;
					CurrentPower = 0;
				}
				
				return power;
			}
			
			return 0;
		}
		
		public float consumeFixedPower(float power)
		{
			if (isActiveOn() == true && isFlippingActivation() == false)
			{
				if (CurrentPower >= power)
				{
					CurrentPower -= power;
					
					return power;
				}
				
				return 0;
			}
			
			return 0;
		}
		
		public float supplyPower(Device device)
		{
			if (isActiveOn() == true && isFlippingActivation() == false)
			{
				float powerConsumption = device.getRequiredPower() * Time.deltaTime;
				
				if (powerConsumption <= CurrentPower)
				{
					CurrentPower -= powerConsumption;
					return powerConsumption;
				}
				else
				{
					return 0;
				}
			}
			
			return 0;
		}
		
		public override void update()
		{
			base.update();
		}
	}
}