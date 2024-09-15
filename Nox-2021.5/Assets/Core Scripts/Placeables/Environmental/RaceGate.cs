using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System;

using NoxCore.Controllers;
using NoxCore.GameModes;
using NoxCore.Managers;

namespace NoxCore.Placeables
{
    public class RacerEventArgs : EventArgs
    {
        public Structure structure;
        public RaceGate curGate;
        public RaceGate nextGate;

        public RacerEventArgs(Structure structure, RaceGate curGate, RaceGate nextGate)
        {
            this.structure = structure;
            this.curGate = curGate;
            this.nextGate = nextGate;
        }
    }

    public class RaceGate : MonoBehaviour 
	{
        public delegate void RaceEventDispatcher(object sender, RacerEventArgs args);
        public event RaceEventDispatcher NotifyRacer;

        protected int maxGates;
		protected int gateID;
        public int GateID { get { return gateID; } }
		protected Transform bouy1, bouy2;
		protected BoxCollider2D trigger;
		protected bool [] triggerable;

        public void init() 
		{
			GameEventManager.MatchIsWaitingToStart += GameEventManager_MatchIsWaitingToStart;
			
			bouy1 = transform.Find ("Race Gate Buoy1");
			bouy2 = transform.Find ("Race Gate Buoy2");
			
			trigger = gameObject.AddComponent<BoxCollider2D>();
			trigger.enabled = false;
			trigger.isTrigger = true;
			trigger.size = new Vector2(Vector2.Distance(bouy1.position, bouy2.position) / transform.localScale.x, 12.5f / transform.localScale.y);		
			trigger.transform.rotation = transform.rotation;
			
			maxGates = GameObject.FindGameObjectsWithTag("NavPoint").Length;
			
			gateID = getGateID(trigger.name).GetValueOrDefault();		
		}
		
		protected int? getGateID(string name)
		{
			int number;
			
			var match = Regex.Match(name, @"(\d+)$");
			
			if (match.Success)
			{
				number = int.Parse(match.Groups[1].ToString());
				return number;
			}
			else
			{
				return null;
			}
		}	
		
		protected void setTriggerableGate(int agentID, bool canBeTriggered)
		{
			triggerable[agentID] = canBeTriggered;
		}
		
		protected RaceGate updateGateTrigger(int agentID)
		{
            GameObject nextGateGO;
            RaceGate nextGate = null;

			// set this gate to not trigger for this agentID
			setTriggerableGate(agentID, false);
			
			// get next gate ID in sequence
			int nextGateID = gateID+1;
			if (nextGateID > maxGates) nextGateID = 1;

            if (nextGateID > 9)
            {
                nextGateGO = GameObject.Find("NavPoint" + nextGateID);
            }
            else
            {
                nextGateGO = GameObject.Find("NavPoint0" + nextGateID);
            }

            if (nextGateGO != null)
            {
                nextGate = nextGateGO.GetComponent<RaceGate>();
                nextGate.setTriggerableGate(agentID, true);
            }
			else
			{
				Debug.LogError("Could not get next race gate");
			}

            return nextGate;
        }

        void OnTriggerEnter2D(Collider2D other)
		{
			if (other.tag == "Ship")
			{
				Structure structure = other.GetComponent<Structure>();

				int agentID = GameManager.Instance.Gamemode.getAgentID(structure);
				
				if (agentID > -1)
				{
					if (triggerable[agentID] == true)
					{
						structure.Controller.Helm.destination = null;
						
						RaceGate nextGate = updateGateTrigger(agentID);

                        Call_NotifyRacer(this, new RacerEventArgs(structure, this, nextGate));
					}
				}
			}
		}
		
		protected virtual void GameEventManager_MatchIsWaitingToStart(object sender)
		{
			int numRacers = RaceMode.getNumRacers();
			
			triggerable = new bool[numRacers];
			
			// set the second gate as triggerable by all racers
			if (trigger.name == "NavPoint02")
			{
				for(int i = 0; i < numRacers; i++)
				{
					setTriggerableGate(i, true);
				}
			}	
			
			trigger.enabled = true;	
		}

        public void Call_NotifyRacer(object sender, RacerEventArgs args)
        {
            if (NotifyRacer != null)
            {
                NotifyRacer(sender, args);
            }
        }
    }
}