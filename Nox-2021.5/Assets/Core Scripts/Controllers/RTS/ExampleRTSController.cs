using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using NoxCore.Cameras;
using NoxCore.Controllers;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Weapons;
using NoxCore.GUIs;
using NoxCore.Helm;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Utilities;

namespace NoxCore.Controllers
{
    public abstract class ExampleRTSController : Controller
    {
        [SerializeField]
        protected List<StructureController> _selectedControllers;
        public List<StructureController> selectedControllers { get { return _selectedControllers; } set { _selectedControllers = value; } }

        [SerializeField]
        protected Dictionary<int, List<StructureController>> _unitGroups;
        public  Dictionary<int, List<StructureController>> unitGroups
        {
            get { return _unitGroups; }
        }

        // methods to manage the unit groups (overwriting a group, selecting a group etc.)

        // methods to perform a marquee selection
    }
}