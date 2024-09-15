using UnityEngine;
using System.Collections;

namespace NoxCore.Cameras
{
	public class CelestialCamera : MonoBehaviour 
	{
		private Vector3 newPosition;
        private float moveRatio;

        // Use this for initialization
        void Start () 
		{		
			newPosition = transform.position;
            moveRatio = 0.05f;
		}
		
		// Update is called once per frame
		void Update () 
		{
			newPosition[0] = Camera.main.transform.position.x * moveRatio;
			newPosition[1] = Camera.main.transform.position.y * moveRatio;
			
			transform.position = newPosition;	
		}
	}
}