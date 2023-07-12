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
        
        /// <summary>
        /// Called once after bootstrapping scene is done.
        /// Then load either menu or a world defined inside DebugSettings.
        /// </summary>
        public void LoadStartupScenes()
        {
            SceneManager.LoadScene(generalSceneName, LoadSceneMode.Additive);
            
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
            
            SceneManager.sceneLoaded += WorldSceneLoaded;
            SceneManager.activeSceneChanged += ActiveSceneChanged;
        }

        /// <summary>
        /// We need to set world scene loaded as active scene. This is the only way OcclusionCulling data is fetched
        /// for the world in a multi-scene scenario.
        /// </summary>
        private void WorldSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.SetActiveScene(scene);
            
            WorldCreator.I.Create(newWorldName);
        }
        
        /// <summary>
        /// Subscribe the SetActiveScene method so wen can properly place the player in the correct spot.
        /// </summary>
        private void ActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            GameObject.Find("VRPlayer_v4 (romey)").transform.position = GameObject.Find(startVobAfterLoading).transform.position;
        }
    }
}