using NoxCore.Data.Placeables;
using NoxCore.Placeables.Ships;

namespace Davin.Placeables.Ships
{
    public class StandardCruiser : Cruiser
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            // change any default values in the parent class here

            base.init(Instantiate(noxObjectData));
        }
    }
}
