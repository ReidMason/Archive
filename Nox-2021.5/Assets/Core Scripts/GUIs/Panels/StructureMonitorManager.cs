using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using NoxCore.Cameras;
using NoxCore.Placeables;

namespace NoxCore.GUIs
{
    public class StructureMonitorManager : MonoBehaviour
    {
        public Structure camTarget; //updated from NoxGUI
        private Structure lastCamTarget;

        public GameObject monitorTitle;
        public GameObject structureTitle;
        public GameObject factionTitle;
        public GameObject commanderTitle;
        
        public GameObject prvPanel;
        public GameObject nxtPanel;

        public GameObject readoutDisplay;
        public GameObject scrollBar;

        private StructureMonitor activeMonitor;
        private int activeIndex = 0;
        public StructureMonitor[] monitors;

        private string currReadout;
        private int numScrollSteps;

        private RectTransform textRect;
        private Text readoutText;
        private Vector2 defaultTextSize;
        private Vector2 previousSize;
        private Scrollbar scrollScript;

        public virtual void OnEnable()
        {
            //Move these to Init -- default values
            //Attach structure monitor scripts to ReadoutDisplay
            monitors = GetComponentsInChildren<StructureMonitor>();
            foreach (StructureMonitor monitor in monitors)
            {
                monitor.init();
            }

            if (activeMonitor == null) activeMonitor = monitors[activeIndex];

            //reset values          
            textRect = readoutDisplay.GetComponent<RectTransform>();
            previousSize = defaultTextSize = textRect.sizeDelta;
            readoutText = readoutDisplay.GetComponent<Text>();
            scrollScript = scrollBar.GetComponent<Scrollbar>();

            switchActivePanel(activeIndex);
            lastCamTarget = null;
        }

        public void switchActivePanel(int direction)
        {
            activeIndex = clampMonitorIndex(activeIndex + direction);          
            
            activeMonitor.enabled = false;
            activeMonitor = monitors[activeIndex];
            activeMonitor.enabled = true;

            scrollScript.value = 1;

            updateMonitorHeadings();
        }

        private int clampMonitorIndex(int inputIndex)
        {
            if (inputIndex >= monitors.Length) return 0;
            if (inputIndex < 0) return (monitors.Length - 1);
            else return inputIndex;
        }

        protected virtual void OnGUI()
        {
            if (camTarget == null)
            {
                gameObject.SetActive(false);
            }
            else
            {
                if (lastCamTarget != camTarget)
                {
                    updateStructureHeadings();
                    lastCamTarget = camTarget;
                }

                currReadout = activeMonitor.callUpdateReadout(camTarget);
                readoutText.text = currReadout;

                if (previousSize.y != readoutText.preferredHeight) //If text box size has changed, check if the scroll bar has too
                {
                    //Debug.Log("new text size");
                    if (textRect.sizeDelta.y < readoutText.preferredHeight)
                    {
                        gameObject.GetComponentInChildren<ScrollRect>().enabled = true;
                        scrollBar.SetActive(true);
                        textRect.sizeDelta = new Vector2(textRect.sizeDelta.x, readoutText.preferredHeight);
                    }
                    else if (defaultTextSize.y >= readoutText.preferredHeight)
                    {
                        gameObject.GetComponentInChildren<ScrollRect>().enabled = false;
                        scrollBar.SetActive(false);
                        textRect.sizeDelta = defaultTextSize;                        
                    }
                }

                previousSize = textRect.sizeDelta;
                
            }
        }
        
        public void updateMonitorHeadings()
        {
            monitorTitle.GetComponent<Text>().text = activeMonitor.monitorName;
            prvPanel.GetComponent<Text>().text = "< " + monitors[clampMonitorIndex(activeIndex - 1)].monitorName;
            nxtPanel.GetComponent<Text>().text = monitors[clampMonitorIndex(activeIndex + 1)].monitorName + " >";
        }

        public void updateStructureHeadings()
        {
            structureTitle.GetComponent<Text>().text = camTarget.name;
            factionTitle.GetComponent<Text>().text = camTarget.Faction.label;
            commanderTitle.GetComponent<Text>().text = camTarget.Command.rankData.abbreviation + " " + camTarget.Command.label;
        }    

        public void onScroll()
        {
            gameObject.GetComponent<Canvas>().pixelPerfect = false;
        }

        public void endScroll()
        {
            gameObject.GetComponent<Canvas>().pixelPerfect = true;
        }

    }
}
