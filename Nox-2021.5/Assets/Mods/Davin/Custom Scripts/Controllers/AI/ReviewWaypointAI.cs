using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NoxCore.Data;
using NoxCore.Helm;
using NoxCore.Placeables;
using NoxCore.Controllers;
using NoxCore.Fittings.Devices;
using NoxCore.Fittings.Modules;
using NoxCore.Fittings.Weapons;
using NoxCore.GUIs;
using NoxCore.Managers;
using NoxCore.Placeables.Ships;

using Davin.Cameras;
using Davin.Fittings.Modules;
using Davin.Fittings.Devices;

namespace Davin.Controllers
{
    public class ReviewWaypointAI : BasicNavigationalAI
    {
        public List<Transform> waypoints = new List<Transform>();
        protected int currentWaypoint = 0;


        private float currentMessageTime = 0;
        
        public override void boot(Structure structure, HelmController helm = null)
        {
            base.boot(structure, helm);

            helm.desiredThrottle = 0;
            helm.throttle = 0;

            NoxGUI.Instance.setHealthBarMode(false);

            // do setup
            StartCoroutine(HideFaction(1001, currentMessageTime));    
            StartCoroutine(HideFaction(1002, currentMessageTime));
            StartCoroutine(HideFaction(1003, currentMessageTime));

            StartCoroutine(DeactivateDevice<ICloakingScanner>("USN Watch Station Mk1", currentMessageTime));
            
            // start the review
            StartCoroutine(PrintMessage("Welcome, Admiral!", getMessageTimeWithOffset(5)));
            StartCoroutine(PrintMessage("Thank you for joining our inspection of the new fleet today!", getMessageTimeWithOffset(5)));
            
            StartCoroutine(PrintMessage("As you can see, we start our inspection on the new resupply station.", getMessageTimeWithOffset(5)));
            StartCoroutine(CameraMovementTo("USN Resupply Station Mk1", currentMessageTime));
            StartCoroutine(CameraZoomTo(-5000f, currentMessageTime));
            StartCoroutine(PrintMessage("It will be used to resupply our ships and provide refuge in deep space.", getMessageTimeWithOffset(5)));
            StartCoroutine(PrintMessage("As it is lightly armed it will need the support of other ships.", getMessageTimeWithOffset(5)));

            StartCoroutine(PrintMessage("Now, starting our tour, we will at first examine the new fleet ships.", getMessageTimeWithOffset(5)));
            StartCoroutine(CameraMovementTo("Admiral's Destroyer", currentMessageTime));
            StartCoroutine(CameraZoomTo(-1000f, currentMessageTime));
            StartCoroutine(throttleFull(currentMessageTime));

            StartCoroutine(PrintMessage("Here we see our tiny ships. Lightly armed, but quick and agile.", getMessageTimeWithOffset(10)));
            StartCoroutine(throttleHalf(currentMessageTime));
            StartCoroutine(CameraMovementTo("USN Review Fighter Mk1", currentMessageTime));
            
            StartCoroutine(PrintMessage("Our small ships, a combination of different capabilities for fleet support coupled with fast movement.", getMessageTimeWithOffset(10)));
            StartCoroutine(CameraMovementTo("USN Review Destroyer", currentMessageTime));

            StartCoroutine(PrintMessage("Now we come to our medium vessels, optimised for combat, but rather slow.", getMessageTimeWithOffset(15)));
            StartCoroutine(CameraMovementTo("USN Review Battlecruiser", currentMessageTime));
            StartCoroutine(CameraZoomTo(-1750f, currentMessageTime));
            StartCoroutine(throttleFull(currentMessageTime));

            StartCoroutine(PrintMessage("Now we come to our large vessels, optimised for heavy engagements but rather slow.", getMessageTimeWithOffset(10)));
            StartCoroutine(CameraMovementTo("USN Review Dreadnought", currentMessageTime));
            StartCoroutine(CameraZoomTo(-2000f, currentMessageTime));
            StartCoroutine(throttleFull(currentMessageTime));

            StartCoroutine(CameraZoomTo(-2500f, currentMessageTime));
            StartCoroutine(PrintMessage("Finally, our new range of fleet carriers. They will project force into enemy regions and be able to transport several smaller ships.", getMessageTimeWithOffset(10)));
            StartCoroutine(CameraMovementTo("USN Review Medium Carrier", currentMessageTime));

            StartCoroutine(PrintMessage("We hope that the ships are to your liking!", getMessageTimeWithOffset(5)));
            StartCoroutine(CameraMovementTo("Admiral's Destroyer", currentMessageTime));
            StartCoroutine(CameraZoomTo(-1000f, currentMessageTime));
            StartCoroutine(PrintMessage("Now we will show you two more station prototypes.", getMessageTimeWithOffset(5)));
            
            StartCoroutine(PrintMessage("This is our command station, it will be used to assign mission objectives and help direct battles.", getMessageTimeWithOffset(5)));
            StartCoroutine(CameraMovementTo("USN Starbase Mk1", currentMessageTime));
            StartCoroutine(CameraZoomTo(-3000f, currentMessageTime));
            StartCoroutine(PrintMessage("By outsourcing commanding tasks to these stations, decisions can be made faster.", getMessageTimeWithOffset(5)));

            StartCoroutine(PrintMessage("Our last new station prototype is the watch station.These can be used to monitor ship movement and detect if they are cloaked.", getMessageTimeWithOffset(5)));
            StartCoroutine(CameraMovementTo("USN Watch Station Mk1", currentMessageTime));
            StartCoroutine(CameraZoomTo(-2000f, currentMessageTime));
            StartCoroutine(PrintMessage("While rather small, they are also relatively cheap to build, and can also serve as a local meeting point.", getMessageTimeWithOffset(5)));
            StartCoroutine(PrintMessage("This covers our new stations, however, we left the most interesting part for the end.", getMessageTimeWithOffset(5)));

            StartCoroutine(CameraMovementTo("Admiral's Destroyer", currentMessageTime));
            StartCoroutine(CameraZoomTo(-1000f, currentMessageTime));
            StartCoroutine(PrintMessage("New Technology! Our scientists worked hard the last couple of months.", getMessageTimeWithOffset(5)));
            StartCoroutine(PrintMessage("We focused on infiltration and enemy control technology.", getMessageTimeWithOffset(5)));
            StartCoroutine(PrintMessage("Our first accomplishment is a cloaking module.", getMessageTimeWithOffset(5)));
            
            //Cloaked Recon
            StartCoroutine(CameraMovementTo("Cloaked Recon", currentMessageTime));
            StartCoroutine(ActivateCloakModule("Cloaked Recon", getMessageTimeWithOffset(5)));
            StartCoroutine(PrintMessage("This can be used to turn nearly invisible to the enemy.", getMessageTimeWithOffset(5)));
            StartCoroutine(PrintMessage("However, if we use our weapons while cloaked, we will still give away our position.", getMessageTimeWithOffset(5)));
            
            //Scanning station
            StartCoroutine(CameraMovementTo("USN Watch Station Mk1", currentMessageTime));
            StartCoroutine(PrintMessage("And while we developed our cloaking technology, we of course also thought of a way to detect cloaked ships.", getMessageTimeWithOffset(5)));
            StartCoroutine(PrintMessage("So we developed a scanner that can pick up ships that want to hide.", getMessageTimeWithOffset(5)));
            StartCoroutine(ActivateDevice<ICloakingScanner>("USN Watch Station Mk1", getMessageTimeWithOffset(5)));
            StartCoroutine(PrintMessage("Well, while visually not very impressive, you can later examine the internal technology to check the functionality.", getMessageTimeWithOffset(5)));                    
            
            //Weapon Test Interdictor1
            StartCoroutine(CameraMovementTo("Weapon Test Interdictor1", currentMessageTime));
            StartCoroutine(PrintMessage("Our scientists worked for quite some time on gravity changing technology.", getMessageTimeWithOffset(5)));
            StartCoroutine(PrintMessage("And we present our final result here: The black hole implosion.", getMessageTimeWithOffset(5)));
            StartCoroutine(CameraZoomTo(-2500f, currentMessageTime));
            StartCoroutine(ShowFaction(1001, getMessageTimeWithOffset(3)));
            StartCoroutine(FireTestLauncher("Weapon Test Interdictor1", getMessageTimeWithOffset(2)));
            StartCoroutine(PrintMessage("It will pull everything in its radius to the ship, that uses it.", getMessageTimeWithOffset(4)));
            StartCoroutine(PrintMessage("It also damages the structures.", getMessageTimeWithOffset(5)));
            
            //Weapon Test Interdictor2
            StartCoroutine(PrintMessage("Now, while researching our gravity modifying technology, our scientists also found a new way to use wormholes.", currentMessageTime));
            StartCoroutine(CameraMovementTo("Weapon Test Interdictor2", currentMessageTime));
            StartCoroutine(ShowFaction(1002, getMessageTimeWithOffset(3)));
            StartCoroutine(FireTestLauncher("Weapon Test Interdictor2", getMessageTimeWithOffset(3)));
            StartCoroutine(PrintMessage("We used this to create a new bomb, which complements our black hole technology, the warp bomb.", getMessageTimeWithOffset(5)));
            StartCoroutine(PrintMessage("The warp bomb gets shot in a direction to one's liking, and shortly before detonating it opens an endpoint for a wormhole.", getMessageTimeWithOffset(5)));
            StartCoroutine(PrintMessage("Our ship can use the gravity changing technology to open the start point to said wormhole.", getMessageTimeWithOffset(5)));
            StartCoroutine(PrintMessage("So, our ship and the bomb switch places, and the bomb detonates at the position of the ship, without harming it.", getMessageTimeWithOffset(5)));
            
            //Weapon Test Interdictor3
            StartCoroutine(PrintMessage("And as I am sure you can already imagine, those two weapon have a great synergy.", currentMessageTime));
            StartCoroutine(ShowFaction(1003, currentMessageTime));
            StartCoroutine(CameraMovementTo("Weapon Test Interdictor3", currentMessageTime));
            StartCoroutine(FireAllTestLaunchers("Weapon Test Interdictor3", currentMessageTime));
            StartCoroutine(PrintMessage("While pulling in enemies with the black hole, the ship also shoots a warp bomb.", getMessageTimeWithOffset(5)));
            StartCoroutine(PrintMessage("And as all enemy get very close to our ship, the warp bomb detonates.", getMessageTimeWithOffset(5)));
            StartCoroutine(PrintMessage("It switches places with the ship and deals high amounts of damage to all nearby ships.", getMessageTimeWithOffset(5)));
            StartCoroutine(PrintMessage("All the while, our ship remains unharmed.", getMessageTimeWithOffset(5)));
            StartCoroutine(PrintMessage("So, this was our last piece of new technology.", getMessageTimeWithOffset(5)));
            StartCoroutine(CameraMovementTo("Review Destroyer", currentMessageTime));
            StartCoroutine(PrintMessage("We hope the presentation was to your liking.", getMessageTimeWithOffset(5)));
            StartCoroutine(PrintMessage("Now we invite you to inspect our new technology a bit closer.", getMessageTimeWithOffset(5)));
            StartCoroutine(PrintMessage("We already provided you access to all plans and design documents of our technology.", getMessageTimeWithOffset(5)));
            StartCoroutine(PrintMessage("We hope to see you again soon! Goodbye!", getMessageTimeWithOffset(5)));
        }

