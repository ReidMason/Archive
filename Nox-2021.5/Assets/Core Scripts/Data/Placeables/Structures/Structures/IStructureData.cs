using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NoxCore.Data.Placeables
{
    public interface IStructureData : INoxObject2DData
    {
        uint HullCost { get; set; }
        float Mass { get; set; }
        float AspectRadius { get; set; }
        int MaxFireGroups { get; set; }
        uint MaxDevices { get; set; }
        uint MaxModules { get; set; }
        uint NumShieldSockets { get; set; }
        uint NumMixedSockets { get; set; }
        uint MaxWeapons { get; set; }
        uint NumLaunchers { get; set; }
        uint NumTurrets { get; set; }
        uint NumEmitters { get; set; }
        uint NumMixedLaunchersAndTurrets { get; set; }
        uint NumMixedGenericWeapons { get; set; }
        uint MaxDockingPorts { get; set; }
        uint MaxHangars { get; set; }
    }
}