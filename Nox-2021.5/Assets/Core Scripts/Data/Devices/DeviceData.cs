using System;

using UnityEngine;

namespace NoxCore.Data.Fittings
{
    public abstract class DeviceData : ScriptableObject, IDeviceData, ISerializationCallbackReceiver
	{
		public KeyCode __debugKey;
		[NonSerialized] protected KeyCode _debugKey;
		public KeyCode DebugKey { get { return _debugKey; } set { _debugKey = value; } }
		
		[NonSerialized]	protected string _resourcePath;
		public string ResourcePath { get { return _resourcePath; } set { _resourcePath = value; } }
		
		[Header("Socket Info")]

		public string __type;
		[NonSerialized] protected string _type;
		public string Type { get { return _type; } set { _type = value; } }

		public string __subType;
		[NonSerialized]	protected string _subType;
		public string SubType { get { return _subType; } set { _subType = value; } }

		public byte __techLevel;
		[NonSerialized]	protected byte _techLevel;
		public byte TechLevel { get { return _techLevel; } set { _techLevel = value; } }

		public byte __priority;
		[NonSerialized]	protected byte _priority;
		public byte Priority { get { return _priority; } set { _priority = value; } }

		public uint __cost;
		[NonSerialized]	protected uint _cost;
		public uint Cost { get { return _cost; } set { _cost = value; } }

		[Header("Power & Heat")]

		public float __requiredPower;
		[NonSerialized]	protected float _requiredPower;
		public float RequiredPower { get { return _requiredPower; } set { _requiredPower = value; } }

		public float __activeHeat;
		[NonSerialized]	protected float _activeHeat;
		public float ActiveHeat { get { return _activeHeat; } set { _activeHeat = value; } }

		public float __emField;
		[NonSerialized]	protected float _emField;
		public float EMField { get { return _emField; } set { _emField = value; } }

		[Header("Active Status")]

		public float __activatingDelay;
		[NonSerialized]	protected float _activatingDelay;
		public float ActivatingDelay { get { return _activatingDelay; } set { _activatingDelay = value; } }

		public float __deactivatingDelay;
		[NonSerialized]	protected float _deactivatingDelay;
		public float DeactivatingDelay { get { return _deactivatingDelay; } set { _deactivatingDelay = value; } }

		public bool __activeOn;
		[NonSerialized]	protected bool _activeOn;
		public bool ActiveOn { get { return _activeOn; } set { _activeOn = value; } }

		public bool __activeOnSpawn;
		[NonSerialized]	protected bool _activeOnSpawn;
		public bool ActiveOnSpawn { get { return _activeOnSpawn; } set { _activeOnSpawn = value; } }

		public virtual void OnAfterDeserialize()
        {
			DebugKey = __debugKey;
			Type = __type;
			SubType = __subType;
			TechLevel = __techLevel;
			Priority = __priority;
			Cost = __cost;
			RequiredPower = __requiredPower;
			ActiveHeat = __activeHeat;
			EMField = __emField;
			ActivatingDelay = __activatingDelay;
			DeactivatingDelay = __deactivatingDelay;
			ActiveOn = __activeOn;
			ActiveOnSpawn = __activeOnSpawn;
		}

		public virtual void OnBeforeSerialize()
		{}
	}
}