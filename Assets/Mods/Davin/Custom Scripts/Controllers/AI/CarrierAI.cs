using UnityEngine;
using System.Collections.Generic;

using NoxCore.Data;
using NoxCore.Fittings.Weapons;
using NoxCore.GameModes;
using NoxCore.Helm;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Fittings.Sockets;

using Davin.Fittings.Devices;

namespace NoxCore.Controllers
{
    [RequireComponent(typeof(BasicThreatEvaluator))]
    public class CarrierAI : BasicCombatAI
    {
        BasicThreatEvaluator threatSys;

        protected List<Structure> squad;

        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, helm);

            structure.HasRespawned += OnHasRespawned;

            Ship ship = structure as Ship;

            if (ship != null)
            {
                ship.HasWarpedIn += OnHasWarpedIn;
                ship.SquadronHasWarpedIn += OnSquadronHasWarpedIn;
                ship.WingHasWarpedIn += OnWingHasWarpedIn;
                ship.FleetHasWarpedIn += OnFleetHasWarpedIn;
            }

            threatSys = GetComponent<BasicThreatEvaluator>();

            booted = true;
        }

        public override string combatAction()
        { 
            if (structure.scanner.isActiveOn() == true)
            {
                List<Structure> enemiesInRange = structure.scanner.getEnemiesInRange();

                if (enemiesInRange.Count > 0)
                {
                    // get sorted threat ratios for all enemy ships and structures in range
                    List<(Structure structure, float threat)> threats = threatSys.calculateThreatRatios(structure, enemiesInRange);

                    // tell all fire groups to acquire the first target's hull (hence null for 2nd parameter)
                    foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                    {
                        fireGroup.setTarget(threats[0].structure);
                    }

                    if (orbitBehaviour != null)
                    {
                        if (orbitBehaviour.Active == false)
                        {
                            orbitBehaviour.enableExclusively();
                        }
                    }

                    // use the first target as the ship/structure to orbit around
                    orbitBehaviour.OrbitObject = threats[0].structure.transform;

                    // use the first weapon's maximum range to determie a suitable orbit range
                    if (structure.Weapons.Count > 0)
                    {
                        orbitBehaviour.OrbitRange = structure.Weapons[0].WeaponData.MaxRange - 1;
                    }

                    return "COMBAT";
                }
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

            return "COMBAT";
        }

        protected void OnHasWarpedIn(object sender, WarpEventArgs args)
        {
            // add any initial targeting etc. here
        }

        protected void OnSquadronHasWarpedIn(object sender, SquadronWarpedEventArgs args)
        {
            // add any inter-squad/squadron setup for this ship here
        }

        protected void OnWingHasWarpedIn(object sender, WingWarpedEventArgs args)
        {
            // add any inter-wing setup for this ship here
        }

        protected void OnFleetHasWarpedIn(object sender, FleetWarpedEventArgs args)
        {
            // add any inter-faction setup for this ship here

            FactionData faction = FactionManager.Instance.findFaction(args.fleetName);

            if (faction != null)
            {
                List<Ship> ships = faction.FleetManager.getAllShips();

                foreach (Ship ship in ships)
                {
                    if (ship.Classification == ShipClassification.FIGHTER || ship.Classification == ShipClassification.BOMBER)
                    {
                        ILand landingController = ship.Controller as ILand;

                        if (landingController != null)
                        {
                            landingController.setHangarStructure(structure);
                        }
                    }
                }
            }
        }

        public void OnHasRespawned(object sender, RespawnEventArgs args)
        {
            // go through all hangars
            List<IHangar> hangars = structure.getSockets<IHangar>();

            List<SquadronData> squadToLaunch = new List<SquadronData>();

            foreach (IHangar shipHangar in hangars)
            {
                List<Ship> shipsInHangar = shipHangar.getShipsInHangar();

                // launch any full squadrons in the hangar
                foreach (Ship ship in shipsInHangar)
                {
                    SquadronData hangarSquadronData = squadronInHangar(ship, shipsInHangar);

                    if (hangarSquadronData != null)
                    {
                        if (!squadToLaunch.Contains(hangarSquadronData))
                        {
                            squadToLaunch.Add(hangarSquadronData);
                        }
                    }
                }

                // launch hangar squadron
                foreach(SquadronData squadron in squadToLaunch)
                {
                    foreach (Ship ship in squadron.ships)
                    {
                        shipHangar.requestLaunch(ship);

                        squadron.MembersAlive++;
                    }
                }

                squadToLaunch.Clear();
            }
        }

        protected SquadronData squadronInHangar(Ship ship, List<Ship> shipsInHangar)
        {
            foreach(Ship squadronShip in ship.SquadronData.ships)
            {
                if (!shipsInHangar.Contains(squadronShip))
                {
                    return null;
                }
            }

            return ship.SquadronData;
        }

        public static IHangar StructureSocketToIHangar(StructureSocket socket)
        {
            return socket as IHangar;
        }
    }
}