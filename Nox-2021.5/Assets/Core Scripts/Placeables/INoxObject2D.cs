using NoxCore.Data.Placeables;

namespace NoxCore.Placeables
{
    public interface INoxObject2D : INoxObject
    {
        NoxObject2DData NoxObject2DData { get; set; }
    }
}