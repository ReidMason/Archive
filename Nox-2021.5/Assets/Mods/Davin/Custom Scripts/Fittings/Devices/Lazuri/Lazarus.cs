using UnityEngine;
using System.Collections;

using NoxCore.Controllers;
using NoxCore.Fittings.Devices;
using NoxCore.Placeables;

namespace Davin.Fittings.Devices
{
    public class Lazarus : Device, ILazarus
    {
        // custom device data goes here if needed
        public int numRaises;
        protected bool origCanRespawn;
        protected float origRespawnTime;

        public override void reset()
        {
            base.reset();

            if (numRaises == 0)
            {
                structure.StructureData.RespawnTime = origRespawnTime;
                structure.CanRespawn = origCanRespawn;

                structure.NotifyKilled -= Lazarus_RaiseFromDead;
            }
        }

        public override void postFitting()
        {
            base.postFitting();

            // this is done here in case the default for RespawnTime or canRespawn is changed in the custom Ship class
            origCanRespawn = structure.CanRespawn;
            origRespawnTime = structure.StructureData.RespawnTime;

            // make the respawn time for the structure instant
            structure.StructureData.RespawnTime = 0;
            structure.CanRespawn = true;

            // this is in postFitting because the AI does not get attached to the controlled structure until spawn is called
            structure.NotifyKilled += Lazarus_RaiseFromDead;
        }

        /*	custom device methods go here if needed	*/

        public void Lazarus_RaiseFromDead(object sender, TargetDestroyedEventArgs args)
        {
            if (isActiveOn() == true && isFlippingActivation() == false && numRaises > 0)
            {
                numRaises--;

                // make the respawn time for the structure instant
                structure.StructureData.RespawnTime = 0;
                structure.CanRespawn = true;
            }
            else
            {
                structure.StructureData.RespawnTime = origRespawnTime;
                structure.CanRespawn = origCanRespawn;
            }
        }
    }
}
