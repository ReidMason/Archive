using UnityEngine;
using System.Collections;

namespace NoxCore.Helm
{
	public class FollowLeaderBehaviour : SteeringBehaviour
	{
		[SerializeField] protected Rigidbody2D _leader;
		public Rigidbody2D Leader { get { return _leader; } set { _leader = value; } }

		[SerializeField] protected float _distanceBehind;
		public float DistanceBehind {  get { return _distanceBehind; } set { _distanceBehind = value; } }

		protected ArriveBehaviour arriveBehaviour;
		protected AlignmentBehaviour alignmentBehaviour;
		protected CohesionBehaviour cohesionBehaviour;
		protected SeparationBehaviour separationBehaviour;

		void Reset()
		{
			Label = "FOLLOW";
			SequenceID = 7;
			Weight = 50;
			DistanceBehind = 50;
		}

		public override void Start()
		{
			base.Start();

			arriveBehaviour = Helm.getBehaviourByName("ARRIVE") as ArriveBehaviour;
			alignmentBehaviour = Helm.getBehaviourByName("ALIGNMENT") as AlignmentBehaviour;
			cohesionBehaviour = Helm.getBehaviourByName("COHESION") as CohesionBehaviour;
			separationBehaviour = Helm.getBehaviourByName("SEPARATION") as SeparationBehaviour;
		}

        public override Vector2 execute()
		{
			Vector2 leaderVelocity = Leader.velocity;
			
			// calculate point behind leader and set as new helm destination
			leaderVelocity *= -1;
			leaderVelocity.Normalize();
			leaderVelocity *= DistanceBehind;
			
			Helm.destination = (Vector2)(Leader.transform.position) + leaderVelocity;
			
			// calculate force to arrive at point behind leader
			if (arriveBehaviour != null)
			{
				Vector2 force = arriveBehaviour.execute();
				
				// multiply the force by the steering behaviour weight
				if (force.magnitude > 0)
				{
					force *= arriveBehaviour.Weight;
				}				

				if (alignmentBehaviour != null)
				{		
					Vector2 alignmentForce = alignmentBehaviour.execute();
					
					// multiply the force by the steering behaviour weight
					if (alignmentForce.magnitude > 0)
					{
						alignmentForce *= alignmentBehaviour.Weight;
						force += alignmentForce;
					}
				}

				if (cohesionBehaviour != null)
				{		
					Vector2 cohesionForce = cohesionBehaviour.execute();
					
					// multiply the force by the steering behaviour weight
					if (cohesionForce.magnitude > 0)
					{
						cohesionForce *= cohesionBehaviour.Weight;
						force += cohesionForce;
					}
				}

				if (separationBehaviour != null)
				{		
					Vector2 separationForce = separationBehaviour.execute();
					
					// multiply the force by the steering behaviour weight
					if (separationForce.magnitude > 0)
					{
						separationForce *= separationBehaviour.Weight;
						force += separationForce;
					}
				}
				
				force = force.normalized * (Helm.ShipStructure.MaxSpeed * Helm.throttle);
				
				return force;
			}
			else
			{
				return Vector2.zero;
			}
		}
	}
}