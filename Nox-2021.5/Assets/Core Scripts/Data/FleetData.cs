using UnityEngine;

using System;
using System.Collections.Generic;

namespace NoxCore.Data
{
	[CreateAssetMenu(fileName = "FleetData Details", menuName = "ScriptableObjects/FleetData")]
	public class FleetData : ScriptableObject, ISerializationCallbackReceiver
	{
		public string label;
		public int ID;
		public List<WingData> wings = new List<WingData>();
		protected GameObject admiral;

		[NonSerialized]
		protected int _maxShipsInFleet;
		public int MaxShipsInFleet { get { return _maxShipsInFleet; } set { _maxShipsInFleet = value; } }

		[NonSerialized]
		protected int _numFleetMembers;
		public int NumFleetMembers { get { return _numFleetMembers; } set { _numFleetMembers = value; } }

		public void incrementFleet()
        {
			MaxShipsInFleet++;
		}

		public GameObject getAdmiral()
		{
			return admiral;
		}	
		
		public void updateAdmiral()
		{
			for (int i = 0; i < wings.Count; i++)
			{			
				if (wings[i].getCommanderData() != null)
				{
					admiral = wings[i].getCommanderData();
					break;
				}
			}
			
			admiral = null;
		}
		
		public bool addWingData(WingData wing)
		{
			if (wings.Contains(wing) == false)
			{
				wings.Add(wing);
				return true;
			}
			
			return false;
		}
		
		public bool removeWingData(WingData wing)
		{
			if (wings.Contains(wing) == true)
			{
				wings.Remove(wing);
				return true;
			}
			
			return false;
		}
		
		public int removeWingDataByName(string wingName)
		{
			return wings.RemoveAll ( f => f.label == wingName);
		}
		
		public int removeWingDataByID(int wingID)
		{
			return wings.RemoveAll ( f => f.ID == wingID);
		}

		public WingData findWingData(int wingID)
		{
			return wings.Find(f => f.ID == wingID);
		}

        public WingData findWingData(string wingName)
        {
            return wings.Find(f => f.label == wingName);
        }

		public virtual void OnAfterDeserialize()
		{
			wings.Clear();
			admiral = null;
		}

		public virtual void OnBeforeSerialize()
		{ }
	}
}