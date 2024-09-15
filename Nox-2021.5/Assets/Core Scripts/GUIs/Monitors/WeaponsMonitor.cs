using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System;
using NoxCore.Utilities;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Fittings;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Weapons;
using NoxCore.Controllers;

namespace NoxCore.GUIs
{
    public class WeaponsMonitor : StructureMonitor
    {
        public override void init()
        {
            base.init();
            monitorName = "Weapons";
        }

        protected override void updateReadout(Structure camTarget)
        {
            base.updateReadout(camTarget);    
            
            // Weapons
            readoutInfo.Append("\n\nWeapons: ");

            foreach (Weapon weapon in camTarget.Weapons)
            {
                TargetableWeapon targetableWeapon = weapon as TargetableWeapon;
                
                if (targetableWeapon != null)
                {
                    (GameObject structure, GameObject system) target = targetableWeapon.Target.GetValueOrDefault();

                    if (target.structure == null)
                    {
                        readoutInfo.Append("\n" + targetableWeapon.gameObject.name + targetableWeapon.getState());
                    }
                    else
                    {
                        if (target.structure != null)
                        {
                            if (target.system == null)
                            {
                                readoutInfo.Append("\n" + targetableWeapon.gameObject.name + targetableWeapon.getState() + target.structure.name);
                            }
                            else
                            {
                                readoutInfo.Append("\n" + targetableWeapon.gameObject.name + targetableWeapon.getState() + target.structure.name + " " + target.system.name);
                            }
                        }
                        else
                        {
                            readoutInfo.Append("\n" + targetableWeapon.gameObject.name + targetableWeapon.getState());
                        }
                    }
                }
            }

            // Fire Group targets			
            if (camTarget.FireControl != null)
            {                
                foreach (FireGroup fireGroup in camTarget.FireControl.FireGroups)
                {
                    if (fireGroup.Target != null)
                    {
                        (GameObject structure, GameObject system) target = fireGroup.Target.GetValueOrDefault();

                        if (target.structure != null)
                        {
                            if (target.system == null)
                            {
                                readoutInfo.Append("\n" + fireGroup.Name + ":\t" + target.structure);
                            }
                            else
                            {
                                readoutInfo.Append("\n" + fireGroup.Name + ":\t" + target.structure.name + " - " + target.system.name);
                            }
                        }
                        else
                        {
                            readoutInfo.Append("\n" + fireGroup.Name + ":");
                        }
                    }
                    else
                    {
                        readoutInfo.Append("\n\nFire Groups:\n Are returning null. Fix this.");
                    }
                }                           
            }
        }
    }
}
