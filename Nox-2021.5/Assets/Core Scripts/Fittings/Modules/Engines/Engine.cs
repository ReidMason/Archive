using UnityEngine;

using System;

using NoxCore.Data.Fittings;
using NoxCore.Fittings.Devices;
using NoxCore.Effects;
using NoxCore.Placeables.Ships;
using NoxCore.Utilities;

using com.spacepuppy;

namespace NoxCore.Fittings.Modules
{
	public class Engine : Module, IEngine
	{
        [Header("Engine")]

        public EngineData __engineData;
        [NonSerialized]
        [ShowNonSerializedProperty("RUNTIME VALUES")]
        protected EngineData _engineData;
        public EngineData EngineData { get { return _engineData; } set { _engineData = value; } }
		
		protected Ship ship;
        protected EngineVFXController engineVFXController;
        protected TrailRenderer trailRenderer;

        public override void init(DeviceData deviceData = null)
		{
            if (deviceData == null)
            {
                EngineData = Instantiate(__engineData);
                base.init(EngineData);
            }
            else
            {
                EngineData = deviceData as EngineData;
                base.init(deviceData);
            }

            ship = structure as Ship;

            Transform engineNozzleTrans = transform.Find("Engine Nozzle");

            if (engineNozzleTrans != null)
            {
                engineVFXController = engineNozzleTrans.GetComponent<EngineVFXController>();
                trailRenderer = engineNozzleTrans.GetComponent<TrailRenderer>();
            }

            requiredSocketTypes.Add("ENGINEBAY");

            DestroyAlert += OnDestroyed;
        }
		
		public float getMaxSpeed()
		{
			if (ship.silentRunning == true)
			{
				return EngineData.MaxSpeed * ship.silentRunningFactor;
			}
			else
			{
				return EngineData.MaxSpeed;
			}
		}

        public void setMaxSpeed(float maxSpeed)
        {
            if (ship.silentRunning == true)
            {
                EngineData.MaxSpeed = maxSpeed * ship.silentRunningFactor;
            }
            else
            {
                EngineData.MaxSpeed = maxSpeed;
            }
        }

        public void resetEngineTrails()
        {
            if (engineVFXController != null)
            {
                trailRenderer.Reset(this);
            }
        }

        public void restartEngineTrails()
        {
            if (engineVFXController != null)
            {
                engineVFXController.startVFX();                
            }
        }

        public override void destroy()
		{
            if (engineVFXController != null)
            {
                engineVFXController.stopVFX();
            }
			
			base.destroy();
		}
		
		public override void explode(int repeatedNumExplosions = 0)
		{
            if (engineVFXController != null)
            {
                engineVFXController.stopVFX();
            }

            base.explode(repeatedNumExplosions);
		}        
		
		public override void update()
		{
			base.update();

            bool trailOff = true;
		
			if (isActiveOn() == true && isFlippingActivation() == false)
			{
                if (destroyed == false)
                {
                    ship.MaxSpeed += getMaxSpeed();

                    if (engineVFXController != null)
                    {
                        if (ship.Helm != null && ship.Helm.totalForceActual > 0)
                        {
                            trailOff = false;
                            engineVFXController.updateVFX();
                        }
                    } 
				}
			}

            if (engineVFXController != null && engineVFXController.isRunning == true && trailOff == true)
            {
                engineVFXController.stopTrailEmission();
            }
		}

        public override void OnDestroyed(Device sender)
        {
            base.OnDestroyed(sender);

            ship.engines.Remove(this);

            if (ship.engines.Count == 0)
            {
                ship.AreEnginesDisabled = true;
            }
        }
    }
}