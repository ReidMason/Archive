using UnityEngine;

using System;

using NoxCore.Data.Fittings;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Utilities;

namespace Davin.Fittings.Devices
{
	public class NanoRepairer : Device, INanoRepairer
	{
		[Header("Nano Repairer")]
		public NanoRepairerData __nanoRepairerData;
		[NonSerialized]
		protected NanoRepairerData _nanoRepairerData;
		public NanoRepairerData NanoRepairerData {  get { return _nanoRepairerData; } set { _nanoRepairerData = value; } }

		[SerializeField]
        [ShowOnly]
        protected Module prevRepairTarget;
        public Module PrevRepairTarget { get { return prevRepairTarget; } }

        [SerializeField]
        protected Module repairTarget;
        public Module RepairTarget { get { return repairTarget; } set { repairTarget = value; } }

        public override void init(DeviceData deviceData = null)
		{
			if (deviceData == null)
            {
				NanoRepairerData = Instantiate(__nanoRepairerData);
				base.init(NanoRepairerData);
			}
			else
			{
				NanoRepairerData = deviceData as NanoRepairerData;
				base.init(deviceData);
			}

			repairTarget = null;
		}	
		
		public override void reset()
		{
			base.reset();
			
			repairTarget = null;
		}

		public void setRepairTarget(Module module)
		{	
			prevRepairTarget = repairTarget;		
			repairTarget = module;
			
			if (repairTarget != prevRepairTarget)
			{
				if (repairTarget == null)
				{
					// D.log("Device", DeviceName + " on " + structure.gameObject.name + " has changed its repair target to the hull");
				}
				else
				{
					// TODO - should have names for module sockets so these can be used in the log
					// D.log("Device", DeviceName + " on " + structure.gameObject.name + " has changed its repair target to: " + repairTarget.DeviceName);
				}
			}			
		}
		
		public override void update()
		{
			base.update();
			
			if (isActiveOn() == true && isFlippingActivation() == false)
			{
                if (repairTarget == null)
                {
                    if (structure.HullStrength < structure.MaxHullStrength)
                    {
                        float repairRequired = Mathf.Min(NanoRepairerData.RepairRate * Time.deltaTime, structure.MaxHullStrength - structure.HullStrength);
                        float repairPower = structure.powergrid.consumePower(repairRequired);

                        if (repairPower > 0)
                        {
                            structure.increaseHullStrength(repairRequired);
                        }
                    }
                }
                else
                {
                    if (repairTarget.Armour < repairTarget.ModuleData.MaxArmour)
                    {
                        float repairRequired = Mathf.Min(NanoRepairerData.RepairRate * Time.deltaTime, repairTarget.ModuleData.MaxArmour - repairTarget.Armour);
                        float repairPower = structure.powergrid.consumePower(repairRequired);

                        if (repairPower > 0)
                        {
                            repairTarget.increaseArmour(repairRequired);
                        }
                    }
                }                   
			}
		}
	}
}