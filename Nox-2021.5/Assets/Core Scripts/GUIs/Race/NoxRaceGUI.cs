using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

using NoxCore.GameModes;
using NoxCore.Managers;
using NoxCore.Stats;
using NoxCore.Utilities;
using NoxCore.Placeables.Ships;

namespace NoxCore.GUIs
{
	public struct FinishStats
	{
		public Transform racerTransform;
		public string raceTime;
		public float [] lapTimes;
        public string commanderName;
        public Sprite portrait;

        public FinishStats(Transform racerTransform, string raceTime, float [] lapTimes)
		{
			this.racerTransform = racerTransform;
			this.raceTime = raceTime;
			this.lapTimes = lapTimes;

            Ship ship = racerTransform.GetComponent<Ship>();
            commanderName = ship.Command.label;
            Texture2D tex = ship.Command.portrait;
            portrait = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
	}

    public struct RaceStanding
    {
        public GameObject standingPanel;
        public Text positionLabel;
        public Text racerLabel;
        public Text timeLabel;
        public Image commanderImage;

        public RaceStanding(GameObject standingPanel)
        {
            this.standingPanel = standingPanel;

            Text label = standingPanel.transform.Find("Position Label").GetComponent<Text>();
            this.positionLabel = label;

            label = standingPanel.transform.Find("Racer Label").GetComponent<Text>();
            this.racerLabel = label;

            label = standingPanel.transform.Find("Time Label").GetComponent<Text>();
            this.timeLabel = label;

            commanderImage = standingPanel.transform.Find("Commander Image").GetComponent<Image>();
        }

        public void setStanding(int position, string racer, string time, Sprite sprite)
        {
            if (standingPanel.activeInHierarchy == false) setActive(true);

            positionLabel.text = position.ToString();
            racerLabel.text = racer;
            timeLabel.text = time;
            commanderImage.sprite = sprite;
            commanderImage.color = Color.white;
        }

        public void setActive(bool active)
        {
            standingPanel.SetActive(active);
        }
    }

	public class NoxRaceGUI : NoxGUI
	{
        protected RaceMode gameMode;

        public GameObject clockGO;
        public GameObject standingPanelHeader, standingLabelsParent;
        protected Text timeLabelHeader;
        protected const string distanceString = "Distance";
        protected const string finalTimeString = "Final Time";
        protected GameObject standingPanelPrefab, standingPanel, positionLabel, racerLabel, timeLabel;

        public GUIStyle readoutStyle;
        public int displayRows;
        protected int numStandingsDisplayed;
        protected float maxTimer;
        public float displayTime;
		protected float displayTimer;
		protected int rowsOffset;
		protected int maxShipNameLength = 4;

		public Transform finishPoint;		
						
        List<RaceStats> startRacers = new List<RaceStats>();
        List<RaceStats> curRacers = new List<RaceStats>();
        List<FinishStats> endRacers = new List<FinishStats>();
        RaceStanding[] standings;

        public bool showPositionIDs;
		public bool showRacerIDs;
		public GameObject raceLabel, raceLabelID;
		public int numVisibleRaceLabels;
		public Vector3 labelOffset, labelIDOffset;
		protected List<GameObject> raceLabels = new List<GameObject>();
		protected List<GameObject> raceLabelIDs = new List<GameObject>();
		
		protected GameObject [] navPoints = null;
        protected float [] navPointDistances;
        protected int maxNavPoints;
        public float totalRaceDistance;
        public float lapDistance;
        protected int maxLaps;

        Text clock;

