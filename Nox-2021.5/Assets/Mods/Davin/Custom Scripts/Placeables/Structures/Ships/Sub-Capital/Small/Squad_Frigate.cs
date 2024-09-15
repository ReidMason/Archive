using NoxCore.Placeables.Ships;
using NoxCore.Data.Placeables;

namespace Davin.Placeables.Ships
{
    public class Squad_Frigate : Frigate
    {
         public override void init(NoxObjectData noxObjectData = null)
        {
            // change any default values in the parent class here
            if (noxObjectData != null)
            {
                base.init(Instantiate(noxObjectData));
            }
            else
            {
                base.init();
            }
        }     
    }
}
