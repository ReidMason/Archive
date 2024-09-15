using UnityEngine;

using System.Linq;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Controllers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;
using NoxCore.Utilities;

namespace NoxCore.Helm
{
    public class HelmController : MonoBehaviour
    {
        protected StructureController _controller;
        public StructureController Controller { get { return _controller; } set { _controller = value; } }

        protected Ship _ShipStructure;
        public Ship ShipStructure { get { return _ShipStructure; } set { _ShipStructure = value; } }

        protected Collider2D _ShipCollider;
        public Collider2D ShipCollider { get { return _ShipCollider; } set { _ShipCollider = value; } }

        protected Rigidbody2D _ShipRigidbody;
        public Rigidbody2D ShipRigidbody { get { return _ShipRigidbody; } set { _ShipRigidbody = value; } }

        protected List<SteeringBehaviour> behaviours;

        [Header("Force Stats")]
        [ShowOnly]
        public bool outOfForce;

        [ShowOnly]
        public bool didHitMaxForce;

        [ShowOnly]
        public float totalForceRequested, totalForceActual, maxForceRequested, maxForceOvershoot;

        [Header ("Navigation")]
        [SerializeField]
        protected Vector2 _Position;        
		public Vector2 Position { get { return _Position; } set { _Position = value; } }

        public Vector2? destination;
        public Vector2 Destination { get { return destination.GetValueOrDefault(); } set { destination = value; if (destination == null) { RangeToDestination = 0; } } }

        [SerializeField]
        protected float rangeToDestination;
        public float RangeToDestination { get { return rangeToDestination; } set { rangeToDestination = value; } }

        public float rangeToKeep;
		
        [Header ("Throttle")]
		[SerializeField]
		[Range(0, 1)]
        [Tooltip("This setting applies to all engines on the ship.")]
        protected float _desiredThrottle = 1.0f;
		public float desiredThrottle { get { return _desiredThrottle; } set { _desiredThrottle = Mathf.Clamp01(value); } }
        
        [Range(0, 1)]
        protected float _throttle = 1.0f;
        public float throttle { get { return _throttle; } set { _throttle = value; }  }

        public void init()
		{
			//behaviours = new List<SteeringBehaviour>();

			behaviours = GetComponents<SteeringBehaviour>().Cast<SteeringBehaviour>().ToList();

			destination = null;

            if (Controller != null)
            {
				//ShipRigidbody.velocity = new Vector3(0, 0, Controller.startRotation.GetValueOrDefault()) * (ShipStructure.MaxSpeed * throttle);
				ShipRigidbody.velocity = new Vector3(0, 0, ShipStructure.transform.rotation.z) * (ShipStructure.MaxSpeed * throttle);

				D.log("Helm", "Helm on " + gameObject.name + " is ready");
            }
            else
            {
                D.warn("Controller: {0}", "No controller set for structure " + ShipStructure.Name);
            }
		}
		
		public void setHelmStructure(Ship ship)
		{
            if (ship != null)
            {
                ShipStructure = ship;
                Controller = ship.Controller;
                ShipCollider = ship.StructureCollider;
                ShipRigidbody = ship.StructureRigidbody;
            }
		}
		
		public List<SteeringBehaviour> getAllBehaviours()
		{
			return behaviours;
		}
		
		public SteeringBehaviour getBehaviourByName(string name)
		{
			for(int i = 0; i < behaviours.Count; i++)
			{
				if (behaviours[i].Label == name)
				{
					return behaviours[i];
				}
			}		
			
			// TODO - get the delegate version working
			/*		
			var behaviour = behaviours.Find(delegate (SteeringBehaviour sb) 
			{
				return sb.title.Equals(name);
			});
*/			
			return null;
		}
		
		public SteeringBehaviour getBehaviourByID(int sequenceID)
		{
			for(int i = 0; i < behaviours.Count; i++)
			{
				if (behaviours[i].SequenceID == sequenceID)
				{
					return behaviours[i];
				}
			}
			/*
			var behaviour = behaviours.Find(delegate (SteeringBehaviour sb) 
			{
				return sb.sequenceID == sequenceID;
			});
*/			
			return null;
		}
		
		public void addBehaviour(SteeringBehaviour behaviour)
		{
			behaviours.Add(behaviour);
			// D.log ("Helm", "Added " + behaviour.title + " to the helm");
		}
		
		// enable/disable all
		public void enableAllBehaviours()
		{
			for (int i = 0; i < behaviours.Count; i++)
			{
				behaviours[i].enable();
			}
		}		
		
		public void disableAllBehaviours()
		{
			for (int i = 0; i < behaviours.Count; i++)
			{
				behaviours[i].disable();
			}
		}
		
		// enable/disable by name
		public SteeringBehaviour enableBehaviourByName(string name)
		{
			SteeringBehaviour behaviour = getBehaviourByName(name);
			
			if (behaviour != null)
			{
				behaviour.enable();
				return behaviour;
			}
			
			return null;
		}
		
		public SteeringBehaviour disableBehaviourByName(string name)
		{
			SteeringBehaviour behaviour = getBehaviourByName(name);
			
			if (behaviour != null)
			{
				behaviour.disable();
				return behaviour;
			}
			
			return null;
		}
		
		// enable/disable by ID
		public SteeringBehaviour enableBehaviourByID(int sequenceID)
		{
			SteeringBehaviour behaviour = getBehaviourByID(sequenceID);
			
			if (behaviour != null)
			{
				behaviour.enable();
				return behaviour;
			}
			
			return null;
		}
		
