using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

using NoxCore.GameModes;
using NoxCore.Managers;
using NoxCore.Stats;
using NoxCore.Utilities;

namespace NoxCore.GUIs
{
    public class SquadvsSquadGUI : NoxGUI
    {
        protected SquadvsSquadMode gameMode;

        public GameObject titlePanel, subtitlePanel;
        public GameObject clockGO;
        public KeyCode miniScorePanelToggleKey;
        public bool miniScorePanelActive;
        public GameObject miniScorePanelLeft, miniScorePanelRight;
        public GameObject fullStatsPanelLeft, fullStatsPanelRight;
        public GameObject insigniaPanelLeft, insigniaPanelRight, versusPanel;
        public GameObject miniInsigniaPanelLeft, miniInsigniaPanelRight;
        public GameObject matchReportPanel;

        public GUIStyle factionStatsStyle;

        protected int numSquads;
        protected List<string> squadLookup;
        protected Dictionary<string, SquadCombatStats> squads;
        protected SquadCombatStats leftSquadStats, rightSquadStats;

        public int numSquadsToDisplay, squadDisplayIndex;
        protected float maxTimer;
        public float displayTime;
        protected float displayTimer;
        protected int squadsOffset;
        protected int screenwidthBy2;

        public bool switchedSides;
        Text clock;
        Text squadLeft, squadRight, miniSquadLeft, miniSquadRight;
        Text scoreLeft, scoreRight;
        Text[] statsLeft = new Text[12];
        Text[] statsRight = new Text[12];
        Animator titleAnimController, subtitleAnimController, insigniaLeftAnimController, insigniaRightAnimController, versusAnimController;
        Text matchReport;

