using UnityEngine;
using System.Collections;

namespace NoxCore.GUIs
{
	[RequireComponent (typeof (TextMesh))]
	public class TimedFadeTextMesh : MonoBehaviour
	{
		//	TimedFadeTextMesh.cs
		//	From the Unity Wiki (modified for TextMesh)
		//	Use with MessageListTextMesh.cs
		//	Use on a TextMesh PreFab
		//  This PreFab will be instantiated by MessageListTextMesh.cs
		
		float liveTime = 5.0f;					//	The number of seconds the GUIText will last before starting to fade
		float fadeTime = 2.0f;					//	The number of seconds to fade until totally transparent
		
		private float time = 0.0f;				//	Static var to track how much time has passed
		private bool isFading = false;			//	Static var to track if we're in the fading stage
		private float startAlpha = 1.0f;		//	Static var to keep track of the initial amount of alpha
/*		private TextMesh textMesh;
		
		void Awake()
		{
			textMesh = this.gameObject.GetComponent<TextMesh>();
		}
*/		
		void Start () 
		{
			//	This script uses the GUIText's material to set the alpha fade.
			//	If the font doesn't have a material, then this won't work, so disable the script.
			
			if (!GetComponent<Renderer>().material) 
			{
				D.warn("GUI: {0}", "TextMesh material missing");
				enabled = false;
			}
			
			//	Get the starting alpha value.
			//	If the developer has the text start transparent, then we need to fade from that point.
			startAlpha = GetComponent<Renderer>().material.color.a;
			//	renderer.material
		}
		
		void OnEnable()
		{
			time = 0;
			isFading = false;
			GetComponent<Renderer>().material.color = new Color(GetComponent<Renderer>().material.color.r, GetComponent<Renderer>().material.color.g, GetComponent<Renderer>().material.color.b, startAlpha);
		}	
		
		void Update () 
		{
			//	Update our time var to keep track of how much time has passed.
			time += Time.deltaTime;
			
			if (isFading) 
			{
				//	We're in the fading stage. If we've reached the end of this stage, then destroy the gameObject.
				if (time >= fadeTime) 
				{
					gameObject.Recycle();
				}
				else
				{
					//	We're still fading, so update the material's alpha color to make it fade a little more.
					GetComponent<Renderer>().material.color = new Color(GetComponent<Renderer>().material.color.r, GetComponent<Renderer>().material.color.g, GetComponent<Renderer>().material.color.b, CalculateAlpha());
				}
			}
			else if (time >= liveTime) 
			{
				//	If we're not fading yet, but should be, then update our values to proceed to the fading stage.
				isFading = true;
				time = 0.0f;
			}
			
			//	If we're not fading yet, and don't need to be yet, then nothign will happen at this point. The
			//	text will just exist, and the timer will keep incrementing until there's a state change.
		}
		
		//	CalculateAlpha() simple takes the static global vars we're using to keep track of everything
		//	to figure out our current alpha value from 0 to 1.
		private float CalculateAlpha() 
		{
			//	Find out the percent of time from 0 to 1 that has gone between when the text starts and stops fading
			float timePercent = Mathf.Clamp01((fadeTime - time) / fadeTime);
			
			//	Generate a nice, smooth value from 1 to 0 to represent how faded the text is
			float smoothAlpha = Mathf.SmoothStep(0.0f, startAlpha, timePercent);
			
			//	We actually could just return the timePercent value for a linear fade, but we want it to be smooth,
			//	so return the smoothAlpha.
			return smoothAlpha;
		}
	}
}
