using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.IO;
using System.Text;

using NoxCore.Cameras;
using NoxCore.Controllers;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Fittings.Weapons;
using NoxCore.GameModes;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Utilities;

namespace NoxCore.GUIs
{
    public class TimedBoardingGUI : NoxGUI
    {
        protected BoardingMode boardingGameMode;

        protected float maxTimer;

        protected bool testCompleted;

        public Structure boardingTarget;
        protected DockingPort dockingPort;

        Text clock;
        Text docking;
        Text boarding;

        // Use this for initialization
        public override void init()
        {
            base.init();

            boardingGameMode = GameManager.Instance.Gamemode as BoardingMode;

            maxTimer = boardingGameMode.maxTime;

            GameObject timerGO = GameObject.Find("Mission Clock");

            if (timerGO != null)
            {
                clock = timerGO.GetComponent<Text>();
            }

            timerGO = GameObject.Find("Docking Clock");

            if (timerGO != null)
            {
                docking = timerGO.GetComponent<Text>();
            }

            timerGO = GameObject.Find("Boarding Clock");

            if (timerGO != null)
            {
                boarding = timerGO.GetComponent<Text>();
            }

            enabled = true;
        }

        public void setBoardingTarget(Structure target)
        {
            boardingTarget = target;
            // D.log("GUI", "Marked target set to: " + target.name);

            foreach (StructureSocket socket in boardingTarget.StructureSockets)
            {
                DockingPort dockingPortSocket = socket as DockingPort;

                if (dockingPortSocket != null)
                {
                    dockingPort = dockingPortSocket;

                    D.log("GameMode", "Docking port located on " + boardingTarget.Name);
                    break;
                }
            }
        } 

        protected override void OnGUI()
        {
            base.OnGUI();

            clock.text = timer.getTimeStr();
            docking.text = Timer.formatTimer(dockingPort.getClampTimer(), true);
            boarding.text = Timer.formatTimer(boardingGameMode.boardingTime - boardingGameMode.getBoardingTimer(), true);            
        }       
    }
}