        protected override Vector2? setHelmDestination()
        {
            Vector2 nextPoint = waypoints[currentWaypoint].position;

            currentWaypoint++;

            if (currentWaypoint == waypoints.Count)
            {
                //Debug.Log("LastWaypoint");
                currentWaypoint = 0;
                Helm.throttle = 0;
                Helm.desiredThrottle = 0;
            }

            return nextPoint;
        }

        IEnumerator throttleZero(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            Helm.throttle = 0;
            Helm.desiredThrottle = 0;
        }
        IEnumerator throttleHalf(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            Helm.throttle = 0.5f;
            Helm.desiredThrottle = 0.5f;
        }
        IEnumerator throttleFull(float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            Helm.throttle = 1;
            Helm.desiredThrottle = 1;
        }

        IEnumerator CameraMovementTo(string toMove, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            GameObject camTarget = GameObject.Find(toMove);

            if (camTarget != null)
            {
                GameManager.Instance.Gamemode.Cam.setFollowTarget(camTarget.transform);
            }
        }

        IEnumerator CameraZoomTo(float toZoom, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            Camera.main.GetComponent<ImprovedTopDownCamera>().targetZ = toZoom;
        }

        IEnumerator PrintMessage(string message, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            NoxGUI.Instance.setMessage(message);
        }

