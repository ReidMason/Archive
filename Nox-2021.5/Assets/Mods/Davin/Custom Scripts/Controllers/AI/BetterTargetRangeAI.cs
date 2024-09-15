using UnityEngine;
using System.Collections.Generic;
using System.Linq;

using NoxCore.Controllers;
using NoxCore.Helm;
using NoxCore.Fittings.Weapons;
using NoxCore.Placeables;
using NoxCore.Rules;

namespace Davin.Controllers
{
    public class BetterTargetRangeAI : AIStateController
    {
        public List<Vector2> waypoints = new List<Vector2>();
        public int currentWaypoint = 0;

        protected SeekBehaviour seekBehaviour;
        protected OrbitBehaviour orbitBehaviour;
        protected AvoidBehaviour avoidBehaviour;

        List<Structure> enemiesInRange = new List<Structure>();

        int bombCounter;

        BlasterTurret blaster;

        List<ProjectileLauncher> missileLaunchers = new List<ProjectileLauncher>();
        List<ProjectileLauncher> bombLaunchers = new List<ProjectileLauncher>();

        List<Structure> bombTargets = new List<Structure>();

        FireGroup fireGroup1, fireGroup2, fireGroup3, fireGroup4;

        List<GameObject> targets = new List<GameObject>();
        List<HashSet<GameObject>> clusters = new List<HashSet<GameObject>>();

        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, helm);

            aiActions.Add("SEARCH", searchAction);
            aiActions.Add("COMBAT", combatAction);

            state = "SEARCH";

            waypoints.Add(new Vector2(-250, 0));
            waypoints.Add(new Vector2(0, 250));
            waypoints.Add(new Vector2(250, 0));
            waypoints.Add(new Vector2(0, -250));

            seekBehaviour = Helm.getBehaviourByName("SEEK") as SeekBehaviour;
            orbitBehaviour = Helm.getBehaviourByName("ORBIT") as OrbitBehaviour;
            avoidBehaviour = Helm.getBehaviourByName("AVOID") as AvoidBehaviour;

            if (orbitBehaviour != null)
            {
                orbitBehaviour.OrbitRange = structure.scanner.ScannerData.Radius;
            }

            if (avoidBehaviour != null)
            {
                // note: could add additional layers to the following.
                // E.G. avoidLayerMask = structure.Gamemode.getCollisionMask("ship").GetValueOrDefault() ^ (1 << LayerMask.NameToLayer("NavPoint"));
                LayerMask avoidLayerMask = structure.Gamemode.getCollisionMask("SHIP").GetValueOrDefault();
                avoidBehaviour.setCollidables(avoidLayerMask);
            }

            fireGroup1 = structure.FireControl.FireGroups[0];
            fireGroup2 = structure.FireControl.FireGroups[1];
            fireGroup3 = structure.FireControl.FireGroups[2];
            fireGroup4 = structure.FireControl.FireGroups[3];

            blaster = fireGroup1.getAllWeapons()[0].GetComponent<BlasterTurret>();

            targets = GameObject.FindGameObjectsWithTag("Target").ToList();

            targets = sortEnemies(targets);

            targets.Sort((a, b) => {

                Structure aStructure = a.GetComponent<Structure>();
                Structure bStructure = b.GetComponent<Structure>();
               
                // compare b to a to get descending order
                return aStructure.HullStrength.CompareTo(bStructure.HullStrength);
            });

            foreach(GameObject target in targets)
            {
                List<GameObject> cluster = new List<GameObject>();

                foreach(GameObject otherTarget in targets)
                {
                    // ignore checking distance with self
                    if (otherTarget == target) continue;

                    // some kind of proximity check
                    if (Vector2.Distance(target.transform.position, otherTarget.transform.position) < 50)
                    {
                        cluster.Add(otherTarget);
                    }
                }

                // if cluster has more than 0 entries, pick the most central target of the cluster and add to a list
            }


            foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
            {
                foreach (Weapon weapon in fireGroup.getAllWeapons())
                {
                    ProjectileLauncher launcher = weapon as ProjectileLauncher;

                    if (launcher != null)
                    {
                        ProjectileLauncher projLauncher = launcher.GetComponent<ProjectileLauncher>();

                        if (projLauncher.DeviceData.SubType == "COMPACTPROXIMITYBOMB")
                        {
                            bombLaunchers.Add(projLauncher);
                            projLauncher.WeaponFired += OnBombWeaponFired;
                        }
                        else if(projLauncher.DeviceData.SubType == "LIGHTHOMINGMISSILE")
                        {
                            missileLaunchers.Add(projLauncher);
                            projLauncher.WeaponFired += OnMissileWeaponFired;
                        }

                        // etc.
                    }
                }
            }

