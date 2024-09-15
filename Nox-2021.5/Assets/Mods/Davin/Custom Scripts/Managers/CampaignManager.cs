using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

using NoxCore.Controllers;
using NoxCore.Placeables;


namespace NoxCore.Managers
{
    public class CampaignManager : MonoBehaviour
    {
        public delegate void WarpEventDispatcher(object sender, WarpEventArgs args);
        public static event WarpEventDispatcher WarpGateActivated;

        List<string> builtOnceBuilders = new List<string>();

        private Structure player;
        public Structure Player { get { return player; } set { player = value; } }

        private bool missionStarted;
        public bool MissionStarted {  get { return missionStarted; } set { missionStarted = value; } }

        bool[] missionCompleted;

        public List<WarpGate> warpGates = new List<WarpGate>();

        /// <summary>
        ///   Provide singleton support for this class.
        ///   The script must still be attached to a game object, but this will allow it to be called
        ///   from anywhere without specifically identifying that game object.
        /// </summary>
        private static CampaignManager instance = null;
        public static CampaignManager Instance {  get { return instance; } set { instance = value; } }

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                init();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        void init()
        {
            Debug.Log("Campaign Manager - Init");

            DontDestroyOnLoad(gameObject);

            missionCompleted = new bool[warpGates.Count];

            GameEventManager.MatchIsWaitingToStart += OnMatchIsWaitingToStart;

            WarpGateActivated += OnWarp;
        }

        public void addToBuiltOnceBuilders(string builderName)
        {
            builtOnceBuilders.Add(builderName);
        }

        public bool hasBeenBuilt(string builderName)
        {
            return builtOnceBuilders.Contains(builderName);
        }

        public bool getMissionStatus(int missionID)
        {
            return missionCompleted[missionID-1];
        }

        public void setMissionCompleted(int missionID)
        {
            missionCompleted[missionID-1] = true;
        }

        public void startMission(int missionID)
        {
            enableWarpGate(missionID, true);
            missionStarted = true;
        }

        private void endMission(int missionID)
        {
            enableWarpGate(missionID, false);
            missionStarted = false;
        }

        private void enableWarpGate(int missionID, bool active)
        {
            warpGates[missionID-1].active = active;
        }

        public static void Call_Warp(object sender, WarpEventArgs args)
        {
            if (WarpGateActivated != null)
            {
                WarpGateActivated(sender, args);
            }
        }

        public void OnMatchIsWaitingToStart(object sender)
        {
            GameObject playerInterceptor = GameObject.Find("PLAYER INTERCEPTOR");

            PlayerController playerController = playerInterceptor.GetComponent<PlayerController>();

            if (playerController != null)
            {
                player = playerController.structure;
            }

            if (SceneManager.GetActiveScene().name == "Home")
            {
                for (int i = 0; i < warpGates.Count; i++)
                {
                    if (warpGates[i] == null)
                    {
                        GameObject warpgate = GameObject.Find("Mission " + i + " Warp Gate");

                        if (warpgate != null)
                        {
                            warpGates[i] = warpgate.GetComponent<WarpGate>();
                        }
                    }
                }
            }
        }

        public void OnWarp(object sender, WarpEventArgs args)
        {
            StartCoroutine(WarpToScene(args));
        }

        IEnumerator WarpToScene(WarpEventArgs args)
        {
            //Set the current Scene to be able to unload it later
            Scene currentScene = SceneManager.GetActiveScene();

            string currentSceneName = currentScene.name;

            // check if coming from a mission scene and set mission completion info
            switch (currentSceneName)
            {
                case "Mission 1":
                    if (getMissionStatus(1) == true)
                    {
                        endMission(1);
                    }
                    break;

                case "Mission 2":
                    if (getMissionStatus(2) == true)
                    {
                        endMission(2);
                    }
                    break;
            }

            GameEventManager.Call_LeavingScene(this);

            // load warped to scene
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(args.warpToScenePath, LoadSceneMode.Additive);

            // Wait until the asynchronous scene fully loads
            while (!asyncLoad.isDone)
            {
                // note: could put some sort of warp tunnel screensaver effect in here
                yield return null;
            }

            // unparent GameObjects that need to be moved (only works with root objects)
            CampaignManager.Instance.gameObject.transform.parent = null;
            FactionManager.Instance.gameObject.transform.parent = null;
            args.warpShipGO.transform.parent = null;

            // move objects to new scene
            Scene warpScene = SceneManager.GetSceneByPath(args.warpToScenePath);

            SceneManager.MoveGameObjectToScene(CampaignManager.Instance.gameObject, warpScene);
            SceneManager.MoveGameObjectToScene(FactionManager.Instance.gameObject, warpScene);
            SceneManager.MoveGameObjectToScene(args.warpShipGO, warpScene);

            if (args.warpPosition.HasValue)
            {
                args.warpShipGO.transform.position = args.warpPosition.GetValueOrDefault();
            }            

            // pause for a frame to ensure this completes (this is the trick bit)
            yield return null;

            //Unload the previous Scene (note: can't do anything after this as the warp gate gameobject no longer exists - see the CampaignManager OnWarped event handler)
            AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(SceneManager.GetSceneByName(currentSceneName));

            while (!asyncUnload.isDone)
            {
                yield return null;
            }

            // start the process of reparenting objects
            List<GameObject> rootGOs = warpScene.GetRootGameObjects().ToList();

            GameObject managers = rootGOs.Where(obj => obj.name == "Managers").SingleOrDefault();

            CampaignManager.Instance.gameObject.transform.parent = managers.transform;
            FactionManager.Instance.gameObject.transform.parent = managers.transform;

            GameObject hierarchy = GameObject.Find("Placeables");

            args.warpShipGO.transform.parent = hierarchy.transform;

            // restart the Game Mananger for the warped to scene

            /*

            Transform gameManagerTrans = managers.transform.Find("Game Manager");

            GameManager gameManager = gameManagerTrans.GetComponent<GameManager>();
            gameManager.reset();
            gameManager.Start();

            */
        }
    }
}
