using UnityEngine;
using System.Collections.Generic;

using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;

namespace NoxCore.Fittings.Sockets
{
    public class HangarEventArgs : EventArgs
    {
        public IHangar hangar;
        public Structure hangarStructure;
        public Ship ship;

        public HangarEventArgs(IHangar hangar, Structure hangarStructure, Ship ship)
        {
            this.hangar = hangar;
            this.hangarStructure = hangarStructure;
            this.ship = ship;
        }
    }

    public class Hangar : StructureSocket, IHangar
    {
        public StructureSize minLandingBaySize, maxLandingBaySize;
        public Vector2 approachVector;
        public float maxLandingSpeed;
        public int maxLandingBays;
        
        [Range(0, 180)]
        public float approachArc;
        private float _ApproachArc;
        public float ApproachArc { get { return _ApproachArc; } }

        protected List<Ship> shipsInHangar;

        public override void init()
        {
            base.init();

            // make sure the approach vector is a unit vector
            approachVector = approachVector.normalized;

            _ApproachArc = approachArc * 2.0f;

            shipsInHangar = new List<Ship>();

            reset();
        }

        public override void reset()
        {
            base.reset();

            // TODO - check for the right management approach to this as maybe you do want o clear the hangar in some circumstances?
            //shipsInHangar.Clear();
        }

        public Transform getTransform()
        {
            return transform;
        }

        public Vector2 getApproachVector()
        {
            return approachVector;
        }

        public float getMaxLandingSpeed()
        {
            return maxLandingSpeed;
        }

        public void addShip(Ship ship)
        {
            // TODO - need to only add if hangar slot available (note: respawning ships to a destroyed hangar structure could cause problems if hangar is full)
            shipsInHangar.Add(ship);
        }

        public bool removeShip(Ship ship)
        {
            return shipsInHangar.Remove(ship);
        }

        public List<Ship> getShipsInHangar()
        {
            return shipsInHangar;
        }        

        public void land(Ship ship)
        {
            GameManager.Instance.Gamemode.Gui.setMessage(ship.Name + " has landed in hangar on " + parentStructure.Name);

            shipsInHangar.Add(ship);

            ship.gameObject.SetActive(false);

            ship.transform.parent = transform;

            HangarEventArgs args = new HangarEventArgs(this, parentStructure, ship);

            ship.Call_LandingInitiatorLanded(this, args);
            parentStructure.Call_LandingReceiverLanded(this, args);
        }

        protected void launch(Ship ship)
        {
            GameManager.Instance.Gamemode.Gui.setMessage(ship.Name + " is launching from hangar on " + parentStructure.Name);

            shipsInHangar.Remove(ship);

            ship.transform.position = transform.position;

            ship.gameObject.SetActive(true);

            ship.Call_Respawn(this, new RespawnEventArgs(ship));

            HangarEventArgs args = new HangarEventArgs(this, parentStructure, ship);

            ship.Call_LaunchInitiatorLaunched(this, args);
            parentStructure.Call_LaunchReceiverLaunched(this, args);
        }

        public bool requestLaunch(Ship ship)
        {
            if (parentStructure.Destroyed == true) return false;

            if (shipsInHangar.Contains(ship))
            {
                launch(ship);
                return true;
            }

            return false;
        }

        public bool emergencyLaunch(Ship ship)
        {
            if (shipsInHangar.Contains(ship))
            {
                launch(ship);
                return true;
            }

            return false;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Ship")) return;

            if (shipsInHangar.Count < maxLandingBays)
            { 
                Ship ship = other.GetComponent<Structure>() as Ship;

                if (ship.shipState != ShipState.LANDING) return;

                // NOTE: may not need this null check as should be a ship by this stage
                if (ship != null && ship.structureSize >= minLandingBaySize && ship.structureSize <= maxLandingBaySize)
                {
                    if (ship.Speed <= maxLandingSpeed)
                    {
                        if (Mathf.Acos(Vector3.Dot(ship.transform.up, approachVector)) * Mathf.Rad2Deg > approachArc)
                        {
                            if (ship.shipState == ShipState.LANDING)
                            {
                                land(ship);
                            }
                        }
                        else
                        {
                            GameManager.Instance.Gamemode.Gui.setMessage(other.name + " is approaching from the wrong direction to land at the hangar on " + parentStructure.Name);
                        }
                    }
                    else
                    {
                        GameManager.Instance.Gamemode.Gui.setMessage(other.name + " is approaching too fast to land in the hangar on " + parentStructure.Name);
                    }
                }
                else
                {
                    GameManager.Instance.Gamemode.Gui.setMessage(other.name + " is the wrong size to land in the hangar on " + parentStructure.Name);
                }
            }
            else
            {
                GameManager.Instance.Gamemode.Gui.setMessage(other.name + " cannot land as there is no free bay in the hangar on " + parentStructure.Name);
            }
        }
    }
}