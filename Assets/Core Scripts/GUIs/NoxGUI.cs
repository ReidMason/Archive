using UnityEngine;
using System.Collections;
using System;
using System.Diagnostics;

using NoxCore.Utilities;
using NoxCore.Cameras;
using NoxCore.Managers;
using NoxCore.Placeables;

namespace NoxCore.GUIs
{
    public enum HealthBarMode { NONE, ALL, TRACK };    

    public class ResizedWindowEventArgs : EventArgs
    {
        public float width, height;

        public ResizedWindowEventArgs(float width, float height)
        {
            this.width = width;
            this.height = height;
        }
    }

    public class NoxGUI : MonoBehaviour
    {
        public delegate void ResizedWindowEventDispatcher(object sender, ResizedWindowEventArgs args);
        public event ResizedWindowEventDispatcher ResizeWindow;

        protected TopDown_Camera Cam;
        public float resizedCheckInterval;

        public bool combatFinished;

        protected int lastWidth;
        protected int lastHeight;
        protected bool stay = true;
        
        public GUIStyle timeStyle;
        protected Rect timeScaleRect;

        protected Timer timer;
        protected float prevTimeScale = 1.0f;
        protected bool normalTimeScale = true;
        protected Stopwatch scaledTimer = new Stopwatch();

        public bool showNames;
        public bool showFactions;
        public bool showCaptains;
        public HealthBarMode healthBarMode;
        protected bool cyclingCaptainNames;
        public bool CyclingCaptainNames { get { return cyclingCaptainNames; } set { cyclingCaptainNames = value; } }

        public string previousMessage, currentMessage;

        public GameObject fpsPanel;
        protected bool fpsPanelToggle;

        public GameObject structureMonitor;
        private StructureMonitorManager structureMonitorScript;
        protected bool structureMonitorToggle;

        //Input Keys
        public KeyCode FPSPanelKey = KeyCode.F;
        public KeyCode StructureMonitorKey = KeyCode.M;
        public KeyCode ToggleCaptainsNameDisplayKey = KeyCode.V;
        public KeyCode ToggleNamesDisplayKey = KeyCode.N;
        public KeyCode ToggleFactionsDisplayKey = KeyCode.M;
        public KeyCode CycleHealthBarsKey = KeyCode.B;
        public KeyCode nextEngPanelKey = KeyCode.Period;
        public KeyCode prevEngPanelKey = KeyCode.Comma;

        protected Structure camTarget;

        #region Singleton
        /// <summary>
        ///   Provide singleton support for this class.
        ///   The script must still be attached to a game object, but this will allow it to be called
        ///   from anywhere without specifically identifying that game object.
        /// </summary>
        private static NoxGUI instance;