        // Use this for initialization
        public override void init()
        {
            base.init();

            gameMode = GameManager.Instance.Gamemode as SquadvsSquadMode;

            if (gameMode == null)
            {
                D.error("GameMode", "Could not cast to the required SquadvsSquad game mode");
                return;
            }

            squads = gameMode.squadStats;

            if (squads.Count > 0)
            {
                squadLookup = new List<string>();

                foreach (KeyValuePair<string, SquadCombatStats> squadStat in squads)
                {
                    squadLookup.Add(squadStat.Key);
                }

                squads.TryGetValue(squadLookup[squadDisplayIndex++], out leftSquadStats);

                if (squadDisplayIndex < squadLookup.Count)
                {
                    squads.TryGetValue(squadLookup[squadDisplayIndex++], out rightSquadStats);

                    if (squadDisplayIndex == squadLookup.Count)
                    {
                        squadDisplayIndex = 0;
                    }
                }
                else
                {
                    squadDisplayIndex = 0;
                }
            }

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

            if (titlePanel != null)
            {
                titleAnimController = titlePanel.GetComponent<Animator>();
            }

            if (subtitlePanel != null)
            {
                subtitleAnimController = subtitlePanel.GetComponent<Animator>();
            }

            if (miniScorePanelLeft != null && miniScorePanelRight != null)
            {
                miniSquadLeft = miniScorePanelLeft.transform.Find("Faction Score Panel/Name").GetComponent<Text>();
                scoreLeft = miniScorePanelLeft.transform.Find("Faction Score Panel/Score").GetComponent<Text>();

                miniSquadRight = miniScorePanelRight.transform.Find("Faction Score Panel/Name").GetComponent<Text>();
                scoreRight = miniScorePanelRight.transform.Find("Faction Score Panel/Score").GetComponent<Text>();
            }

            if (versusPanel != null)
            {
                versusAnimController = versusPanel.GetComponent<Animator>();
            }

            if (insigniaPanelLeft != null && insigniaPanelRight != null)
            {
                insigniaLeftAnimController = insigniaPanelLeft.GetComponent<Animator>();
                insigniaRightAnimController = insigniaPanelRight.GetComponent<Animator>();

                RawImage imgLeft = insigniaPanelLeft.GetComponent<RawImage>();
                Texture2D insignia;

                if (switchedSides == false)
                {
                    insignia = gameMode.insignias[0];
                }
                else
                {
                    insignia = gameMode.insignias[1];
                }

                imgLeft.texture = insignia;

                RawImage imgRight = insigniaPanelRight.GetComponent<RawImage>();

                if (switchedSides == false)
                {
                    insignia = gameMode.insignias[1];
                }
                else
                {
                    insignia = gameMode.insignias[0];
                }

                imgRight.texture = insignia;
            }

            if (miniInsigniaPanelLeft != null && miniInsigniaPanelRight != null)
            {
                RawImage imgLeft = miniInsigniaPanelLeft.GetComponent<RawImage>();
                Texture2D miniInsignia;

                if (switchedSides == false)
                {
                    miniInsignia = gameMode.miniInsignias[0];
                }
                else
                {
                    miniInsignia = gameMode.miniInsignias[1];
                }

                imgLeft.texture = miniInsignia;

                RawImage imgRight = miniInsigniaPanelRight.GetComponent<RawImage>();

                if (switchedSides == false)
                {
                    miniInsignia = gameMode.miniInsignias[1];
                }
                else
                {
                    miniInsignia = gameMode.miniInsignias[0];
                }

                imgRight.texture = miniInsignia;
            }

            if (fullStatsPanelLeft != null && fullStatsPanelRight != null)
            {
                Transform statsLeftPanel = fullStatsPanelLeft.transform.Find("Stats Panel");
                Transform statsRightPanel = fullStatsPanelRight.transform.Find("Stats Panel");

                squadLeft = fullStatsPanelLeft.transform.Find("Faction").GetComponent<Text>();

                statsLeft[0] = statsLeftPanel.transform.Find("Cost").GetComponent<Text>();
                statsLeft[1] = statsLeftPanel.transform.Find("Surv").GetComponent<Text>();
                statsLeft[2] = statsLeftPanel.transform.Find("Kills").GetComponent<Text>();
                statsLeft[3] = statsLeftPanel.transform.Find("Deaths").GetComponent<Text>();
                statsLeft[4] = statsLeftPanel.transform.Find("Assists").GetComponent<Text>();
                statsLeft[5] = statsLeftPanel.transform.Find("Mod Dest").GetComponent<Text>();
                statsLeft[6] = statsLeftPanel.transform.Find("Mod Lost").GetComponent<Text>();
                statsLeft[7] = statsLeftPanel.transform.Find("Mass Dest").GetComponent<Text>();
                statsLeft[8] = statsLeftPanel.transform.Find("Mass Lost").GetComponent<Text>();
                statsLeft[9] = statsLeftPanel.transform.Find("Dmg Caused").GetComponent<Text>();
                statsLeft[10] = statsLeftPanel.transform.Find("Dmg Taken").GetComponent<Text>();
                statsLeft[11] = statsLeftPanel.transform.Find("Kill Part").GetComponent<Text>();

              //  leftSquadStats.stationKillTime = maxTimer;

                squadRight = fullStatsPanelRight.transform.Find("Faction").GetComponent<Text>();

                statsRight[0] = statsRightPanel.transform.Find("Cost").GetComponent<Text>();
                statsRight[1] = statsRightPanel.transform.Find("Surv").GetComponent<Text>();
                statsRight[2] = statsRightPanel.transform.Find("Kills").GetComponent<Text>();
                statsRight[3] = statsRightPanel.transform.Find("Deaths").GetComponent<Text>();
                statsRight[4] = statsRightPanel.transform.Find("Assists").GetComponent<Text>();
                statsRight[5] = statsRightPanel.transform.Find("Mod Dest").GetComponent<Text>();
                statsRight[6] = statsRightPanel.transform.Find("Mod Lost").GetComponent<Text>();
                statsRight[7] = statsRightPanel.transform.Find("Mass Dest").GetComponent<Text>();
                statsRight[8] = statsRightPanel.transform.Find("Mass Lost").GetComponent<Text>();
                statsRight[9] = statsRightPanel.transform.Find("Dmg Caused").GetComponent<Text>();
                statsRight[10] = statsRightPanel.transform.Find("Dmg Taken").GetComponent<Text>();
                statsRight[11] = statsRightPanel.transform.Find("Kill Part").GetComponent<Text>();

              //  rightSquadStats.firstKillTime = maxTimer;
             //   rightSquadStats.stationKillTime = maxTimer;
            }

            if (matchReportPanel != null)
            {
                matchReport = matchReportPanel.GetComponent<Text>();

                if (matchReport == null)
                {
                    D.error("GUI", "Could not find the text component on the match report panel");
                }
                else
                {
                    matchReportPanel.SetActive(false);
                }
            }

            screenwidthBy2 = Screen.width / 2;

            titlePanel.SetActive(false);
            subtitlePanel.SetActive(false);
            clockGO.SetActive(false);
            miniScorePanelLeft.SetActive(false);
            miniScorePanelRight.SetActive(false);
            fullStatsPanelLeft.SetActive(false);
            fullStatsPanelRight.SetActive(false);
            insigniaPanelLeft.SetActive(false);
            insigniaPanelRight.SetActive(false);
            versusPanel.SetActive(false);

            enabled = true;

            StartCoroutine(startIntro());
        }

