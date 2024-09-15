using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Data.Fittings;
using NoxCore.Debugs;
using NoxCore.Fittings.Modules;
using NoxCore.GameModes;
using NoxCore.GUIs;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;

using Davin.Buffs;
using Davin.Data.Fittings;

namespace Davin.Fittings.Modules
{
	public class CloakingDevice : Module, ICloakingDevice, ICloakDebuggable
	{
		public CloakingDeviceData __cloakingDeviceData;
		[NonSerialized]
		protected CloakingDeviceData _cloakingDeviceData;
		public CloakingDeviceData CloakingDeviceData { get { return _cloakingDeviceData; } set { _cloakingDeviceData = value; } }

		protected Structure cloakedStation, cloakedShip;
		protected bool cloakedIsShip;
		protected float cloakActiveTimer, cloakInactiveTimer;	
		
		[SerializeField]
		protected bool _cloakActive;
		public bool cloakActive { get { return _cloakActive; } set { _cloakActive = value; } }
		
		protected bool flippingCloak = false;		
		
		protected List<IEngine> engines = new List<IEngine>();	
		protected List<IShieldGenerator> shieldGenerators = new List<IShieldGenerator>();
		protected Transform shieldMesh;

		protected MaxSpeedBuff maxSpeedBuff;

		protected int origLayer;
		
		public override void init(DeviceData deviceData = null)
		{
			if (deviceData == null)
			{
				CloakingDeviceData = Instantiate(__cloakingDeviceData);
				base.init(CloakingDeviceData);
			}
			else
			{
				CloakingDeviceData = deviceData as CloakingDeviceData;
				base.init(deviceData);
			}

			// ship or structure?
			Ship ship = structure as Ship;
			
			if (ship != null)
			{
				cloakedIsShip = true;
				cloakedShip = ship;
				origLayer = LayerMask.NameToLayer("Ship");
			}
			else
			{
				cloakedIsShip = false;
				cloakedStation = structure;
				origLayer = LayerMask.NameToLayer("Structure");
			}
			
			flippingCloak = false;
			cloakActiveTimer = 0;
			cloakInactiveTimer = 0;
			
			requiredSocketTypes.Add("CLOAKBAY");
		}
		
		public override void reset()
		{
			base.reset();
			
			flippingCloak = false;
			cloakActiveTimer = 0;
			cloakInactiveTimer = 0;		
			
			activateCloakImmediately();	
		}
		
		public override void postFitting()
		{
			base.postFitting();

			if (cloakedIsShip == true)
			{
				// get references to all engines on the structure
				foreach (Module module in structure.Modules)
				{
					IEngine engine = module as IEngine;

					if (engine != null)
					{
						engines.Add(engine);
					}
				}
			}
			
			// get references to all shield generators on the structure
			foreach(Module module in structure.Modules)
			{
				IShieldGenerator shieldGenerator = module as IShieldGenerator;
				
				if (shieldGenerator != null)
				{
					shieldGenerators.Add(shieldGenerator);
				}
			}
			
			shieldMesh = getStructure().gameObject.transform.Find("ShieldMesh");		
		}
		
		public bool isCloakActive()
		{
			return cloakActive;
		}		
		
		public bool isFlippingCloak()
		{
			return flippingCloak;
		}
		
		protected void disable()
		{
			/*
			// turn off cloak
			int index = 0;
			
			// change structure and module renderer materials back to originals
			foreach(MeshRenderer renderer in structure.GetComponentsInChildren<MeshRenderer>())
			{
				if (renderer.tag == "Ship" || renderer.tag == "Structure" || renderer.tag == "Module" || renderer.tag == "Mesh")
				{
					Material [] origRendererMaterials = new Material[renderer.materials.Length];
					
					for(int i = 0; i < renderer.materials.Length; i++)
					{
						origRendererMaterials[i] = origMaterials[index];
						index++;
					}					
					
					renderer.materials = origRendererMaterials;
				}				
			}
			*/

			structure.enableAllRenderers();
			
			// reset layer for GameObject			
			structure.gameObject.layer = origLayer;			
		}
		
		protected void enable()
		{
			// turn on cloak
			/*
			    // change structure and module renderer materials back to cloaked material
				foreach (Renderer renderer in structure.gameObject.GetComponentsInChildren<Renderer>())
				{
					if (renderer.tag == "Ship" || renderer.tag == "Structure" || renderer.tag == "Module" || renderer.tag == "Mesh")
					{
						Material[] cloakMaterials = new Material[renderer.materials.Length];

						for (int i = 0; i < renderer.materials.Length; i++)
						{
							cloakMaterials[i] = cloakMaterial;
						}

						renderer.materials = cloakMaterials;
					}
				} 
			*/

			structure.disableAllRenderers();

			// change layer for GameObject
			structure.gameObject.layer = LayerMask.NameToLayer("Cloaked");
		}

