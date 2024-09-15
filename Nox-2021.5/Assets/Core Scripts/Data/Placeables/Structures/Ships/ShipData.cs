using UnityEngine;

using System;

using com.spacepuppy;

namespace NoxCore.Data.Placeables
{
    [CreateAssetMenu(fileName = "ShipData", menuName = "ScriptableObjects/Placeables/Ship")]
    public class ShipData : StructureData, IShipData
    {
        public float speedLimiter;
        [NonSerialized] protected float _speedLimiter;
        public float SpeedLimiter { get { return _speedLimiter; } set { _speedLimiter = value; } }

        public bool afterburnerCapable;
        [NonSerialized] protected bool _afterburnerCapable;
        public bool AfterburnerCapable { get { return _afterburnerCapable; } set { _afterburnerCapable = value; } }

        public bool warpCapable;
        [NonSerialized] protected bool _warpCapable;
        public bool WarpCapable { get { return _warpCapable; } set { _warpCapable = value; } }

        public uint numEngineSockets;
        [NonSerialized] protected uint _numEngineSockets;
        public uint NumEngineSockets { get { return _numEngineSockets; } set { _numEngineSockets = value; } }

        public float maxForce;
        [NonSerialized] protected float _maxForce;
        public float MaxForce { get { return _maxForce; } set { _maxForce = value; } }

        public float maxTurnRate;
        [NonSerialized] [ShowNonSerializedPropertyAttribute("RUNTIME VALUES")] protected float _maxTurnRate;
        public float MaxTurnRate { get { return _maxTurnRate; } set { _maxTurnRate = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            SpeedLimiter = speedLimiter;
            AfterburnerCapable = afterburnerCapable;
            WarpCapable = warpCapable;
            NumEngineSockets = numEngineSockets;
            MaxForce = maxForce;
            MaxTurnRate = maxTurnRate;
        }
    }
}