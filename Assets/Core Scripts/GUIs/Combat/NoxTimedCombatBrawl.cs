using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

using NoxCore.Cameras;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.GameModes;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Stats;
using NoxCore.Utilities;

namespace NoxCore.GUIs
{
	public class NoxTimedCombatBrawlGUI : NoxGUI
	{
		protected CadetBrawlMode gameMode;

		public GUIStyle readoutStyle;	
		public int displayRows;
		public float displayTime;
		protected float displayTimer;
		protected int rowsOffset;
		protected Rect timerInfoRect;  
		
		protected List<string> shipNames = new List<string>();
		protected List<string> shipCaptains = new List<string>();
		
		protected GameObject [] shipGOs;
		public float labelOffsetZ;
		
		// Use this for initialization
		public override void init()
		{
			base.init();
			
			gameMode = GameManager.Instance.Gamemode as CadetBrawlMode;
			
			timerInfoRect = new Rect(Screen.width - 300, 5, 300, 20);

			shipGOs = GameObject.FindGameObjectsWithTag("Ship");
			
			List<Color32> labelColours = GenerateColors_GoldenRatioRainbow(shipGOs.Length, 1.0f, 0.5f);
			
			int col = 0;
			
			foreach(GameObject shipGO in shipGOs)
			{
                NameLabel labelGO = shipGO.GetComponent<NameLabel>();
				
				labelGO.transform.parent = shipGO.transform;
				labelGO.transform.position = new Vector3(shipGO.transform.position.x, shipGO.transform.position.y + 20, shipGO.transform.position.z + labelOffsetZ);
				
				Structure structure = shipGO.GetComponent<Structure>();
				Ship ship = structure as Ship;
				
				if (ship != null)
				{
                    labelGO.SetBackgroundColour(labelColours[col]);				
				}					

                shipNames.Add(shipGO.name);
				shipCaptains.Add(structure.Command.rankData.abbreviation + " " + structure.Command.label);
				
				col++;
			}			
									
			enabled = true;
		}
		
		public static List<Color32> GenerateColors_GoldenRatioRainbow(int colorCount, float saturation, float luminance)
		{
			List<Color32> colors = new List<Color32>();
			
			float goldenRatioConjugate = 0.618033988749895f;
			float currentHue = UnityEngine.Random.value;
			
			for (int i = 0; i < colorCount; i++)
			{
				HSBColor hsbColor = new HSBColor(currentHue, saturation, luminance);
				
				colors.Add(hsbColor.ToColor32());
				
				currentHue += goldenRatioConjugate;
				currentHue %= 1.0f;
			}
			
			return colors;
		}			
		
		protected override void Update ()
		{
			base.Update();		
			
            displayTimer += Time.deltaTime;
			
			if (displayTimer > displayTime)
			{
				displayTimer = 0;
				
				rowsOffset += displayRows;
				
				if (rowsOffset >= shipGOs.Length)
				{
					rowsOffset = 0;
				}
			}				
			
		}
	}
}
