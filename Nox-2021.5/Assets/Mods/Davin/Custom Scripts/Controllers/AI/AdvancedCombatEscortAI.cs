using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

using NoxCore.Controllers;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Weapons;
using NoxCore.GameModes;
using NoxCore.Helm;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Rules;

using Davin.Messages;

namespace Davin.Controllers
{
    [RequireComponent(typeof(BasicThreatEvaluator))]
    public class AdvancedCombatEscortAI : AIStateController
    {
        BasicThreatEvaluator threatSys;

        protected ArriveBehaviour arriveBehaviour;
        protected SeekBehaviour seekBehaviour;
        protected OrbitBehaviour orbitBehaviour;
        protected AvoidBehaviour avoidBehaviour;

        protected IComms comms;

        protected List<Structure> squad;

        [SerializeField]
        protected String vipName;
        public String VIPName
        { 
            get { return vipName; } 
            set 
            {
                if (vipName != value)
                {
                    vipName = value;

                    GameObject vipGO = GameObject.Find(vipName);

                    if (vipGO != null)
                    {
                        vip = vipGO.GetComponent<Structure>();
                    }
                    else
                    {
                        vip = null;
                    }
                }
            } 
        }

        [SerializeField]
        protected Structure vip;
        public Structure VIP { get { return vip; } }

        [SerializeField]
        protected Vector2 offsetFromVIP;
        public Vector2 OffsetFromVIP { get { return offsetFromVIP; } set { offsetFromVIP = value; } }

        protected Vector2? vipLastKnownPosition;

        protected bool vipInDistress;

        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, helm);

            Ship ship = structure as Ship;

            if (ship != null)
            {
                GameMode.ShipHasWarpedIn += OnShipHasWarpedIn;
                ship.HasWarpedIn += OnHasWarpedIn;
            }

            aiActions.Add("IDLE", idleAction);
            aiActions.Add("SEARCH", searchAction);
            aiActions.Add("PROTECT", protectAction);

            state = "SEARCH";
            