        IEnumerator ActivateCloakModule(string structure, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
         
            GameObject structureGO = GameObject.Find(structure);
            
            if (structureGO != null)
            {
                structureGO.GetComponent<Structure>().getModule<ICloakingDevice>().activateCloak();
            }
        }

        IEnumerator FireTestLauncher(string structure, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            GameObject structureGO = GameObject.Find(structure);

            if (structureGO != null)
            {
                structureGO.GetComponent<Structure>().getWeapon<IUnguidedLauncher>().AllowFiring = true;
            }
        }

        IEnumerator FireAllTestLaunchers(string structure, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            GameObject structureGO = GameObject.Find(structure);

            if (structureGO != null)
            {
                List<IUnguidedLauncher> testLaunchers = structureGO.GetComponent<Structure>().getWeapons<IUnguidedLauncher>();

                foreach (IUnguidedLauncher testLauncher in testLaunchers)
                {
                    testLauncher.AllowFiring = true;
                }
            }
        }

        IEnumerator ActivateWeapon<T>(string structureName, float delayTime) where T : IWeapon
        {
            yield return new WaitForSeconds(delayTime);

            GameObject structureGO = GameObject.Find(structureName);

            if (structureGO != null)
            {
                Structure structure = structureGO.GetComponent<Structure>();

                List<T> weaponlist = structure.getWeapons<T>();

                foreach (T wp in weaponlist)
                {
                    wp.activate();
                }
            }
        }

