using System;

using UnityEngine;

using System.Collections.Generic;

using NoxCore.Placeables.Ships;

namespace NoxCore.Data
{
	[CreateAssetMenu(fileName = "SquadronData Details", menuName = "ScriptableObjects/SquadronData")]
	public class SquadronData : ScriptableObject, ISerializationCallbackReceiver
	{
		public string label;
		public int ID;
		public List<Ship> ships = new List<Ship>();
		protected GameObject leader;

		[NonSerialized]
		protected int _maxShipsInSquadron;
		public int MaxShipsInSquadron { get { return _maxShipsInSquadron; } set { _maxShipsInSquadron = value; } }

		[NonSerialized]
		protected int _numSquadronMembers;
		public int NumSquadronMembers { get { return _numSquadronMembers; } set { _numSquadronMembers = value; } }

		[NonSerialized]
		protected int _membersAlive;
		public int MembersAlive { get { return _membersAlive; } set { _membersAlive = value; } }

		[SerializeField]
		protected bool _CanRespawn;
		public bool CanRespawn { get { return _CanRespawn; } set { _CanRespawn = value; } }

		public void incrementSquadron(FleetData fleet, WingData wing)
        {
			MaxShipsInSquadron++;

			wing.incrementWing(fleet);
        }

		public GameObject getLeader()
		{
			return leader;
		}

		public void setLeader(GameObject leader)
		{
			this.leader = leader;
		}

		public Ship getLeaderShip()
		{
			return leader.GetComponent<Ship>();
		}

		public List<Ship> getNonLeaderShips()
		{
			List<Ship> members = new List<Ship>();

			foreach (Ship ship in ships)
			{
				if (ship.gameObject != leader)
				{
					members.Add(ship);
				}
			}

			return members;
		}

		public int getShipIndex(Ship ship)
        {
			for (int i = 0; i < ships.Count; i++)
            {
				if (ships[i] == ship)
                {
					return i;
                }
            }

			return -1;
        }

		public List<Ship> getAllShips()
		{
			return ships;
		}

		public int removeShipByName(string shipName)
		{
			return ships.RemoveAll(f => f.name == shipName);
		}

		public Ship updateLeader()
		{
			for (int i = 0; i < ships.Count; i++)
			{
				if (ships[i].Destroyed == false && ships[i].shipState == ShipState.FLYING)
				{
					setLeader(ships[i].gameObject);
					return ships[i];
				}
			}

			leader = null;
			return null;
		}

		public bool addShip(Ship ship)
		{
			if (ships.Contains(ship) == false)
			{
				ships.Add(ship);

				NumSquadronMembers++;
				ship.WingData.NumWingMembers++;
				ship.FleetData.NumFleetMembers++;

				return true;
			}

			return false;
		}

		public bool removeShip(Ship ship)
		{
			if (ships.Contains(ship) == true)
			{
				NumSquadronMembers--;
				ship.WingData.NumWingMembers--;
				ship.FleetData.NumFleetMembers--;

				return ships.Remove(ship);
			}

			return false;
		}

		public Ship findShip(string shipName)
		{
			return ships.Find(f => f.name == shipName);
		}

		public void OnAfterDeserialize()
		{
			ships.Clear();
			leader = null;
		}

		public void OnBeforeSerialize()
		{ }
	}
}