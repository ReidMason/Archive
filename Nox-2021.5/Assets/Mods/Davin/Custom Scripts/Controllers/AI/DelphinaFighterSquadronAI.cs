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
    public class DelphinaFighterSquadronAI : AIStateController, IFormationFly, ILand
    {
        public bool isLeader;

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

        protected Ship ship;
        protected List<Structure> squad;
        protected SquadronData squadron;

        protected Structure boardingTarget;
        protected List<Weapon> boardingTargetWeapons;

        public Structure primaryTargetStructure;
        public Module primaryTargetSystem;

        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, helm);

            ship = structure as Ship;

            ship.HasWarpedIn += OnHasWarpedIn;

            structure.Respawn += AI_SquadronDataMemberRespawned;

            structure.NotifyKilled += AI_NotifyKilled;

            aiActions.Add("SEARCH", searchAction);
            aiActions.Add("FOLLOW", followAction);
            aiActions.Add("COMBAT", combatAction);
            aiActions.Add("LAND", landAction);

            state = "SEARCH";

            seekBehaviour = Helm.getBehaviourByName("SEEK") as SeekBehaviour;
            orbitBehaviour = Helm.getBehaviourByName("ORBIT") as OrbitBehaviour;
            avoidBehaviour = Helm.getBehaviourByName("AVOID") as AvoidBehaviour;
            followBehaviour = Helm.getBehaviourByName("FOLLOW") as FormationFollowLeaderBehaviour;
            alignBehaviour = Helm.getBehaviourByName("ALIGN") as AlignmentBehaviour;
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

            booted = true;

            threatSys = GetComponent<BasicThreatEvaluator>();

            comms = structure.getDevice<IComms>() as IComms;

            if (followBehaviour != null)
            {
                // note: this is a bit of defensive coding and this value should be overriden via the IFormationFly interface from the ship's builder
                followBehaviour.FormationOffset = Vector2.zero;
            }

            booted = true;
        }

        public void initController()
        {
            FactionData faction = FactionManager.Instance.findFaction(structure.Faction.label);

            List<Ship> ships = faction.FleetManager.getAllShips();

            squad = ships.Cast<Structure>().ToList();            

            squadron = faction.FleetManager.findSquadronData(ship.FleetData.ID, ship.WingData.ID, ship.SquadronData.ID);

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
                    alignBehaviour.SquadronMembers = squadron.ships.Cast<Ship>().ToList();
                }

                if (cohesionBehaviour != null)
                {
                    cohesionBehaviour.SquadronMembers = squadron.ships.Cast<Ship>().ToList();
                }

                if (separationBehaviour != null)
                {
                    separationBehaviour.SquadronMembers = squadron.ships.Cast<Ship>().ToList();
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
                        memberAlignmentBehaviour.SquadronMembers = squadron.ships.Cast<Ship>().ToList();
                    }

                    if (memberCohesionBehaviour != null)
                    {
                        memberCohesionBehaviour.SquadronMembers = squadron.ships.Cast<Ship>().ToList();
                    }

                    if (memberSeparationBehaviour != null)
                    {
                        memberSeparationBehaviour.SquadronMembers = squadron.ships.Cast<Ship>().ToList();
                    }
                }

                squadron.MembersAlive = squadron.ships.Count;
            }
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
            if (this.primaryTargetStructure != null)
            {
                Structure curPrimaryStructure = this.primaryTargetStructure.GetComponent<Structure>();

                curPrimaryStructure.NotifyKilled -= AI_TargetDestroyed;
            }

            this.primaryTargetStructure = primaryTargetStructure;
            this.primaryTargetSystem = primaryTargetSystem;

            if (this.primaryTargetStructure != null)
            {
                Structure curPrimaryStructure = this.primaryTargetStructure.GetComponent<Structure>();

                curPrimaryStructure.NotifyKilled += AI_TargetDestroyed;
            }
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
            if (hangarStructure != null)
            {
                foreach (StructureSocket socket in hangarStructure.StructureSockets)
                {
                    IHangar hangar = socket as Hangar;

                    if (hangar != null)
                    {
                        return hangar;
                    }
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

            if (squadron.getLeader() == null)
            {
                squadron.updateLeader();
                isLeader = true;

                state = "SEARCH";

                Helm.Destination = setHelmDestination();
            }
            else
            {
                isLeader = false;
                state = "FOLLOW";
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

                        // pick new leader (or stay as this one if no others?)
                        pickNewLeader();

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

                    /*
                    // draw a line to the destination
                    if (Helm.destination != null && Cam.followTarget != null && Cam.followTarget.gameObject == Helm.ShipStructure.gameObject)
                    {
                        Debug.DrawLine(structure.transform.position, Helm.destination.GetValueOrDefault(), Color.blue, Time.deltaTime, true);
                    }
                    */

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
            if (structure.scanner.isActiveOn() == true)
            {
                if (followBehaviour != null)
                {
                    if (followBehaviour.Active == false)
                    {
                        followBehaviour.enableExclusively();
                    }
                }

                // now, you can shoot at things when in combat but you might just want to get the leader's target and use that
                // or just do the usual and search for a target in scanner range (but this may not work very well...)

                // new scanner data?
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
            }

            return "FOLLOW";
        }

        public virtual string combatAction()
        {
            if (structure.scanner.isActiveOn() == true)
            {
                #region targeting
                List<Structure> enemiesInRange = structure.scanner.getEnemiesInRange();

                if (enemiesInRange.Count > 0)
                {
                    List<Structure> enemies = structure.Faction.EnemyStructures;

                    if (state == "FOLLOW")
                    {
                        IFormationFly formationController = squadron.getLeaderShip().Controller as IFormationFly;

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
                            List<(Structure structure, float threat)> threats = threatSys.calculateThreatRatios(structure, enemies);

                            if (threats.Count > 0)
                            {
                                targetStructure = threats[0].structure;

                                Helm.destination = targetStructure.transform.position;
                            }

                            // note: could also select a target system here as well
                        }

                        if (targetStructure == null)
                        {
                            foreach (Weapon weap in structure.Weapons)
                            {
                                TargetableWeapon tWeap = (TargetableWeapon)weap;

                                if (tWeap != null)
                                {
                                    tWeap.unacquireTarget();
                                }
                            }
                        }
                        else
                        {
                            /*
                            // get list of weapons off the target structure
                            List<Weapon> targetWeapons = targetStructure.GetComponent<Structure>().weapons;

                            List<LaserTurret> laserTurrets = new List<LaserTurret>();
                            List<BlasterTurret> blasterTurrets = new List<BlasterTurret>();
                            List<ProjectileLauncher> missileLaunchers = new List<ProjectileLauncher>();

                            // find next intact weapon to target
                            foreach (Weapon weapon in targetWeapons)
                            {
                                // if this weapon is destroyed, move onto the next one
                                if (weapon.destroyed == true) continue;

                                // check for intact laser turrets
                                LaserTurret laserWeapon = weapon as LaserTurret;

                                if (laserWeapon != null)
                                {
                                    laserTurrets.Add(laserWeapon);
                                }

                                BlasterTurret blasterWeapon = weapon as BlasterTurret;

                                if (blasterWeapon != null)
                                {
                                    blasterTurrets.Add(blasterWeapon);
                                }

                                ProjectileLauncher projectileWeapon = weapon as ProjectileLauncher;

                                if (projectileWeapon != null)
                                {
                                    if (projectileWeapon.Type == "GUIDEDPROJECTILELAUNCHER" && projectileWeapon.SubType.Contains("MISSILE"))
                                    {
                                        missileLaunchers.Add(projectileWeapon);
                                    }                            
                                }
                            }
                            */

                            // tell all fire groups to acquire the target
                            foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                            {
                                fireGroup.setTarget(targetStructure.gameObject, targetSystem.gameObject);
                            }
                        }
                    }
                    else if (state == "COMBAT" && enemies.Count > 0)
                    {
                        if (seekBehaviour != null)
                        {
                            if (seekBehaviour.Active == false)
                            {
                                seekBehaviour.enableExclusively();
                            }
                        }

                        Helm.destination = setHelmDestination();

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
                                TargetableWeapon tWeap = (TargetableWeapon)weap;

                                if (tWeap != null)
                                {
                                    tWeap.unacquireTarget();
                                }
                            }

                            state = "SEARCH";
                        }
                        else
                        {
                            // tell all fire groups to acquire the first target's hull (hence null for 2nd parameter)
                            foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                            {
                                fireGroup.setTarget(targetStructure.gameObject, targetSystem.gameObject);
                            }
                        }
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
                #endregion
            }
            else
            {
                return "SEARCH";
            }
        }

        public string landAction()
        {
            // D.log("Controller", "Processing DOCK state");

            if (hangarStructure.Destroyed == true) return "SEARCH";

            if (ship.shipState == ShipState.LANDED) return "LAND";

            if (hangar == null)
            {
                hangar = findHangar(hangarStructure);

                if (hangar == null) return "SEARCH";
            }

            float distanceToPort = Vector2.Distance(transform.position, hangar.getTransform().position);

            if (distanceToPort < 100)
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

            /*
            // draw a line to the destination
            if (Helm.destination != null && Cam.followTarget != null)
            {
                if (Cam.followTarget != null && Cam.followTarget.gameObject == structure.gameObject)
                {
                    Debug.DrawLine(structure.transform.position, Helm.destination.GetValueOrDefault(), Color.blue, Time.deltaTime, true);
                }
            }
            */

            return "LAND";
        }

        protected void OnHasWarpedIn(object sender, WarpEventArgs args)
        {
            // add any initial targeting etc. here based on fleet, wing, squadron ids

            Structure primaryTarget = null;

            if (ship.SquadronData.ID == 2)
            {
                primaryTarget = GameObject.Find("N'Thus Bomber 1-5").GetComponent<Structure>();
            }
            else if (ship.SquadronData.ID == 3)
            {
                primaryTarget = GameObject.Find("N'Thus Bomber 1-6").GetComponent<Structure>();
            }

            setPrimaryTarget(primaryTarget);
        }

        public void LauncherFired(object sender, WeaponFiredEventArgs args)
        {
            //Gui.setMessage(args.weaponFired + " has fired!");
        }

        public void AI_TargetDestroyed(object sender, TargetDestroyedEventArgs args)
        {
            if (isLeader == false) return;

            if (args != null)
            {
                if (args.structureAttacked == primaryTargetStructure)
                {
                    Structure primaryTarget = null;

                    if (squadron.ID == 2)
                    {
                        for (int i = 1; i < 6; i++)
                        {
                            GameObject enemy = GameObject.Find("N'Thus Bomber " + i + "-5");

                            Structure enemyStructure = enemy.GetComponent<Structure>();

                            if (enemyStructure.Destroyed == false)
                            {
                                primaryTarget = enemyStructure;
                                break;
                            }
                        }
                    }
                    else if (squadron.ID == 3)
                    {
                        for (int i = 1; i < 6; i++)
                        {
                            GameObject enemy = GameObject.Find("N'Thus Bomber " + i + "-6");

                            Structure enemyStructure = enemy.GetComponent<Structure>();

                            if (enemyStructure.Destroyed == false)
                            {
                                primaryTarget = enemyStructure;
                                break;
                            }
                        }
                    }

                    if (primaryTarget != null)
                    {
                        foreach (Ship ship in squadron.ships)
                        {
                            BasicFighterSquadronAI squadronController = ship.Controller as BasicFighterSquadronAI;

                            if (squadronController != null)
                            {
                                squadronController.setPrimaryTarget(primaryTarget);
                            }
                        }
                    }
                }
            }
        }

        public void AI_NotifyKilled(object sender, TargetDestroyedEventArgs args)
        {
            if (args != null)
            {
                if (squadron != null)
                {
                    squadron.MembersAlive--;

                    pickNewLeader();
                }
            }
        }

        public void AI_SquadronDataMemberRespawned(object sender, RespawnEventArgs args)
        {
            if (squadron != null)
            {
                reset();

                if (squadron.MembersAlive == 0)
                {
                    if (hangar != null && hangarStructure.Destroyed == false)
                    {
                        foreach (Ship ship in squadron.ships)
                        {
                            hangar.requestLaunch(ship);
                        }
                    }
                }
            }
        }
    }
}