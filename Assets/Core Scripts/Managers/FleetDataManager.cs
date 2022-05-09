using System.Collections.Generic;

using NoxCore.Data;
using NoxCore.Placeables.Ships;

namespace NoxCore.Managers
{
    public class FleetDataManager
	{
		public List<FleetData> fleets = new List<FleetData>();

        public List<Ship> getAllShips()
        {
            List<Ship> ships = new List<Ship>();

            foreach (FleetData fleet in fleets)
            {
                foreach (WingData wing in fleet.wings)
                {
                    foreach (SquadronData squadron in wing.squadrons)
                    {
                        foreach (Ship ship in squadron.ships)
                        {
                            ships.Add(ship);
                        }
                    }
                }
            }

            // include ships in faction not in a fleet


            return ships;
        }

        public int getNumShips()
        {
            int count = 0;

            foreach (FleetData fleet in fleets)
            {
                foreach (WingData wing in fleet.wings)
                {
                    foreach (SquadronData squadron in wing.squadrons)
                    {
                        foreach (Ship ship in squadron.ships)
                        {
                            count++;
                        }
                    }
                }
            }

            return count;
        }

        public FleetData findFleetData(int fleetID)
        {
            return fleets.Find(f => f.ID == fleetID);
        }

        public FleetData findFleetData(string fleetName)
        {
            return fleets.Find(f => f.name == fleetName);
        }

        public bool addFleetData(FleetData fleet)
		{
			if (fleets.Contains(fleet) == false)
			{
				fleets.Add(fleet);
				return true;
			}
			
			return false;
		}
			
		public bool removeFleetData(FleetData fleet)
		{
            if (fleets.Contains(fleet))
            {
                fleets.Remove(fleet);
                return true;
            }

            return false;
		}

        public bool removeFleetData(int fleetID)
        {
            FleetData fleet = findFleetData(fleetID);

            if (fleet != null)
            {
                if (fleet != null)
                {
                    fleets.Remove(fleet);
                    return true;
                }
            }

            return false;
        }

        public bool removeFleetData(string fleetName)
		{
            FleetData fleet = findFleetData(fleetName);

            if (fleets != null)
            {
                fleets.Remove(fleet);
                return true;
            }

            return false;
		}

        public WingData findWingData(int fleetID, int wingID)
        {
            FleetData fleet = findFleetData(fleetID);

            if (fleet != null)
            {
                return fleet.findWingData(wingID);
            }

            return null;
        }

        public WingData findWingData(int fleetID, string wingName)
        {
            FleetData fleet = findFleetData(fleetID);

            if (fleet != null)
            {
                return fleet.findWingData(wingName);
            }

            return null;
        }

        public SquadronData findSquadronData(int fleetID, int wingID, int squadronID)
        {
            FleetData fleet = findFleetData(fleetID);

            if (fleet != null)
            {
                WingData wing = fleet.findWingData(wingID);

                if (wing != null)
                {
                    return wing.findSquadronData(squadronID);
                }
            }

            return null;
        }

        public SquadronData findSquadronData(int fleetID, int wingID, string squadronName)
        {
            FleetData fleet = findFleetData(fleetID);

            if (fleet != null)
            {
                WingData wing = fleet.findWingData(wingID);

                if (wing != null)
                {
                    return wing.findSquadronData(squadronName);
                }
            }

            return null;
        }
    }
}