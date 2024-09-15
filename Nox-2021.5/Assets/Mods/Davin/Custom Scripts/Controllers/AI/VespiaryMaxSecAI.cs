using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Controllers;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Weapons;
using NoxCore.GameModes;
using NoxCore.Helm;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Rules;
using NoxCore.Utilities;

namespace Davin.Controllers
{
    [RequireComponent(typeof(BasicThreatEvaluator))]
    public class VespiaryMaxSecAI : AIStateController
    {
        BasicThreatEvaluator threatSys;

        public Ship boardingShip;

        public Structure primaryTargetStructure;
        public Module primaryTargetSystem;

        List<Structure> possibleTargets = new List<Structure>();
        List<(Structure structure, float threat)> threats = new List<(Structure, float)>();
        List<Ship> enemyShips = new List<Ship>();
        List<Ship> fighters = new List<Ship>();
        List<Ship> bombers = new List<Ship>();
        List<Ship> small = new List<Ship>();
        List<Ship> medium = new List<Ship>();
        List<Ship> large = new List<Ship>();
        Ship carrier;

        public (Structure, Module) getPrimaryTarget()
        {
            return (primaryTargetStructure, primaryTargetSystem);
        }

        public void setPrimaryTarget(Structure primaryTargetStructure, Module primaryTargetSystem = null)
        {
            this.primaryTargetStructure = primaryTargetStructure;
            this.primaryTargetSystem = primaryTargetSystem;
        }

        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, helm);

            threatSys = GetComponent<BasicThreatEvaluator>();

            aiActions.Add("SEARCH", searchAction);
            aiActions.Add("COMBAT", combatAction);

            state = "SEARCH";

