using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Collections.Generic;
using System.Text;

using NoxCore.GameModes;
using NoxCore.Managers;
using NoxCore.Stats;
using NoxCore.Utilities;

namespace NoxCore.GUIs
{
    public class TeamDeathMatchGUI : NoxGUI
    {
        protected TeamDeathMatchMode gameMode;

        public GameObject clockGO;

        public GameObject leftPanel = null;
        public GameObject rightPanel = null;

        public GUIStyle readoutStyle;
        protected Rect readoutInfoRect;
        protected bool finalStatsRecorded;
        protected bool showStats;
        protected string statsText, scoreText;
        protected bool pausePanelCycling;

        protected Rect timerInfoRect;

        protected int numFactions;
        protected List<string> factionLookup;
        protected Dictionary<string, FactionCombatStats> tdmFactions;
        protected Dictionary<string, FactionCombatStatsWeighted> sortedFinalFactions;
        protected FactionCombatStats leftFactionStats, rightFactionStats;

        public int numFactionsToDisplay, factionDisplayIndex;
        protected float maxTimer;
        public float displayTime;
        protected float displayTimer;
        protected int factionsOffset;
        protected int screenwidthBy2;

        Text factionLeft, factionRight;
        Text[] statsLeft = new Text[12];
        Text[] statsRight = new Text[12];

        Text clock;

        public KeyCode showNextScreen = KeyCode.PageDown;
        public KeyCode pauseStatPanelCycling = KeyCode.P;

        // Use this for initialization
        public override void init()
        {
            base.init();

            gameMode = GameManager.Instance.Gamemode as TeamDeathMatchMode;

            if (gameMode == null)
            {
                D.error("GameMode", "Could not cast to the required TeamDeathMatchMode game mode");
                return;
            }

            tdmFactions = gameMode.factionStats;

            if (tdmFactions.Count > 0)
            {
                factionLookup = new List<string>();

                foreach (KeyValuePair<string, FactionCombatStats> factionStat in tdmFactions)
                {
                    factionLookup.Add(factionStat.Key);
                }

                tdmFactions.TryGetValue(factionLookup[factionDisplayIndex++], out leftFactionStats);

                if (factionDisplayIndex < factionLookup.Count)
                {
                    tdmFactions.TryGetValue(factionLookup[factionDisplayIndex++], out rightFactionStats);

                    if (factionDisplayIndex == factionLookup.Count)
                    {
                        factionDisplayIndex = 0;
                    }
                }
                else
                {
                    factionDisplayIndex = 0;
                }
            }

            maxTimer = gameMode.maxTime;

            if (clockGO != null)
            {
                Transform clockTimerTrans = clockGO.transform.Find("Round Clock");

                if (clockTimerTrans != null)
                {
                    clock = clockTimerTrans.GetComponent<Text>();
                }
                else
                {
                    D.error("GUI", "Could not find the match timer text component in UI Manager/Canvas/");
                }
            }

            leftPanel = GameObject.Find("DeathMatch Stats Left Panel");
            rightPanel = GameObject.Find("DeathMatch Stats Right Panel");

            if (leftPanel != null && rightPanel != null)
            {
                Transform statsLeftPanel = leftPanel.transform.Find("Stats Panel");
                Transform statsRightPanel = rightPanel.transform.Find("Stats Panel");

                factionLeft = leftPanel.transform.Find("Faction").GetComponent<Text>();

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

                factionRight = rightPanel.transform.Find("Faction").GetComponent<Text>();

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
            }

            screenwidthBy2 = Screen.width / 2;

            readoutInfoRect = new Rect(10, 10, Screen.width, Screen.height - 20);

            enabled = true;
        }

        public Dictionary<string, FactionCombatStats> getTDMFactions()
        {
            return tdmFactions;
        }

