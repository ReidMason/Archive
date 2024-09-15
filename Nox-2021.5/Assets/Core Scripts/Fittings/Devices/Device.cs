using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using NoxCore.Debugs;
using NoxCore.Effects;
using NoxCore.GameModes;
using NoxCore.GUIs;
using NoxCore.Placeables;
using NoxCore.Utilities;
using NoxCore.Data.Fittings;

namespace NoxCore.Fittings.Devices
{
    public abstract class Device : MonoBehaviour, IDevice, ISystemDebuggable
	{
		#region variables

		[NonSerialized]
		protected DeviceData _deviceData;
		public DeviceData DeviceData { get { return _deviceData; } set { _deviceData = value; } }
		
        protected Structure structure;
		
		protected IPowerGrid _Powergrid;
        public IPowerGrid Powergrid { get{ return _Powergrid; } set { _Powergrid = value; } }
		
		protected IThermalControl _ThermalControl;
		public IThermalControl ThermalControl { get { return _ThermalControl; } set { _ThermalControl = value; } }

        // TODO - check if you want a separate component that stores this list instead
        protected List<IVisualEffect> vfxs;
						
		protected bool isFlippingActive = false;
        [Header("Active Status")]

		[ShowOnly]
		public float activatingTimer = 0.0f;

		[ShowOnly]
		public float deactivatingTimer = 0.0f;

		[ShowOnly]
		public bool destroyed;
        #endregion

        #region delegates
        //Can override in children and/or subscribe to as AI
        public delegate void DeviceDelegates(Device sender);
        public event DeviceDelegates DeactivateAlert;
        public event DeviceDelegates ActivateAlert;
        public event DeviceDelegates DestroyAlert;
        public event DeviceDelegates ResetAlert;       
        #endregion

        #region initialization
        public virtual void init(DeviceData deviceData = null)
		{
			//D.log ("Device", "Initialised device/module/weapon " + DeviceName);

			DeviceData = deviceData as DeviceData;

			isFlippingActive = false;
			
			activatingTimer = 0;
			deactivatingTimer = 0;

			DeviceData.ActiveOnSpawn = DeviceData.ActiveOn;
			
			destroyed = false;

            vfxs = new List<IVisualEffect>();

            Transform[] allChildren = GetComponentsInChildren<Transform>();
            foreach (Transform child in allChildren)
            {
                IVisualEffect[] vfxComponents = child.GetComponents<IVisualEffect>();

                if (vfxComponents.Length > 0)
                {
                    vfxs.AddRange(vfxComponents);
                }
            }
        }
        #endregion       

        #region respawn and destruction
        public virtual void reset()
		{
			isFlippingActive = false;
			activatingTimer = 0;
			deactivatingTimer = 0;
			DeviceData.ActiveOn = DeviceData.ActiveOnSpawn;
			destroyed = false;
            Call_Reset();
		}
		
		public virtual void destroy()
		{
			destroyed = true;
			DeviceData.ActiveOn = false;
            Call_Destroy();
		}
		
		public bool isDestroyed()
		{
			return destroyed;
		}
        #endregion

        #region getters
        public GameObject getGameObject()
        {
            return gameObject;
        }
        
        public Structure getStructure()
		{
			return structure;
		}
		
		public void setStructure(Structure structure)
		{
			this.structure = structure;            
        }

        public List<IVisualEffect> getVFXs()
        {
            return vfxs;
        }
        #endregion

        #region fitting
        public virtual void postFitting()
		{            
            Powergrid = structure.powergrid;
			ThermalControl = structure.thermalcontrol;
		}
        #endregion

        #region activation
        public bool isActiveOn()
		{
			return DeviceData.ActiveOn;
		}

        public bool isActiveOnSpawn()
        {
            return DeviceData.ActiveOnSpawn;
        }

        public void setActiveOn(bool activeOn)
		{
			DeviceData.ActiveOn = activeOn;
		}
		
		public void setActiveOnSpawn(bool activeOnSpawn)
		{
			DeviceData.ActiveOnSpawn = activeOnSpawn;
		}
		
		public string getState()
		{
			if (DeviceData.ActiveOn)
			{
				return " ACTIVE ";
			}
			
			return " INACTIVE ";
		}
		
		public bool isFlippingActivation()
		{
			return isFlippingActive;
		}	
        
		public void activate()
		{
			if (DeviceData.ActiveOn == false && isFlippingActivation() == false && destroyed == false)
			{
				if (gameObject.activeInHierarchy == true)
				{
					StartCoroutine(activateDevice());
				}
				else
				{
					activatingTimer = 0;
					DeviceData.ActiveOn = true;
					isFlippingActive = false;
					// D.log ("Device", DeviceName + " on " + structure.gameObject.name + " is now active");
					Call_Activate();
				}
			}
		}
	
		private IEnumerator activateDevice()
		{
			// D.log("Device", "Activating " + DeviceName + " on " + structure.gameObject.name + " in " + activatingDelay + " seconds");
			
			while (DeviceData.ActiveOn == false)
			{
				if (activatingTimer < DeviceData.ActivatingDelay)
				{
					isFlippingActive = true;
					activatingTimer += Time.deltaTime;
					//// D.log ("Device", "Activating timer at " + activatingTimer + "   Delay until " + activatingDelay);
					yield return null;
				}
				else
				{
					activatingTimer = 0;
					DeviceData.ActiveOn = true;
                    isFlippingActive = false;
                    // D.log ("Device", DeviceName + " on " + structure.gameObject.name + " is now active");
                    Call_Activate();
				}
			}
		}
		