        IEnumerator ActivateDevice<T>(string structure, float delayTime) where T : IDevice
        {
            yield return new WaitForSeconds(delayTime);
            
            GameObject structureGO = GameObject.Find(structure);

            if (structureGO != null)
            {
                structureGO.GetComponent<Structure>().getDevice<T>().activate();
            }
        }

        IEnumerator DeactivateModule<T>(string structure, float delayTime) where T : IModule
        {
            yield return new WaitForSeconds(delayTime);

            GameObject structureGO = GameObject.Find(structure);

            if (structureGO != null)
            {
                structureGO.GetComponent<Structure>().getModule<T>().deactivate();
            }
        }

        IEnumerator DeactivateWeapon<T>(string structure, float delayTime) where T : IWeapon
        {
            yield return new WaitForSeconds(delayTime);

            GameObject structureGO = GameObject.Find(structure);

            if (structureGO != null)
            {
                structureGO.GetComponent<Structure>().getWeapon<T>().deactivate();
            }
        }

        IEnumerator DeactivateDevice<T>(string structure, float delayTime) where T : IDevice
        {
            yield return new WaitForSeconds(delayTime);

            GameObject structureGO = GameObject.Find(structure);

            if (structureGO != null)
            {
                structureGO.GetComponent<Structure>().getDevice<T>().deactivate();
            }
        }

        IEnumerator HideFaction(int factionID, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);

            FactionData faction = FactionManager.Instance.findFaction(factionID);

            if (faction != null)
            {
                List<Ship> ships = faction.FleetManager.getAllShips();
                
                foreach (Ship ship in ships)
                {
                    ship.gameObject.SetActive(false);
                }
            }
        }

        IEnumerator ShowFaction(int factionID, float delayTime)
        {
            yield return new WaitForSeconds(delayTime);
            
            FactionData faction = FactionManager.Instance.findFaction(factionID);

            if (faction != null)
            {
                List<Ship> ships = faction.FleetManager.getAllShips();

                foreach (Ship ship in ships)
                {
                    ship.gameObject.SetActive(true);
                }
            }
        }

        private float getMessageTimeWithOffset(float OffsetTime)
        {
            return (currentMessageTime += OffsetTime);
        }
    }
}