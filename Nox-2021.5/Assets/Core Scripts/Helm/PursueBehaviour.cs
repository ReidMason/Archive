using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Utilities;

namespace NoxCore.Helm
{
	public class PursueBehaviour : SteeringBehaviour
	{
		[SerializeField] [Range(0, 1)] protected float _lookAheadFactor;
		public float LookAheadFactor { get { return _lookAheadFactor; } set { _lookAheadFactor = value; } }

		[SerializeField] protected float _predictionTime;
		public float PredictionTime { get { return _predictionTime; } set { _predictionTime = value; } }

        [SerializeField] [ShowOnly] protected bool _evadeGroup;
        public bool EvadeGroup { get { return _evadeGroup; } set { _evadeGroup = value; } }

        [SerializeField] [ShowOnly] protected Rigidbody2D _target;
        public Rigidbody2D Target { get { return _target; } set { _target = value; } }

        [SerializeField] [ShowOnly] protected List<Rigidbody2D> _targets;
        public List<Rigidbody2D> Targets { get { return _targets; } set { _targets = value; } }

		public Vector2 offset;
		protected float length;

        void Reset()
        {
			Label = "PURSUE";
			SequenceID = 6;
			LookAheadFactor = 1.0f;
			PredictionTime = 1.5f;			
        }

        void Awake()
		{
			length = transform.parent.GetComponent<PolygonCollider2D>().bounds.extents.y;
		}
		
		public override Vector2 execute()
		{
			if (Helm.destination != null)
			{
				desiredVelocity = Vector2.zero;
				
				Vector2 futurePosition;
				
				if (EvadeGroup == true && Target != null)
				{
					float distanceToTarget = Vector2.Distance(Target.position + offset, Helm.ShipStructure.transform.position);
					
					PredictionTime = distanceToTarget / (Helm.ShipStructure.MaxSpeed * Helm.throttle);
					
					futurePosition = (Target.position + offset) + (Target.velocity * PredictionTime);
				}
				else if (Targets != null)
				{
					Vector2 groupCentre = Vector2.zero;
					Vector2 groupVelocity = Vector2.zero;
					
					foreach(Rigidbody2D targ in Targets)
					{
						groupCentre += targ.position;
						groupVelocity += targ.velocity;
					}
					
					groupCentre /= Targets.Count;
					
					float distanceToGroupCentre = Vector2.Distance(groupCentre, Helm.ShipStructure.transform.position);
					
					PredictionTime = distanceToGroupCentre / (Helm.ShipStructure.MaxSpeed * Helm.throttle);					
					
					futurePosition = groupCentre + (groupVelocity * PredictionTime);
				}
				else
				{
					return Vector2.zero;
				}
				
				steeringVector = futurePosition - Helm.Position;
				
				float rangeToPosition = steeringVector.magnitude;

                // calculate a look ahead distance based on the ship's length and its current speed
                float lookAhead = length + (length * Helm.ShipStructure.Speed * LookAheadFactor);

                if (rangeToPosition < lookAhead)
				{
					Helm.destination = null;
				}
				else
				{
					desiredVelocity = steeringVector.normalized;
					desiredVelocity *= (Helm.ShipStructure.MaxSpeed * Helm.throttle);
				}
				
				return desiredVelocity - Helm.ShipRigidbody.velocity;
			}
			else
			{
				return Vector2.zero;
			}
		}
	}
}