        public Dictionary<string, SquadCombatStats> getSquads()
        {
            return squads;
        }

        protected IEnumerator startIntro()
        {
            if (gameMode.skipIntro == false)
            {
                yield return new WaitForSeconds(1f);

                titlePanel.SetActive(true);

                yield return new WaitForSeconds(1f);

                subtitlePanel.SetActive(true);

                yield return new WaitForSeconds(2.8f);

                insigniaPanelLeft.SetActive(true);
                insigniaPanelRight.SetActive(true);
                versusPanel.SetActive(true);

                yield return new WaitForSeconds(0.5f);

                titleAnimController.SetTrigger("Out");

                yield return new WaitForSeconds(0.5f);

                subtitleAnimController.SetTrigger("Out");

                yield return new WaitForSeconds(1.5f);

                versusAnimController.SetTrigger("Out");
                insigniaLeftAnimController.SetTrigger("Out");
                insigniaRightAnimController.SetTrigger("Out");

                yield return new WaitForSeconds(3.0f);
            }

            endIntro();
        }

        public void endIntro()
        {
            if (gameMode.skipIntro == false)
            {
                titlePanel.SetActive(false);
                subtitlePanel.SetActive(false);
                versusPanel.SetActive(false);
                insigniaPanelLeft.SetActive(false);
                insigniaPanelRight.SetActive(false);
            }

            setScorePanels();

            clockGO.SetActive(true);

            gameMode.triggerStartMatch();
        }

        protected void setScorePanels()
        {
            miniScorePanelLeft.SetActive(miniScorePanelActive);
            miniScorePanelRight.SetActive(miniScorePanelActive);

            fullStatsPanelLeft.SetActive(!miniScorePanelActive);
            fullStatsPanelRight.SetActive(!miniScorePanelActive);
        }

        protected void toggleActiveStatPanels()
        {
            miniScorePanelActive = !miniScorePanelActive;

            setScorePanels();
        }

