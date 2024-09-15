using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Fittings.Devices;
using NoxCore.Helm;
using NoxCore.Placeables;
using NoxCore.Rules;

namespace NoxCore.Controllers
{
    public class BasicScanningAI : AIStateController
    {
        protected List<Vector2> waypoints = new List<Vector2>();
        protected int currentWaypoint = 0;

        protected SeekBehaviour seekBehaviour;
        protected OrbitBehaviour orbitBehaviour;

        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, helm);

            aiActions.Add("SEARCH", searchAction);
            aiActions.Add("ORBIT", orbitAction);

            state = "SEARCH";

            waypoints.Add(new Vector2(0, 750));
            waypoints.Add(new Vector2(-750, 0));
            waypoints.Add(new Vector2(0, -750));
            waypoints.Add(new Vector2(750, -0));

            seekBehaviour = Helm.getBehaviourByName("SEEK") as SeekBehaviour;
            orbitBehaviour = Helm.getBehaviourByName("ORBIT") as OrbitBehaviour;

            if (orbitBehaviour != null)
            {
                orbitBehaviour.OrbitRange = structure.scanner.ScannerData.Radius;
            }

            booted = true;
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

        public virtual string searchAction()
        {
            if (structure.scanner.isActiveOn() == true)
            {
                if (structure.scanner.getEnemiesInRange().Count == 0)
                {
                    #region search pattern
                    if (seekBehaviour != null)
                    {
                        if (seekBehaviour.Active == false)
                        {
                            seekBehaviour.enableExclusively();
                            currentWaypoint = 0;
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
                    }
                    else
                    {
                        return null;
                    }

                    return "SEARCH";
                    #endregion
                }
                else
                {
                    return "ORBIT";
                }
            }
            else
            {
                return "SEARCH";
            }
        }

        public virtual string orbitAction()
        {
            if (structure.scanner.isActiveOn() == true)
            {
                List<Structure> enemiesInRange = structure.scanner.getEnemiesInRange();

                if (enemiesInRange.Count > 0)
                {
                    if (orbitBehaviour != null)
                    {
                        if (orbitBehaviour.Active == false)
                        {
                            orbitBehaviour.enableExclusively();
                        }
                    }

                    orbitBehaviour.OrbitObject = enemiesInRange[0].transform;

                    return "ORBIT";
                }
                else
                {
                    return "SEARCH";
                }
            }

            return "SEARCH";
        }
    }
}