        public static NoxGUI Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = (NoxGUI)FindObjectOfType(typeof(NoxGUI));
                    if (instance == null)
                        D.error("Content: {0}", "There needs to be one NoxGUI-derived script on the UIManager GameObject in your scene.");
                }
                return instance;
            }
        }
        #endregion

        public virtual void init()
        {
            Cam = Camera.main.GetComponent<TopDown_Camera>();

            timeScaleRect = new Rect(Screen.width - 60, Screen.height - 20, 58, 20);

            timer = GameManager.Instance.GetComponent<Timer>();

            ResizeWindow += OnResizedWindow;

            setHealthBarMode(true);

            if (structureMonitor == null)
            {
                D.log("GUI", "No Structure Monitor set in GUI.");
            }
            else
            {
                structureMonitorScript = structureMonitor.GetComponent<StructureMonitorManager>();
            }

            StartCoroutine(checkResized());
            StartCoroutine(checkPause());

            if (showNames == true)
            {
                displayNames();
            }

            if (showFactions == true)
            {
                displayFactions();
            }
        }

        public void toggleCaptainsNames()
        {
            showCaptains = !showCaptains;

            displayNames();
        }

        public void showingCaptains(Structure structure)
        {
            if (showCaptains == true && cyclingCaptainNames == false)
            {
                if (structure.NameLabel.Captain == null) return;

                structure.NameLabel.startCyclingNamesCoroutine();
            }
            else if(showCaptains == false && cyclingCaptainNames == true)
            {
                if (structure.NameLabel.Captain == null) return;

                structure.NameLabel.stopCyclingNamesCoroutine();

                if (showNames == true)
                {
                    structure.NameLabel.LabelText.text = structure.Name;
                }
            }
        }

        public void displayNames()
        {
            // TODO - these labels should be cached and updated if new structures enter the scene during runtime
            foreach (Structure structure in GameManager.Instance.getAllStructures())
            {
                showingCaptains(structure);

                if (structure.NameLabel != null)
                {
                    structure.NameLabel.ShowLabel(showNames);
                }
                else
                {
                    UnityEngine.Debug.Log(structure.name + "has no name label");
                }
            }

            if (showCaptains == true) cyclingCaptainNames = true;
            else cyclingCaptainNames = false;
        }

        public void toggleNames()
        {
            showNames = !showNames;

            displayNames();
        }

        public void displayFactions()
        {
            // TODO - these labels should be cached and updated if new structures enter the scene during runtime
            foreach (Structure structure in GameManager.Instance.getAllStructures())
            {
                structure.FactionLabel.ShowLabel(showFactions);
            }
        }

        public void toggleFactions()
        {            
            showFactions = !showFactions;

            displayFactions();
        }

        public void setAllHealthBars(bool active)
        {
            // TODO - should keep a running list of health bars so no finding is necessary

            GameObject[] hBars = GameObject.FindGameObjectsWithTag("HealthBar");

            foreach (GameObject hBar in hBars)
            {
                GameObject healthBar = hBar.transform.Find("Canvas").gameObject;
                healthBar.SetActive(active);
            }
        }

        public void setHealthBarMode(bool suppressMessage = false)
        {            
            switch (healthBarMode)
            {
                case HealthBarMode.NONE:
                    setAllHealthBars(false);

                    if (suppressMessage == false) setMessage("Changed health bar mode to NONE");
                    break;

                case HealthBarMode.ALL:
                    setAllHealthBars(true);

                    if (suppressMessage == false) setMessage("Changed health bar mode to ALL");
                    break;

                case HealthBarMode.TRACK:
                    setAllHealthBars(false);

                    if (Cam.followTarget != null)
                    {
                        Transform healthBarTrans = Cam.followTarget.FindChildWithTag("HealthBar");

                        if (healthBarTrans != null)
                        {
                            healthBarTrans.Find("Canvas").gameObject.SetActive(true);
                        }
                    }

                    if (suppressMessage == false) setMessage("Changed health bar mode to TRACK");
                    break;

                default:
                    // D.warn("GUI", "Unknown health bar mode set in NoxGUI");
                    break;
            }
        }

        public void setMessage(string message, Color? colour = null)
        {
            if (currentMessage != message)
            {
                previousMessage = currentMessage;

                MessageList.Instance.AddMessage(message, colour);

                currentMessage = message;
            }

            // D.log("GUI", message);
        }

        protected void recalculateRects()
        {
            timeScaleRect = new Rect(Screen.width - 60, Screen.height - 20, 58, 20);
        }

        protected IEnumerator checkResized()
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;

            while (stay)
            {
                if (lastWidth != Screen.width || lastHeight != Screen.height)
                {
                    Call_ResizeWindow(this, new ResizedWindowEventArgs(Screen.width, Screen.height));
                }
                yield return new WaitForSeconds(resizedCheckInterval);
            }
        }

        protected IEnumerator checkPause()
        {
            while (stay)
            {
                if (Input.GetKey(KeyCode.Space) && Input.GetKeyUp(KeyCode.P))
                {
                    if (Time.timeScale == 0)
                    {
                        Time.timeScale = 1;
                        normalTimeScale = true;
                        timer.startTimer();
                        scaledTimer.Start();
                    }
                    else
                    {
                        Time.timeScale = 0;
                        normalTimeScale = false;
                        timer.stopTimer();
                        scaledTimer.Stop();
                    }
                }
                yield return null;
            }
        }

        protected virtual void OnGUI()
        {
            //UI Camera Target
            //Switch this to an event binding to when follow target is set in TopDownCamera

            //If the structure monitor is active, update the current cam/follow target. If there is no follow target: close the monitor.
            if (structureMonitorToggle == true)
            {
                if (Cam != null && Cam.followTarget != null)
                {
                   structureMonitorScript.camTarget = Cam.followTarget.GetComponent<Structure>();
                }
                else
                {
                    structureMonitorToggle = false;
                    structureMonitorScript.gameObject.SetActive(structureMonitorToggle);
                }
            }

            //Time
            if ((int)(Time.timeScale) != 1)
            {
                if (Time.timeScale == 0)
                {
                    GUI.Label(timeScaleRect, "Paused", timeStyle);
                }
                else if (Time.timeScale < 1)
                {
                    GUI.Label(timeScaleRect, "x1:" + (int)(1.0f / (Time.timeScale)), timeStyle);
                }
                else
                {
                    GUI.Label(timeScaleRect, "x" + (int)(Time.timeScale), timeStyle);
                }
            }
        }

        //ALL UI INPUTS ARE HERE
        protected virtual void Update()
        {
            if (scaledTimer.IsRunning == true)
            {
                timer.scaledTimeAccumulator = (((float)scaledTimer.Elapsed.TotalSeconds) * Time.timeScale);
            }

            if (Input.GetKeyUp(FPSPanelKey))
            {
                fpsPanelToggle = !fpsPanelToggle;
                fpsPanel.SetActive(fpsPanelToggle);
            }

            if (Input.GetKeyUp(StructureMonitorKey) && Cam.followTarget != null)
            {
                if (structureMonitorScript != null)
                {
                    structureMonitorToggle = !structureMonitorToggle;
                    structureMonitorScript.gameObject.SetActive(structureMonitorToggle);
                }
            }

            if (Input.GetKeyUp(prevEngPanelKey) && structureMonitorToggle == true)
            {
                structureMonitorScript.switchActivePanel(-1);
            }

            if (Input.GetKeyDown(nextEngPanelKey) && structureMonitorToggle == true)
            {
                structureMonitorScript.switchActivePanel(+1);
            }

            if (Input.GetKeyDown(CycleHealthBarsKey))
            {
                healthBarMode++;

                if (healthBarMode > HealthBarMode.TRACK)
                {
                    healthBarMode = HealthBarMode.NONE;
                }

                setHealthBarMode(true);
            }

            if (Input.GetKeyDown(ToggleCaptainsNameDisplayKey))
            {
                toggleCaptainsNames();
            }

            if (Input.GetKeyDown(ToggleFactionsDisplayKey))
            {
                toggleFactions();
            }

            if (Input.GetKeyDown(ToggleNamesDisplayKey))
            {
                toggleNames();
            }

            // allows us to change the timescale of the game (note: this is quite complex due to the use of a system timer for match times)
            if (Input.GetKey(KeyCode.Space))
            {
                if (Input.GetKeyDown(KeyCode.Period))
                {
                    if (Time.timeScale < 32)
                    {
                        prevTimeScale = Time.timeScale;

                        Time.timeScale *= 2.0f;
                        Time.fixedDeltaTime = 0.02f * Time.timeScale;

                        if (Time.timeScale != 1)
                        {
                            if (prevTimeScale == 1)
                            {
                                timer.stopTimer();
                                normalTimeScale = false;
                            }

                            if (scaledTimer.IsRunning == true)
                            {
                                // hoof total seconds off to stopwatchtimer buffer
                                timer.addScaledTime();

                                // reset scaled timer
                                scaledTimer.Reset();
                            }

                            scaledTimer.Start();
                        }
                        else if (normalTimeScale == false)
                        {
                            // hoof total seconds off to stopwatchtimer buffer
                            timer.addScaledTime();

                            // reset scaled timer
                            scaledTimer.Reset();

                            timer.startTimer();
                            normalTimeScale = true;
                        }

                        // D.log("System", "Timescale now: " + Time.timeScale + "   Fixed DeltaTime now: " + Time.fixedDeltaTime);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Comma))
                {
                    if (Time.timeScale > 0.03125f)
                    {
                        prevTimeScale = Time.timeScale;

                        Time.timeScale /= 2.0f;
                        Time.fixedDeltaTime = 0.02f * Time.timeScale;

                        if (Time.timeScale != 1)
                        {
                            if (prevTimeScale == 1)
                            {
                                timer.stopTimer();
                                normalTimeScale = false;
                            }

                            if (scaledTimer.IsRunning == true)
                            {
                                // hoof total seconds off to stopwatchtimer buffer
                                timer.addScaledTime();

                                // reset scaled timer
                                scaledTimer.Reset();
                            }

                            scaledTimer.Start();
                        }
                        else if (normalTimeScale == false)
                        {
                            // hoof total seconds off to stopwatchtimer buffer
                            timer.addScaledTime();

                            scaledTimer.Reset();

                            timer.startTimer();
                            normalTimeScale = true;
                        }

                        // D.log("System", "Timescale now: " + Time.timeScale + "   Fixed DeltaTime now: " + Time.fixedDeltaTime);
                    }
                }
            }
        }

        protected void OnDestroy()
        {
            stay = false;
        }

        #region event dispatchers
        ////////////////////////////////////
        /*
			Event dispatchers for all Structure events
		*/
        ////////////////////////////////////
        public void Call_ResizeWindow(object sender, ResizedWindowEventArgs args)
        {
            if (ResizeWindow != null)
            {
                ResizeWindow(sender, args);
            }
        }
        #endregion

        #region event handlers
        ///////////////////////////////////////////
        /*
			Handlers for Structure events
		*/
        ///////////////////////////////////////////	
        public virtual void OnResizedWindow(object sender, ResizedWindowEventArgs args)
        {
            recalculateRects();

            lastWidth = Screen.width;
            lastHeight = Screen.height;
        }
        #endregion
    }
}