        public void showMatchReport()
        {
            string winningFaction = "", reason = "", leftSquad, rightSquad;
            bool draw = false;

            if (switchedSides == false)
            {
                if (squadLeft.text.Length > 0) leftSquad = squadLeft.text;
                else leftSquad = miniSquadLeft.text;

                if (squadRight.text.Length > 0) rightSquad = squadRight.text;
                else rightSquad = miniSquadRight.text;
            }
            else
            {
                if (squadLeft.text.Length > 0) leftSquad = squadRight.text;
                else leftSquad = miniSquadRight.text;

                if (squadRight.text.Length > 0) rightSquad = squadLeft.text;
                else rightSquad = miniSquadLeft.text;
            }

            if (leftSquadStats.squadMassKilled > rightSquadStats.squadMassKilled) { winningFaction = leftSquad; reason = "Mass Killed"; }
            else if (rightSquadStats.squadMassKilled > leftSquadStats.squadMassKilled) { winningFaction = rightSquad; reason = "Mass Killed"; }
            else
            {
                if (leftSquadStats.stationKillTime < rightSquadStats.stationKillTime) { winningFaction = leftSquad; reason = "Fastest Enemy Base Killed"; }
                else if (rightSquadStats.stationKillTime < leftSquadStats.stationKillTime) { winningFaction = rightSquad; reason = "Fastest Enemy Base Killed"; }
                else
                {
                    if (leftSquadStats.numRespawns < rightSquadStats.numRespawns) { winningFaction = leftSquad; reason = "Fewest Respawns"; }
                    else if (rightSquadStats.numRespawns < leftSquadStats.numRespawns) { winningFaction = rightSquad; reason = "Fewest Respawns"; }
                    else
                    {
                        if (leftSquadStats.squadKills > rightSquadStats.squadKills) { winningFaction = leftSquad; reason = "Most Kills"; }
                        else if (rightSquadStats.squadKills > leftSquadStats.squadKills) { winningFaction = rightSquad; reason = "Most Kills"; }
                        else
                        {
                            if (leftSquadStats.squadDeaths < rightSquadStats.squadDeaths) { winningFaction = leftSquad; reason = "Fewest Deaths"; }
                            else if (rightSquadStats.squadDeaths < leftSquadStats.squadDeaths) { winningFaction = rightSquad; reason = "Fewest Deaths"; }
                            else
                            {
                                if (leftSquadStats.squadDAM > rightSquadStats.squadDAM) { winningFaction = leftSquad; reason = "Highest Damage Gain"; }
                                else if (rightSquadStats.squadDAM > leftSquadStats.squadDAM) { winningFaction = rightSquad; reason = "Highest Damage Gain"; }
                                else
                                {
                                    if (leftSquadStats.firstKillTime < rightSquadStats.firstKillTime) { winningFaction = leftSquad; reason = "Fastest Kill"; }
                                    else if (rightSquadStats.firstKillTime < leftSquadStats.firstKillTime) { winningFaction = rightSquad; reason = "Fastest Kill"; }
                                    else draw = true;
                                }
                            }
                        }
                    }
                }
            }

            matchReportPanel.SetActive(true);

            if (draw == false) matchReport.text = winningFaction + "\nWins by\n" + reason;
            else matchReport.text = "Match Drawn!!!";

            StartCoroutine(displayMatchReport());
        }

        private IEnumerator displayMatchReport()
        {
            while (true)
            {
                matchReport.fontSize++;

                if (matchReport.fontSize >= 110) break;

                yield return new WaitForEndOfFrame();
            }
        }

