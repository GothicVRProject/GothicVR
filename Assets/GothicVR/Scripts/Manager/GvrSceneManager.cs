using System;
using System.ComponentModel;
using System.Linq;
using GVR.Creator;
using GVR.Phoenix.Interface;
using GVR.Util;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GVR.Manager
{
    public class GvrSceneManager : SingletonBehaviour<GvrSceneManager>
    {
        private const string generalSceneName = "General";

        private string newWorldName;
        private string startVobAfterLoading;
        private Scene generalScene;

        private GameObject player;

        /// <summary>
        /// Called once after bootstrapping scene is done.
        /// Then load either menu or a world defined inside DebugSettings.
        /// </summary>
        public void LoadStartupScenes()
        {
            generalScene = SceneManager.LoadScene(generalSceneName, new LoadSceneParameters(LoadSceneMode.Additive));

            LoadWorld("world", "ENTRANCE_SURFACE_OLDMINE");
        }

        public void LoadWorld(string worldName, string startVob)
        {
            newWorldName = worldName;
            startVobAfterLoading = startVob;
            var newWorldScene = SceneManager.LoadScene(worldName, new LoadSceneParameters(LoadSceneMode.Additive));

            // Remove previous scene.
            // TODO - it might be, that we need to wait for old map to be removed before loading new one. Let's see...
            if (GameData.I.WorldScene.HasValue)
            {
                SceneManager.UnloadSceneAsync(GameData.I.WorldScene.Value);
            }

            GameData.I.WorldScene = newWorldScene;

            WorldCreator.I.Create(newWorldName);

            SceneManager.sceneLoaded += WorldSceneLoaded;
            SceneManager.activeSceneChanged += ActiveSceneChanged;
        }

        /// <summary>
        /// We need to set world scene loaded as active scene. This is the only way OcclusionCulling data is fetched
        /// for the world in a multi-scene scenario.
        /// </summary>
        private void WorldSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene == generalScene)
            {
                var playerParent = generalScene.GetRootGameObjects().FirstOrDefault(o => o.name == "PlayerController");
                player = playerParent.transform.Find("VRPlayer_v4 (romey)").gameObject;
                return;
            }
            SceneManager.SetActiveScene(scene);
        }

        /// <summary>
        /// Subscribe the SetActiveScene method so wen can properly place the player in the correct spot.
        /// </summary>
        private void ActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            // TODO: Dont use GameObject.Find as it will be more computationally expensive in the future with more GO to check
            if (newScene == GameData.I.WorldScene)
                player.transform.position = GameObject.Find(startVobAfterLoading).transform.position;
        }
    }
}