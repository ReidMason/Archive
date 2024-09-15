using UnityEngine;
using System.Collections;

using NoxCore.Fittings.Modules;

namespace NoxCore.Effects
{
	[RequireComponent (typeof(LineRenderer))]
	public class BarrierBeamEffect : MonoBehaviour
	{	
		LineRenderer lineRenderer;
		public Vector2 uvAnimationRate;
		protected Vector2 uvOffset;
		
		Transform target;
				
		// Use this for initialization
		void Start ()
		{			
			lineRenderer = GetComponent<LineRenderer>();
			lineRenderer.sortingLayerName = "TransparentFX";
			lineRenderer.positionCount = 2;			
		}
		
		public void setTarget(Transform target)
		{
			this.target = target;
		}
		
		// Update is called once per frame
		void Update () 
		{
			if (target != null)
			{
				RenderLaser();
			}
		}
		
		void RenderLaser()
		{
			lineRenderer.SetPosition(0, gameObject.transform.position);
			lineRenderer.SetPosition(1, target.position);
						
			uvOffset += ( uvAnimationRate * Time.deltaTime );
			
			lineRenderer.material.SetTextureOffset ("_MainTex", uvOffset);
		}
	}
}