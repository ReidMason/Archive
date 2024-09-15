using UnityEngine;

using NoxCore.Buffs;
using NoxCore.Controllers;
using NoxCore.Data.Placeables;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;

using Davin.Buffs;

namespace Davin.Placeables.Ships
{
    public class Ska_Freighter_TypeD : Freighter
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

        protected override void Structure_UltimateActivated(object sender, UltimateEventArgs args)
        {
            base.Structure_UltimateActivated(sender, args);

            MaxSpeedBuff maxSpeedBuff = new MaxSpeedBuff(engines, BuffType.STANDARD, 1, 200, true, 10);

            BuffManager.addBuff(maxSpeedBuff);
        }

        protected override void Structure_DockInitiatorDocked(object sender, DockingPortEventArgs args)
        {
            base.Structure_DockInitiatorDocked(sender, args);

            foreach (IEngine engine in engines)
            {
                if (engine.isActiveOn() == true)
                {
                    engine.deactivate();
                }
            }
        }

        protected override void Structure_DockInitiatorUndocked(object sender, DockingPortEventArgs args)
        {
            base.Structure_DockInitiatorUndocked(sender, args);

            AIStateController aiStateController = Controller as AIStateController;

            if (aiStateController != null)
            {
                aiStateController.reset();
            }

            foreach (IEngine engine in engines)
            {
                if (engine.isActiveOn() == true)
                {
                    engine.activate();
                }
            }
        }
    }
}