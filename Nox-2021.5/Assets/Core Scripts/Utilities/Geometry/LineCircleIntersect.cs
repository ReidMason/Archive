using UnityEngine;

namespace NoxCore.Utilities
{
	public class LineCircleIntersect
	{
		static public Vector2? lineTouchCircle(Vector2 p1, Vector2 p2, Vector2 circleCenter, float radius)
		{
			Vector2 d = p2 - p1;
			Vector2 f = p1 - circleCenter;
			
			float a = Vector2.Dot(d,d);
			float b = 2 * Vector2.Dot(f,d);
			float c = Vector2.Dot(f,f) - radius * radius;
			
			float discriminant = b*b-4*a*c;
			if( discriminant < 0 )
			{
				return null;
			}
			else
			{
				// ray didn't totally miss sphere,
				// so there is a solution to
				// the equation.
				
				discriminant = Mathf.Sqrt( discriminant );
				
				// either solution may be on or off the ray so need to test both
				// t1 is always the smaller value, because BOTH discriminant and
				// a are nonnegative.
				float t1 = (-b - discriminant)/(2*a);
				float t2 = (-b + discriminant)/(2*a);
				
				// 3x HIT cases:
				//          -o->             --|-->  |            |  --|->
				// Impale(t1 hit,t2 hit), Poke(t1 hit,t2>1), ExitWound(t1<0, t2 hit), 
				
				// 3x MISS cases:
				//       ->  o                     o ->              | -> |
				// FallShort (t1>1,t2>1), Past (t1<0,t2<0), CompletelyInside(t1<0, t2>1)
				
				if( t1 >= 0 && t1 <= 1 )
				{
					// t1 is an intersection, and if it hits,
					// it's closer than t2 would be
					// Impale, Poke
					return (p1 + t1 * d);
				}
				
				// here t1 didn't intersect so we are either started
				// inside the sphere or completely past it
				if( t2 >= 0 && t2 <= 1 )
				{
					// ExitWound
					return (p1 + t2 * d);
				}
				
				// no intn: FallShort, Past, CompletelyInside
				return null;
			}
		}
	}
}
