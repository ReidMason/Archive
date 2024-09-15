using UnityEngine;
using System.Collections;

using NoxCore.Utilities;
using NoxCore.Helm;

namespace Example.TargetRange
{
	public class ArriveBehaviour : SteeringBehaviour
	{
        [SerializeField]
        protected float _SlowingRadius;
        public float SlowingRadius { get { return _SlowingRadius; } set { _SlowingRadius = value; } }

        protected float rangeToDestination;

		void Reset()
		{
			Label = "ARRIVE";
			SequenceID = 2;
			Weight = 1000;

			SlowingRadius = 250;
        }

        public override Vector2 execute()
		{
			if (Helm.destination != null)
			{
				desiredVelocity = Vector2.zero;
				
				steeringVector = Helm.destination.GetValueOrDefault() - Helm.Position;
				
				desiredVelocity = steeringVector.normalized;
				
                rangeToDestination = steeringVector.magnitude;
                Helm.RangeToDestination = rangeToDestination;
                
                if (rangeToDestination <= SlowingRadius && SlowingRadius > 0)
				{
					desiredVelocity *= (Helm.ShipStructure.MaxSpeed * Helm.throttle) * (rangeToDestination / SlowingRadius);
				}
				else
				{					
					desiredVelocity *= (Helm.ShipStructure.MaxSpeed * Helm.throttle);
				}
				
				return desiredVelocity - Helm.ShipRigidbody.velocity;
			}
			
			return Vector2.zero;
		}
	}
}
