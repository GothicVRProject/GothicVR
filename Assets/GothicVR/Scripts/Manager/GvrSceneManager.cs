using System.Linq;
using GVR.Creator;
using GVR.Phoenix.Interface;
using GVR.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.Events;

namespace GVR.Manager
{
    public class GvrSceneManager : SingletonBehaviour<GvrSceneManager>
    {
        private const string generalSceneName = "General";

        private string newWorldName;
        private string startVobAfterLoading;
        private Scene generalScene;

        private GameObject startPoint;

        public GameObject interactionManager;

        /// <summary>
        /// Called once after bootstrapping scene is done.
        /// Then load either menu or a world defined inside DebugSettings.
        /// </summary>
        public void LoadStartupScenes()
        {
            LoadWorld("world.zen", "OC");
            // PxCs.Interface.PxVm.CallFunction(GameData.I.VmGothicPtr, "STARTUP_SUB_OLDCAMP");
        }

        public async void LoadWorld(string worldName, string startVob)
        {
            if (generalScene.isLoaded)
            {
                SceneManager.MoveGameObjectToScene(interactionManager, SceneManager.GetSceneByName("Bootstrap"));
                SceneManager.UnloadSceneAsync(generalScene);
            }
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

            var worldGo = await WorldCreator.I.Create(newWorldName);
            generalScene = SceneManager.LoadScene(generalSceneName, new LoadSceneParameters(LoadSceneMode.Additive));

            SceneManager.MoveGameObjectToScene(worldGo, newWorldScene);

            FindSpot(newWorldScene);

            if (worldGo)
            {
                SceneManager.SetActiveScene(newWorldScene);
            }

            SceneManager.sceneLoaded += WorldSceneLoaded;
        }

        /// <summary>
        /// We need to set the player's position.
        /// </summary>
        private void WorldSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene == generalScene)
            {
                SceneManager.MoveGameObjectToScene(interactionManager, generalScene);

                WorldCreator.I.PostCreate(interactionManager.GetComponent<XRInteractionManager>());

                var playerParent = scene.GetRootGameObjects().FirstOrDefault(go => go.name == "PlayerController");
                playerParent.transform.Find("VRPlayer_v4 (romey)").transform.position = startPoint.transform.position;

                return;
            }
        }

        private void FindSpot(Scene worldScene)
        {
            var spots = GameObject.FindGameObjectsWithTag("PxVob_zCVobSpot");
            for (int i = 0; i < spots.Length; i++)
            {
                if (spots[i].name == startVobAfterLoading)
                {
                    startPoint = spots[i];
                }
            }
            if (startPoint == null)
            {
                for (int i = 0; i < spots.Length; i++)
                {
                    if ((spots[i].name == "START" || spots[i].name == "START_GOTHIC2") && spots[i].scene == worldScene)
                    {
                        startPoint = spots[i];
                    }
                }
            }
        }
    }
}