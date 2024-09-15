using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

using NoxCore.Data;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Fittings.Weapons;
using NoxCore.Helm;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Rules;

namespace NoxCore.Controllers
{
    [RequireComponent(typeof(BasicThreatEvaluator))]
    public class BasicSquadronAI : AIStateController, IFormationFly, ILand
    {
        public bool isLeader;
        protected bool squadronInitialised;

        BasicThreatEvaluator threatSys;

        protected SeekBehaviour seekBehaviour;
        protected OrbitBehaviour orbitBehaviour;
        protected AvoidBehaviour avoidBehaviour;
        protected FormationFollowLeaderBehaviour followBehaviour;
        protected AlignmentBehaviour alignBehaviour;
        protected CohesionBehaviour cohesionBehaviour;
        protected SeparationBehaviour separationBehaviour;

        protected IComms comms;

        public Structure hangarStructure;
        protected IHangar hangar;

        public Structure boardingShip;

        protected Ship ship;
        protected List<Ship> squad;
        protected SquadronData squadron;

        protected Structure boardingTarget;
        protected List<IWeapon> boardingTargetWeapons;

        public Structure primaryTargetStructure;
        public Module primaryTargetSystem;

        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, helm);

            ship = structure as Ship;

            if (ship != null)
            {
                squadron = ship.SquadronData;
                squad = squadron.getAllShips();

                GameEventManager.MatchIsWaitingToStart += OnMatchIsWaitingToStart;

                ship.HasWarpedIn += OnHasWarpedIn;
                ship.SquadronHasWarpedIn += OnSquadronHasWarpedIn;
                ship.NotifyKilled += OnKilled;
            }

            aiActions.Add("SEARCH", searchAction);
            aiActions.Add("FOLLOW", followAction);
            aiActions.Add("COMBAT", combatAction);
            aiActions.Add("LAND", landAction);

            state = "SEARCH";

            seekBehaviour = Helm.getBehaviourByName("SEEK") as SeekBehaviour;
            orbitBehaviour = Helm.getBehaviourByName("ORBIT") as OrbitBehaviour;
            avoidBehaviour = Helm.getBehaviourByName("AVOID") as AvoidBehaviour;
            followBehaviour = Helm.getBehaviourByName("FOLLOW") as FormationFollowLeaderBehaviour;
            alignBehaviour = Helm.getBehaviourByName("ALIGNMENT") as AlignmentBehaviour;
            cohesionBehaviour = Helm.getBehaviourByName("COHESION") as CohesionBehaviour;
            separationBehaviour = Helm.getBehaviourByName("SEPARATION") as SeparationBehaviour;

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

