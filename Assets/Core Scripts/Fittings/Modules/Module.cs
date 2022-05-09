using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using NoxCore.Data.Fittings;
using NoxCore.Debugs;
using NoxCore.Effects;
using NoxCore.Fittings.Devices;
using NoxCore.GameModes;
using NoxCore.GUIs;
using NoxCore.Placeables;
using NoxCore.Fittings.Sockets;
using NoxCore.Utilities;

//http://unitypatterns.com/resource/objectpool/

namespace NoxCore.Fittings.Modules
{
	public abstract class Module : Device, IModule, ISystemDebuggable
	{
		#region variables
		protected List<string> requiredSocketTypes = new List<string>();

		protected StructureSocket _Socket;
		public StructureSocket Socket { get { return _Socket; } set { _Socket = value; } }

        [Header("Module")]

		[NonSerialized]
		protected ModuleData _moduleData;
		public ModuleData ModuleData { get { return _moduleData; } set { _moduleData = value; } }

		[ShowOnly]
		protected float _Armour;
		public float Armour { get { return _Armour; } set { _Armour = value; } }

        protected ExplosionVFXController explosionVFXController;

		protected SpriteRenderer fittingRenderer;
		public SpriteRenderer FittingRenderer {  get { return fittingRenderer; } set { fittingRenderer = value; } }
        #endregion

        #region initialize, reset and fittings
        public override void init(DeviceData deviceData = null)
		{
			ModuleData = deviceData as ModuleData;

			base.init(deviceData);
			
			Armour = ModuleData.MaxArmour;

			requiredSocketTypes.Add("MODULEBAY");

			FittingRenderer = GetComponent<SpriteRenderer>();
        }
		
		public override void reset()
		{
			base.reset();
			
			Armour = ModuleData.MaxArmour;
		}
		
		public override void postFitting()
		{
			base.postFitting();
			
			Socket = gameObject.transform.parent.GetComponent<StructureSocket>();	
			
			TurretSocket turretSocket = Socket as TurretSocket;
			
			if (turretSocket != null)
			{
				if (turretSocket.fixedFiringArc == true)
				{					
					gameObject.transform.rotation = turretSocket.transform.rotation;
				}
			}
		}
        #endregion

        #region getters
        public Vector2 getPosition()
		{
			return (Vector2)(gameObject.transform.position);
		}
		
        public StructureSocket getSocket()
        {
            return Socket;
        }

		public List<string> getSocketTypes()
		{
			return requiredSocketTypes;
		}
        #endregion

        #region armour
        public void resetArmour()
		{
			Armour = ModuleData.MaxArmour;
		}       

		public void setArmour(float amount)
		{
			Armour = amount;
			Armour = Mathf.Clamp(Armour, 0, ModuleData.MaxArmour);
		}

        public float getArmour()
        {
            return Armour;
        }

        public void increaseArmour(float amount)
		{
			if (Armour < ModuleData.MaxArmour && isDestroyed() == false)
			{
				Armour = Mathf.Min(Armour + amount, ModuleData.MaxArmour);
			}			
		}
		
		public (bool destroyed, float damageOnDestroy) decreaseArmour(float amount)
		{
			if (Armour > 0 && isDestroyed() == false)
			{
				Armour = Mathf.Max(Armour - amount, 0);
				
				if (Armour == 0)
				{
					destroy();
					
					return (true, ModuleData.DamageOnDestroy);
				}
			}
			
			return (false, 0.0f);
		}

        public float getMaxArmour()
        {
            return ModuleData.MaxArmour;
        }

        public void setMaxArmour(float amount)
        {
			ModuleData.MaxArmour = amount;
        }
        #endregion

