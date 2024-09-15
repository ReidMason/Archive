using UnityEngine;
using System.Collections;

using NoxCore.Cameras;
using NoxCore.Utilities;
using UnityEngine.UI;

namespace NoxCore.GUIs
{
	public class VelocityReadout : MonoBehaviour 
	{
		public TopDown_Camera cam;
		protected Text label;
		protected string textPrefix;
		
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
				Rigidbody movingBody = cam.followTarget.GetComponent<Rigidbody>();
				
				if (movingBody != null)
				{
					label.text = textPrefix + movingBody.velocity.magnitude;
				}
			}	
		}
	}
}