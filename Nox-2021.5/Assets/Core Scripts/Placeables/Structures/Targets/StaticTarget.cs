using NoxCore.Data.Placeables;

namespace NoxCore.Placeables
{
    public class StaticTarget : Structure
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            if (noxObjectData == null)
            {
                StructureData = noxObject2DData as StructureData;

                base.init(StructureData);
            }
            else
            {
                StructureData = noxObjectData as StructureData;
                base.init(noxObjectData);
            }
        }
    }
}