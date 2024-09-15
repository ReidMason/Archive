using UnityEngine;
using System.Collections;
using NoxCore.Helm;

namespace Example.Snowball
{
	public class ExampleSnowballSeekBehaviour : SteeringBehaviour
	{
        [SerializeField] protected float _lookAheadDistance;
        public float LookAheadDistance {  get { return _lookAheadDistance; } set { _lookAheadDistance = value; } }

        [SerializeField] protected bool _continuousSeek;
        public bool ContinuousSeek { get { return _continuousSeek; } set { _continuousSeek = value; } }

        [SerializeField] protected bool _dynamicLookAhead;
        public bool DynamicLookAhead {  get { return _dynamicLookAhead; } set { _dynamicLookAhead = value; } }

		[SerializeField] [Range(0, 1)] protected float _dynamicLookAheadFactor = 0f;
        public float DynamicLookAheadFactor { get { return _dynamicLookAheadFactor; } set { _dynamicLookAheadFactor = value; } }

        protected float length;
        protected float rangeToDestination;        

        void Reset()
		{
            Label = "SEEK";
            SequenceID = 1;
            Weight = 1000;
        }

        void Awake()
        {
            length = transform.parent.GetComponent<PolygonCollider2D>().bounds.extents.y;
            LookAheadDistance = length;
        }

        public override Vector2 execute()
		{
            if (Helm.destination != null)
            {
                desiredVelocity = Vector2.zero;

                steeringVector = Helm.destination.GetValueOrDefault() - Helm.Position;

                rangeToDestination = steeringVector.magnitude;
                Helm.RangeToDestination = rangeToDestination;

                // calculate a look ahead distance based on the ship's length and its current speed
                if (DynamicLookAhead == true)
                {
                    LookAheadDistance = length + (length * Helm.ShipStructure.Speed * DynamicLookAheadFactor);
                }

                if (rangeToDestination < LookAheadDistance && ContinuousSeek == false)
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