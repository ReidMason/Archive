using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Fittings.Modules;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Utilities;

// TODO - will need something for multiple ships attempting to dock/undock simultaneoulsy

namespace NoxCore.Fittings.Sockets
{
    public enum DockState { UNDOCKED, DOCKING, DOCKINGPAUSED, DOCKINGABORTED, DOCKED, UNDOCKING }

	public class DockingPortEventArgs : EventArgs
	{
		public IDockable port;
		public Structure portStructure;
		public Ship ship;
		
		public DockingPortEventArgs(IDockable port, Structure portStructure, Ship ship)
		{
			this.port = port;
			this.portStructure = portStructure;
			this.ship = ship;
		}
	}
	
	public class DockingPort : StructureSocket, IDockable
	{
        protected class SortingInfo
        {
            public string layerName;
            public int sortOrder;

            public SortingInfo(string layerName, int sortOrder)
            {
                this.layerName = layerName;
                this.sortOrder = sortOrder;
            }
        }

        public StructureSize minDockingSize, maxDockingSize;
		public Vector2 approachVector;
		public float maxDockingSpeed;
		public float tetherDistance;

        protected Dictionary<Structure, SortingInfo> dockingSortInfo;
		
		[Range (0, 180)]
		public float approachArc;		
		public float ApproachArc { get { return approachArc; } }

        public StructureSize MinDockingSize { get => minDockingSize; set => minDockingSize = value; }
        public StructureSize MaxDockingSize { get => maxDockingSize; set => maxDockingSize = value; }

        protected Ship shipAtPort;

        protected List<Ship> dockedShips;

        public DockState dockState;
        public DockState DockState { get { return dockState; } set { dockState = value; } }

        public float dockingTime;
        public float undockingTime;

        [SerializeField]
        [ShowOnly]
		protected float clampTimer;
		
		public override void init()
		{
            base.init();

            dockingSortInfo = new Dictionary<Structure, SortingInfo>();

			// make sure the approach vector is a unit vector
			approachVector = approachVector.normalized;
			
			approachArc = approachArc * 2.0f;

            dockedShips = new List<Ship>();

            reset();
		}

        public override void reset()
        {
            base.reset();

            DockState = DockState.UNDOCKED;
            clampTimer = dockingTime;

            shipAtPort = null;

            dockedShips.Clear();
        }

        public override StructureSocketInfo getSocketInfo()
        {
            StructureSocketInfo socketInfo = base.getSocketInfo();

            DockingSocketInfo dockingSocketInfo = DockingSocketInfo.CopyToDockingSocketInfo(socketInfo);

            dockingSocketInfo.minDockingSize = minDockingSize;
            dockingSocketInfo.maxDockingSize = maxDockingSize;
            dockingSocketInfo.approachVector = approachVector;
            dockingSocketInfo.maxDockingSpeed = maxDockingSpeed;
            dockingSocketInfo.tetherDistance = tetherDistance;
            dockingSocketInfo.approachArc = ApproachArc;
            dockingSocketInfo.dockingTime = dockingTime;
            dockingSocketInfo.undockingTime = undockingTime;            

            return dockingSocketInfo;
        }

        public List<Ship> getDockedShips()
        {
            return dockedShips;
        }

        public bool isPortFree()
        {
            if (DockState == DockState.UNDOCKED) return true;
            else return false;
        }

		public float getClampTimer()
		{
			return clampTimer;
		}

        public void engageDockingClamp()
        {
            if (shipAtPort == null)
            {
                DockState = DockState.UNDOCKED;

                clampTimer = dockingTime;
            }
            else
            {
                if (DockState == DockState.DOCKING)
                {
                    DockState = DockState.DOCKED;

                    shipAtPort.transform.parent = transform;

                    dockedShips.Add(shipAtPort);

                    shipAtPort.TakenAnyDamage -= UnderAttack;

                    clampTimer = dockingTime;

                    DockingPortEventArgs args = new DockingPortEventArgs(this, parentStructure, shipAtPort);

                    shipAtPort.Call_DockInitiatorDocked(this, args);
                    parentStructure.Call_DockReceiverDocked(this, args);
                }
            }
        }

        public void releaseDockingClamp()
        {
            if (DockState == DockState.DOCKED)
            {
                DockState = DockState.UNDOCKING;

                SortingInfo origSortingInfo;

                dockingSortInfo.TryGetValue(shipAtPort, out origSortingInfo);

                if (origSortingInfo != null)
                {
                    shipAtPort.setSortingLayerOrder(origSortingInfo.layerName, origSortingInfo.sortOrder);

                    dockingSortInfo.Remove(shipAtPort);
                }

                dockedShips.Remove(shipAtPort);

                clampTimer = undockingTime;

                GameManager.Instance.Gamemode.Gui.setMessage(shipAtPort.Name + " is undocking from " + gameObject.name + " on " + parentStructure.Name);
            }
        }

