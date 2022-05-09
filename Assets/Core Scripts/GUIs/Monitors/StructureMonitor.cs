using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;

namespace NoxCore.GUIs
{
    public class StructureMonitor : MonoBehaviour
    {
        protected StringBuilder readoutInfo;

        public string monitorName;

        public int textPerPanel { get; protected set;  }
        public float panelPercent { get; protected set; }

        private Text textDisplay;
        private RectTransform panel;

        public virtual void init()
        {
            textDisplay = gameObject.GetComponent<Text>();
            panel = gameObject.GetComponent<RectTransform>();
            readoutInfo = new StringBuilder();
        }

        public string callUpdateReadout(Structure camTarget)
        {
            updateReadout(camTarget);            

            return readoutInfo.ToString().Trim();
        }

        protected virtual void updateReadout(Structure camTarget)
        {
            //readoutInfo = new StringBuilder();

            // better to reset the current stringbuilder object than make a new one due to memory spikes / garbage collection
            // TODO - in .NET4.0 can use readoutInfo.Clear() but not in previous versions
            // https://stackoverflow.com/questions/1709471/best-way-to-clear-contents-of-nets-stringbuilder

            int origCapacity = readoutInfo.Capacity;

            readoutInfo.Length = 0;
            readoutInfo.Capacity = 1;
            readoutInfo.Capacity = origCapacity;

            readoutInfo.Append("\n");            
        }        

        public int calcNumPanels()
        {
            textDisplay = gameObject.GetComponent<Text>();
            
            float textHeight = LayoutUtility.GetPreferredHeight(textDisplay.rectTransform);
            float panelHeight = panel.rect.height;            

            panelPercent = textHeight / panelHeight;

            textPerPanel = (int)Mathf.Ceil(readoutInfo.Length * panelPercent);

            //Debug.Log("actual h " + textHeight + " / desired h " + panelHeight + " = " + numPanels + " panels. Length is " + readoutInfo.Length + " so it is " + TextPerPanel + " per panel");

            return (int)Mathf.Ceil(panelPercent);
        }

    }
}
