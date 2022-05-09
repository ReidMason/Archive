using UnityEngine;
using System.Collections;

using NoxCore.Utilities;

namespace NoxCore.Helm
{
	public abstract class SteeringBehaviour : MonoBehaviour, ISteeringBehaviour
	{
		[ShowOnly]
		[SerializeField]
		protected bool _Active;
		public bool Active { get { return _Active; } set { _Active = value; } }

		[SerializeField] protected string _Label;
		public string Label { get { return _Label; } set { _Label = value; } }

		[SerializeField] protected int _SequenceID;
		public int SequenceID { get { return _SequenceID; } set { _SequenceID = value; } }
		
		[SerializeField] protected int _Weight;
		public int Weight { get { return _Weight; } set { _Weight = value; } }

		protected HelmController _Helm;
		public HelmController Helm { get { return _Helm; } set { _Helm = value; } }

		[Header("Force Stats")]

        [ShowOnly]
        public float currentForceRequested;

        [ShowOnly]
        public float currentForceActual;

        [ShowOnly]
        public float maxForceRequested;

        protected int defaultWeight;		
		protected Vector2 steeringVector;
		protected Vector2 desiredVelocity;

        public virtual void Start()
        {}

        public virtual void init()
        {
			Helm = GetComponent<HelmController>();
			defaultWeight = Weight;
		}
		
		public void enable()
		{
			Active = true;
		}		
		
		public void enableExclusively()
		{
			Helm.disableAllBehaviours();
			
			Active = true;
		}
		
		public void disable()
		{
			Active = false;
		}
		
		public void toggleEnabled()
		{
			Active = !Active;
		}
		
		public void resetWeight()
		{
			Weight = 0;
		}
		
		public void resetWeightToDefault()
		{
			Weight = defaultWeight;
		}		
		
		public virtual Vector2 execute()
		{
			return Vector2.zero;
		}
	}
}
