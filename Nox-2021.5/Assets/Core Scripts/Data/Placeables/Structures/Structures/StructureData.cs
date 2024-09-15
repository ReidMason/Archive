using UnityEngine;

using System;

namespace NoxCore.Data.Placeables
{
    [CreateAssetMenu(fileName = "StructureData", menuName = "ScriptableObjects/Placeables/Structure")]
    public class StructureData : NoxObject2DData, IStructureData
    {
        [Header("Structure")]

        public uint hullCost;
        [NonSerialized] protected uint _hullCost;
        public uint HullCost { get { return _hullCost; } set { _hullCost = value; } }

        public float hullStrength;
        [NonSerialized] protected float _hullStrength;
        public float HullStrength { get { return _hullStrength; } set { _hullStrength = value; } }

        public float mass;
        [NonSerialized] protected float _mass;
        public float Mass { get { return _mass; } set { _mass = value; } }

        public float aspectRadius;
        [NonSerialized] protected float _aspectRadius;
        public float AspectRadius { get { return _aspectRadius; } set { _aspectRadius = value; } }

        public int maxFireGroups = -1;
        [NonSerialized] protected int _maxFireGroups;
        public int MaxFireGroups { get { return _maxFireGroups; } set { _maxFireGroups = value; } }

        public uint maxDevices;
        [NonSerialized] protected uint _maxDevices;
        public uint MaxDevices { get { return _maxDevices; } set { _maxDevices = value; } }

        public uint maxModules;
        [NonSerialized] protected uint _maxModules;
        public uint MaxModules { get { return _maxModules; } set { _maxModules = value; } }

        public uint numShieldSockets;
        [NonSerialized] protected uint _numShieldSockets;
        public uint NumShieldSockets { get { return _numShieldSockets; } set { _numShieldSockets = value; } }

        public uint numMixedSockets;
        [NonSerialized] protected uint _numMixedSockets;
        public uint NumMixedSockets { get { return _numMixedSockets; } set { _numMixedSockets = value; } }

        public uint maxWeapons;
        [NonSerialized] protected uint _maxWeapons;
        public uint MaxWeapons { get { return _maxWeapons; } set { _maxWeapons = value; } }

        public uint numLaunchers;
        [NonSerialized] protected uint _numLaunchers;
        public uint NumLaunchers { get { return _numLaunchers; } set { _numLaunchers = value; } }

        public uint numTurrets;
        [NonSerialized] protected uint _numTurrets;
        public uint NumTurrets { get { return _numTurrets; } set { _numTurrets = value; } }

        public uint numEmitters;
        [NonSerialized] protected uint _numEmitters;
        public uint NumEmitters { get { return _numEmitters; } set { _numEmitters = value; } }

        public uint numMixedLaunchersAndTurrets;
        [NonSerialized] protected uint _numMixedLaunchersAndTurrets;
        public uint NumMixedLaunchersAndTurrets { get { return _numMixedLaunchersAndTurrets; } set { _numMixedLaunchersAndTurrets = value; } }

        public uint numMixedGenericWeapons;
        [NonSerialized] protected uint _numMixedGenericWeapons;
        public uint NumMixedGenericWeapons { get { return _numMixedGenericWeapons; } set { _numMixedGenericWeapons = value; } }

        public uint maxDockingPorts;
        [NonSerialized] protected uint _maxDockingPorts;
        public uint MaxDockingPorts { get { return _maxDockingPorts; } set { _maxDockingPorts = value; } }

        public uint maxHangars;
        [NonSerialized] protected uint _maxHangars;
        public uint MaxHangars { get { return _maxHangars; } set { _maxHangars = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            HullCost = hullCost;
            HullStrength = hullStrength;
            Mass = mass;
            AspectRadius = aspectRadius;
            MaxFireGroups = maxFireGroups;
            MaxDevices = maxDevices;
            MaxModules = maxModules;
            NumShieldSockets = numShieldSockets;
            NumMixedSockets = numMixedSockets;
            MaxWeapons = maxWeapons;
            NumLaunchers = numLaunchers;
            NumTurrets = numTurrets;
            NumEmitters = numEmitters;
            NumMixedLaunchersAndTurrets = numMixedLaunchersAndTurrets;
            NumMixedGenericWeapons = numMixedGenericWeapons;
            MaxDockingPorts = maxDockingPorts;
            MaxHangars = maxHangars;
        }
    }
}