        protected override void Update()
        {
            base.Update();

            if(GameManager.Instance.Gamemode.matchState == GameMode.MatchState.WAITINGPOSTMATCH)
            {
                if (Input.GetKeyDown(showNextScreen))
                {
                    showStats = !showStats;
                }
            }
            else if(GameManager.Instance.Gamemode.matchState == GameMode.MatchState.INPROGRESS)
            {
                if (Input.GetKeyDown(pauseStatPanelCycling))
                {
                    pausePanelCycling = !pausePanelCycling;
                }
            }

            if (pausePanelCycling == false)
            {
                displayTimer += Time.deltaTime;
            }
        }

        protected override void OnGUI()
        {
            base.OnGUI();

            if (GameManager.Instance.Gamemode.matchState == GameMode.MatchState.INPROGRESS)
            {
                if (clock != null && timer != null)
                {
                    clock.text = timer.getTimeStr();
                }

                if (tdmFactions.Count > 0)
                {
                    if (displayTimer > displayTime)
                    {
                        displayTimer = 0;

                        tdmFactions.TryGetValue(factionLookup[factionDisplayIndex++], out leftFactionStats);

                        if (factionDisplayIndex < factionLookup.Count)
                        {
                            tdmFactions.TryGetValue(factionLookup[factionDisplayIndex++], out rightFactionStats);

                            if (factionDisplayIndex == factionLookup.Count)
                            {
                                factionDisplayIndex = 0;
                            }
                        }
                        else
                        {
                            factionDisplayIndex = 0;
                        }
                    }

                    FactionCombatStats factionStats = leftFactionStats;

                    if (factionStats != null)
                    {
                        factionLeft.text = factionStats.factionName;

                        statsLeft[0].text = factionStats.factionTotalCost.ToString();

                        if (factionStats.factionAST > 0)
                        {
                            statsLeft[1].text = Timer.formatTimer(factionStats.factionAST, true);
                        }
                        else
                        {
                            statsLeft[1].text = Timer.formatTimer(maxTimer, true);
                        }

                        statsLeft[2].text = factionStats.factionKills.ToString();
                        statsLeft[3].text = factionStats.factionDeaths.ToString();
                        statsLeft[4].text = factionStats.factionAssists.ToString();
                        statsLeft[5].text = factionStats.factionModulesDestroyed.ToString();
                        statsLeft[6].text = factionStats.factionModulesLost.ToString();
                        statsLeft[7].text = factionStats.factionMassKilled.ToString();
                        statsLeft[8].text = factionStats.factionMassLost.ToString();
                        statsLeft[9].text = ((int)(factionStats.factionDamageCaused)).ToString();
                        statsLeft[10].text = ((int)(factionStats.factionDamageTaken)).ToString();
                        statsLeft[11].text = factionStats.factionKillParticipation.ToString("0.000");
                    }

                    factionStats = rightFactionStats;

                    if (factionStats != null)
                    {
                        factionRight.text = factionStats.factionName;

                        statsRight[0].text = factionStats.factionTotalCost.ToString();

                        if (factionStats.factionAST > 0)
                        {
                            statsRight[1].text = Timer.formatTimer(factionStats.factionAST, true);
                        }
                        else
                        {
                            statsRight[1].text = Timer.formatTimer(maxTimer, true);
                        }

                        statsRight[2].text = factionStats.factionKills.ToString();
                        statsRight[3].text = factionStats.factionDeaths.ToString();
                        statsRight[4].text = factionStats.factionAssists.ToString();
                        statsRight[5].text = factionStats.factionModulesDestroyed.ToString();
                        statsRight[6].text = factionStats.factionModulesLost.ToString();
                        statsRight[7].text = factionStats.factionMassKilled.ToString();
                        statsRight[8].text = factionStats.factionMassLost.ToString();
                        statsRight[9].text = ((int)(factionStats.factionDamageCaused)).ToString();
                        statsRight[10].text = ((int)(factionStats.factionDamageTaken)).ToString();
                        statsRight[11].text = factionStats.factionKillParticipation.ToString("0.000");
                    }
                }
            }
            else if (GameManager.Instance.Gamemode.matchState == GameMode.MatchState.WAITINGPOSTMATCH)
            {
                if (finalStatsRecorded == false)
                {
                    int position = 1;

                    StringBuilder statsReadout = new StringBuilder();

                    statsReadout.Append("\n\n" + string.Format("{0, -4}{1, -" + (FactionCombatStats.maxFactionNameLength + 4) + "}{2, 4}{3, 4}{4, 4}{5, 4}{6, 4}{7, 8}{8, 8}{9, 7}{10, 7}{11, 11}{12, 8}", "Pos", "Faction", "K", "A", "D", "+Md", "-Md", "+MASS", "-MASS", "+Dam", "-Dam", "AST", "Cost"));

                    foreach (FactionCombatStats stats in gameMode.combatStatsSorted.Values)
                    {
                        if (stats.factionName == "ZERO") continue;

                        statsReadout.Append("\n" + string.Format("{0, -4}{1, -" + (FactionCombatStats.maxFactionNameLength + 4) + "}{2, 4}{3, 4}{4, 4}{5, 4}{6, 4}{7, 8}{8, 8}{9, 7}{10, 7}{11, 11}{12, 8}", position, stats.factionName, stats.factionKills, stats.factionAssists, stats.factionDeaths, stats.factionModulesDestroyed, stats.factionModulesLost, stats.factionMassKilled, stats.factionMassLost, (int)(stats.factionDamageCaused), (int)(stats.factionDamageTaken), Timer.formatTimer(stats.factionAST, true), stats.factionTotalCost));

                        position++;
                    }

                    statsText = statsReadout.ToString();

                    statsText += ("\n\nPress " + showNextScreen.ToString() + " for ranked faction z-scored metrics");

                    position = 1;

                    StringBuilder scoresReadout = new StringBuilder();
                    
                    scoresReadout.Append("\n\n" + string.Format("{0, -4}{1, -" + (FactionCombatStats.maxFactionNameLength + 4) + "}{2, 9}{3, 9}{4, 9}{5, 9}{6, 9}", "Pos", "Faction", "KAD:" + gameMode.KADPercent + "%", "MAS:" + gameMode.MASPercent + "%", "DMG:" + gameMode.DAMPercent + "%", "AST:" + gameMode.ASTPercent + "%", "Tot:100%"));

                    foreach (FactionCombatStatsWeighted stats in gameMode.factionStatsSorted.Values)
                    {
                        if (stats.factionName == "ZERO") continue;

                        scoresReadout.Append("\n" + string.Format("{0, -4}{1, -" + (FactionCombatStats.maxFactionNameLength + 4) + "}{2, 9}{3, 9}{4, 9}{5, 9}{6, 9}", position, stats.factionName, stats.scaledFactionKADPC.ToString("0.00"), stats.scaledFactionMASRatioPC.ToString("0.00"), stats.scaledFactionDAMRatioPC.ToString("0.00"), stats.scaledFactionAverageSurvivalTimePC.ToString("0.00"), stats.scaledTotalScore.ToString("0.00")));

                        position++;
                    }          

                    scoreText = scoresReadout.ToString();

                    scoreText += ("\n\nCadet Brawl Faction Scoring Metrics:");

                    scoreText += ("\nKAD = (Kills + (Assists / 5)) - Deaths");

                    scoreText += ("\nMAS = Mass Destroyed / Mass Lost");
                    
                    scoreText += ("\nDMG = (Damage Inflicted * Kills) / (Damage Received * Deaths)");
                    
                    scoreText += ("\nAST = Average Survival Time");

                    scoreText += ("\n\nPress " + showNextScreen.ToString() + " for ranked faction stats");

                    finalStatsRecorded = true;
                }
                else
                {
                    if (showStats == true)
                    {
                        GUI.Label(readoutInfoRect, statsText, readoutStyle);
                    }
                    else
                    {
                        GUI.Label(readoutInfoRect, scoreText, readoutStyle);
                    }
                }
            }
        }

        public override void OnResizedWindow(object sender, ResizedWindowEventArgs args)
        {
            base.OnResizedWindow(sender, args);

            screenwidthBy2 = Screen.width / 2;
        }
    }
}
