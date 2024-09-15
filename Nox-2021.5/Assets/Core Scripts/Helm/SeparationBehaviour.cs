using UnityEngine;
using System.Collections.Generic;

using NoxCore.Placeables.Ships;
using NoxCore.Utilities;

namespace NoxCore.Helm
{
	public class SeparationBehaviour : SteeringBehaviour
	{
		[SerializeField] protected float _neighbourDistance;
		public float NeighbourDistance { get { return _neighbourDistance; } set { _neighbourDistance = value; } }

		[SerializeField] [Range(0,1)]protected float _separationFactor;
		public float SeparationFactor { get { return _separationFactor; } set { _separationFactor = value; } }

		[ShowOnly]
		protected List<Ship> _squadronMembers;
		public List<Ship> SquadronMembers { get { return _squadronMembers; } set { _squadronMembers = value; } }

		void Reset()
		{
			Label = "SEPARATION";
			SequenceID = 10;
			Weight = 100;
			NeighbourDistance = 200;
			SeparationFactor = 1;
		}
		
		public override Vector2 execute()
		{
			Vector2 force = Vector2.zero;

            if (SquadronMembers == null) return force;

            for (int i = 0; i < SquadronMembers.Count; i++)
			{			
				if (SquadronMembers[i] != Helm.ShipStructure && SquadronMembers[i].Destroyed == false)
				{
					float distance = Vector2.Distance(Helm.ShipStructure.transform.position, SquadronMembers[i].transform.position);									
				
					if (distance <= NeighbourDistance)
					{
						force += (Vector2)(SquadronMembers[i].transform.position - Helm.ShipStructure.transform.position);
					}
				}
			}
			
			force /= SquadronMembers.Count;
			force *= -1;
			
			force.Normalize();
			force *= SeparationFactor;
			
			return force;
		}
	}
}