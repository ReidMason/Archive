using NoxCore.GUIs;
using System;
using UnityEngine;

namespace NoxCore.Data.Placeables
{
    public class NoxObjectData : ScriptableObject, INoxObjectData, ISerializationCallbackReceiver
    {
		public string label;
		[NonSerialized] protected string _name;
		public string Name { get { return _name; } set { _name = value; } }

		public FactionData faction;
		[NonSerialized] protected FactionData _faction;
		public FactionData Faction { get { return _faction; } set { _faction = value; } }

		public FactionLabel factionLabel;
		[NonSerialized] protected FactionLabel _factionLabel;
		public FactionLabel FactionLabel { get { return _factionLabel; } set { _factionLabel = value; } }

		public NameLabel nameLabel;
		[NonSerialized] protected NameLabel _nameLabel;
		public NameLabel NameLabel { get { return _nameLabel; } set { _nameLabel = value; } }

		public virtual void OnAfterDeserialize()
		{
			Name = label;
			Faction = faction;
			FactionLabel = factionLabel;
			NameLabel = nameLabel;
		}

		public virtual void OnBeforeSerialize()
		{ }
	}
}