		public SteeringBehaviour disableBehaviourByID(int sequenceID)
		{
			SteeringBehaviour behaviour = getBehaviourByID(sequenceID);
			
			if (behaviour != null)
			{
				behaviour.disable();
				return behaviour;
			}
			
			return null;
		}
		
		// enable/disable with names
		public List<SteeringBehaviour> enableBehavioursWithNames(string [] names)
		{
			List<SteeringBehaviour> modifiedBehaviours = new List<SteeringBehaviour>();
			
			for (int i = 0; i < names.Length; i++)
			{
				SteeringBehaviour sb = getBehaviourByName(names[i]);
				
				if (sb != null)
				{
					sb.enable();
					modifiedBehaviours.Add(sb);
				}
			}
			
			return modifiedBehaviours;
		}
		
		public List<SteeringBehaviour> disableBehavioursWithNames(string [] names)
		{
			List<SteeringBehaviour> modifiedBehaviours = new List<SteeringBehaviour>();
			
			for (int i = 0; i < names.Length; i++)
			{
				SteeringBehaviour sb = getBehaviourByName(names[i]);
				
				if (sb != null)
				{
					sb.disable();
					modifiedBehaviours.Add(sb);
				}
			}
			
			return modifiedBehaviours;
		}
		
		// enable/disable with IDs
		public List<SteeringBehaviour> enableBehavioursWithIDs(int [] sequenceIDs)
		{
			List<SteeringBehaviour> modifiedBehaviours = new List<SteeringBehaviour>();
			
			for (int i = 0; i < sequenceIDs.Length; i++)
			{
				SteeringBehaviour sb = getBehaviourByID(sequenceIDs[i]);
				
				if (sb != null)
				{
					sb.enable();
					modifiedBehaviours.Add(sb);
				}
			}
			
			return modifiedBehaviours;
		}
		
		public List<SteeringBehaviour> disableBehavioursWithIDs(int [] sequenceIDs)
		{
			List<SteeringBehaviour> modifiedBehaviours = new List<SteeringBehaviour>();
			
			for (int i = 0; i < sequenceIDs.Length; i++)
			{
				SteeringBehaviour sb = getBehaviourByID(sequenceIDs[i]);
				
				if (sb != null)
				{
					sb.disable();
					modifiedBehaviours.Add(sb);
				}
			}
			
			return modifiedBehaviours;
		}		
		
		public void resetSteeringWeights()
		{
			for(int i = 0; i < behaviours.Count; i++)
			{
				behaviours[i].resetWeight();
			}
		}
				
		protected Vector2 accumulateForce(Vector2 runningTotalForce, Vector2 forceToAdd, float remainingMagnitude)
		{
			// if magnitude of the sum of the current magnitude and the force to be added does not exceed maximum force, just add it
			if (forceToAdd.magnitude <= remainingMagnitude)
			{
				runningTotalForce += forceToAdd;
			}
			else
			{
				Vector2 normalisedForce = forceToAdd.normalized;
				normalisedForce *= remainingMagnitude;
				runningTotalForce += normalisedForce;
			}
			
			return runningTotalForce;
		}
		
		public Vector2 calculateSteeringForces()
		{
			// reset forces
			Vector2 steeringForce = Vector2.zero;
			Vector2 force = Vector2.zero;

            outOfForce = false;
            totalForceRequested = 0;
            totalForceActual = 0;

            // calculate weighted truncated running sum with prioritisation on active steering behaviours
            for (int i = 0; i < behaviours.Count; i++)
			{
				SteeringBehaviour behaviour = behaviours[i];
				
				// D.log ("Helm", "Checking steering behaviour: " + behaviour.title);
				
				if (behaviour.Active == true)
				{
					// D.log ("Helm", "Behaviour: " + behaviour.title + " is active");
					
					float remainingMagnitude = ShipStructure.ShipData.MaxForce - steeringForce.magnitude;
					
					// if the ship has reached its maximum force, stop calculating behaviours
					if (remainingMagnitude <= 0)
					{
                        // D.log ("Helm", "Ship has reached it's maximum steering force");
                        outOfForce = true;
						break;
					}
					
					// perform the behaviour calculation to obtain its steering force
					force = behaviour.execute();
					
					// D.log ("Helm", "Ship force: " + force);
					
					// multiply the force by the steering behaviour weight
					if (force.magnitude > 0)
					{
						force *= behaviour.Weight;

                        behaviour.currentForceRequested = force.magnitude;

                        if (behaviour.currentForceRequested > behaviour.maxForceRequested)
                        {
                            behaviour.maxForceRequested = behaviour.currentForceRequested;
                        }

                        totalForceRequested += behaviour.currentForceRequested;
						
						// accumulate as much of the force as possible
						steeringForce = accumulateForce(steeringForce, force, remainingMagnitude);

                        behaviour.currentForceActual = steeringForce.magnitude;
                        totalForceActual += behaviour.currentForceActual;
                    }
                    else
                    {
                        behaviour.currentForceRequested = 0;
                        behaviour.currentForceActual = 0;
                    }
				}
            }

            if (totalForceRequested > maxForceRequested)
            {
                maxForceRequested = totalForceRequested;

                if (totalForceRequested > ShipStructure.MaxForce)
                {
                    didHitMaxForce = true;

                    maxForceOvershoot = totalForceRequested - ShipStructure.MaxForce;
                }
            }
			
			return steeringForce;
		}
		
		void LateUpdate()
		{
			Position = transform.position;

            // note: this was supposed to exist so it was easy to see the destination of the ship whilst looking in the inspector
            // TODO - needs to be replaced so doesn't set the destination to (0,0) when it should stay null
            //Destination = destination.GetValueOrDefault();
		}
	}
}
