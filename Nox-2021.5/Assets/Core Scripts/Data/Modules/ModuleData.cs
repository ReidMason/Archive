using UnityEngine;

using System;

namespace NoxCore.Data.Fittings
{
	public abstract class ModuleData : DeviceData, IModuleData, ISerializationCallbackReceiver
	{
		[Header("Module")]

		public float __aspectRadius;
		[NonSerialized] protected float _aspectRadius;
		public float AspectRadius { get { return _aspectRadius; } set { _aspectRadius = value; } }

		public float __maxArmour;
		[NonSerialized] protected float _maxArmour;
		public float MaxArmour { get { return _maxArmour; } set { _maxArmour = value; } }

		public GameObject __explosion;
		[NonSerialized] protected GameObject _explosion;
		public GameObject Explosion { get { return _explosion; } set { _explosion = value; } }

		public float __damageOnDestroy;
		[NonSerialized] protected float _damageOnDestroy;
		public float DamageOnDestroy { get { return _damageOnDestroy; } set { _damageOnDestroy = value; } }

		public override void OnAfterDeserialize()
		{
			base.OnAfterDeserialize();

			AspectRadius = __aspectRadius;
			MaxArmour = __maxArmour;
			Explosion = __explosion;
			DamageOnDestroy = __damageOnDestroy;
		}
	}
}