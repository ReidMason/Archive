using System.Collections;
using UnityEngine;

namespace NoxCore.GUIs
{
    public class NameLabel : StructureLabel
    {
        protected string captain;
        public string Captain { get { return captain; } set { captain = value; } }

        public float cycleNamesDelay = 4.0f;

        protected Coroutine cycleNamesCoroutine;

        public override void init()
        {
            base.init();
        }

        public override void Reset()
        {
            base.Reset();

            if (labelText != null && structure != null)
            {
                if (structure.Name == "")
                {
                    structure.Name = gameObject.name;    
                }

                labelText.text = structure.Name;
            }
            
            SetLabelColour(Color.white);
            SetBackgroundColour(Color.black);

            if (structure != null && structure.Command != null)
            {
                captain = structure.Command.rankData.abbreviation + " " + structure.Command.label;
            }
            
            gameObject.name = "Name Label";
        }

        public void startCyclingNamesCoroutine()
        {
            cycleNamesCoroutine = StartCoroutine(cycleNames());
        }

        public void stopCyclingNamesCoroutine()
        {
            if (cycleNamesCoroutine != null)
            {
                StopCoroutine(cycleNamesCoroutine);
            }

            cycleNamesCoroutine = null;
        }

        protected IEnumerator cycleNames()        
        {
            while (true)
            {
                if (captain != null)
                {
                    labelText.text = captain;

                    yield return new WaitForSeconds(cycleNamesDelay);

                    labelText.text = structure.Name;

                    yield return new WaitForSeconds(cycleNamesDelay);
                }
                else
                {
                    break;
                }
            }
        }
    }
}
