using System.Linq;
using System.Threading.Tasks;
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
        private bool generalSceneLoaded = false;

        private GameObject startPoint;

        public GameObject interactionManager;

        private const int EnsureLoadingBarDelayMilliseconds = 5;

        /// <summary>
        /// Called once after bootstrapping scene is done.
        /// Then load either menu or a world defined inside DebugSettings.
        /// </summary>
        public void LoadStartupScenes()
        {
            LoadWorld("oldmine.zen", "ENTRANCE_SURFACE_OLDMINE");
            // PxCs.Interface.PxVm.CallFunction(GameData.I.VmGothicPtr, "STARTUP_SUB_OLDCAMP"); FP_GUARD_A_OC_179
        }

        public async void LoadWorld(string worldName, string startVob)
        {
            SceneManagerActionHandler();

            await ShowLoadingScene(worldName);

            newWorldName = worldName;
            startVobAfterLoading = startVob;

            LoadNewWorldScene(newWorldName);
            await CreateWorld();

            HideLoadingScene();
        }

        private void LoadNewWorldScene(string worldName)
        {
            var newWorldScene = SceneManager.LoadScene(worldName, new LoadSceneParameters(LoadSceneMode.Additive));

            // Remove previous scene if it exists
            if (GameData.I.WorldScene.HasValue)
            {
                SceneManager.UnloadSceneAsync(GameData.I.WorldScene.Value);
            }

            GameData.I.WorldScene = newWorldScene;
        }

        private async Task CreateWorld()
        {
            var newWorldScene = SceneManager.GetSceneByName(newWorldName);

            var worldGo = await WorldCreator.I.Create(newWorldName);

            // Delay for one frame to allow the scene to be set active successfully
            await Task.Delay(1);

            if (worldGo)
            {
                SceneManager.MoveGameObjectToScene(worldGo, newWorldScene);
                FindSpot(newWorldScene);
            }
        }

        private async Task ShowLoadingScene(string worldName = null)
        {
            generalScene = SceneManager.GetSceneByName(generalSceneName);
            if (generalScene.isLoaded)
            {
                SceneManager.MoveGameObjectToScene(interactionManager, SceneManager.GetSceneByName("Bootstrap"));
                SceneManager.UnloadSceneAsync(generalScene);
                generalSceneLoaded = false;
            }

            SetLoadingTextureForWorld(worldName);

            var loadingScene = SceneManager.LoadScene("Loading", new LoadSceneParameters(LoadSceneMode.Additive));

            // Delay for magic number amount to make sure that bar can be found
            // 1 and 2 caused issues for the 3rd time showing the loading scene in editor
            await Task.Delay(EnsureLoadingBarDelayMilliseconds);
        }

        private void SetLoadingTextureForWorld(string worldName)
        {
            if (worldName == null)
                return;

            // set the loading background texture properly
            // TODO: for new game we need to load texture "LOADING.TGA"
            var textureString = "LOADING_" + worldName.Split('.')[0].ToUpper() + ".TGA";
            UIManager.I.SetTexture(textureString, UIManager.I.GothicLoadingMenuMaterial);
        }

        private void HideLoadingScene()
        {
            SceneManager.UnloadSceneAsync("Loading");

            LoadingManager.I.ResetProgress();
        }

        private void SceneManagerActionHandler()
        {
            SceneManager.sceneLoaded += OnWorldSceneLoaded;

            SceneManager.sceneUnloaded += OnLoadingSceneUnloaded;
        }
        private void OnWorldSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == newWorldName)
            {
                SceneManager.SetActiveScene(scene);
            }
            if (scene == generalScene)
            {
                SceneManager.MoveGameObjectToScene(interactionManager, generalScene);

                WorldCreator.I.PostCreate(interactionManager.GetComponent<XRInteractionManager>());

                var playerParent = scene.GetRootGameObjects().FirstOrDefault(go => go.name == "PlayerController");
                playerParent.transform.Find("VRPlayer_v4 (romey)").transform.position = startPoint.transform.position;
            }
            if (scene.name == "Loading")
            {
                LoadingManager.I.SetBarFromScene(scene);
                LoadingManager.I.SetMaterialForLoading(scene);
            }
        }

        private void OnLoadingSceneUnloaded(Scene scene)
        {
            if (scene.name == "Loading" && !generalSceneLoaded)
            {
                generalScene = SceneManager.LoadScene(generalSceneName, new LoadSceneParameters(LoadSceneMode.Additive));
                generalSceneLoaded = true;
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