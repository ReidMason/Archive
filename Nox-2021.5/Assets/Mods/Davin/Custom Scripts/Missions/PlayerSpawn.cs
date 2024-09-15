using UnityEngine;
using UnityEngine.SceneManagement;

using NoxCore.Controllers;
using NoxCore.Managers;
using NoxCore.Placeables;
using NoxCore.Placeables.Ships;

namespace Davin.Missions
{
    public class PlayerSpawn : MonoBehaviour
    {
        public GameObject playerPrefab;
        public Vector2 spawnPoint;
        public float spawnRotation;
        public Vector2 spawnDestinationPoint;

        [Range(0.0f, 1.0f)]
        public float spawnThrottle;

        protected bool spawned;
        public bool Spawned { get { return spawned; } set { spawned = value; } }

        public void init()
        {
            if (Spawned == false)
            {
                GameObject shipGO = Instantiate(playerPrefab, spawnPoint, Quaternion.Euler(0, 0, spawnRotation));

                SceneManager.MoveGameObjectToScene(shipGO, SceneManager.GetSceneByName("Home"));
                shipGO.transform.parent = GameManager.Instance.PlaceablesParent;

                Ship ship = shipGO.GetComponent<Ship>();

				// initialise ship				
				ship.init();

				Debug.Log("Spawning ship: " + ship.name);

				// call spawn
				ship.spawn();

				if (ship.Controller != null)
				{
					Debug.Log("Booting controller for " + ship.name);

					ship.Controller.boot(ship, ship.Helm);
				}

				ship.gameObject.SetActive(true);

				GameManager.Instance.addStructure(ship);

                GameObject initWaypoint = new GameObject();
                initWaypoint.transform.position = spawnDestinationPoint;

                ship.Helm.destination = spawnDestinationPoint;

                ((ship.Controller) as PlayerController)?.WaypointMarkers.Add(initWaypoint.transform);

                Spawned = true;
            }
        }
    }
}