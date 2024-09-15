using NoxCore.Data.Placeables;

namespace NoxCore.Placeables
{
    public interface INoxObject
    {
        NoxObjectData NoxObjectData { get; set; }

        void init(NoxObjectData noxObjectData = null);
    }
}