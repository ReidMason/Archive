using UnityEngine;
using System.Collections;

namespace NoxCore.Debugs
{
	public class DebugLine : MonoBehaviour 
	{
	    public Color c1 = Color.yellow;
	    public Color c2 = Color.red;
	    public int lengthOfLineRenderer = 2;
		public Vector3? point;

		void Start() 
		{
	        LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
	        lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
            lineRenderer.startColor = c1;
            lineRenderer.endColor = c2;
	        lineRenderer.startWidth = lineRenderer.endWidth = 0.2F;
	        lineRenderer.positionCount = lengthOfLineRenderer;
	    }
	    
		void Update() 
		{
			if (point.HasValue == true)
			{
		        LineRenderer lineRenderer = GetComponent<LineRenderer>();        
		        lineRenderer.SetPosition(0, gameObject.transform.position);
		        lineRenderer.SetPosition(1, point.Value);
			}
	    }
	}
}