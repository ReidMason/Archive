using UnityEngine;

using NoxCore.Data.Placeables;
using NoxCore.Fittings.Sockets;

namespace NoxCore.Placeables
{
    [RequireComponent(typeof(Outline))]
    public abstract class Station : Structure
    {
        public float spin;

        public override void init(NoxObjectData noxObjectData = null)
        {
            base.init(noxObject2DData);
        }

        protected override void Structure_DockReceiverDocked(object sender, DockingPortEventArgs args)
        {
            // D.log("Structure", args.ship.gameObject.name + " has docked with " + args.portStructure.gameObject.name);            

            args.ship.transform.parent = args.port.Transform;
        }

        protected override void Structure_DockReceiverUndocked(object sender, DockingPortEventArgs args)
        {
            // D.log("Structure", args.ship.gameObject.name + " has undocked from " + args.portStructure.gameObject.name);

            GameObject hierarchy = GameObject.Find("Placeables");

            args.ship.transform.parent = hierarchy.transform;
        }

        public override void Update()
        {
            base.Update();

            transform.Rotate(new Vector3(0, 0, spin * Time.deltaTime));
        }
    }
}