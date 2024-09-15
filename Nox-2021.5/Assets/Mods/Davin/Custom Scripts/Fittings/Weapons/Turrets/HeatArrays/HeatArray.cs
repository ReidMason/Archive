using UnityEngine;

using System.Collections.Generic;
using System;

using NoxCore.Data.Fittings;
using NoxCore.Effects;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Fittings.Weapons;
using NoxCore.Placeables;

using Davin.Data.Fittings;

namespace Davin.Fittings.Weapons
{
	public class HeatArray : RotatingTurret, IVisibleEffect, IHeatArray
	{
		public HeatArrayData __heatArrayData;
		[NonSerialized]
		protected HeatArrayData _heatArrayData;
		public HeatArrayData HeatArrayData { get { return _heatArrayData; } set { _heatArrayData = value; } }

		protected bool effectVisible;	
		protected bool destroyedTargetHit;
		
		public override void init(DeviceData deviceData = null)
		{
			if (deviceData == null)
			{
				HeatArrayData = Instantiate(__heatArrayData);
				base.init(HeatArrayData);
			}
			else
			{
				HeatArrayData = deviceData as HeatArrayData;
				base.init(deviceData);
			}

            requiredSocketTypes.Add("HEATARRAYBAY");
			
			Target = null;

            Projectile projectile = HeatArrayData.EffectPrefab.GetComponent<Projectile>();

            if (projectile != null)
            {
                projectile.init();
            }
        }	
		
		public override void reset()
		{
			base.reset();
			
			effectVisible = false;
		}
		
		public bool isEffectVisible()
		{
			return effectVisible;
		}
		
		protected override void fired()
		{
			base.fired();
		
			float heatDamage = getDamage();

            (GameObject structure, GameObject system) lockedTarget = LockedTarget.GetValueOrDefault();

			// do something to heat up the target
			if (lockedTarget.system == null)
			{						
				int numberOfModules = 0;
				
				List<StructureSocket> structureSockets = lockedTarget.structure.GetComponent<Structure>().StructureSockets;
				
				foreach(StructureSocket structureSocket in structureSockets)
				{
					ModuleSocket socket = structureSocket as ModuleSocket;
					if (socket.InstalledModule != null) numberOfModules++;
				}
				
				float averagedHeatDamage = heatDamage / numberOfModules;
				
				// heat up ship directly by averaging heat damage for each module							
				foreach(StructureSocket structureSocket in structureSockets)
				{
					ModuleSocket socket = structureSocket as ModuleSocket;
					
					if (socket.InstalledModule != null)
					{
						lockedTarget.structure.GetComponent<Structure>().thermalcontrol.addHeat(socket.InstalledModule, averagedHeatDamage);
					}
				}
			}
			else
			{
				// heat up targeted module directly
				lockedTarget.structure.GetComponent<Structure>().thermalcontrol.addHeat(lockedTarget.system.GetComponent<Module>(), heatDamage);
			}					
			
			// D.log("Ordnance", weapon.DeviceName + " on " + weapon.getStructure().gameObject.name + " has fired its heat ray");
		
			effectVisible = true;
		}
		
		public override void update()
		{
			base.update();
			
			if (isActiveOn() == true && isFlippingActivation() == false)
			{
				if (firing)
				{
					if (FireTimer >= WeaponData.FireRate)
					{
						firing = false;
						effectVisible = false;
						FireTimer = 0;
					}
					else
					{
						FireTimer += Time.deltaTime;
						
						if (FireTimer > HeatArrayData.EffectDuration)
						{
							effectVisible = false;
						}
					}
				}
				else if(LockedTarget != null && TargetIsLocked == true)
				{
					fire();
				}
			}
			else if(effectVisible == true)
			{
				effectVisible = false;
			}
		}
	}
}