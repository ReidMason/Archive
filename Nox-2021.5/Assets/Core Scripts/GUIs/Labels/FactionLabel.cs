using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using NoxCore.Placeables;

namespace NoxCore.GUIs
{
    public class FactionLabel : StructureLabel
    {

        public override void Reset()
        {
            base.Reset();

            if (labelText != null && structure != null && structure.Faction != null)
            {
                labelText.text = structure.Faction.label;
            }
            
            SetLabelColour(Color.white);
            SetBackgroundColour(Color.black);
            
            gameObject.name = "Faction Label";
        }
    }
}