        public (bool requestGranted, string message) requestDocking(Ship ship)
		{
            if (DockState != DockState.UNDOCKED)
            {
                if (shipAtPort != null)
                {
                    return (false, "Docking port " + gameObject.name + " on " + parentStructure.Name + " is currently engaged with " + shipAtPort.Name);
                }
                else
                {
                    return (false, "Docking port " + gameObject.name + " on " + parentStructure.Name + " is currently engaged with another ship");
                }
            }

            // is the docking port at capacity
			if (shipAtPort != null)
			{
				return (false, "No free mooring slot for " + ship.Name + " at " + gameObject.name + " on " + parentStructure.Name);
			}

            // is the arrival speed too high?
            if (ship.Speed > maxDockingSpeed)
            {
                return (false, "Approach velocity for " + ship.Name + " is too high to dock at " + gameObject.name + " on " + transform.parent.gameObject.name);
            }

            // is the docking structure too small or too big?
            if (ship.structureSize < minDockingSize || ship.structureSize > maxDockingSize)
            {
                if (ship.structureSize < minDockingSize) return (false, "Structure size for " + ship.Name + " is too small to dock at " + gameObject.name + " on " + transform.parent.gameObject.name);
                else return (false, "Structure size for " + ship.Name + " is too large to dock at " + gameObject.name + " on " + transform.parent.gameObject.name);
            }
						
            if (Vector2.Distance(ship.transform.position, transform.position) > tetherDistance)
            {
                return (false, ship.Name + " is not in range of " + gameObject.name + " on " + transform.parent.gameObject.name);
            }

            if (Mathf.Acos(Vector3.Dot(ship.transform.forward, approachVector)) * Mathf.Rad2Deg > approachArc)
            {
                return (false, "Approach vector for " + ship.Name + " is not valid for " + gameObject.name + " on " + transform.parent.gameObject.name);
            }

            DockState = DockState.DOCKING;
            shipAtPort = ship;

            if (dockingSortInfo.ContainsKey(ship))
            {
                dockingSortInfo.Remove(ship);
            }

            dockingSortInfo.Add(ship, new SortingInfo(shipAtPort.StructureRenderer.sortingLayerName, shipAtPort.StructureRenderer.sortingOrder));

            ship.setSortingLayerOrder(socketRenderer.sortingLayerName, socketRenderer.sortingOrder + 1);

            ship.TakenAnyDamage += UnderAttack;

            return (true, ship.Name + " is docking at " + gameObject.name + " on " + parentStructure.Name);
		}
		
		public bool requestUndocking(Ship ship)
		{
			if (shipAtPort == ship && (DockState == DockState.DOCKED || DockState == DockState.DOCKING))
			{
                if (DockState == DockState.DOCKED)
                {
                    DockState = DockState.UNDOCKING;
                    releaseDockingClamp();
                }
                else
                {
                    DockState = DockState.DOCKINGABORTED;
                    clampTimer = undockingTime - clampTimer;
                }

                SortingInfo origSortingInfo;

                dockingSortInfo.TryGetValue(ship, out origSortingInfo);

                if (origSortingInfo != null)
                {
                    ship.setSortingLayerOrder(origSortingInfo.layerName, origSortingInfo.sortOrder);

                    dockingSortInfo.Remove(ship);
                }

                return true;
            }
			
            return false;
		}
		

        public Ship getShipAtPort()
        {
            return shipAtPort;
        }

        public bool isShipDocking(Ship ship)
        {
            if (shipAtPort == ship && DockState == DockState.DOCKING) return true;
            else return false;
        }

        public bool isShipUndocking(Ship ship)
        {
            if (shipAtPort == ship && DockState == DockState.UNDOCKING) return true;
            else return false;
        }

        public bool isShipDocked(Ship ship)
		{
			if (shipAtPort == ship && DockState == DockState.DOCKED) return true;
			else return false;
		}

        public bool isShipAtPort(Ship ship)
        {
            if (shipAtPort == ship) return true;
            else return false;
        }
	
        protected IEnumerator dockInterrupted(float delay)
        {
            GameManager.Instance.Gamemode.Gui.setMessage("Dock interrupted due to enemy fire for: " + delay + "seconds");

            DockState = DockState.DOCKINGPAUSED;          

            yield return new WaitForSeconds(delay);

            DockState = DockState.DOCKING;
        }
		
		public override void update()
		{
            if (shipAtPort != null && shipAtPort.Destroyed == true) reset();

			if (DockState == DockState.DOCKING)
			{
				clampTimer -= Time.deltaTime;
				
				if (clampTimer < 0)
				{
                    engageDockingClamp();                    
                }
			}
			else if (DockState == DockState.UNDOCKING || DockState == DockState.DOCKINGABORTED)
			{
				clampTimer -= Time.deltaTime;
				
				if (clampTimer < 0)
				{
                    DockState = DockState.UNDOCKED;

                    clampTimer = dockingTime;

                    // allow hot start of docked ship
                    foreach (IEngine engine in shipAtPort.engines)
                    {
                        if (engine.isActiveOn() == false)
                        {
                            engine.setActiveOn(true);
                        }
                    }

                    DockingPortEventArgs args = new DockingPortEventArgs(this, parentStructure, shipAtPort);

                    GameObject hierarchy = GameObject.Find("Placeables");

                    shipAtPort.transform.parent = hierarchy.transform;

                    parentStructure.Call_DockReceiverUndocked(this, args);

                    shipAtPort.Call_DockInitiatorUndocked(this, args);

                    shipAtPort = null;
                }
            }
		}

        protected void UnderAttack(object sender, DamageEventArgs args)
        {
            if (args.damage > 0 && DockState == DockState.DOCKING)
            {
                StartCoroutine(dockInterrupted(args.damage * 100 / args.damagedStructure.MaxHullStrength));
            }
        }
    }
}