using UnityEngine;
using System.Collections.Generic;

using NoxCore.Placeables.Ships;
using NoxCore.Utilities;

namespace NoxCore.Helm
{
	public class CohesionBehaviour : SteeringBehaviour
	{
		[SerializeField] protected float _neighbourDistance;
		public float NeighbourDistance { get { return _neighbourDistance; } set { _neighbourDistance = value; } }

		[SerializeField] [Range(0, 1)] protected float _cohesionFactor;
		public float CohesionFactor { get { return _cohesionFactor; } set { _cohesionFactor = value; } }
		
		[ShowOnly]
		protected List<Ship> _squadronMembers;
		public List<Ship> SquadronMembers { get { return _squadronMembers; } set { _squadronMembers = value; } }

        void Reset()
        {
			Label = "COHESION";
			SequenceID = 9;
			Weight = 50;
			NeighbourDistance = 200;
			CohesionFactor = 1;
        }
		
		public override Vector2 execute()
		{
			Vector2 force = Vector2.zero;

            if (SquadronMembers == null) return force;

            int neighbourCount = 0;
			
			for (int i = 0; i < SquadronMembers.Count; i++)
			{			
				if (SquadronMembers[i] != Helm.ShipStructure && SquadronMembers[i].Destroyed == false)
				{					
					float distance = Vector2.Distance(Helm.ShipStructure.transform.position, SquadronMembers[i].transform.position);									
					
					if (distance < NeighbourDistance)
					{
						force += (Vector2)(SquadronMembers[i].transform.position);
						neighbourCount++;
					}
				}
			}

			if (neighbourCount == 0)
			{
				return force;
			}
			
			force /= neighbourCount;
			force -= (Vector2)Helm.ShipStructure.transform.position;
			force.Normalize();
			force *= CohesionFactor;
			
			return force;
		}
	}
}