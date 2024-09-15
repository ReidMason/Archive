using NoxCore.Controllers;
using NoxCore.Fittings.Weapons;
using NoxCore.Helm;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomCombatAI : ImprovedCombatAI
{
    Station station;
    int targetAssignmentId;

    public override void boot(Structure structure, HelmController helm = null)
    {
        base.boot(structure, helm);

        // Find the station so we can use it later
        station = FindObjectOfType<Station>();

        Ship ship = structure as Ship;

        if (ship != null)
        {
            ship.HasWarpedIn += OnHasWarpedIn;
            ship.SquadronHasWarpedIn += OnSquadronHasWarpedIn;
            ship.WingHasWarpedIn += OnWingHasWarpedIn;
            ship.FleetHasWarpedIn += OnFleetHasWarpedIn;
        }
    }

    public override string combatAction()
    {
        List<Structure> enemiesInRange = structure.scanner.getEnemiesInRange();
        // We want to exlcude the station from the list of enemies in range
        var nonStationEnemiesInRange = enemiesInRange.Where(x => x.GetInstanceID() != station.GetInstanceID()).ToList();
        if (nonStationEnemiesInRange.Count > 0)
        {
            List<(Structure structure, float threat)> threats = threatSys.calculateThreatRatios(structure, nonStationEnemiesInRange);

            // The first enemy in the list is the one with the highest threat
            var targetEnemy = threats[0].structure;
            // Target all guns at this structure
            foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
            {
                fireGroup.setTarget(targetEnemy);
            }

            // Steer towards the target
            Helm.destination = targetEnemy.gameObject.transform.position;

            return "COMBAT";
        }

        // Because there are no active threats we can target the station
        // Get the alive turrents on the station and sort them by dps
        List<Weapon> activeStationTurrets = station.Weapons.Where(x => !x.destroyed).OrderByDescending(x => x.getDPS()).ToList();

        if (activeStationTurrets.Count > 0)
        {
            // We want each fighter to target their own turrent to kill them all faster
            // We use the targetAssignmentId for this
            // For instance if the targetAssignmentId is 2 it will target the third highest dps turret
            // If the target turrent has already been destroyed the ship will target the first turret instead
            int targetIndex = targetAssignmentId > activeStationTurrets.Count - 1 ? 0 : targetAssignmentId;
            var targetWeapon = activeStationTurrets[targetIndex];

            // Target all guns at the turret
            foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
            {
                fireGroup.setTarget(station, targetWeapon);
            }

            // Steer towards the turret
            Helm.destination = targetWeapon.getPosition();

            return "COMBAT";
        }
        else
        {
            // We can relax the guns
            foreach (FireGroup fireGroup in structure.FireControl.FireGroups)
            {
                fireGroup.unacquireTarget();
            }

            return "SEARCH";
        }
    }

    new protected void OnSquadronHasWarpedIn(object sender, SquadronWarpedEventArgs args)
    {
        // Get our current ship
        Ship ship = structure as Ship;
        // Get all other ships on the field
        List<Ship> ships = ship.WingData.squadrons.SelectMany(x => x.ships).ToList();
        // This ships assignemntId is its index in this list
        // So if this ship is 3rd in the list it's assignment id will be 2
        targetAssignmentId = ships.FindIndex(x => x.GetInstanceID() == ship.GetInstanceID());
    }
}
