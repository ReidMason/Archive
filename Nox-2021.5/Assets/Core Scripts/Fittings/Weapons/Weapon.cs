using UnityEngine;
using System;

using NoxCore.Controllers;
using NoxCore.Data.Fittings;
using NoxCore.Debugs;
using NoxCore.Fittings.Modules;
using NoxCore.GameModes;
using NoxCore.Managers;
using NoxCore.Placeables.Ships;
using NoxCore.Utilities;

namespace NoxCore.Fittings.Weapons
{
	#region EventArgs
	public class WeaponFiredEventArgs : EventArgs
	{
		public Weapon weaponFired;

		public WeaponFiredEventArgs(Weapon weaponFired)
		{
			this.weaponFired = weaponFired;
		}
	}
	#endregion

	public abstract class Weapon : Module, IWeapon, IAmmoDebuggable
	{
		#region variables
		[Header("Weapon")]

		[NonSerialized]
		protected WeaponData _weaponData;
		public WeaponData WeaponData { get { return _weaponData; } set { _weaponData = value; } }

		[ShowOnly]
		protected float _Ammo;
		public float Ammo
		{
			get { return _Ammo; }
			set
			{
				if (WeaponData.MaxAmmo != -1)
				{
					_Ammo = Mathf.Clamp(value, 0, WeaponData.MaxAmmo);
				}
			}
		}

		protected FireGroup fireGroup;
		public FireGroup FireGroup { get { return fireGroup; } set { fireGroup = value; } }
		
		protected int minRangeSqr, maxRangeSqr;
		
		protected bool firing;

        [ShowOnly]
        protected float _FireTimer;
		public float FireTimer { get { return _FireTimer; } set { _FireTimer = value; } }
		
		protected AudioSource audioSource;
        #endregion

        #region delegates
        //Inherits from Device & Module

        public delegate void WeaponEventDispatcher(object sender, WeaponFiredEventArgs args);
        public event WeaponEventDispatcher WeaponFired;
        public event WeaponEventDispatcher WeaponAmmoEmpty;
        public event WeaponEventDispatcher WeaponAmmoFull;
        #endregion

        #region initialize and reset
        public override void init(DeviceData deviceData = null)
		{
			WeaponData = deviceData as WeaponData;

			base.init(deviceData);
			
			if (WeaponData.MaxAmmo != -1)
			{
				Ammo = WeaponData.MaxAmmo;
			}

			fireGroup = structure.FireControl.findFireGroup(this);

			audioSource = GetComponent<AudioSource>();

			requiredSocketTypes.Remove("MODULEBAY");
			requiredSocketTypes.Add("WEAPONBAY");
		}
		
		public override void reset()
		{
			base.reset();
			
			if (WeaponData.MaxAmmo != -1)
			{
				Ammo = WeaponData.MaxAmmo;
			}
			
			firing = false;
			FireTimer = 0;
		}
        #endregion

        #region ammo
        public void changeAmmoAmount(float amount)
		{
			if (WeaponData.MaxAmmo != -1)
			{
				Ammo += amount;
				Ammo = Mathf.Clamp(Ammo, 0, WeaponData.MaxAmmo);			
			}
		}
		
		public void increaseAmmo(float amount)
		{
			if (WeaponData.MaxAmmo != -1 && amount > 0)
			{
				Ammo += amount;
				Ammo = Mathf.Clamp(Ammo, 0, WeaponData.MaxAmmo);
                if (Ammo == WeaponData.MaxAmmo) Call_WeaponAmmoFull();
            }
		}
		
		public void decreaseAmmo(float amount)
		{
			if (WeaponData.MaxAmmo != -1 && amount > 0)
			{
				Ammo -= amount;
				Ammo = Mathf.Clamp(Ammo, 0, WeaponData.MaxAmmo);
                if (Ammo == 0) Call_WeaponAmmoEmpty();
            }
		}	
		
		public void changeMaxAmmoAmount(float amount)
		{
			if (amount >= -1)
			{
				WeaponData.MaxAmmo = amount;
			}
		}
		
		public void increaseMaxAmmo(float amount)
		{
			if (amount > 0)
			{
				if (WeaponData.MaxAmmo != -1)
				{
					WeaponData.MaxAmmo += amount;
				}
				else
				{
					WeaponData.MaxAmmo = amount;
				}
			}
		}
		