		public void activateCloakImmediately()
		{
			structure.enableAllRenderers();

			enable();

			if (cloakedIsShip == true && maxSpeedBuff == null)
			{
				maxSpeedBuff = new MaxSpeedBuff(engines, CloakingDeviceData.BuffData);

				cloakedShip.BuffManager.addBuff(maxSpeedBuff);
			}

			foreach (IShieldGenerator shieldGenerator in structure.shields)
			{
				shieldGenerator.disable();
			}
		}

		public void activateCloak()
		{
			if (structure.structureSize > CloakingDeviceData.MaxStructureSize) return;
			
			if (isActiveOn() == true && isFlippingActivation() == false && isFlippingCloak() == false && cloakActive == false && destroyed == false)
			{			
				// tell all shield generators to lower shields (if shield is up)
				foreach(IShieldGenerator shieldGenerator in shieldGenerators)
				{
					// Debug.Log("Lowering all shields");
					shieldGenerator.lowerShield();
				}

                if (cloakedIsShip == true && maxSpeedBuff == null)
                {
					maxSpeedBuff = new MaxSpeedBuff(engines, CloakingDeviceData.BuffData);

					cloakedShip.BuffManager.addBuff(maxSpeedBuff);
				}
				
				StartCoroutine (beginActivatingCloak());
			}
		}	
		
		protected IEnumerator beginActivatingCloak()
		{				
			bool allShieldsLowered;
			
			flippingCloak = true;
			
			// wait for all shields to lower
			do
			{
				// assume true and let the loop alter this
				allShieldsLowered = true;
				
				foreach(IShieldGenerator shieldGenerator in shieldGenerators)
				{
					// Debug.Log("SHIELD OFF: " + shieldGenerator);
					if (shieldGenerator.isShieldUp() == true)
					{						
						allShieldsLowered = false;
						yield return new WaitForEndOfFrame();
						break;
					}					
				}
			}
			while(allShieldsLowered == false);
			
			// wait for the cloaking device to activate
			while (cloakActive == false)
			{
				if (!(isActiveOn() == true && isFlippingActivation() == false))
				{
					cloakActiveTimer = 0;
					cloakActive = false;
					flippingCloak = false;
					break;
				}                
				
				if (cloakActiveTimer < CloakingDeviceData.CloakDelay)
				{
					cloakActiveTimer += Time.deltaTime;
					yield return null;
				}
				else
				{
					cloakActiveTimer = 0;
					cloakActive = true;
					
					enable();
					
					flippingCloak = false;
				}
			}
		}

		public void deactivateCloakImmediately()
		{
			cloakActive = false;

			disable();

			// tell all shield generators to attempt to raise shields
			foreach (IShieldGenerator shieldGenerator in shieldGenerators)
			{
				shieldGenerator.raiseShield();
			}

			if (cloakedIsShip == true && maxSpeedBuff != null)
			{
				cloakedShip.BuffManager.removeBuff(maxSpeedBuff);
				maxSpeedBuff = null;
			}
		}

		public void deactivateCloak()
		{
			if (isActiveOn() == true && isFlippingActivation() == false && isFlippingCloak() == false && cloakActive == true && destroyed == false)
			{	
				StartCoroutine (beginDeactivatingCloak());
			}
		}
		
		protected IEnumerator beginDeactivatingCloak()
		{			
			flippingCloak = true;
			
			// wait for the cloaking device to deactivate
			while (cloakActive == true)
			{
				if (cloakInactiveTimer < CloakingDeviceData.CloakDelay)
				{
					cloakInactiveTimer += Time.deltaTime;
					yield return null;
				}
				else
				{
					cloakInactiveTimer = 0;
					cloakActive = false;
					
					disable();
					
					flippingCloak = false;
				}
			}	
			
			// tell all shield generators to attempt to raise shields
			foreach(IShieldGenerator shieldGenerator in shieldGenerators)
			{
				shieldGenerator.raiseShield();
			}

			if (cloakedIsShip == true && maxSpeedBuff != null)
			{
				cloakedShip.BuffManager.removeBuff(maxSpeedBuff);
				maxSpeedBuff = null;
			}
		}	
		
		public override void destroy()
		{
			disable();
			
			base.destroy();
		}
		
		public override void explode(int repeatedNumExplosions = 0)
		{
			disable();
			
			base.explode(repeatedNumExplosions);
		}   
		
		public override void update()
		{
			base.update();
		}	
		
		public void debugCloak(object sender, DebugEventArgs args)
		{
			activateCloak();
            NoxGUI.Instance.setMessage("DEBUG: " + DeviceData.Type + ":" + DeviceData.SubType + " is being activated");
		}		
		
		public void debugDecloak(object sender, DebugEventArgs args)
		{
			deactivateCloak();
            NoxGUI.Instance.setMessage("DEBUG: " + DeviceData.Type + ":" + DeviceData.SubType + " is being deactivated");
		}					
	}
}
