using UnityEngine;
using System.Collections;
using UnityEngine.UI;

using NoxCore.Cameras;
using NoxCore.Placeables;

namespace NoxCore.GUIs
{
	public class DistanceReadout : MonoBehaviour 
	{
		public TopDown_Camera cam;
		protected Text label;
		protected string textPrefix;
		public Transform pos;
		
		void Awake()
		{
			label = GetComponent<Text>();	
		}
		
		void Start () 
		{
			if (label != null)
			{
				textPrefix = label.text;
			}	
		}
		
		// Update is called once per frame
		void Update () 
		{
			if (label != null && cam != null)
			{
				Transform ship = cam.followTarget;
				
				if (ship != null)
				{
					label.text = textPrefix + (int)((pos.position - ship.transform.position).magnitude);
				}
			}	
		}
	}
}