        // Use this for initialization
        public override void init()
		{
			base.init();

            gameMode = GameManager.Instance.Gamemode as RaceMode;

            if (gameMode == null)
            {
                D.error("GameMode", "Could not cast to the required RaceMode game mode");
                return;
            }

            GameObject labelsGO = GameObject.Find("Race Labels");

			GameObject [] racerGOs = GameObject.FindGameObjectsWithTag("Ship");
			
			List<Color32> labelColours = GenerateColors_GoldenRatioRainbow(racerGOs.Length, 1.0f, 0.5f);
			
			/*
			for(int i = 0; i < labelColours.Count; i++)
			{
				// D.log("GUI", "Colour " + i + ": " + labelColours[i].ToString());
			}
			*/
			
			navPoints = sortGOArray(GameObject.FindGameObjectsWithTag("NavPoint"));
            
            maxNavPoints = navPoints.Length;
            
            navPointDistances = new float[maxNavPoints];
			
			GameObject prevNavpoint = null;
            int i = 0;
			
			foreach(GameObject navpoint in navPoints)
			{
				// D.log ("GUI", "Navpoint: " + navpoint.name);
				
				if (prevNavpoint != null)
				{
					float distanceBetween = Vector3.Distance (prevNavpoint.transform.position, navpoint.transform.position);
					// D.log("GUI", "Distance between " + prevNavpoint.name + " and " + navpoint.name + ": " + distanceBetween);
					totalRaceDistance += distanceBetween;
                    navPointDistances[i] = distanceBetween;
                    i++;
				}
				
				prevNavpoint = navpoint;
			}
            
            navPointDistances[navPoints.Length-1] = Vector3.Distance (navPoints[navPoints.Length-1].transform.position, navPoints[0].transform.position);
			
			totalRaceDistance += navPointDistances[navPoints.Length-1];
			
            lapDistance = totalRaceDistance;
            
			// D.log("GUI", "Total lap distance: " + totalRaceDistance);
			
			maxLaps = ((RaceMode)GameManager.Instance.Gamemode).maxLaps;
			
			totalRaceDistance *= maxLaps;
			
			// D.log("GUI", "Total race distance: " + totalRaceDistance);

			int col = 0;
			
			foreach(GameObject racerGO in racerGOs)
			{
				startRacers.Add(new RaceStats(racerGO.transform, maxLaps, totalRaceDistance));
                curRacers.Add(new RaceStats(racerGO.transform, maxLaps, totalRaceDistance));
				
				GameObject labelGO = Instantiate(raceLabel, racerGO.transform.position, raceLabel.transform.rotation) as GameObject;
				GameObject labelIDGO = Instantiate(raceLabelID, racerGO.transform.position, raceLabelID.transform.rotation) as GameObject;

				labelGO.transform.parent = labelsGO.transform;
                labelGO.name = "Race Position " + (col+1);
				TextMesh text = labelGO.GetComponent<TextMesh>();
				text.GetComponent<Renderer>().material.SetColor("_BackgroundColor", Color.black);
				raceLabels.Add (labelGO);

				labelIDGO.transform.parent = labelsGO.transform;
                labelIDGO.name = "Race ID " + (col+1);
                text = labelIDGO.GetComponent<TextMesh>();
				text.GetComponent<Renderer>().material.SetColor("_BackgroundColor", labelColours[col]);
				text.text = (col+1).ToString();
				raceLabelIDs.Add (labelIDGO);

				if (racerGO.name.Length > maxShipNameLength)
				{
					maxShipNameLength = racerGO.name.Length+3;
				}
				
				col++;
			}

            // D.log("GUI", "Number of racers: " + startRacers.Count);

            maxTimer = gameMode.maxTime;

            if (clockGO != null)
            {
                Transform clockTimerTrans = clockGO.transform.Find("Clock");

                if (clockTimerTrans != null)
                {
                    clock = clockTimerTrans.GetComponent<Text>();
                }
                else
                {
                    D.error("GUI", "Could not find the match timer text component (should be UI Manager/Canvas/Clock Display/Clock in Hierarchy)");
                }
            }

            if (standingPanelHeader != null)
            {
                timeLabelHeader = standingPanelHeader.transform.Find("Time Label").GetComponent<Text>();
            }

            standingPanelPrefab = Resources.Load<GameObject>("UI/Standing Panel");

            numStandingsDisplayed = racerGOs.Length < displayRows ? racerGOs.Length : displayRows;

            standings = new RaceStanding[numStandingsDisplayed];

            for (i = 0; i < numStandingsDisplayed; i++)
            {
                GameObject standingPanel = Instantiate<GameObject>(standingPanelPrefab);
                standingPanel.transform.SetParent(standingLabelsParent.transform);
                standingPanel.transform.position = new Vector3(standingPanelHeader.transform.position.x, standingPanelHeader.transform.position.y - ((i + 1) * 30), 0);

                standings[i] = new RaceStanding(standingPanel);
            }

            enabled = true;
		}
		
        public void togglePositionIDs()
        {
            showPositionIDs = !showPositionIDs;
        }

        public void toggleRacerIDs()
        {
            showRacerIDs = !showRacerIDs;
        }

		public Timer getTimer()
		{
			return timer;
		}

		public List<RaceStats> getStartRacers()
		{
			return startRacers;
		}

		public List<RaceStats> getCurrentRacers()
		{
			return curRacers;
		}

		public List<FinishStats> getEndRacers()
		{
			return endRacers;
		}
		