        protected override void Update()
        {
            base.Update();

            if (Input.GetKeyDown(miniScorePanelToggleKey))
            {
                toggleActiveStatPanels();
            }
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            if (clock != null && timer != null)
            {
                clock.text = timer.getTimeStr();
            }

            if (gameMode != null && gameMode.matchState == GameMode.MatchState.INPROGRESS)
            {
                displayTimer += Time.deltaTime;

                if (squads.Count > 0)
                {
                    if (displayTimer > displayTime)
                    {
                        displayTimer = 0;

                        squads.TryGetValue(squadLookup[squadDisplayIndex++], out leftSquadStats);

                        if (squadDisplayIndex < squadLookup.Count)
                        {
                            squads.TryGetValue(squadLookup[squadDisplayIndex++], out rightSquadStats);

                            if (squadDisplayIndex == squadLookup.Count)
                            {
                                squadDisplayIndex = 0;
                            }
                        }
                        else
                        {
                            squadDisplayIndex = 0;
                        }
                    }

                    SquadCombatStats squadStats;

                    if (miniScorePanelActive == false)
                    {
                        if (switchedSides == false)
                        {
                            squadStats = leftSquadStats;
                        }
                        else
                        {
                            squadStats = rightSquadStats;
                        }

                        if (squadStats != null)
                        {
                            squadLeft.text = squadStats.squadName;

                            statsLeft[0].text = squadStats.squadTotalCost.ToString();

                            if (squadStats.squadAverageSurvivalTime > 0)
                            {
                                statsLeft[1].text = Timer.formatTimer(squadStats.squadAverageSurvivalTime, true);
                            }
                            else
                            {
                                statsLeft[1].text = Timer.formatTimer(maxTimer, true);
                            }

                            statsLeft[2].text = squadStats.squadKills.ToString();
                            statsLeft[3].text = squadStats.squadDeaths.ToString();
                            statsLeft[4].text = squadStats.squadAssists.ToString();
                            statsLeft[5].text = squadStats.squadModulesDestroyed.ToString();
                            statsLeft[6].text = squadStats.squadModulesLost.ToString();
                            statsLeft[7].text = squadStats.squadMassKilled.ToString();
                            statsLeft[8].text = squadStats.squadMassLost.ToString();
                            statsLeft[9].text = ((int)(squadStats.squadDamageCaused)).ToString();
                            statsLeft[10].text = ((int)(squadStats.squadDamageTaken)).ToString();
                            statsLeft[11].text = squadStats.squadKillParticipation.ToString("0.000");
                        }

                        if (switchedSides == false)
                        {
                            squadStats = rightSquadStats;
                        }
                        else
                        {
                            squadStats = leftSquadStats;
                        }

                        if (squadStats != null)
                        {
                            squadRight.text = squadStats.squadName;

                            statsRight[0].text = squadStats.squadTotalCost.ToString();

                            if (squadStats.squadAverageSurvivalTime > 0)
                            {
                                statsRight[1].text = Timer.formatTimer(squadStats.squadAverageSurvivalTime, true);
                            }
                            else
                            {
                                statsRight[1].text = Timer.formatTimer(maxTimer, true);
                            }

                            statsRight[2].text = squadStats.squadKills.ToString();
                            statsRight[3].text = squadStats.squadDeaths.ToString();
                            statsRight[4].text = squadStats.squadAssists.ToString();
                            statsRight[5].text = squadStats.squadModulesDestroyed.ToString();
                            statsRight[6].text = squadStats.squadModulesLost.ToString();
                            statsRight[7].text = squadStats.squadMassKilled.ToString();
                            statsRight[8].text = squadStats.squadMassLost.ToString();
                            statsRight[9].text = ((int)(squadStats.squadDamageCaused)).ToString();
                            statsRight[10].text = ((int)(squadStats.squadDamageTaken)).ToString();
                            statsRight[11].text = squadStats.squadKillParticipation.ToString("0.000");
                        }
                    }
                    else
                    {
                        if (miniScorePanelLeft != null)
                        {
                            if (switchedSides == false)
                            {
                                squadStats = leftSquadStats;
                            }
                            else
                            {
                                squadStats = rightSquadStats;
                            }

                            miniSquadLeft.text = squadStats.squadName;
                            scoreLeft.text = squadStats.squadMassKilled.ToString();
                        }

                        if (miniScorePanelRight != null)
                        {
                            if (switchedSides == false)
                            {
                                squadStats = rightSquadStats;
                            }
                            else
                            {
                                squadStats = leftSquadStats;
                            }

                            miniSquadRight.text = squadStats.squadName;
                            scoreRight.text = squadStats.squadMassKilled.ToString();
                        }
                    }
                }
            }
        }

        public override void OnResizedWindow(object sender, ResizedWindowEventArgs args)
        {
            base.OnResizedWindow(sender, args);

            //float x, y = 0, w = 120, h = 20;

            screenwidthBy2 = Screen.width / 2;
        }
    }
}
