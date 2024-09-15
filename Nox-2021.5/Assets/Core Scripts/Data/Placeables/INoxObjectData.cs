using NoxCore.GUIs;
using UnityEngine;

namespace NoxCore.Data.Placeables
{
    public interface INoxObjectData
    {
        string Name { get; set; }
        FactionData Faction { get; set; }
        FactionLabel FactionLabel { get; set; }
        NameLabel NameLabel { get; set; }
    }
}