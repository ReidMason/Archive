using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System;

using NoxCore.Managers;
using NoxCore.Placeables.Ships;

namespace NoxCore.Placeables
{
    public class NavEventArgs : EventArgs
    {
        public Ship ship;
        public NavBuoy curBuoy;
        public NavBuoy nextBuoy;

        public NavEventArgs(Ship ship, NavBuoy curBuoy, NavBuoy nextBuoy)
        {
            this.ship = ship;
            this.curBuoy = curBuoy;
            this.nextBuoy = nextBuoy;
        }
    }

    public class NavBuoy : MonoBehaviour
    {
        public delegate void NavEventDispatcher(object sender, NavEventArgs args);
        public event NavEventDispatcher NotifyShip;

        protected string buoyPrefix;
        public string BuoyPrefix { get { return buoyPrefix; } }
        protected int buoyID;
        public int BuoyID { get { return buoyID; } }
        public float radius;
        protected CircleCollider2D trigger;
        protected List<int> triggerable = new List<int>();

        public void init()
        {
            buoyPrefix = getBuoyPrefix(gameObject.name);
            buoyID = getBuoyID(gameObject.name).GetValueOrDefault();

            trigger = gameObject.AddComponent<CircleCollider2D>();
            trigger.enabled = true;
            trigger.isTrigger = true;
            trigger.radius = radius;
            trigger.transform.rotation = transform.rotation;
        }

        protected string getBuoyPrefix(string name)
        {
            return Regex.Match(name, @"^[^0-9]*").Value;
        }

        protected int? getBuoyID(string name)
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

        protected void setTriggerableBuoy(int agentID, bool canBeTriggered)
        {
            if (canBeTriggered == true)
            {
                triggerable.Add(agentID);
            }
            else
            {
                triggerable.Remove(agentID);
            }
        }

        protected NavBuoy updateBuoyTrigger(int agentID)
        {
            GameObject nextBuoyGO = null;
            NavBuoy nextBuoy = null;

            setTriggerableBuoy(agentID, false);

            // get next buoy ID in sequence
            int nextBuoyID = buoyID + 1;

            if (nextBuoyID > 9)
            {
                nextBuoyGO = GameObject.Find("NavPoint" + nextBuoyID);
            }
            else if(nextBuoyID > 9)
            {
                nextBuoyGO = GameObject.Find("NavPoint0" + nextBuoyID);
            }

            if (nextBuoyGO != null)
            {
                nextBuoy = nextBuoyGO.GetComponent<NavBuoy>();
                nextBuoy.setTriggerableBuoy(agentID, true);
            }

            return nextBuoy;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Ship")
            {
                int agentID = GameManager.Instance.Gamemode.getAgentID(other.GetComponent<Structure>());

                if (agentID > -1 && triggerable.Contains(agentID))
                {
                    Ship ship = other.GetComponent<Structure>() as Ship;

                    if (ship != null)
                    {
                        ship.Controller.Helm.destination = null;

                        NavBuoy nextBuoy = updateBuoyTrigger(agentID);

                        Call_NotifyShip(this, new NavEventArgs(ship, this, nextBuoy));
                    }
                }
            }
        }

        public void Call_NotifyShip(object sender, NavEventArgs args)
        {
            if (NotifyShip != null)
            {
                NotifyShip(sender, args);
            }
        }
    }
}