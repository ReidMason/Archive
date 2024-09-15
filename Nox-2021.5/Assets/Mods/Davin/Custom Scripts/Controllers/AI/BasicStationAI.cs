using System.Collections.Generic;
using System.Linq;

using NoxCore.Data;
using NoxCore.Helm;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;

namespace NoxCore.Controllers
{
    public class BasicStationAI : AIStateController
    {
        protected List<Structure> squad;

        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, null);

            //GameEventManager.MatchIsWaitingToStart += AI_MatchIsWaitingToStart;
            //GameEventManager.MatchHasStarted += AI_MatchHasStarted;

            //structure.NotifyKilled += AI_NotifyKilled;

            aiActions.Add("IDLE", idleAction);

            state = "IDLE";

            booted = true;
        }

        public virtual string idleAction()
        {
            return "IDLE";
        }
        /*
        protected void AI_MatchIsWaitingToStart(object sender)
        {
            FactionData faction = FactionManager.Instance.findFaction(structure.Faction.ID);

            List<Ship> ships = faction.FleetManager.getAllShips();

            squad = ships.Cast<Structure>().ToList();
        }

        protected void AI_MatchHasStarted(object sender)
        {
            foreach(Ship ship in squad)
            {
                ship.Controller.startSpot = transform.position;

                if (transform.position.x > 0)
                {
                    ship.Controller.startRotation = -90;
                }
                else
                {
                    ship.Controller.startRotation = 90;
                }
            }
        }

        public void AI_NotifyKilled(object sender, TargetDestroyedEventArgs args)
        {
            foreach (Ship ship in squad)
            {
                ship.StructureData.CanRespawn = false;
            }
        }
        */
    }
}