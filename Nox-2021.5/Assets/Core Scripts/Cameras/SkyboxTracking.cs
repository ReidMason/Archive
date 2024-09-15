using UnityEngine;
using System.Collections;

namespace NoxCore.Cameras
{
	public class SkyboxTracking : MonoBehaviour 
	{
		public GameObject background;
        private float tiling;
        private float scale;
        private int repeatFactor;
		private float moveRatio;	
		private Vector3 newPosition;
		
		// Use this for initialization
		void Start () 
		{
            tiling = background.GetComponent<MeshRenderer>().material.mainTextureScale.x;
            scale = background.transform.localScale.x;
            repeatFactor = (int)(scale / tiling);
			moveRatio = background.transform.localScale.x / 75000.0f; 
			newPosition = transform.position;
		}
		
		// Update is called once per frame
		void Update () 
		{
			newPosition[0] = (Camera.main.transform.position.x * moveRatio) % repeatFactor;
			newPosition[1] = (Camera.main.transform.position.y * moveRatio) % repeatFactor;
			
			transform.position = newPosition;
		}
	}
}