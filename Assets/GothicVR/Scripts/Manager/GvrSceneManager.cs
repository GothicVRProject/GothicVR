using System.Linq;
using GVR.Creator;
using GVR.Phoenix.Interface;
using GVR.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;

namespace GVR.Manager
{
    public class GvrSceneManager : SingletonBehaviour<GvrSceneManager>
    {
        private const string generalSceneName = "General";

        private string newWorldName;
        private string startVobAfterLoading;
        private Scene generalScene;

        private GameObject player;

        public GameObject interactionManager;

        /// <summary>
        /// Called once after bootstrapping scene is done.
        /// Then load either menu or a world defined inside DebugSettings.
        /// </summary>
        public void LoadStartupScenes()
        {
            generalScene = SceneManager.LoadScene(generalSceneName, new LoadSceneParameters(LoadSceneMode.Additive));
            SceneManager.MoveGameObjectToScene(interactionManager, generalScene);

            LoadWorld("world", "FP_PICKRICE_SWAMP_02");
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
        /// for the world in a multi-scene scenario. Also set the player to we have a reference regardless of scene
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
            if (newScene == GameData.I.WorldScene)
            {
                // we set manually the XR Interaction Manager as Unity creates a new one in Bootstrap 
                // and we don't want to use it since we have one in General scene
                WorldCreator.I.PostCreate(interactionManager.GetComponent<XRInteractionManager>());
                GameObject startPoint = null;
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
                        if ((spots[i].name == "START" || spots[i].name == "START_GOTHIC2") && spots[i].scene == newScene)
                        {
                            startPoint = spots[i];
                        }
                    }
                }
                if (startPoint != null)
                    player.transform.position = startPoint.transform.position;
            }
        }
    }
}