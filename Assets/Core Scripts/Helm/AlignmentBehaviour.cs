using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Placeables.Ships;
using NoxCore.Utilities;

namespace NoxCore.Helm
{
	public class AlignmentBehaviour : SteeringBehaviour
	{
		[SerializeField] protected float _neighbourDistance;
		public float NeighbourDistance { get { return _neighbourDistance; } set { _neighbourDistance = value; } }

		[SerializeField] [Range(0, 1)] protected float _alignmentFactor;
		public float AlignmentFactor {  get { return _alignmentFactor; } set { _alignmentFactor = value; } }

		[ShowOnly]
		protected List<Ship> _squadronMembers;
		public List<Ship> SquadronMembers { get { return _squadronMembers; } set { _squadronMembers = value; } }

		void Reset()
		{
			Label = "ALIGNMENT";
			SequenceID = 8;
			Weight = 50;
			NeighbourDistance = 200;
			AlignmentFactor = 1;
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
					
					if (distance <= NeighbourDistance)
					{
						force += SquadronMembers[i].StructureRigidbody.velocity;
						neighbourCount++;
					}
				}
			}
			
			if (neighbourCount == 0)
			{
				return force;
			}
			
			force /= neighbourCount;
			force.Normalize();
			force *= AlignmentFactor;
			
			return force;			
		}
	}
}