            arriveBehaviour = Helm.getBehaviourByName("ARRIVE") as ArriveBehaviour;
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
                // E.G. avoidLayerMask = structure.Gamemode.getCollisionMask("SHIP").GetValueOrDefault() ^ (1 << LayerMask.NameToLayer("NavPoint"));
                LayerMask avoidLayerMask = structure.Gamemode.getCollisionMask("SHIP").GetValueOrDefault();
                avoidBehaviour.setCollidables(avoidLayerMask);
            }

            foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
            {
                foreach (Weapon weapon in fireGroup.getAllWeapons())
                {
                    ProjectileLauncher launcher = weapon as ProjectileLauncher;

                    if (launcher != null)
                    {
                        launcher.WeaponFired += LauncherFired;
                    }
                }
            }

            threatSys = GetComponent<BasicThreatEvaluator>();

            comms = structure.getDevice<IComms>() as IComms;

            calculateVIPOffset();

            booted = true;
        }

        protected void calculateVIPOffset()
        {
            AdvancedCombatEscortAI[] escortShips = FindObjectsOfType<AdvancedCombatEscortAI>();

            int windings = Mathf.FloorToInt((escortShips.Length - 1) / 10.0f) + 1;
            float radius = 250 * windings;

            int pointNum = Mathf.FloorToInt(((escortShips.Length - 1) % 10) / 2.0f) + 1;

            float angleDeg = 30.0f * pointNum;

            if (escortShips.Length % 2 == 0)
            {
                angleDeg = -angleDeg;
            }

            offsetFromVIP = radius * new Vector2(Mathf.Sin(angleDeg * Mathf.Deg2Rad), Mathf.Cos(angleDeg * Mathf.Deg2Rad));
        }

        protected override void newScannerData(Scanner sender)
        {
            if (vip == null)
            {
                GameObject vipGO = GameObject.Find(vipName);

                if (vipGO != null)
                {
                    Structure vipStructure = vipGO.GetComponent<Structure>();

                    if (vipStructure != null && structure.scanner.getFriendliesInRange().Contains(vipStructure))
                    {
                        vip = vipGO.GetComponent<Structure>();
                    }
                }
            }

            List<Structure> enemiesInRange = structure.scanner.getEnemiesInRange();

            if (enemiesInRange.Count > 0)
            {
                Structure target = null;

                if (vipInDistress == true)
                {
                    if (enemiesInRange.Contains(vip.LastHitBy))
                    {
                        target = vip.LastHitBy;
                        vipInDistress = false;
                    }
                }
                else
                {
                    List<(Structure enemy, float threat)> threats = threatSys.calculateThreatRatios(vip, enemiesInRange);

                    foreach ((Structure enemy, float threatScore) threat in threats)
                    {
                        if (threat.enemy.Destroyed == false)
                        {
                            target = threats[0].enemy;
                            break;
                        }
                    }
                }

                if (target != null)
                {
                    // tell all fire groups to acquire the main threat
                    foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                    {
                        fireGroup.setTarget(target);
                    }

                    orbitBehaviour.OrbitObject = target.transform;
                    orbitBehaviour.OrbitRange = 300;
                }
            }
            else
            {
                foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                {
                    fireGroup.unacquireTarget();
                }
            }
        }

        protected virtual Vector2 setHelmDestination()
        {
            if (vip != null)
            {
                switch (state)
                {
                    case "IDLE":
                        return transform.position;

                    case "SEARCH":
                        if (vipLastKnownPosition != null)
                        {
                            return vipLastKnownPosition.GetValueOrDefault();
                        }
                        else
                        {
                            // run a search
                            return UnityEngine.Random.insideUnitCircle * 2000;
                        }

                    case "PROTECT":
                        if (structure.scanner.getEnemiesInRange().Count == 0)
                        {
                            if (structure.scanner.getFriendliesInRange().Contains(vip))
                            {
                                return (Vector2)(vip.transform.position + vip.transform.TransformDirection(offsetFromVIP));
                            }
                        }
                        break;
                }
            }

            return transform.position;
        }

        public override void update()
        {
            if (booted == true)
            {
                base.update();

                #region comms
                // send any messages
                if (vipLastKnownPosition == null)
                {
                    comms.sendMessage(vip, new RequestLocation(structure));
                }

                // process any received messages
                if (comms != null && comms.hasMessages() == true)
                {
                    foreach (EventArgs message in comms.getMessages().Reverse<EventArgs>())
                    {
                        // process known message types
                        DistressCallMessage distressCallMessage = message as DistressCallMessage;

                        if (distressCallMessage != null)
                        {
                            if (distressCallMessage.distressed.name == vipName)
                            {
                                vipInDistress = true;
                            }

                            comms.removeMessage(distressCallMessage);
                        }

                        StructureLocation structureLocationMessage = message as StructureLocation;

                        if (structureLocationMessage != null)
                        {
                            if (structureLocationMessage.structure.name == vipName)
                            {
                                vipLastKnownPosition = structureLocationMessage.location;
                            }

                            comms.removeMessage(structureLocationMessage);
                        }
                    }
                }
                #endregion
            }
        }    

        public virtual string idleAction()
        {
            if (vip != null)
            {
                if (structure.scanner.getFriendliesInRange().Contains(vip))
                {
                    return "PROTECT";
                }
                else
                {
                    return "SEARCH";
                }
            }

            return "IDLE";
        }

        public virtual string searchAction()
        {
            if (structure.scanner.isActiveOn() == true)
            {
                // is the ship we are escorting still in scanner range? If so, follow it and protect it
                if (vip != null && structure.scanner.getFriendliesInRange().Contains(vip))
                {
                    Helm.destination = vip.transform.position;
                    return "PROTECT";
                }

                if (seekBehaviour != null && seekBehaviour.Active == false)
                {
                    seekBehaviour.enableExclusively();
                }

                // set destination
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

            return "SEARCH";
        }

        public string protectAction()
        {
            if (structure.scanner.isActiveOn() == true)
            {
                // is the ship we are escorting still in scanner range? If not, run a search pattern to find it.
                if (vip != null && !structure.scanner.getFriendliesInRange().Contains(vip))
                {
                    Helm.destination = null;
                    return "SEARCH";
                }

                if (structure.scanner.getEnemiesInRange().Count == 0)
                {
                    Helm.destination = setHelmDestination();

                    if (arriveBehaviour != null && arriveBehaviour.Active == false)
                    {
                        arriveBehaviour.enableExclusively();
                    }
                }
                else
                {
                    if (orbitBehaviour != null && orbitBehaviour.Active == false)
                    {
                        orbitBehaviour.enableExclusively();
                    }
                }

                // draw a line to the destination
                if (Helm.destination != null && Cam.followTarget != null && Cam.followTarget.gameObject == Helm.ShipStructure.gameObject)
                {
                    Debug.DrawLine(structure.transform.position, Helm.destination.GetValueOrDefault(), Color.blue, Time.deltaTime, true);
                }
            }

            return "PROTECT";
        }

        protected void OnShipHasWarpedIn(object sender, ShipWarpedEventArgs args)
        {
            if (args != null)
            {
                if (args.shipGO.name == vipName)
                {
                    vipLastKnownPosition = null;
                }
            }
        }

        protected void OnHasWarpedIn(object sender, WarpEventArgs args)
        {
            calculateVIPOffset();
        }

        public void LauncherFired(object sender, WeaponFiredEventArgs args)
        {
            //Gui.setMessage(args.weaponFired + " has fired!");
        }
    }
}
 