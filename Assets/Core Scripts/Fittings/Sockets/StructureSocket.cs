using UnityEngine;
using System.Collections.Generic;

using NoxCore.Fittings.Modules;
using NoxCore.Placeables;
using NoxCore.Utilities;

namespace NoxCore.Fittings.Sockets
{
    /*
	public enum SocketType
	{
		NONSPECIFIC_DEVICE,
		POWERCORE,
		POWERCORE_ENHANCER,
		THERMAL_ENHANCER,
		BRIDGE_SYSTEM,
		CARGO_ENHANCER,
		NONSPECIFIC_MODULE,
		ENGINE,
		ENGINE_ENHANCER,
		WARPDRIVE_ENHANCER,
		WEAPON,
		WEAPON_ENHANCER,
		SHIELD,
		SHIELD_ENHANCER,
		DRONE_ENHANCER
	};
*/
    public abstract class StructureSocket : MonoBehaviour 
	{
        protected Structure parentStructure;
		protected SpriteRenderer socketRenderer;
		public SpriteRenderer SocketRenderer { get { return socketRenderer; } set { socketRenderer = value; } }

		public Transform Transform { get { return transform; } }

		public string label;
		public List<string> allowedFittedTypes = new List<string>();
		public byte minTechLevel, maxTechLevel;
		public StructureSize minStructureSize, maxStructureSize;	
		public Vector2 position;
        public float? rotation;

        [SerializeField]
		protected Module _InstalledModule;
		public Module InstalledModule { get { return _InstalledModule; } set { _InstalledModule = value; } }

		[SerializeField]
		protected int _Cost;
		public int Cost { get { return _Cost; } set { _Cost = value; } }		
				
		public virtual void init()
        {
			Transform socketParent = transform.FindParentWithName("Sockets");

			if (socketParent != null)
			{
				parentStructure = socketParent.parent.GetComponent<Structure>();
			}

			SocketRenderer = GetComponent<SpriteRenderer>();
        }

        public virtual void reset()
        { }
        
        public virtual StructureSocketInfo getSocketInfo()
        {
            StructureSocketInfo socketInfo = ScriptableObject.CreateInstance<StructureSocketInfo>();            

            socketInfo.label = label;
            socketInfo.allowedFittedTypes = allowedFittedTypes;
            socketInfo.minTechLevel = minTechLevel;
            socketInfo.maxTechLevel = maxTechLevel;
            socketInfo.minStructureSize = minStructureSize;
            socketInfo.maxStructureSize = maxStructureSize;
            socketInfo.position = position;
            socketInfo.rotation = rotation;

            return socketInfo;
        }
        
        public virtual void postFitting(){}
				
		public virtual bool install(GameObject go) { return true; }

        public virtual void update() { }
    }
}
 