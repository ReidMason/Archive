using UnityEngine;

using System;

namespace NoxCore.Data.Placeables
{
    public class NoxObject2DData : NoxObjectData, INoxObject2DData
    {
        [Header("NoxObject2D")]

        public bool spawnHidden;
        [NonSerialized] protected bool _spawnHidden;
        public bool SpawnHidden { get { return _spawnHidden; } set { _spawnHidden = value; } }

        public bool respawnsAtStartSpot;
        [NonSerialized] protected bool _respawnsAtStartSpot;
        public bool RespawnsAtStartSpot { get { return _respawnsAtStartSpot; } set { _respawnsAtStartSpot = value; } }

        public float despawnTime;
        [NonSerialized] protected float _despawnTime;
        public float DespawnTime { get { return _despawnTime; } set { _despawnTime = value; } }

        public float respawnTime;
        [NonSerialized] protected float _respawnTime;
        public float RespawnTime { get { return _respawnTime; } set { _respawnTime = value; } }

        public override void OnAfterDeserialize()
        {
            base.OnAfterDeserialize();

            SpawnHidden = spawnHidden;
            RespawnsAtStartSpot = respawnsAtStartSpot;
            DespawnTime = despawnTime;
            RespawnTime = respawnTime;
        }
    }
}