            booted = true;
        }

        protected List<GameObject> sortEnemies(List<GameObject> sortedEnemies)
        {
            sortedEnemies.Sort((a, b) => {

                Structure aStructure = a.GetComponent<Structure>();
                Structure bStructure = b.GetComponent<Structure>();

                // compare b to a to get descending order
                return aStructure.HullStrength.CompareTo(bStructure.HullStrength);
            });

            return sortedEnemies;
        }

        public override void reset()
        {
            base.reset();

            state = "SEARCH";
        }

        protected virtual Vector2 setHelmDestination()
        {
            Vector2 nextPoint = waypoints[currentWaypoint];

            currentWaypoint++;

            if (currentWaypoint == waypoints.Count)
            {
                currentWaypoint = 0;
            }

            return nextPoint;
        }

        public List<Structure> sortTargetsByDistance(List<Structure> enemies)
        {
            List<Structure> sortedEnemies = new List<Structure>();

            // sort the list

            return sortedEnemies;
        }

        public virtual string searchAction()
        {
            if (structure.Faction.EnemyStructures.Count == 0)
            {
                #region search pattern
                if (seekBehaviour != null && seekBehaviour.Active == false)
                {
                    seekBehaviour.enableExclusively();
                    currentWaypoint = 0;
                }

                if (avoidBehaviour != null && avoidBehaviour.Active == false)
                {
                    avoidBehaviour.enable();
                }

                // run search pattern
                if (Helm.destination == null)
                {
                    Helm.destination = setHelmDestination();
                }

                // don't allow a destination outside of the arena
                if (ArenaRules.radius > 0)
                {
                    if (Helm.destination.GetValueOrDefault().magnitude > ArenaRules.radius)
                    {
                        Helm.destination = null;
                    }
                }

                // draw a line to the destination
                if (Helm.destination != null && Cam.followTarget != null && Cam.followTarget.gameObject == Helm.ShipStructure.gameObject)
                {
                    Debug.DrawLine(structure.transform.position, Helm.destination.GetValueOrDefault(), Color.blue, Time.deltaTime, true);
                }

                return "SEARCH";
                #endregion
            }
            else
            {
                return "COMBAT";
            }
        }

        public virtual string combatAction()
        {
            enemiesInRange = structure.scanner.getEnemiesInRange();

            if (structure.scanner.isActiveOn() == true)
            {
                if (enemiesInRange.Count > 0)
                {
                    List<Structure> sortedList = sortTargetsByDistance(enemiesInRange);

                    // pick target for firegroup1
                    foreach(Structure target in enemiesInRange)
                    {
                        // pick target for based on whether it can be hit by the blaster turret
                        if (blaster.isWithinFireArc(target.gameObject) && Vector2.Distance(blaster.transform.position, target.transform.position) < blaster.BlasterTurretData.MaxRange)
                        {
                            fireGroup1.setTarget(target);
                            break;
                        }
                    }


                    // pick target for firegroup2
                    foreach (Structure target in enemiesInRange)
                    {
                        // pick moving target for the missile launchers
                        if (target.name.Contains("Moving"))
                        {
                            fireGroup2.setTarget(target);
                            break;
                        }
                    }


                    // pick target in range of firegroup3
                    foreach (Structure target in enemiesInRange)
                    {
                        // pick shielded target for the plasma cannon
                        if (target.name.Contains("Moving"))
                        {
                            fireGroup3.setTarget(target);
                            break;
                        }
                    }

                    // pick target in range of firegroup4
                    foreach (Structure target in enemiesInRange)
                    {
                        // pick a target in a cluster
                        // something like this but you have to supply a clusterTarget you have previously found and stored in your class data
                        fireGroup4.setTarget(target);
                        break;
                    }

                    if (orbitBehaviour != null && orbitBehaviour.Active == false)
                    {
                        orbitBehaviour.enableExclusively();

                        // use the first target as the ship/structure to orbit around
                        orbitBehaviour.OrbitObject = enemiesInRange[0].transform;

                        // use the first weapon's maximum range to determine a suitable orbit range (with a wiggle room factor e.g. -50 units)
                        if (structure.Weapons.Count > 0)
                        {
                            orbitBehaviour.OrbitRange = structure.Weapons[0].WeaponData.MaxRange - 50;
                        }
                    }

                    return "COMBAT";
                }
                else
                {
                    foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                    {
                        fireGroup.unacquireTarget();
                    }

                    return "SEARCH";
                }
            }
            else
            {
                return "SEARCH";
            }
        }

        public void OnBombWeaponFired(object sender, WeaponFiredEventArgs args)
        {
            Gui.setMessage(args.weaponFired + " has fired!");
        }

        public void OnMissileWeaponFired(object sender, WeaponFiredEventArgs args)
        {
            Gui.setMessage(args.weaponFired + " has fired!");
        }
    }
}