            booted = true;
        }

        public void initController()
        {
            if (ship == null) return;

            if (isLeader == true)
            {
                squadron.setLeader(ship.gameObject);
            }

            // if there is no leader set than pick first non-detroyed ship in squadron
            if (squadron.getLeader() == null)
            {
                squadron.updateLeader();
                isLeader = true;
            }

            if (isLeader == false) state = "FOLLOW";

            if (isLeader == true)
            {
                if (alignBehaviour != null)
                {
                    alignBehaviour.SquadronMembers = squad;
                }

                if (cohesionBehaviour != null)
                {
                    cohesionBehaviour.SquadronMembers = squad;
                }

                if (separationBehaviour != null)
                {
                    separationBehaviour.SquadronMembers = squad;
                }

                foreach (Ship member in squadron.getNonLeaderShips())
                {
                    FormationFollowLeaderBehaviour memberFollowLeaderBehaviour = member.Helm.getBehaviourByName("FOLLOW") as FormationFollowLeaderBehaviour;
                    AlignmentBehaviour memberAlignmentBehaviour = member.Helm.getBehaviourByName("ALIGNMENT") as AlignmentBehaviour;
                    CohesionBehaviour memberCohesionBehaviour = member.Helm.getBehaviourByName("COHESION") as CohesionBehaviour;
                    SeparationBehaviour memberSeparationBehaviour = member.Helm.getBehaviourByName("SEPARATION") as SeparationBehaviour;

                    if (memberFollowLeaderBehaviour != null)
                    {
                        memberFollowLeaderBehaviour.Leader = ship.StructureRigidbody;
                    }

                    if (memberAlignmentBehaviour != null)
                    {
                        memberAlignmentBehaviour.SquadronMembers = squad;
                    }

                    if (memberCohesionBehaviour != null)
                    {
                        memberCohesionBehaviour.SquadronMembers = squad;
                    }

                    if (memberSeparationBehaviour != null)
                    {
                        memberSeparationBehaviour.SquadronMembers = squad;
                    }
                }

                squadron.MembersAlive = squad.Count;
            }
        }

        public static IHangar StructureSocketToIHangar(StructureSocket socket)
        {
            return socket as IHangar;
        }

        protected virtual Vector2 setHelmDestination()
        {
            switch (state)
            {
                case "SEARCH":
                    if (primaryTargetSystem != null)
                    {
                        return primaryTargetSystem.transform.position;
                    }
                    else if (primaryTargetStructure != null)
                    {
                        return primaryTargetStructure.transform.position;
                    }
                    else if (boardingTarget != null)
                    {
                        return boardingTarget.transform.position;
                    }
                    break;

                case "COMBAT":
                    if (primaryTargetSystem != null)
                    {
                        return primaryTargetSystem.transform.position;
                    }
                    else if (primaryTargetStructure != null)
                    {
                        return primaryTargetStructure.transform.position;
                    }
                    else if (boardingTarget != null)
                    {
                        return boardingTarget.transform.position;
                    }
                    break;

                case "LAND":
                    if (hangarStructure != null)
                    {
                        if (hangar == null)
                        {
                            hangar = findHangar(hangarStructure);

                            if (hangar == null)
                            {
                                Gui.setMessage("No hangar found on " + hangarStructure.Name);
                            }
                            else
                            {
                                //Gui.setMessage(structure.Name + " is heading for the hangar on " + hangarStructure.Name);
                                return hangar.getTransform().position;
                            }
                        }
                        else
                        {
                            //Gui.setMessage(structure.Name + " is heading for the hangar on " + hangarStructure.Name);                            

                            return hangar.getTransform().position;
                        }
                    }
                    break;
            }

            return structure.Controller.startSpot.GetValueOrDefault();
        }

        public (Structure primaryTargetStructure, Module primaryTargetSystem) getPrimaryTarget()
        {
            return (primaryTargetStructure, primaryTargetSystem);
        }

        public void setPrimaryTarget(Structure primaryTargetStructure, Module primaryTargetSystem = null)
        {
            this.primaryTargetStructure = primaryTargetStructure;
            this.primaryTargetSystem = primaryTargetSystem;
        }

        public void setBoardingTarget(Structure boardingTarget)
        {
            this.boardingTarget = boardingTarget;
        }

        public Vector2 getFormationOffset()
        {
            return followBehaviour.FormationOffset;
        }

        public void setFormationOffset(Vector2 formationOffset)
        {
            followBehaviour.FormationOffset = formationOffset;
        }

        public IHangar getHangar()
        {
            if (hangar == null) hangar = findHangar(hangarStructure);

            return hangar;
        }

        public void setHangar(IHangar hangar)
        {
            this.hangar = hangar;
        }

        public GameObject getHangarGO()
        {
            return getHangar().getTransform().gameObject;
        }

        public Structure getHangarStructure()
        {
            return hangarStructure;
        }

        public void setHangarStructure(Structure hangarStructure)
        {
            this.hangarStructure = hangarStructure;

            hangar = findHangar(hangarStructure);
        }

        protected IHangar findHangar(Structure hangarStructure)
        {
            if (hangarStructure == null) return null;

            foreach (StructureSocket socket in hangarStructure.StructureSockets)
            {
                IHangar hangar = socket as IHangar;

                if (hangar != null)
                {
                    return hangar;
                }
            }

            return null;
        }

        public Ship pickNewLeader()
        {
            if (isLeader == true)
            {
                isLeader = false;
                Ship newLeaderShip = squadron.updateLeader();

                // is there anyone that can lead left alive? If so, set them as the leader and tell the squadron or set the leader to null
                if (newLeaderShip != null)
                {
                    IFormationFly formationController = newLeaderShip.Controller as IFormationFly;

                    if (formationController != null)
                    {
                        formationController.setAsLeader();
                        newLeaderShip.Helm.Destination = setHelmDestination();
                    }
                }
                else
                {
                    squadron.setLeader(null);
                }

                return newLeaderShip;
            }

            return null;
        }

        public void setAsLeader()
        {
            isLeader = true;

            squadron.setLeader(structure.gameObject);

            state = "SEARCH";

            foreach (Ship member in squadron.getNonLeaderShips())
            {
                FormationFollowLeaderBehaviour memberFollowLeaderBehaviour = member.Helm.getBehaviourByName("FOLLOW") as FormationFollowLeaderBehaviour;

                if (memberFollowLeaderBehaviour != null)
                {
                    memberFollowLeaderBehaviour.Leader = structure.StructureRigidbody;

                    IStateController stateController = member.Controller as IStateController;

                    if (stateController != null)
                    {
                        stateController.setState("FOLLOW");
                    }
                }
            }
        }

        public override void reset()
        {
            base.reset();

            if (squadron != null && squadron.getLeader() == null)
            {
                squadron.updateLeader();
                isLeader = true;

                state = "SEARCH";

                Helm.Destination = setHelmDestination();
            }
            else
            {
                if (gameObject != squadron.getLeader())
                {
                    isLeader = false;
                    state = "FOLLOW";
                }
            }

            ship.shipState = ShipState.FLYING;
        }

        public override void update()
        {
            if (booted == true)
            {
                processState();

                if (hangarStructure != null && hangarStructure.isDestroyed() == false && state != "LAND" && structure.HullStrength < 0.5f * structure.MaxHullStrength)
                {
                    if (ship.shipState != ShipState.LANDED)
                    {
                        state = "LAND";
                        ship.shipState = ShipState.LANDING;

                        Helm.destination = setHelmDestination();
                    }
                }

                // some kind of test to check if the primary target has been destroyed and switch to something else...

                if (comms != null)
                {
                    if (comms.hasMessages() == true)
                    {
                        foreach (EventArgs message in comms.getMessages().Reverse<EventArgs>())
                        {
                            // check type of message
                            // e.g.
                            /*
                            DistressCallMessage distressCallMessage = message as DistressCallMessage;

                            if (distressCallMessage != null)
                            {
                                // do something with the information in the message
                                D.log("Oh, " + distressCallMessage.distressed + " is in distress");

                                comms.removeMessage(distressCallMessage);
                            }

                            // could be a different type of message so try to cast to each in turn and process the information contained inside the message's args
                            NewPrimaryTargetMessage newPrimaryTargetMessage = message as NewPrimaryTargetMessage;

                            if (newPrimaryTargetMessage != null)
                            {
                                // do something with the information in the message
                                D.log("Oh, I should be attacking " + newPrimaryTargetMessage.target.name);

                                comms.removeMessage(newPrimaryTargetMessage);
                            }
                            */
                        }
                    }

                    // example of sending a message
                    /*
                    if (structure.HullStrength < 0.2f * structure.MaxHullStrength)
                    {
                        if (comms.isSending() == false)
                        {
                            comms.broadcastMessage(squad, new DistressCallMessage(structure));
                        }
                    }
                    */
                }
            }
        }

        public virtual string searchAction()
        {
            if (structure.scanner.isActiveOn() == true)
            {
                if (structure.scanner.getEnemiesInRange().Count == 0)
                {
                    if (seekBehaviour != null && seekBehaviour.Active == false)
                    {
                        seekBehaviour.enableExclusively();
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
                }
                else
                {
                    return "COMBAT";
                }
            }
            else
            {
                return "SEARCH";
            }
        }

        public virtual string followAction()
        {
            if (Helm != null)
            {
                if (structure.scanner.isActiveOn() == true)
                {
                    List<Structure> enemies = structure.scanner.getEnemiesInRange();

                    IFormationFly formationController = null;

                    if (squadron != null)
                    {
                        formationController = squadron.getLeaderShip().Controller as IFormationFly;
                    }

                    if (formationController != null)
                    {
                        (Structure primaryTargetStructure, Module primaryTargetSystem) leaderTarget = formationController.getPrimaryTarget();
                        setPrimaryTarget(leaderTarget.primaryTargetStructure, leaderTarget.primaryTargetSystem);
                    }

                    Structure targetStructure = null;
                    Module targetSystem = null;

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
                        if (enemies.Contains(primaryTargetStructure))
                        {
                            targetStructure = primaryTargetStructure;
                        }
                    }
                    else
                    {
                        // get sorted threat ratios for all enemy ships and structures in range
                        List<(Structure enemy, float threat)> threats = threatSys.calculateThreatRatios(structure, enemies);

                        if (threats.Count > 0)
                        {
                            targetStructure = threats[0].enemy;

                            Helm.destination = targetStructure.transform.position;
                        }

                        // note: could also select a target system here as well
                    }

                    if (targetStructure == null)
                    {
                        foreach (Weapon weap in structure.Weapons)
                        {
                            TargetableWeapon tWeap = weap as TargetableWeapon;

                            if (tWeap != null)
                            {
                                tWeap.unacquireTarget();
                            }
                        }
                    }
                    else
                    {
                        // tell all fire groups to acquire the first target's hull (hence null for 2nd parameter)
                        foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                        {
                            if (targetSystem != null)
                            {
                                fireGroup.setTarget(targetStructure, targetSystem);
                            }
                            else
                            {
                                fireGroup.setTarget(targetStructure);
                            }
                        }
                    }

                    if (followBehaviour != null)
                    {
                        if (followBehaviour.Active == false)
                        {
                            followBehaviour.enableExclusively();
                        }
                    }
                    // now, you can shoot at things when in combat but you might just want to get the leader's target and use that
                    // or just do the usual and search for a target in scanner range (but this may not work very well...)                    
                }
                else
                {
                    foreach (Weapon weap in structure.Weapons)
                    {
                        TargetableWeapon tWeap = weap as TargetableWeapon;

                        if (tWeap != null)
                        {
                            tWeap.unacquireTarget();
                        }
                    }
                }
            }

            return "FOLLOW";
        }

        public virtual string combatAction()
        {
            if (structure.scanner.isActiveOn() == true)
            {
                List<Structure> enemies = structure.scanner.getEnemiesInRange();

                if (enemies.Count > 0)
                {
                    Structure targetStructure = null;
                    Module targetSystem = null;

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
                        if (enemies.Contains(primaryTargetStructure))
                        {
                            targetStructure = primaryTargetStructure;
                        }
                    }
                    else
                    {
                        targetStructure = boardingTarget;

                        // prioritise blasters
                        List<IBlasterTurret> blasters = boardingTarget.getWeapons<IBlasterTurret>();

                        // pick nearest

                        foreach (IBlasterTurret blaster in blasters)
                        {
                            if (blaster.isDestroyed() == false)
                            {
                                targetSystem = blaster.getGameObject().GetComponent<Module>();
                                break;
                            }
                        }

                        if (targetSystem == null)
                        {
                            // then prioritse lasers
                            List<ILaserTurret> lasers = boardingTarget.getWeapons<ILaserTurret>();

                            foreach (ILaserTurret laser in lasers)
                            {
                                if (laser.isDestroyed() == false)
                                {
                                    targetSystem = laser.getGameObject().GetComponent<Module>();
                                    break;
                                }
                            }
                        }









                        // get sorted threat ratios for all enemy ships and structures in range
                        List<(Structure enemy, float threat)> threats = threatSys.calculateThreatRatios(structure, enemies);

                        if (threats.Count > 0)
                        {
                            targetStructure = threats[0].enemy;
                        }
                    }

                    if (targetStructure != null)
                    {
                        foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                        {
                            if (targetSystem != null)
                            {
                                fireGroup.setTarget(targetStructure, targetSystem);
                            }
                            else
                            {
                                fireGroup.setTarget(targetStructure);
                            }
                        }

                        if (targetSystem != null)
                        {
                            Helm.destination = targetSystem.transform.position;
                        }
                        else
                        { 
                            Helm.destination = targetStructure.transform.position;
                        }

                        if (seekBehaviour != null)
                        {
                            if (seekBehaviour.Active == false)
                            {
                                seekBehaviour.enableExclusively();
                            }
                        }

                        Helm.destination = setHelmDestination();

                        state = "COMBAT";
                    }                    
                }

                foreach (Weapon weap in structure.Weapons)
                {
                    TargetableWeapon tWeap = weap as TargetableWeapon;

                    if (tWeap != null)
                    {
                        tWeap.unacquireTarget();
                    }
                }

                return "SEARCH";
            }

            return "SEARCH";
        }

        public string landAction()
        {
            // D.log("Controller", "Processing DOCK state");

            if (hangarStructure.Destroyed == true) return "SEARCH";

            if (ship.shipState == ShipState.LANDED) return "LAND";

            if (hangar != null)
            {
                float distanceToPort = Vector2.Distance(transform.position, hangar.getTransform().position);

                if (distanceToPort < 50)
                {
                    Helm.desiredThrottle = hangar.getMaxLandingSpeed() / ship.MaxSpeed;
                }

                if (seekBehaviour != null)
                {
                    if (seekBehaviour.Active == false)
                    {
                        seekBehaviour.enableExclusively();
                    }
                }

                Helm.destination = setHelmDestination();

                // draw a line to the destination
                if (Helm.destination != null && Cam.followTarget != null)
                {
                    if (Cam.followTarget != null && Cam.followTarget.gameObject == structure.gameObject)
                    {
                        Debug.DrawLine(structure.transform.position, Helm.destination.GetValueOrDefault(), Color.blue, Time.deltaTime, true);
                    }
                }

                return "LAND";
            }

            return "SEARCH";
        }

        // event handlers
        protected void OnMatchIsWaitingToStart(object sender)
        {
            // perfrom one-time squadron check to see if a leader has been selected in the inspector
            if (squadronInitialised == false)
            {
                foreach(Ship ship in squad)
                {
                    BasicSquadronAI ai = ship.GetComponent<BasicSquadronAI>();

                    if (ai.isLeader == true)
                    {
                        squadron.setLeader(ship.gameObject);
                        break;
                    }
                }

                squadronInitialised = true;
            }

            if (ship.shipState == ShipState.FLYING)
            {
                initController();
            }
        }

        protected void OnHasWarpedIn(object sender, WarpEventArgs args)
        {
            // add any initial targeting etc. here

            initController();

            // set boarding target
            setBoardingTarget(primaryTargetStructure);

            // set primary target
            setPrimaryTarget(primaryTargetStructure, primaryTargetSystem);
        }

        protected void OnSquadronHasWarpedIn(object sender, SquadronWarpedEventArgs args)
        {
            // add any inter-squad/squadron setup for this ship here

            squad = squadron.getAllShips();

            if (followBehaviour != null)
            {
                // make sure squadron has a leader
                if (squadron.getLeader() == null)
                {
                    squadron.updateLeader();
                }

                // find squadron leader and calculate formation offset
                Ship leaderShip = squadron.getLeaderShip();

                if (leaderShip != null)
                {
                    followBehaviour.FormationOffset = transform.position - leaderShip.transform.position;
                }
            }
        }

        public void LauncherFired(object sender, WeaponFiredEventArgs args)
        {
            //Gui.setMessage(args.weaponFired + " has fired!");
        }

        public void OnKilled(object sender, TargetDestroyedEventArgs args)
        {
            squadron.MembersAlive--;

            pickNewLeader();

            if (hangarStructure != null)
            {
                IHangar hangar = hangarStructure.getSocket<IHangar>() as IHangar;

                if (hangar != null)
                {
                    hangar.addShip(ship);
                }
            }

            if (squadron != null && squadron.MembersAlive == 0)
            {
                if (squadron.CanRespawn == true)
                {
                    foreach (Ship ship in squad)
                    {
                        IHangar hangar = hangarStructure.getSocket<IHangar>() as IHangar;

                        if (hangar != null)
                        {
                            if (hangar.requestLaunch(ship) == true)
                            {
                                //ship.Call_Respawn(this, new RespawnEventArgs(ship));
                                squadron.MembersAlive++;
                            }
                        }
                    }
                }
            }
        }
    }
}