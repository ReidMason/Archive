using UnityEngine;

using System;

using NoxCore.Utilities;
using NoxCore.Data.Fittings;
using NoxCore.Placeables;

namespace NoxCore.Fittings.Devices
{
	public class ThermalControl : Device, IThermalControl
	{
        [Header("Thermal Control")]

        public ThermalControlData __thermalControlData;
        [NonSerialized]
        protected ThermalControlData _thermalControlData;
        public ThermalControlData ThermalControlData { get { return _thermalControlData; } set { _thermalControlData = value; } }

        [SerializeField]
        protected float _CurrentHeat;
        public float CurrentHeat {  get { return _CurrentHeat; } set { _CurrentHeat = value; } }

        [SerializeField]
        [ShowOnly]
        protected bool _Overheated;
        public bool Overheated { get { return _Overheated; } }

        public override void init(DeviceData deviceData = null)
        {
            if (deviceData == null)
            {
                ThermalControlData = Instantiate(__thermalControlData);
                base.init(ThermalControlData);
            }
            else
            {
                ThermalControlData = deviceData as ThermalControlData;
                base.init(deviceData);
            }
            
            structure.ActivateUltimate += OnUltimateActivated;
        }

        public float getCurrentHeat()
        {
            return CurrentHeat;
        }

        public void setCurrentHeat(float heat)
        {
            CurrentHeat = heat;
        }

        public float getHeatCapacity()
        {
            return ThermalControlData.HeatCapacity;
        }

        public string getHeatPercentage()
        {
            return " Heat (" + (int)(CurrentHeat * 100 / ThermalControlData.HeatCapacity) + "%) ";
        }

        public bool isOverheated()
        {
            return Overheated;
        }

        public void addHeat(Device device, float heat)
		{
            if (Overheated == false)
            {
                // add all device heat to thermal control system
                CurrentHeat += heat;

                // check to see if thermal control system has reached maximum heat capacity
                if (CurrentHeat >= ThermalControlData.HeatCapacity)
                {
                    // thermal control system is set to maximum heat capacity
                    CurrentHeat = ThermalControlData.HeatCapacity;

                    // turn the heat outline of the structure on
                    structure.showOutline(true);

                    // set overheated
                    _Overheated = true;
                }
            }
		}
		
		public void radiateHeat(float heat)
		{
			CurrentHeat -= heat;
			
			if (CurrentHeat < 0)
			{
				CurrentHeat = 0;
			}
		}
		
		public override void update()
		{
			base.update();

            if (Input.GetKeyDown(ThermalControlData.DebugKey))
            {
                structure.Call_ActivateUltimate(this, new UltimateEventArgs(structure));
            }
        }

        protected void OnUltimateActivated(object sender, UltimateEventArgs args)
        {
            if (DeviceData.ActiveOn == true)
            {
                CurrentHeat = 0;
                _Overheated = false;

                structure.showOutline(false);
            }
        }
	}
}