            booted = true;
        }

        public void classifyTarget(Ship ship)
        {
            if (ship.Classification == ShipClassification.FIGHTER && !fighters.Contains(ship)) fighters.Add(ship);
            else if (ship.Classification == ShipClassification.BOMBER && !bombers.Contains(ship)) bombers.Add(ship);
            else if (ship.Classification == ShipClassification.CARRIER && carrier == null) carrier = ship;
            else if (ship.structureSize == StructureSize.SMALL && !small.Contains(ship)) small.Add(ship);
            else if (ship.structureSize == StructureSize.MEDIUM && !medium.Contains(ship)) medium.Add(ship);
            else if (ship.structureSize == StructureSize.LARGE && !large.Contains(ship)) large.Add(ship);
        }

        protected List<Structure> getHittable(Weapon weapon, List<Ship> ships)
        {
            List<Structure> hittable = new List<Structure>();

            for (int i = 0; i < ships.Count; i++)
            {
                // check if in range
                if (Vector2.Distance(weapon.transform.position, ships[i].transform.position) <= weapon.WeaponData.MaxRange)
                {
                    RotatingTurret turret = weapon as RotatingTurret;

                    // check if in firing arc
                    if (turret != null && turret.isWithinFireArc(ships[i].gameObject))
                    {
                        hittable.Add(ships[i]);
                        continue;
                    }

                    hittable.Add(ships[i]);
                }
            }

            return hittable;
        }

        protected Structure getBiggestThreat(FireGroup fireGroup, List<Ship> ships)
        {
            Structure biggestThreat = null;
            float bestScore = Mathf.Infinity;

            List<Weapon> weapons = fireGroup.getAllWeapons();

            for (int i = 0; i < fireGroup.getNumWeapons(); i++)
            {
                possibleTargets = getHittable(weapons[i], ships);

                if (possibleTargets.Count > 0)
                {
                    // remove possible targets that are dead
                    for (int j = possibleTargets.Count - 1; j >= 0; j--)
                    {
                        if (possibleTargets[j].Destroyed == true)
                        {
                            possibleTargets.Remove(possibleTargets[j]);
                        }
                    }

                    threats = threatSys.calculateThreatRatios(structure, possibleTargets);

                    if (threats.Count > 0 && threats[0].threat < bestScore)
                    {
                        bestScore = threats[0].threat;
                        biggestThreat = threats[0].structure;
                    }
                }
            }

            return biggestThreat;
        }

        protected void setTarget(FireGroup fireGroup, Structure targetStructure, Module targetSystem = null)
        {
            if (targetStructure != null)
            {
                if (targetSystem != null)
                {
                    fireGroup.setTarget(targetStructure.gameObject, targetSystem.gameObject);
                }
                else
                {
                    fireGroup.setTarget(targetStructure.gameObject, null);
                }
            }
            else
            {
                foreach (Weapon weapon in fireGroup.getAllWeapons())
                {
                    TargetableWeapon tWeap = weapon as TargetableWeapon;

                    if (tWeap != null)
                    {
                        tWeap.unacquireTarget();
                    }
                }
            }
        }

        public void setBoardingShip(Ship boardingShip)
        {
            this.boardingShip = boardingShip;
        }

        public virtual string searchAction()
        {
            if (structure.scanner.isActiveOn() == true)
            {
                List<Structure> enemiesInRange = structure.scanner.getEnemiesInRange();

                if (enemiesInRange.Count > 0)
                {
                    return "COMBAT";
                }
            }

            return "SEARCH";
        }

        public virtual string combatAction()
        {
            List<Structure> enemiesInRange = structure.scanner.getEnemiesInRange();

            if (enemiesInRange.Count > 0)
            {
                Structure targetStructure = null;
                Module targetSystem = null;

                foreach (Structure enemy in enemiesInRange)
                {
                    Ship ship = enemy as Ship;

                    if (ship != null)
                    {
                        classifyTarget(ship);
                    }
                }

                // focus on primary target (boarding ship) if in range or use fire group priorities
                if (boardingShip != null && enemiesInRange.Contains(boardingShip) && Vector2.Distance(transform.position, boardingShip.transform.position) < 1200)
                {
                    setPrimaryTarget(boardingShip);
                }
                else
                {
                    setPrimaryTarget(null);
                }

                if (primaryTargetStructure != null && primaryTargetSystem != null)
                {
                    if (Vector2.Distance(transform.position, primaryTargetSystem.transform.position) < structure.scanner.ScannerData.Radius)
                    {
                        targetStructure = primaryTargetStructure;
                        targetSystem = primaryTargetSystem;
                    }
                }
                else if (primaryTargetStructure != null && primaryTargetSystem == null)
                {
                    if (enemiesInRange.Contains(primaryTargetStructure))
                    {
                        targetStructure = primaryTargetStructure;
                    }
                }
                else
                {
                    #region set fire group priorities
                    foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                    {
                        possibleTargets.Clear();

                        List<Weapon> weapons = fireGroup.getAllWeapons();

                        switch (fireGroup.Name.ToUpper())
                        {
                            case "ORBITAL":
                                possibleTargets = getHittable(weapons[0], medium);
                                threats = threatSys.calculateThreatRatios(structure, possibleTargets);

                                if (threats.Count > 0) setTarget(fireGroup, threats[0].structure);
                                break;

                            case "STANDARDPLATFORMS":
                                targetStructure = getBiggestThreat(fireGroup, bombers);

                                if (targetStructure == null)
                                {
                                    targetStructure = getBiggestThreat(fireGroup, small);

                                    if (targetStructure == null)
                                    {
                                        targetStructure = getBiggestThreat(fireGroup, medium);

                                        if (targetStructure == null)
                                        {
                                            targetStructure = getBiggestThreat(fireGroup, fighters);
                                        }
                                    }
                                }

                                setTarget(fireGroup, targetStructure);
                                break;

                            case "HEAVYPLATFORMS":
                                targetStructure = getBiggestThreat(fireGroup, bombers);

                                if (targetStructure == null)
                                {
                                    targetStructure = getBiggestThreat(fireGroup, medium);

                                    if (targetStructure == null)
                                    {
                                        targetStructure = getBiggestThreat(fireGroup, small);

                                        if (targetStructure == null)
                                        {
                                            targetStructure = getBiggestThreat(fireGroup, fighters);
                                        }
                                    }
                                }

                                setTarget(fireGroup, targetStructure);
                                break;

                            case "LAUNCHERS":
                                targetStructure = getBiggestThreat(fireGroup, small);

                                if (targetStructure == null)
                                {
                                    targetStructure = getBiggestThreat(fireGroup, medium);
                                }

                                setTarget(fireGroup, targetStructure);
                                break;

                            case "LASERS":
                                targetStructure = getBiggestThreat(fireGroup, bombers);

                                if (targetStructure == null)
                                {
                                    targetStructure = getBiggestThreat(fireGroup, fighters);

                                    if (targetStructure == null)
                                    {
                                        targetStructure = getBiggestThreat(fireGroup, small);

                                        if (targetStructure == null)
                                        {
                                            targetStructure = getBiggestThreat(fireGroup, medium);
                                        }
                                    }
                                }

                                setTarget(fireGroup, targetStructure);
                                break;

                            case "BLASTERS":
                                targetStructure = getBiggestThreat(fireGroup, fighters);

                                if (targetStructure == null)
                                {
                                    targetStructure = getBiggestThreat(fireGroup, bombers);

                                    if (targetStructure == null)
                                    {
                                        targetStructure = getBiggestThreat(fireGroup, medium);

                                        if (targetStructure == null)
                                        {
                                            targetStructure = getBiggestThreat(fireGroup, small);
                                        }
                                    }
                                }

                                setTarget(fireGroup, targetStructure);
                                break;
                        }
                    }
                    #endregion
                }

                foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                {
                    setTarget(fireGroup, targetStructure);
                }
          
                return "COMBAT";
            }
            else
            {
                foreach (Weapon weap in structure.Weapons)
                {
                    TargetableWeapon tWeap = (TargetableWeapon)weap;

                    if (tWeap != null)
                    {
                        tWeap.unacquireTarget();
                    }
                }

                return "SEARCH";
            }
        }
    }
}