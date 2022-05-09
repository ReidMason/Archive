using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Linq;
using System.Text;

using NoxCore.Buffs;
using NoxCore.Builders;
using NoxCore.Controllers;
using NoxCore.Cameras;
using NoxCore.Debugs;
using NoxCore.Effects;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Fittings.Weapons;
using NoxCore.GameModes;
using NoxCore.GUIs;
using NoxCore.Managers;
using NoxCore.Placeables.Ships;
using NoxCore.Placeables;
using NoxCore.Stats;
using NoxCore.Utilities;
using NoxCore.Utilities.Geometry;

namespace Nox.Extensions
{
    public static class StructureExtensionsMethods
    {
        #region Shields
        public static void DrainShields(this Structure structure, float drainAmount)
        {
            if (structure.shields.Count > 0)
            {
                float remainingDamage = drainAmount;

                foreach (IShieldGenerator shield in structure.shields)
                {

                    if (shield.isShieldUp() == true)
                    {
                        float shieldStrength = shield.CurrentCharge;
                        
                        if (remainingDamage == 0)
                            continue;

                        if (remainingDamage > shieldStrength)
                        {
                            shield.failed();

                            remainingDamage -= shieldStrength;
                            remainingDamage = Mathf.Max(remainingDamage, 0);
                        }
                        else
                        {
                            shield.decreaseCharge(remainingDamage);
                            remainingDamage = 0;
                        }
                    }
                }

            }
        }

        public static void ChargeShields(this Structure structure, float chargeAmount)
        {
            if (structure.shields.Count > 0)
            {
                float remainingCharge = chargeAmount;

                foreach (IShieldGenerator shield in structure.shields)
                {
                    float maximumChargeAmount = shield.ShieldGeneratorData.MaxCharge - shield.CurrentCharge;

                    if (maximumChargeAmount <= 0 || remainingCharge == 0)
                        continue;

                    if (remainingCharge > maximumChargeAmount)
                    {
                        shield.increaseCharge(maximumChargeAmount);
                        remainingCharge -= maximumChargeAmount;
                    }
                    else
                    {
                        shield.increaseCharge(remainingCharge);
                        remainingCharge = 0;
                    }
                }
            }
        }

        public static float? GetShieldsCurrentChargeNormalized(this Structure structure)
        {
            if (structure.shields.Count > 0)
            {
                float currentShieldCharge = 0;
                float maxShieldCharge = 0;

                foreach (IShieldGenerator shield in structure.shields)
                {
                    maxShieldCharge += shield.ShieldGeneratorData.MaxCharge;

                    currentShieldCharge += shield.CurrentCharge;
                }

                return currentShieldCharge / maxShieldCharge;
            }
            else
                return null;
        }

        //structure.ShieldStrength returns 0 as it is reset on every tick so this gets the real current strength
        public static float GetCurrentShieldStrength(this Structure structure)
        {
            float strength = 0;

            foreach (IShieldGenerator shield in structure.shields)
            {
                strength += shield.CurrentCharge;
            }

            return strength;
        }
        #endregion