		public void decreaseMaxAmmo(float amount)
		{
			if (WeaponData.MaxAmmo > 0)
			{
				WeaponData.MaxAmmo -= amount;
				
				if (WeaponData.MaxAmmo < 0)
				{
					WeaponData.MaxAmmo = 0;
				}
			}
		}
        #endregion

        #region destroy/explode
        public override void destroy()
		{
			firing = false;
			FireTimer = 0;
			
			base.destroy();			
		}
		
		public override void explode(int repeatedNumExplosions = 0)
		{				            
			firing = false;
			FireTimer = 0;
			
			base.explode(repeatedNumExplosions);
		}
        #endregion

        #region Getters
        public bool isFiring()
		{
			return firing;
		}

        public virtual Transform getFirePoint()
        {
            return transform;
        }

        public virtual float getDamage()
		{
			return WeaponData.BaseDamage;
		}
        
        public virtual float getDPS(GameObject go = null)
		{
            if (go != null && gameObject != null)
            {
                float distToTarget = Vector2.Distance(go.transform.position, gameObject.transform.position);

                if (distToTarget < WeaponData.MinRange || distToTarget > WeaponData.MaxRange)
                {
                    return 0;
                }
            }

            if (WeaponData.FireRate > 0)
            {
                return WeaponData.BaseDamage / WeaponData.FireRate;
            }
            else
            {
                return WeaponData.BaseDamage;
            }
        }	
		
		protected virtual bool canFire()
		{
			Ship ship = structure as Ship;

			if (ship != null && ship.shipState == ShipState.UNKNOWN && ship.spawnedIn == true) return false;

			if (!firing && WeaponData.AutoFire == true)
			{
				bool ammoOK = true;
				
				// check ammo
				if (WeaponData.MaxAmmo != -1 && Ammo == 0)
				{
					ammoOK = false;
				}
				
				// if ammo and required power ok
				if (ammoOK == true && Powergrid.getCurrentPower() >= WeaponData.PowerPerShot)
				{
					return true;
				}
			}
			
			return false;
		}
        #endregion

        #region fire
        protected virtual void fired()
		{
			if (WeaponData.MaxAmmo != -1)
			{
				decreaseAmmo(1);
			}
			
			Powergrid.consumePower(WeaponData.PowerPerShot);

            // add the heat generated to the thermal control system on the structure
            structure.thermalcontrol.addHeat(this, WeaponData.HeatPerShot);
			
			if (audioSource != null && audioSource.clip != null)
			{
				if (structure.transform == GameManager.Instance.MainCamera.followTarget)
				{
					audioSource.PlayOneShot(audioSource.clip);
				}
			} 
			
			firing = true;

            Call_WeaponFired();
        }
		
		public virtual bool fire()
		{
			if (canFire() == true)
			{
				fired();
				return true;
			}
			
			return false;
		}
        #endregion

        public override void update()
		{
			base.update();
		}

        #region debug
        public void debugAmmoMaximise(object sender, DebugEventArgs args)
		{
			Ammo = WeaponData.MaxAmmo;
		}
		
		public void debugAmmoMinimise(object sender, DebugEventArgs args)
		{
			Ammo = 0;
		}
		
		public void debugAmmoIncrease(object sender, DebugEventArgs args, int amount)
		{
			increaseAmmo(amount);
		}
		
		public void debugAmmoDecrease(object sender, DebugEventArgs args, int amount)
		{
			decreaseAmmo(amount);
		}
		
		public void debugAmmoIncrement(object sender, DebugEventArgs args)
		{
			increaseAmmo(1);
		}
		
		public void debugAmmoDecrement(object sender, DebugEventArgs args)
		{
			decreaseAmmo(1);
		}
        #endregion

        #region Event Dispatchers
        public void Call_WeaponFired()
        {
            if (WeaponFired != null)
            {
                WeaponFired(this, new WeaponFiredEventArgs(this));
            }
        }
        public void Call_WeaponAmmoFull()
        {
            if (WeaponAmmoFull != null)
            {
                WeaponAmmoFull(this, new WeaponFiredEventArgs(this));
            }
        }
        public void Call_WeaponAmmoEmpty()
        {
            if (WeaponAmmoEmpty != null)
            {
                WeaponAmmoEmpty(this, new WeaponFiredEventArgs(this));
            }
        } 
        #endregion
    }
}