using UnityEngine;
using System.Collections;

using NoxCore.Placeables;

namespace NoxCore.Rules
{
	public class ArenaRules : MonoBehaviour 
	{
		public static float radius = Mathf.Infinity;
		protected ArenaBarrier arena;
		
		protected float roundDuration;
		protected float startRadius;
		protected float endRadius;
		protected float resizeDelay;
		protected float resizeDuration;
		protected bool bounce;
		
		protected float rate;
		protected float resizeTime;
		protected float arenaTimer;
		protected bool arenaMoving;
		protected bool radiusShrinking;	
		
		public void Init(float roundDuration, float startRadius, float endRadius, float resizeDelay, float resizeDuration, bool bounce)
		{
			this.roundDuration = roundDuration;
			this.startRadius = startRadius;
			this.endRadius = endRadius;
			radius = startRadius;
			this.resizeDelay = resizeDelay;
			this.resizeTime = resizeDelay;
			this.resizeDuration = resizeDuration;
			this.bounce = bounce;
			
			arena = gameObject.GetComponent<ArenaBarrier>();
			arena.setInitialRadius(radius);
			
			arenaTimer = 0;
			arenaMoving = false;
			radiusShrinking = false;
			
			rate = 0;
			
			if (resizeDuration > 0)
			{
				rate = (startRadius - endRadius) / resizeDuration;
			}
			
			if (startRadius != endRadius)
			{
				arenaMoving = true;
				
				if (startRadius > endRadius)
				{
					radiusShrinking = true;
				}
				else
				{
					radiusShrinking = false;
				}
			}
			
			// D.log("GameLogic", "Arena radius set to: " + radius);
		}
		
		public ArenaBarrier getArenaBarrier()
		{
			return arena;
		}
		
		public void flip()
		{
			resizeTime = arenaTimer + resizeDelay;
			float temp = startRadius;
			startRadius = endRadius;
			endRadius = temp;
            rate = -rate;
			radiusShrinking = !radiusShrinking;
		}
		
		// Update is called once per frame
		void Update () 
		{
			arenaTimer += Time.deltaTime;
			
			if (arenaMoving == true)
			{
                radius -= (rate * Time.deltaTime);

                if (radiusShrinking == true)
				{
					if (radius < endRadius)
					{
						radius = endRadius;
						
						if (bounce == true)
						{
							flip();
						}
					}
				}
				else
				{
					if (radius > endRadius)
					{
						radius = endRadius;
						
						if (bounce == true)
						{
							flip();
						}
					}
				}
				
				arena.setRadius(radius);
			}
		}	
	}
}