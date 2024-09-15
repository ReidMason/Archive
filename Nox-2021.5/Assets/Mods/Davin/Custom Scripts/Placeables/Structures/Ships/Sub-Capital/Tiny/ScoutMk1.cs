using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Controllers;
using NoxCore.Data.Placeables;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Sockets;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;

namespace Davin.Placeables.Ships
{
    public class ScoutMk1 : Scout
    {
        public override void init(NoxObjectData noxObjectData = null)
        {
            // change any default values in the parent class here
            //TotalCost = 100;

            if (noxObjectData != null)
            {
                base.init(Instantiate(noxObjectData));
            }
            else
            {
                base.init();
            }
        }
        /*
        public override void setDefaults()
        {
            base.setDefaults();

            // add default socket layout

            addDefaultSocket("EngineBays/TinyEngineBay", "LeftEngine", new Vector2(-4.159f, -11.55f));
            addDefaultSocket("EngineBays/TinyEngineBay", "CentreEngine", new Vector2(0f, -14.3f));
            addDefaultSocket("EngineBays/TinyEngineBay", "RightEngine", new Vector2(4.159f, -11.55f));
        }
        */
        public override void respawn()
        {
            base.respawn();

            ILand tinyAI = Controller as ILand;

            if (tinyAI != null)
            {
                IHangar hangar = tinyAI.getHangar();

                if (hangar != null)
                {
                    transform.position = hangar.getTransform().position;
                    transform.rotation = hangar.getTransform().rotation;

                    Controller.startSpot = transform.position;
                    Controller.startRotation = transform.rotation.eulerAngles.z;

                    Bearing = -transform.rotation.eulerAngles.z;

                    if (Bearing < 0) Bearing += 360;

                    float theta = 90 - Bearing;

                    if (theta < 0) theta += 360;

                    float x = Mathf.Cos(theta * Mathf.Deg2Rad);
                    float y = Mathf.Sin(theta * Mathf.Deg2Rad);

                    Heading = new Vector2(x, y);

                    setSpawnInSpeed(spawnInSpeedFraction);
                }
            }
        }

        protected override void Structure_LandingInitiatorLanded(object sender, HangarEventArgs args)
        {
            base.Structure_LandingInitiatorLanded(sender, args);

            foreach (IEngine engine in engines)
            {
                if (engine.isActiveOn() == true)
                {
                    engine.setActiveOn(false);
                }
            }
        }

        protected override void Structure_LaunchInitiatorLaunched(object sender, HangarEventArgs args)
        {
            base.Structure_LaunchInitiatorLaunched(sender, args);

            AIStateController aiStateController = Controller as AIStateController;

            if (aiStateController != null)
            {
                aiStateController.reset();
            }

            foreach (IEngine engine in engines)
            {
                if (engine.isActiveOn() == true)
                {
                    engine.setActiveOn(true);
                }
            }
        }
    }
}