        #region Hull
        public static void DrainOwnHullStrength(this Structure structure, float amount)
        {
            if (structure.Destroyed || amount <= 0)
                return;

            float currentAmount = amount;

            if (currentAmount > structure.HullStrength)
                currentAmount = structure.HullStrength;

            if (structure.Stats != null)
            {
                structure.Stats.damageThisLife += currentAmount;
                structure.Stats.actualDamageThisLife += currentAmount;
                structure.Stats.totalDamageTaken += currentAmount;
                structure.Stats.totalHullDamageTaken += currentAmount;
            }

            structure.HullStrength -= currentAmount;

            // structure destruction handling
            #region structure destroyed
            if (structure.HullStrength <= 0)
            {
                // D.log("Structure", gameObject.name + " hull has failed!");

                structure.HullStrength = 0;

                structure.recalculateHealthBar();
                
                structure.Gamemode.Gui.setMessage(structure.gameObject.name + " has been destroyed");

                // disable the shield mesh (if present)
                if (structure.shields.Count > 0)
                {
                    // just use the reference to the first shield for disabling
                    structure.shields[0].failed();
                }

                // randomly spawn module explosions 
                for (int moduleIndex = 0; moduleIndex < structure.StructureSockets.Count; moduleIndex++)
                {
                    if (UnityEngine.Random.value > 0.5f)
                    {
                        if (structure.StructureSockets[moduleIndex].InstalledModule != null)
                        {
                            structure.StructureSockets[moduleIndex].InstalledModule.explode(2 * ((int)(structure.structureSize) + 1));
                        }
                    }
                }

                // spawn structure explosion
                if (structure.explosionInitial != null)
                {
                    GameObject clonedExplosion = structure.explosionInitial.Spawn(structure.transform);

                    // make any changes to the explosion here (if any)
                    clonedExplosion.GetComponent<ExplosionVFXController>().setSortingLayerOrder(structure.transform, -1);
                }

                // call the despawn event dispatcher
                structure.Call_Despawn(structure, new DespawnEventArgs(structure));

                // set destroyed flag
                structure.Destroyed = true;
                
                // unboot the controller if present
                if (structure.Controller != null)
                {
                    // D.log("Controller", "Controller for " + gameObject.name + " has gone offline!");
                    structure.Controller.booted = false;
                    //Controller.StopCoroutine(Controller.update());
                    structure.Controller.stop();
                }

                // change structure labels to red				
                if (structure.FactionLabel != null)
                {
                    structure.FactionLabel.GetComponent<FactionLabel>().SetLabelColour(Color.red);
                }

                if (structure.NameLabel != null)
                {
                    structure.NameLabel.GetComponent<NameLabel>().SetLabelColour(Color.red);
                }

                // update structure stats
                if (structure.Stats != null)
                {
                    structure.Stats.numDeaths++;

                    foreach ((Structure structure, float damage) entry in structure.Stats.assistList)
                    {
                        structure.Call_NotifyAssister(structure, new AssistEventArgs(entry.structure, structure, entry.damage / structure.Stats.actualDamageThisLife));

                        entry.structure.Stats.numAssists++;
                    }

                    // zero out damage this life
                    structure.Stats.damageThisLife = 0;
                    structure.Stats.actualDamageThisLife = 0;

                    structure.Stats.survivalTimes.Add((float)(structure.AliveTimer.Elapsed.TotalSeconds));
                }

                // add alive time to current list of survival times
                structure.Call_SurvivalTimeUpdated(structure, new SurvivalTimeEventArgs(structure, (float)(structure.AliveTimer.Elapsed.TotalSeconds), false));

                // calculates the current average survivial time
                structure.calculateAverageSurvivalTime(false);

                // stop the alive timer until respawn
                structure.stopSurvivalClock();

                // reset alive timer
                structure.resetSurvivalClock();

                // set flag for systems initiated
                structure.SystemsInitiated = false;
            }
            #endregion
        }


        //public static void DrainHullIntegrity(this Structure structure, float amount, Structure damageInflicter)
        //{
        //    if (structure.Destroyed || amount <= 0)
        //        return;

        //    float currentAmount = amount;

        //    if (currentAmount > structure.HullStrength)
        //        currentAmount = structure.HullStrength;

        //    if (structure.Stats != null)
        //    {
        //        structure.Stats.damageThisLife += currentAmount;
        //        structure.Stats.actualDamageThisLife += currentAmount;
        //        structure.Stats.totalDamageTaken += currentAmount;
        //        structure.Stats.totalHullDamageTaken += currentAmount;
        //    }

        //    // structure destruction handling
        //    #region structure destroyed
        //    if (structure.HullStrength <= 0)
        //    {
        //        // D.log("Structure", gameObject.name + " hull has failed!");

        //        structure.HullStrength = 0;

        //        structure.recalculateHealthBar();

        //        if (damageInflicter != structure)
        //        {
        //            TopDown_Camera.Instance.setLastKillBy(damageInflicter.transform);
        //            structure.Gamemode.Gui.setMessage(structure.gameObject.name + " has been destroyed by " + damageInflicter.commandedBy + " in " + damageInflicter.gameObject.name + "!");
        //        }
        //        else
        //        {
        //            structure.Gamemode.Gui.setMessage(structure.gameObject.name + " has been destroyed");
        //        }

