using UnityEngine;
using UnityEngine.UI;

namespace NoxCore.GUIs
{
    public class PositionMarker : MonoBehaviour
    {
        public bool showX = true, showY = true;
        public TMPro.TextAlignmentOptions alignment = TMPro.TextAlignmentOptions.TopLeft;
        public Color colour = Color.white;

        protected TMPro.TextMeshProUGUI textMesh;

        void Start()
        {
            textMesh = GetComponent<TMPro.TextMeshProUGUI>();

            if (textMesh)
            {
                string positionText = null;

                if (showX == true)
                {
                    positionText = ((int)transform.parent.transform.parent.transform.position.x).ToString();

                    if (showY == true)
                    {
                        positionText += (", " + ((int)transform.parent.transform.parent.transform.position.y).ToString());
                    }
                }
                else if (showY == true)
                {
                    positionText = ((int)transform.parent.transform.parent.transform.position.y).ToString();
                }
                
                textMesh.text = positionText;

                textMesh.alignment = alignment;
                textMesh.color = colour;
            }
        }
    }
}