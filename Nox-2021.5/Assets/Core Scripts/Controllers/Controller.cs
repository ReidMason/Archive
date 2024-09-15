using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

using NoxCore.Cameras;
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
    public enum RunningState { INITIALISING, RUNNING, STOPPED };

    public abstract class Controller : MonoBehaviour
    {
        public RunningState runningState;

        // reference to camera
        protected TopDown_Camera _Cam;
        public TopDown_Camera Cam { get { return _Cam; } set { _Cam = value; } }

        // reference to GUI
        protected NoxGUI _Gui;
        public NoxGUI Gui { get { return _Gui; } set { _Gui = value; } }

        public virtual void init()
        {
            runningState = RunningState.INITIALISING;

            // set reference to camera
            Cam = GameManager.Instance.getMainCamera();

            // TODO - why does this not work when casting to a specific NoxGUI in an AI Controller???
            // set reference to GUI
            if (_Cam != null)
            {
                Gui = GameObject.Find("UI Manager").GetComponent<NoxGUI>();
            }
            else
            {
                Gui = GameManager.Instance.Gamemode.Gui;
            }
        }

        public virtual void reset() {}

        public virtual void start(float initialDelay = 0) {}

        public virtual void stop() {}
    }
}