		public static List<Color32> GenerateColors_GoldenRatioRainbow(int colorCount, float saturation, float luminance)
		{
			List<Color32> colors = new List<Color32>();
			
			float goldenRatioConjugate = 0.618033988749895f;
			float currentHue = Random.value;
			
			for (int i = 0; i < colorCount; i++)
			{
				HSBColor hsbColor = new HSBColor(currentHue, saturation, luminance);
				
				colors.Add(hsbColor.ToColor32());
				
				currentHue += goldenRatioConjugate;
				currentHue %= 1.0f;
			}
			
			return colors;
		}		
		
        protected static GameObject[] sortGOArray(GameObject [] goArray)
        {           
            for(int i = 0; i < goArray.Length; i++)
            {
                for(int j = i+1; j < goArray.Length; j++) 
                {
                    if(string.Compare(goArray[i].name, goArray[j].name) > 0) 
                    {
                        GameObject temp = goArray[i];  
                        goArray[i] = goArray[j];
                        goArray[j] = temp;
                    }
                }
            }
            
            return goArray;
        }
        
		public void racerAtNavpoint(Transform racerTransform, int navpointID)
		{
            if (GameManager.Instance.Gamemode.matchState != GameMode.MatchState.INPROGRESS) return;

            RaceStats racer = null;
            
            // TODO - should pass through some kind of racer ID to prevent need to search like this        
			foreach (RaceStats rs in curRacers)
			{
				if (rs.racerTransform == racerTransform)
				{
                    racer = rs;
                    break;
				}
			}
            
            if (racer != null)
            {
                racer.currentNavPoint = navpointID;
                
                float distanceFromNextPoint = 0;
                                
                if (navpointID == 1)
                {
                    racer.lap++;
                    if (racer.lap <= maxLaps)
                    {
                        for (int curNavPoint = racer.currentNavPoint; curNavPoint < maxNavPoints; curNavPoint++)
                        {
                            distanceFromNextPoint += navPointDistances[curNavPoint];
                        }                         
                    
                        racer.distanceRemaining = (maxLaps - racer.lap + 1) * lapDistance;                        
                        racer.distanceFromNextNavPoint = racer.distanceRemaining - navPointDistances[0];
                    }
                    else
                    {
                        racer.distanceRemaining = 0;
                        racer.distanceFromNextNavPoint = 0;
                    }                           
                }
                else
                {   
                    if (navpointID < maxNavPoints)
                    {
                        for (int curNavPoint = racer.currentNavPoint; curNavPoint < maxNavPoints; curNavPoint++)
                        {
                            distanceFromNextPoint += navPointDistances[curNavPoint];
                        }                
                        racer.distanceFromNextNavPoint = distanceFromNextPoint + (maxLaps - racer.lap) * lapDistance;
                        racer.distanceRemaining = navPointDistances[racer.currentNavPoint-1] + racer.distanceFromNextNavPoint;                        
                    }
                    else
                    {
                        distanceFromNextPoint = (maxLaps - racer.lap) * lapDistance;
                        
                        racer.distanceFromNextNavPoint = distanceFromNextPoint;
                        racer.distanceRemaining = navPointDistances[racer.currentNavPoint-1] + racer.distanceFromNextNavPoint;
                    }
                }

                // Debug.Log("Distance from next navpoint to end of lap: " + distanceFromNextPoint);
				// Debug.Log("Distance remaining for " + racer.racerTransform.name + ": " + racer.distanceRemaining);
            }
		}

		public void racerFinished(Transform racerTransform, string raceTime)
		{
            if (GameManager.Instance.Gamemode.matchState != GameMode.MatchState.INPROGRESS) return;

            RaceStats finishedRacer = null;
		
			foreach (RaceStats rs in curRacers)
			{
				if (rs.racerTransform == racerTransform)
				{
					finishedRacer = rs;
					break;
				}
			}
		
            if (finishedRacer != null)
            {
    			FinishStats stats = new FinishStats(finishedRacer.racerTransform, raceTime, finishedRacer.lapTimes);

    			curRacers.Remove(finishedRacer);
    			endRacers.Add(stats);
            }
    			
			if (curRacers.Count == 0)
			{
				timer.stopTimer();
			}
		}
		
