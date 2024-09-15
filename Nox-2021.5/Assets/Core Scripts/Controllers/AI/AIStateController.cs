using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;

using NoxCore.Helm;
using NoxCore.Managers;
using NoxCore.Placeables;

namespace NoxCore.Controllers
{
	public class OrderMessageEventArgs : MessageEventArgs
	{
		public string newState;
		
		public OrderMessageEventArgs(Structure sender, string newState) : base(sender)
		{
			this.newState = newState;
		}
	}
	
	public abstract class AIStateController : AIController, IStateController
	{	
		public delegate string aiHandler();
		protected Dictionary<string, aiHandler> aiActions;
		
		protected string prevState;
		
		[SerializeField]
		protected string _state;
		public string state
		{
			get { return _state; } 
			
			set 
			{
				_state = value;
				
				if (_state != prevState)
				{
					if (_state != null)
					{
						if (prevState != null)
						{
							// D.log("Controller", "AI for " + structure.gameObject.name + " has changed state from " + prevState + " to " + _state);
						}
						else
						{
                            // D.log("Controller", "AI for " + structure.gameObject.name + " has an initial state of " + _state);
                        }
                    }
					else
					{
                        // D.log("Controller", "AI for " + structure.gameObject.name + " has a null state");
                    }

                    prevState = _state;
				}
			}
		}

		protected bool debugState;
		public bool DebugState { get { return debugState; } set { debugState = value; } }

		public override void boot(Structure structure, HelmController helm = null)
		{
			base.boot(structure, helm);
			
			// create the state table
			aiActions = new Dictionary<string, aiHandler>();

			DebugState = true;
		}

        public bool setState(string newState)
        {
            if (aiActions.ContainsKey(newState))
            {
                state = newState;
                return true;
            }

            return false;
        }

        public override void update()
		{
            if (booted == true)
            {
                processState();

				// put ai update method call in here (or call event)
            }
		}		
		
		public void processState()
		{
			if (state != null)
			{
				if (!aiActions.ContainsKey(state))
				{
                    // D.log("Controller", "AI for " + structure.gameObject.name + " has no state called " + state + " in its controller");

                    // no matching key
                    state = null;
					return;
				}
				
				aiHandler handler = aiActions[state];
				
				if (handler != null)
				{
                    // // D.log("Controller", "Processing handler for state: " + state);
                    state = handler();
				}
				else
				{
                    // D.log("Controller", "AI for " + structure.gameObject.name + " has no state handler for state called " + state + " in its controller");
                    state = null;
				}
			}
		}

        public override void start(float initialDelay = 0)
        {
            if (GameManager.Instance.Gamemode.randomAIStartDelay == true)
            {
                InvokeRepeating("update", initialDelay, AITickRate);
            }
            else
            {
                InvokeRepeating("update", 0, AITickRate);
            }

            runningState = RunningState.RUNNING;
        }

        public override void stop()
        {
            CancelInvoke();
            runningState = RunningState.STOPPED;
        }

#if UNITY_EDITOR
		void OnDrawGizmos()
        {
			if (DebugState == true)
			{
				Handles.Label(transform.position, state);
			}
        }
#endif
		void OnEnable()
        {
            if (runningState == RunningState.STOPPED)
            {
                start();
            }
        }

        void OnDisable()
        {
            if (runningState == RunningState.RUNNING)
            {
                stop();
            }
        }
	}
}