using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using NoxCore.Utilities;

namespace NoxCore.Managers
{
    public abstract class StreamingCampaignManager : MonoBehaviour
    {
        public List<SceneReference> scenes = new List<SceneReference>();

        [ShowOnly]
        public string prevScenePath;

        [ShowOnly]
        public string currentScenePath;

        protected virtual void init()
        {
            FindObjectOfType<GameManager>().Start();

            loadScene(scenes[0], 0, true, false);
        }

        public virtual void OnEnable()
        {
            GameEventManager.EnteringScene += OnEnteringScene;
            GameEventManager.MatchIsWaitingToStart += OnMatchIsWaitingToStart;
            GameEventManager.MatchHasStarted += OnMatchHasStarted;
            GameEventManager.MatchHasEnded += OnMatchHasEnded;
            GameEventManager.LeavingScene += OnLeavingScene;
            GameEventManager.AbortedMatch += OnAbortedMatch;

            SceneManager.sceneLoaded += OnSceneFinishedLoading;
            SceneManager.sceneUnloaded += OnSceneFinishedUnloading;

            Debug.Log("Streaming Campaign Manager enabled");
        }

        public virtual void OnDisable()
        {
            GameEventManager.EnteringScene -= OnEnteringScene;
            GameEventManager.MatchIsWaitingToStart -= OnMatchIsWaitingToStart;
            GameEventManager.MatchHasStarted -= OnMatchHasStarted;
            GameEventManager.MatchHasEnded -= OnMatchHasEnded;
            GameEventManager.LeavingScene -= OnLeavingScene;
            GameEventManager.AbortedMatch -= OnAbortedMatch;

            SceneManager.sceneLoaded -= OnSceneFinishedLoading;
            SceneManager.sceneUnloaded -= OnSceneFinishedUnloading;

            Debug.Log("Streaming Campaign Manager disabled");
        }

        #region load
        public void loadScene(SceneReference scene, float delay = 0, bool setActive = false, bool unload = true)
        {
            Debug.Log("Loading scene: " + scene);

            D.log("Loading scene: " + scene);

            prevScenePath = SceneManager.GetActiveScene().path;

            StartCoroutine(load(scene, delay, setActive, unload));
        }

        public void loadScene(string scenePath, float delay = 0, bool setActive = false, bool unload = true)
        {
            Debug.Log("Loading scene: " + scenePath);

            D.log("Loading scene: " + scenePath);

            prevScenePath = SceneManager.GetActiveScene().path;

            StartCoroutine(load(scenePath, delay, setActive, unload));
        }

        protected IEnumerator load(SceneReference scene, float delay, bool setActive = false, bool unload = true)
        {
            if (delay > 0) yield return new WaitForSeconds(delay);

            AudioListener audioListener = FindObjectOfType<AudioListener>();
            
            if (audioListener != null)
            {
                audioListener.enabled = false;
            }

            AsyncOperation sceneLoading = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

            while (!sceneLoading.isDone)
            {
                Debug.Log(sceneLoading.progress);

                yield return null;
            }

            if (setActive == true)
            {
                Scene loadedScene = SceneManager.GetSceneByPath(scene.ScenePath);
                SceneManager.SetActiveScene(loadedScene);
            }

            yield return new WaitForEndOfFrame();

            if (unload == true)
            {
                unloadScene(prevScenePath);
            }
        }

        protected IEnumerator load(string scenePath, float delay, bool setActive = false, bool unload = true)
        {
            if (delay > 0) yield return new WaitForSeconds(delay);

            AudioListener audioListener = FindObjectOfType<AudioListener>();

            if (audioListener != null)
            {
                audioListener.enabled = false;
            }

            AsyncOperation sceneLoading = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);

            while (!sceneLoading.isDone)
            {
                Debug.Log(scenePath + " loading. Progress: " + sceneLoading.progress);

                yield return null;
            }

            if (setActive == true)
            {
                Scene loadedScene = SceneManager.GetSceneByPath(scenePath);
                SceneManager.SetActiveScene(loadedScene);
            }

            if (unload == true)
            {
                unloadScene(prevScenePath);
            }
        }
        #endregion

        #region unload
        public void unloadScene(SceneReference scene, float delay = 0)
        {
            D.log("Unloading scene: " + scene);

            StartCoroutine(unload(scene, delay));
        }

        public void unloadScene(string scenePath, float delay = 0)
        {
            Debug.Log("Unloading scene: " + scenePath);

            StartCoroutine(unload(scenePath, delay));
        }

        protected IEnumerator unload(SceneReference scene, float delay = 0)
        {
            if (delay > 0) yield return new WaitForSeconds(delay);

            AsyncOperation sceneUnloading = SceneManager.UnloadSceneAsync(scene);

            while (!sceneUnloading.isDone)
            {
                Debug.Log(sceneUnloading.progress);

                yield return null;
            }
        }

        protected IEnumerator unload(string scenePath, float delay = 0)
        {
            if (delay > 0) yield return new WaitForSeconds(delay);

            AsyncOperation sceneUnloading = SceneManager.UnloadSceneAsync(scenePath);

            while (!sceneUnloading.isDone)
            {
                Debug.Log(scenePath + " unloading. Progress: " + sceneUnloading.progress);

                yield return null;
            }
        }
        #endregion

        public virtual void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
        {
            Debug.Log("Streaming Campaign Manager - OnSceneFinishedLoading: " + scene.name);

            D.log("Scene: " + scene.name + " loaded");

            currentScenePath = scene.path;
        }

        public virtual void OnSceneFinishedUnloading(Scene scene)
        {
            Debug.Log("Streaming Campaign Manager - OnSceneFinishedUnloading: " + scene.name);

            D.log("Scene: " + scene.name + " unloaded");
        }

        public virtual void OnEnteringScene(object sender)
        {}

        public virtual void OnMatchIsWaitingToStart(object sender)
        {}

        public virtual void OnMatchHasStarted(object sender)
        {}

        public virtual void OnMatchHasEnded(object sender)
        {}

        public virtual void OnLeavingScene(object sender)
        {}

        public virtual void OnAbortedMatch(object sender)
        {}
    }
}