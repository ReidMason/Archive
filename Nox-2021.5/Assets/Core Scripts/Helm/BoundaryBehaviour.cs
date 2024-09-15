using UnityEngine;
using System.Collections;

using NoxCore.Placeables.Ships;

namespace NoxCore.Helm
{
	public class BoundaryBehaviour : SteeringBehaviour
	{
		[SerializeField] protected float _lookAheadDistance = 250;
		public float LookAheadDistance {  get { return _lookAheadDistance; } set { _lookAheadDistance = value; } }

		void Reset()
		{
			LookAheadDistance = 250;
		}

        protected void drawForce(Vector2 start, Vector2 intersection, Vector2 force)
		{
			if (force.magnitude > 0)
			{
				if (Helm.Controller.Cam.followTarget != null && Helm.Controller.Cam.followTarget.gameObject == Helm.ShipStructure.gameObject)
				{
					Debug.DrawLine(new Vector3(start.x, start.y, 0), intersection, Color.white, Time.deltaTime, true);
					Debug.DrawLine(start, start + force, Color.yellow, Time.deltaTime, true);
				}
			}
		}
		
		protected static Vector2? lineXCircle(Vector2 start, Vector2 end, Vector2 centreOffset, float radius)
		{
			Vector2? intersectionPoint;
		
			Vector2 ray = new Vector2(end.x - start.x, end.y - start.y);
			Vector2 circleRay = new Vector2(start.x - centreOffset.x, start.y - centreOffset.y);
			
			float a = Vector2.Dot(ray, ray);
			float b = 2 * Vector2.Dot(ray, circleRay);
			float c = Vector2.Dot(circleRay, circleRay) - (radius * radius);
			
			float discriminant = (b*b) - 4*a*c;
			
			if (discriminant < 0)
			{
				return Vector2.zero;
			}
			else
			{
				discriminant = Mathf.Sqrt(discriminant);
				
				float ax2 = 2*a;
				
				float t1 = (-b + discriminant) / ax2;
				
				if (t1 >= 0 && t1 <= 1)
				{
					intersectionPoint = new Vector2(start.x + t1 * ray.x, start.y + t1 * ray.y);
				}
				else
				{
					intersectionPoint = null;
				}
				
				return intersectionPoint;
			}
		}
		
		public override Vector2 execute()
		{
			Ship ship = Helm.ShipStructure as Ship;
			
			if (ship != null)
			{		
				Vector2 force;
				Vector2? boundaryIntersection;
				float overshootBoundary;
						
				if (Helm.Position.magnitude > ship.BoundaryRadius)	// are we outside the arena boundary?
				{
					float outsidefraction = ship.BoundaryRadius / Helm.Position.magnitude;
					boundaryIntersection = new Vector3(Helm.Position.x * outsidefraction, Helm.Position.y * outsidefraction);
					overshootBoundary = Helm.Position.magnitude - ship.BoundaryRadius;
										
					force = -(boundaryIntersection.GetValueOrDefault().normalized) * overshootBoundary;
					
					drawForce(Helm.Position, boundaryIntersection.GetValueOrDefault(), force);
					
					return force;										
				}
				else if (Helm.Position.magnitude + LookAheadDistance >= ship.BoundaryRadius)	// are we close enough to the arena boundary?
				{
					Vector2 aheadPoint = new Vector2(Helm.Position.x + LookAheadDistance * Mathf.Sin(ship.Bearing * Mathf.Deg2Rad), Helm.Position.y + LookAheadDistance * Mathf.Cos(ship.Bearing * Mathf.Deg2Rad));
					
					boundaryIntersection = lineXCircle(Helm.Position, aheadPoint, Vector2.zero, ship.BoundaryRadius);
					
					if (boundaryIntersection != null)
					{
						overshootBoundary = Vector2.Distance(aheadPoint, boundaryIntersection.GetValueOrDefault());
						
						force = -(boundaryIntersection.GetValueOrDefault().normalized) * overshootBoundary;
						
						drawForce(Helm.Position, boundaryIntersection.GetValueOrDefault(), force);
						
						return force;
					}
					else
					{
						return Vector3.zero;
					}
				}
				else
				{
					return Vector3.zero;
				}			
			}
			else
			{
				return Vector3.zero;
			}
		}
	}
}