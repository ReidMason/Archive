using UnityEngine;
using System.Collections;

using NoxCore.Fittings.Modules;
using NoxCore.Placeables;
using NoxCore.Utilities;
using NoxCore.Helm;

namespace Example.TargetRange
{
	public class OrbitBehaviour : SteeringBehaviour
	{
        [SerializeField] protected float _orbitRange;
		public float OrbitRange { get { return _orbitRange; } set { _orbitRange = value; } }

        [SerializeField] protected Transform _orbitObject;
		public Transform OrbitObject { get { return _orbitObject; } set { _orbitObject = value; } }

        [SerializeField] protected Module _orbitModule;
		public Module OrbitModule { get { return _orbitModule; } set { _orbitModule = value; } }
		
		protected Vector2? _orbitVector;
		public Vector2? OrbitVector { get { return _orbitVector; } set { _orbitVector = value; } }

        public bool clockwise;
		protected Vector2 tangentPoint;

		void Reset()
		{
			Label = "ORBIT";
			SequenceID = 3;
			Weight = 1000;

			OrbitRange = 250;
		}

		public override Vector2 execute()
		{
			if (OrbitObject != null || OrbitVector != null)
			{
				desiredVelocity = Vector2.zero;
				
				if (OrbitObject != null)
				{
					steeringVector = new Vector2(OrbitObject.position.x - Helm.Position.x, OrbitObject.position.y - Helm.Position.y);
				}
				else if (OrbitModule != null)
				{
					steeringVector = new Vector2(OrbitModule.transform.position.x - Helm.Position.x, OrbitModule.transform.position.y - Helm.Position.y);
				}
				else if (OrbitVector != null)
				{
					steeringVector = new Vector2(OrbitVector.Value.x - Helm.Position.x, OrbitVector.Value.y - Helm.Position.y);
				}
				
				float rangeToPosition = steeringVector.magnitude;
				
				if (rangeToPosition > OrbitRange)
				{
					float alpha = Mathf.Asin(OrbitRange / rangeToPosition);
					
					float beta = Mathf.Atan2(steeringVector.y, steeringVector.x);

                    float theta;

                    if (clockwise == true)
                    {
                        theta = beta + alpha;
                    }
                    else
                    {
                        theta = beta - alpha;
                    }
					
					float tangentLength = Mathf.Sqrt(Mathf.Pow(rangeToPosition, 2) - Mathf.Pow(OrbitRange, 2));
					
					tangentPoint = new Vector3(Helm.Position.x + tangentLength * Mathf.Cos(theta), Helm.Position.y + tangentLength * Mathf.Sin(theta));
					
					// have we approximately reached tangent point?
					if (Vector2.Distance(Helm.Position, tangentPoint) < (Helm.ShipStructure.MaxSpeed * Helm.throttle))
					{
						tangentPoint = new Vector2(Helm.Position.x + steeringVector.y, Helm.Position.y - steeringVector.x);
					}
				}
				else
				{
                    if (clockwise == false)
                    {
                        tangentPoint = new Vector2(Helm.Position.x + steeringVector.y, Helm.Position.y - steeringVector.x);
                    }
                    else
                    {
                        tangentPoint = new Vector2(Helm.Position.x - steeringVector.y, Helm.Position.y + steeringVector.x);
                    }
				}
				
				steeringVector = tangentPoint - Helm.Position;

                // draw a line to the destination
                if (Helm.Controller.Cam.followTarget != null && Helm.Controller.Cam.followTarget.gameObject == Helm.Controller.structure.gameObject)
                {
                    Debug.DrawLine(Helm.Position, Helm.Position + (steeringVector), Color.blue, Time.deltaTime, true);
                }

                desiredVelocity = steeringVector.normalized;
				desiredVelocity *= (Helm.ShipStructure.MaxSpeed * Helm.throttle);
							
				return desiredVelocity - Helm.ShipRigidbody.velocity;
			}
			else
			{
				return Vector2.zero;
			}
		}
	}
}