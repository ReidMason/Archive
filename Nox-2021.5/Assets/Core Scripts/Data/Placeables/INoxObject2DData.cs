namespace NoxCore.Data.Placeables
{
    public interface INoxObject2DData : INoxObjectData
    {
        bool SpawnHidden { get; set; }
        bool RespawnsAtStartSpot { get; set; }
        float DespawnTime { get; set; }
        float RespawnTime { get; set; }
    }
}