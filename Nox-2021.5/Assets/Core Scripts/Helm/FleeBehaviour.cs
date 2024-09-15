using UnityEngine;
using System.Collections;

namespace NoxCore.Helm
{
    public class FleeBehaviour : SteeringBehaviour
    {
        protected float length;
        protected float rangeToDestination;

        void Reset()
        {
            Label = "FLEE";
            SequenceID = 5;
            Weight = 1000;
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

                steeringVector = -(Helm.destination.GetValueOrDefault() - Helm.Position);

                rangeToDestination = steeringVector.magnitude;
                Helm.RangeToDestination = rangeToDestination;

                if (rangeToDestination < length)
                {
                    Helm.RangeToDestination = 0;
                    Helm.destination = null;
                    //Debug.Log("Reached destination");
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