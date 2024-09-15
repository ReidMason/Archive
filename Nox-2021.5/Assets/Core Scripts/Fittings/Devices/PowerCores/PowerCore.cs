using UnityEngine;

using System;

using NoxCore.Data.Fittings;

namespace NoxCore.Fittings.Devices
{
	public class PowerCore : Device, IPowerCore
	{
		[Header("Power Core")]

		public PowerCoreData __powerCoreData;
		[NonSerialized]
		protected PowerCoreData _powerCoreData;
		public PowerCoreData PowerCoreData { get { return _powerCoreData; } set { _powerCoreData = value; } }

		public override void init(DeviceData deviceData = null)
        {
			if (deviceData == null)
            {
				PowerCoreData = Instantiate(__powerCoreData);
				base.init(PowerCoreData);
			}
			else
			{
				PowerCoreData = deviceData as PowerCoreData;
				base.init(deviceData);
			}
		}

        // TODO - need to detach device from ship and turn it into a free floating entity but this cannot be instantaneous or dying ships may use it always as revenge	
        // could be picked up by tractor beam and rehoused
        // device could detonate if destroyed causing radial damage
        /*
		public void ejectCore()
		{        
		}
		*/

        public override void update()
		{
			base.update();
			
			if (isActiveOn() == true && isFlippingActivation() == false)
			{
				Powergrid.addPower(PowerCoreData.PowerGeneration * Time.deltaTime);
			}
		}
	}
}