        #region destroy/explode
        //destroy delegate called in Device parent
        // this is the same as explode but without spawning an explosion (if present)
        public override void destroy()
		{
			// note: should already be 0 if destroyed naturally
			Armour = 0;
			
			foreach (SpriteRenderer moduleRenderer in GetComponentsInChildren<SpriteRenderer>())
			{
				moduleRenderer.enabled = false;
			}

            if (ModuleData.Explosion != null)
            {
                GameObject clonedExplosion = ModuleData.Explosion.Spawn(transform);

                explosionVFXController = clonedExplosion.GetComponent<ExplosionVFXController>();

                // make any changes to the explosion here (if any)
                explosionVFXController.setSortingLayerOrder(transform);
            }

            base.destroy();			
		}

        protected IEnumerator repeatedExplosion(int numRepeats)
        {
            int numExplosions = 0;

            while (true)
            {
                GameObject clonedExplosion = ModuleData.Explosion.Spawn(transform);

                explosionVFXController = clonedExplosion.GetComponent<ExplosionVFXController>();

                // make any changes to the explosion here (if any)
                explosionVFXController.setSortingLayerOrder(transform);

                numExplosions++;

                if (numExplosions == numRepeats) break;

                yield return new WaitForSeconds(UnityEngine.Random.Range(1.0f, 2.0f));
            }
        }

        public virtual void explode(int repeatedNumExplosions = 0)
		{
            if (ModuleData.Explosion != null)
            {
                if (repeatedNumExplosions > 0)
                {
                    if (gameObject.activeInHierarchy == true)
                    {
                        StartCoroutine(repeatedExplosion(repeatedNumExplosions));
                    }
                }
                else
                {
                    GameObject clonedExplosion = ModuleData.Explosion.Spawn(transform);

                    explosionVFXController = clonedExplosion.GetComponent<ExplosionVFXController>();

                    // make any changes to the explosion here (if any)
                    explosionVFXController.setSortingLayerOrder(transform);
                }
            }
		}
        #endregion

        #region debug
        public override void debugMaximise(object sender, DebugEventArgs args)
		{
			Armour = ModuleData.MaxArmour;
            NoxGUI.Instance.setMessage("DEBUG: " + DeviceData.Type + ":" + DeviceData.SubType + " armour at maximum");
		}
		
		public override void debugMinimise(object sender, DebugEventArgs args)
		{
			Armour = 0;
            NoxGUI.Instance.setMessage("DEBUG: " + DeviceData.Type + ":" + DeviceData.SubType + " armour at minimum");			
		}
		
		public override void debugIncrease(object sender, DebugEventArgs args, int amount)
		{
			Armour = Mathf.Clamp(Armour + amount, 0, ModuleData.MaxArmour);
            NoxGUI.Instance.setMessage("DEBUG: " + DeviceData.Type + ":" + DeviceData.SubType + " armour increased by " + amount);				
		}
		
		public override void debugDecrease(object sender, DebugEventArgs args, int amount)
		{
			Armour = Mathf.Clamp(Armour - amount, 0, ModuleData.MaxArmour);
            NoxGUI.Instance.setMessage("DEBUG: " + DeviceData.Type + ":" + DeviceData.SubType + " armour increased by " + amount);				
		}
		
		public override void debugExplode(object sender, DebugEventArgs args)
		{
            destroy();

            structure.Call_ModuleDamaged(this, new ModuleDamageEventArgs(null, structure, this, Mathf.Infinity, true));

            NoxGUI.Instance.setMessage("DEBUG: " + DeviceData.Type + ":" + DeviceData.SubType + " has been destroyed");
		}
		
		public override void debugActivate(object sender, DebugEventArgs args)
		{
			activate();
            NoxGUI.Instance.setMessage("DEBUG: " + DeviceData.Type + ":" + DeviceData.SubType + " has been activated");
		}
		
		public override void debugDeactivate(object sender, DebugEventArgs args)
		{
			deactivate();
            NoxGUI.Instance.setMessage("DEBUG: " + DeviceData.Type + ":" + DeviceData.SubType + " has been deactivated");
		}
        #endregion

        #region event dispatchers
        //inherited from Device
        #endregion

        #region event handlers
        //inherited from Device       
        #endregion
    }
}