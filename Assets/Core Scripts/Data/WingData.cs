using UnityEngine;

using System;
using System.Collections.Generic;

namespace NoxCore.Data
{
	[CreateAssetMenu(fileName = "WingData Details", menuName = "ScriptableObjects/WingData")]
	public class WingData : ScriptableObject, ISerializationCallbackReceiver
	{
		public string label;
		public int ID;
		public List<SquadronData> squadrons = new List<SquadronData>();
		protected GameObject commander;

		[NonSerialized]
		protected int _maxShipsInWing;
		public int MaxShipsInWing { get { return _maxShipsInWing; } set { _maxShipsInWing = value; } }

		[NonSerialized]
		protected int _numWingMembers;
		public int NumWingMembers { get { return _numWingMembers; } set { _numWingMembers = value; } }

		public void incrementWing(FleetData fleet)
        {
			MaxShipsInWing++;

			fleet.incrementFleet();
		}

		public GameObject getCommanderData()
		{
			return commander;
		}	
		
		public void updateCommanderData()
		{
			for (int i = 0; i < squadrons.Count; i++)
			{
				if (squadrons[i].getLeader() != null)
				{
					commander = squadrons[i].getLeader();
					return;
				}
			}
			
			commander = null;
		}
		
		public bool addSquadronData(SquadronData squadron)
		{
			if (squadrons.Contains(squadron) == false)
			{
				squadrons.Add(squadron);
				return true;
			}
			
			return false;
		}
		
		public bool removeSquadronData(SquadronData squadron)
		{
			if (squadrons.Contains(squadron) == true)
			{
				squadrons.Remove(squadron);
				return true;
			}
			
			return false;
		}

        public int removeSquadronDataByID(int squadronID)
        {
            return squadrons.RemoveAll(f => f.ID == squadronID);
        }

        public int removeSquadronDataByName(string squadronName)
		{
			return squadrons.RemoveAll ( f => f.label == squadronName);
		}
		
        public SquadronData findSquadronData(int squadronID)
        {
            return squadrons.Find(f => f.ID == squadronID);
        }

        public SquadronData findSquadronData(string squadronName)
        {
            return squadrons.Find(f => f.label == squadronName);
        }

		public virtual void OnAfterDeserialize()
		{
			squadrons.Clear();
			commander = null;
		}
		
        public virtual void OnBeforeSerialize()
        {}
    }
}