        //        // disable the shield mesh (if present)
        //        if (structure.shields.Count > 0)
        //        {
        //            // just use the reference to the first shield for disabling
        //            structure.shields[0].failed();
        //        }

        //        // randomly spawn module explosions 
        //        for (int moduleIndex = 0; moduleIndex < structure.structureSockets.Count; moduleIndex++)
        //        {
        //            if (UnityEngine.Random.value > 0.5f)
        //            {
        //                if (structure.structureSockets[moduleIndex].InstalledModule != null)
        //                {
        //                    structure.structureSockets[moduleIndex].InstalledModule.explode(2 * ((int)(structure.structureSize) + 1));
        //                }
        //            }
        //        }

        //        // spawn structure explosion
        //        if (structure.explosionInitial != null)
        //        {
        //            GameObject clonedExplosion = structure.explosionInitial.Spawn(structure.transform);

        //            // make any changes to the explosion here (if any)
        //            clonedExplosion.GetComponent<ExplosionVFXController>().setSortingLayerOrder(structure.transform, -1);
        //        }

        //        // call the despawn event dispatcher
        //        structure.Call_Despawn(structure, new DespawnEventArgs(structure));

        //        // set destroyed flag
        //        structure.Destroyed = true;

        //        // call the NotifyKiller event on the damage causer's structure
        //        if (damageInflicter != structure)
        //        {
        //            if (target != null)
        //            {
        //                lastHitBy.Call_NotifyKiller(structure, new TargetDestroyedEventArgs(structure, null, lastHitBy, weaponOriginator));
        //            }
        //        }

        //        // call the NotifyKilled and TargetDestroyed events on this structure
        //        Call_NotifyKilled(this, new TargetDestroyedEventArgs(this, null, lastHitBy, weaponOriginator));
        //        Call_TargetDestroyed(this, new TargetDestroyedEventArgs(this, null, lastHitBy, weaponOriginator));

        //        // unboot the controller if present
        //        if (Controller != null)
        //        {
        //            // D.log("Controller", "Controller for " + gameObject.name + " has gone offline!");
        //            Controller.booted = false;
        //            //Controller.StopCoroutine(Controller.update());
        //            Controller.stop();
        //        }

        //        // change structure labels to red				
        //        if (FactionLabel != null)
        //        {
        //            FactionLabel.GetComponent<FactionLabel>().SetLabelColour(Color.red);
        //        }

        //        if (NameLabel != null)
        //        {
        //            NameLabel.GetComponent<NameLabel>().SetLabelColour(Color.red);
        //        }

        //        // update attacker stats
        //        if (weapon != null && lastHitBy.Stats != null)
        //        {
        //            lastHitBy.Stats.numStructuresDestroyed++;
        //        }

        //        // update structure stats
        //        if (structure.Stats != null)
        //        {
        //            structure.Stats.numDeaths++;

        //            foreach (Tuple<Structure, float> entry in structure.Stats.assistList)
        //            {
        //                structure.Call_NotifyAssister(structure, new AssistEventArgs(entry._1, structure, entry._2 / structure.Stats.actualDamageThisLife));

        //                entry._1.Stats.numAssists++;
        //            }

        //            // zero out damage this life
        //            structure.Stats.damageThisLife = 0;
        //            structure.Stats.actualDamageThisLife = 0;

        //            structure.Stats.survivalTimes.Add((float)(structure.AliveTimer.Elapsed.TotalSeconds));
        //        }

        //        // add alive time to current list of survival times
        //        structure.Call_SurvivalTimeUpdated(structure, new SurvivalTimeEventArgs(structure, (float)(structure.AliveTimer.Elapsed.TotalSeconds), false));

        //        // calculates the current average survivial time
        //        structure.calculateAverageSurvivalTime(false);

        //        // stop the alive timer until respawn
        //        structure.stopSurvivalClock();

        //        // reset alive timer
        //        structure.resetSurvivalClock();

        //        // set flag for systems initiated
        //        structure.SystemsInitiated = false;
        //    }
        //    #endregion
        //}

        #endregion
    }
}
