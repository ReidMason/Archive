using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text;

using NoxCore.Placeables;

namespace NoxCore.GUIs
{
    public class StructureLabel : MonoBehaviour
    {
        protected TextMesh labelText;
        public TextMesh LabelText {  get { return labelText; } set { labelText = value; } }

        protected Renderer labelRenderer;
        public Structure structure;
        public int labelScale = 1;
        public Vector3 labelOffset;
        protected Color labelColour;
        protected Color backgroundColour;

        public virtual void init()
        {
            labelText = gameObject.GetComponent<TextMesh>();

            if (labelText != null)
            {
                labelRenderer = labelText.GetComponent<Renderer>();
            }
            
            structure = gameObject.GetComponentInParent<Structure>();

            if (structure != null)
            {
                structure.NotifyKilled += Structure_NotifyKilled;
            }

            Reset();                    
        }

        void OnEnable()
        {
            labelRenderer = GetComponent<MeshRenderer>();

            if (labelRenderer != null)
            {
                init();
            }
        }

        public virtual void Reset()
        {
            transform.localScale = new Vector2(labelScale, labelScale);
            transform.position = transform.position + labelOffset;
        }

        private void Structure_NotifyKilled(object sender, TargetDestroyedEventArgs args)
        {
            Fade();
        }

        public void Fade()
        {
            labelColour.a *= 0.9975f;
            backgroundColour.a *= 0.9975f;
            SetLabelColour(labelColour);
            SetBackgroundColour(backgroundColour);
        }

        public void SetLabelColour(Color newColour)
        {
            labelColour = newColour;
            labelRenderer.material.SetColor("_Color", labelColour);
        }

        public void SetBackgroundColour(Color newColour)
        {
            backgroundColour = newColour;
            labelRenderer.material.SetColor("_BackgroundColor", backgroundColour);
        }
        
        public void ShowLabel(bool display)
        {
            labelRenderer.enabled = display;
        }
    }
}