		public void deactivate()
		{
			if (DeviceData.ActiveOn == true && isFlippingActivation() == false && destroyed == false)
			{
				if (gameObject.activeInHierarchy == true)
				{
					StartCoroutine(deactivateDevice());
				}
				else
				{
					deactivatingTimer = 0;
					DeviceData.ActiveOn = false;
					isFlippingActive = false;
					// D.log ("Device", DeviceName + " on " + structure.gameObject.name + " is now inactive");
					Call_Deactivate();
				}
			}
		}
		
		private IEnumerator deactivateDevice()
		{
			// D.log("Device", "Deactivating " + DeviceName + " on " + structure.gameObject.name + " in " + deactivatingDelay + " seconds");
			
			while (DeviceData.ActiveOn == true)
			{
				if (deactivatingTimer < DeviceData.DeactivatingDelay)
				{
                    isFlippingActive = true;
                    deactivatingTimer += Time.deltaTime;
					// D.log("Device", "Deactivating timer at " + deactivatingTimer + "   Delay until " + deactivatingDelay);
					yield return null;
				}
				else
				{
					deactivatingTimer = 0;
					DeviceData.ActiveOn = false;
                    isFlippingActive = false;
                    // D.log ("Device", DeviceName + " on " + structure.gameObject.name + " is now inactive");
                    Call_Deactivate();
				}
			}
		}
        #endregion

        #region power systems
        public float getRequiredPower()
		{
			return DeviceData.RequiredPower;
		}
		
        #endregion

        #region heat systems
        public float getActiveHeat()
		{
			return DeviceData.ActiveHeat;
		}
		
        #endregion

        #region update
        public virtual void update()
		{
			bool powerDevice = true;
            float currentHeat = 0.0f;

            // attempt to supply power to the device if active or becoming so
            if (DeviceData.ActiveOn == true || (DeviceData.ActiveOn == false && isFlippingActive == true))
			{
				// attempt to power device
				if (DeviceData.RequiredPower > 0)
				{
					// check there is enough power to activate an inactive module
					if (DeviceData.ActiveOn == false)
					{
						if (getRequiredPower() * deactivatingTimer > _Powergrid.getCurrentPower())
						{
							powerDevice = false;
						}
					}
					
					// is there enough power to run the device?
					if (powerDevice == true && _Powergrid.supplyPower(this) > 0)
					{
                        if (DeviceData.ActiveOn == false) activate();
                        else
                        {
                            //// D.log("Power", "Powering device");

                            // active module heating
                            currentHeat = (DeviceData.ActiveHeat * Time.deltaTime);
                        }
					}
					else
					{
                        //// D.log("Power", "Could not power device");
                        if (DeviceData.ActiveOn == true) deactivate();
					}
				}
				else
				{
					//// D.log("Power", "Device needs no power to function");
					
					// active module heating (dump to TCS)
					currentHeat = (DeviceData.ActiveHeat * Time.deltaTime);					
				}
			}
			
            // attempt to dump excess device heat into thermal control system
            if (_ThermalControl != null)
            {
                _ThermalControl.addHeat(this, currentHeat);
            }
		}
        #endregion

        #region debug
        public virtual void debugMaximise(object sender, DebugEventArgs args)
		{}
		
		public virtual void debugMinimise(object sender, DebugEventArgs args)
		{}
		
		public virtual void debugIncrease(object sender, DebugEventArgs args, int amount)
		{}
		
		public virtual void debugDecrease(object sender, DebugEventArgs args, int amount)
		{}
		
		public virtual void debugExplode(object sender, DebugEventArgs args)
		{
			destroy();
            NoxGUI.Instance.setMessage("DEBUG: " + DeviceData.Type + ":" + DeviceData.SubType + " has been destroyed");
		}
		
		public virtual void debugActivate(object sender, DebugEventArgs args)
		{
			activate();
            NoxGUI.Instance.setMessage("DEBUG: " + DeviceData.Type + ":" + DeviceData.SubType + " has been activated");
		}
		
		public virtual void debugDeactivate(object sender, DebugEventArgs args)
		{
			deactivate();
            NoxGUI.Instance.setMessage("DEBUG: " + DeviceData.Type + ":" + DeviceData.SubType + " has been deactivated");
		}
        #endregion

        #region event dispatchers
        //Check if an event exists/is subscribed to the delegate before calling it
        public void Call_Deactivate()
        {
            if (DeactivateAlert != null)
            {
                DeactivateAlert(this);
            }
        }

        public void Call_Activate()
        {
            if (ActivateAlert != null)
            {
                ActivateAlert(this);
            }
        }

        public void Call_Destroy()
        {
            if (DestroyAlert != null)
            {
                DestroyAlert(this);
            }
        }

        public void Call_Reset()
        {
            if (ResetAlert != null)
            {
                ResetAlert(this);
            }
        }
        #endregion

        #region event handlers
        public virtual void OnDeactivated(Device sender) { /* Debug.Log(this + " has been deactivated"); */ }
        public virtual void OnActivated(Device sender) { /* Debug.Log(this + " has been activated"); */ }
        public virtual void OnDestroyed(Device sender) { /* Debug.Log(this + " has been destroyed"); */ }
        public virtual void OnReset(Device sender) { /* Debug.Log(this + " has been reset"); */}
        #endregion
	}
}