        public void updateRacers()
        {
            if (GameManager.Instance.Gamemode.matchState != GameMode.MatchState.INPROGRESS) return;

            foreach (RaceStats rs in curRacers)
            {
                Vector3 nextNavPoint;
                
                if (rs.currentNavPoint == maxNavPoints)
                {
                    nextNavPoint = navPoints[0].transform.position;
                }
                else
                {
                    nextNavPoint = navPoints[rs.currentNavPoint].transform.position;
                }
                
				Vector3 planarDistance = new Vector3(rs.racerTransform.position.x, rs.racerTransform.position.y, 0);
				rs.distanceToNextNavPoint = Vector3.Distance(planarDistance, nextNavPoint);
                rs.distanceRemaining = rs.distanceToNextNavPoint + rs.distanceFromNextNavPoint;
            }
        }
        
		public void sortRacers()
		{
			curRacers.Sort(delegate(RaceStats rs1, RaceStats rs2)
			{
				return rs1.distanceRemaining.CompareTo(rs2.distanceRemaining);
			});	
		}

		protected void updateRaceLabel(int position, Transform racerTransform)
		{
			GameObject label;
			TextMesh labelText;
					
			label = raceLabels[position-1];
			labelText = label.GetComponent<TextMesh>();
			
			if (position <= numVisibleRaceLabels && showPositionIDs == true)
			{
				label.transform.position = racerTransform.position + labelOffset;
				labelText.text = position.ToString();
				labelText.GetComponent<Renderer>().enabled = true;
			}
			else
			{
				labelText.GetComponent<Renderer>().enabled = false;
			}
		}
		
		protected void updateRaceLabelIDs()
		{
			GameObject label;
			TextMesh labelText;
			
			for(int i = 0; i < raceLabelIDs.Count; i++)
			{
				label = raceLabelIDs[i];
				labelText = label.GetComponent<TextMesh>();
				
				label.transform.position = startRacers[i].racerTransform.position + labelIDOffset;
				
				if (showRacerIDs == true)
				{
					labelText.GetComponent<Renderer>().enabled = true;
				}
				else
				{
					labelText.GetComponent<Renderer>().enabled = false;
				}
			}
		}

        protected override void Update()
		{
			base.Update();

            if (gameMode != null && (gameMode.matchState == GameMode.MatchState.INPROGRESS || gameMode.matchState == GameMode.MatchState.WAITINGPOSTMATCH))
            {
                displayTimer += Time.deltaTime;

                if (displayTimer > displayTime)
                {
                    displayTimer = 0;

                    rowsOffset += numStandingsDisplayed;

                    if (rowsOffset >= endRacers.Count + curRacers.Count)
                    {
                        rowsOffset = 0;
                    }
                }
            }
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            if (clock != null && timer != null)
            {
                clock.text = timer.getTimeStr();
            }

            if (gameMode != null && (gameMode.matchState == GameMode.MatchState.INPROGRESS || gameMode.matchState == GameMode.MatchState.WAITINGPOSTMATCH))
            {
                bool displayedRows = false;

                updateRacers();

                updateRaceLabelIDs();

                sortRacers();

                int standingIndex = 0;
                int position = 1;

                if (endRacers.Count == 0)
                {
                    timeLabelHeader.text = distanceString;
                }
                else
                {
                    timeLabelHeader.text = finalTimeString;
                }

                foreach (FinishStats stat in endRacers)
                {
                    if (position <= rowsOffset)
                    {
                        updateRaceLabel(position, stat.racerTransform);
                        position++;
                        continue;
                    }

                    if (displayedRows == false)
                    {
                        standings[standingIndex].setStanding(position, stat.commanderName, stat.raceTime, stat.portrait);
                    }

                    updateRaceLabel(position, stat.racerTransform);
                    standingIndex++;
                    position++;

                    if (displayedRows == false && position - rowsOffset > numStandingsDisplayed)
                    {
                        displayedRows = true;
                    }
                }

                foreach (RaceStats racer in curRacers)
                {
                    if (position <= rowsOffset)
                    {
                        updateRaceLabel(position, racer.racerTransform);
                        position++;
                        continue;
                    }

                    if (displayedRows == false)
                    {
                        standings[standingIndex].setStanding(position, racer.commanderName, racer.distanceRemaining.ToString("F3"), racer.portrait);
                    }

                    updateRaceLabel(position, racer.racerTransform);
                    standingIndex++;
                    position++;

                    if (position - rowsOffset > numStandingsDisplayed)
                    {
                        displayedRows = true;
                    }
                }

                if (displayedRows == false)
                {
                    for (int i = standingIndex; i < displayRows; i++)
                    {
                        standings[i].setActive(false);
                    }
                }
            }
        }
    }
}
