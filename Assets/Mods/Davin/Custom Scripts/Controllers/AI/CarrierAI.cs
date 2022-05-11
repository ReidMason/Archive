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
using System.Linq;

namespace NoxCore.Controllers
{
    [RequireComponent(typeof(BasicThreatEvaluator))]
    public class CarrierAI : BasicCombatAI
    {
        BasicThreatEvaluator threatSys;

        protected List<Structure> squad;

        Station station;

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
            
            station = FindObjectOfType<Station>();

            booted = true;
        }

        public override string combatAction()
        {
            // Replace carrier AI with something similar to the customCombat AI
            List<Structure> enemiesInRange = structure.scanner.getEnemiesInRange();
            var nonStationEnemiesInRange = enemiesInRange.Where(x => x.GetInstanceID() != station.GetInstanceID()).ToList();
            if (nonStationEnemiesInRange.Count > 0)
            {
                List<(Structure structure, float threat)> threats = threatSys.calculateThreatRatios(structure, nonStationEnemiesInRange);

                var targetEnemy = threats[0].structure;
                foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                {
                    fireGroup.setTarget(targetEnemy);
                }

                Helm.destination = targetEnemy.gameObject.transform.position;

                return "COMBAT";
            }

            List<Weapon> activeStationTurrets = station.Weapons.Where(x => !x.destroyed).OrderByDescending(x => x.getDPS()).ToList();

            if (activeStationTurrets.Count > 0)
            {
                var targetWeapon = activeStationTurrets[0];

                foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
                {
                    fireGroup.setTarget(station, targetWeapon);
                }

                Helm.destination = targetWeapon.getPosition();

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