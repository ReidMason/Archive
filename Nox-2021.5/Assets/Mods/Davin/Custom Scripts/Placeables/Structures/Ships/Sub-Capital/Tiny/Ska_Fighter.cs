using NoxCore.Data.Placeables;
using NoxCore.Placeables.Ships;

namespace Davin.Placeables.Ships
{
    public class Ska_Fighter : Fighter
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