using System.Linq;
using System.Threading.Tasks;
using GVR.Creator;
using GVR.Phoenix.Interface;
using GVR.Util;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

namespace GVR.Manager
{
    public class GvrSceneManager : SingletonBehaviour<GvrSceneManager>
    {
        private const string generalSceneName = "General";

        private string newWorldName;
        private string startVobAfterLoading;

        private Scene generalScene;
        private bool generalSceneLoaded = false;

        private GameObject bar;

        private GameObject startPoint;

        public GameObject interactionManager;

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
            await ShowLoadingScene(worldName);
            newWorldName = worldName;
            startVobAfterLoading = startVob;
            var asyncLoad = SceneManager.LoadSceneAsync(worldName, new LoadSceneParameters(LoadSceneMode.Additive));
            Scene newWorldScene;

            // Remove previous scene.
            // TODO - it might be, that we need to wait for old map to be removed before loading new one. Let's see...
            if (GameData.I.WorldScene.HasValue)
            {
                // Try to delete everything in old scene, so we can load everything again from scratch
                var objects = GameData.I.WorldScene.Value.GetRootGameObjects();
                for (int i = 0; i < objects.Length; i++)
                {
                    GameObject.Destroy(objects[i]);
                }
                SceneManager.UnloadSceneAsync(GameData.I.WorldScene.Value);
            }

            asyncLoad.completed += async (asyncOperation) =>
            {
                newWorldScene = SceneManager.GetSceneByName(newWorldName);

                GameData.I.WorldScene = newWorldScene;

                var worldGo = await WorldCreator.I.Create(newWorldName, progress =>
                {
                    UpdateLoadingBar(progress);
                });

                SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) =>
                {
                    if (scene.name == newWorldName)
                        SceneManager.SetActiveScene(SceneManager.GetSceneByName(newWorldName));
                };
                // Delay for one frame to make sure that the scene can be set active successfully
                await Task.Delay(1);

                if (worldGo)
                {
                    SceneManager.MoveGameObjectToScene(worldGo, newWorldScene);
                    FindSpot(newWorldScene);
                    HideLoadingScene();
                }
            };
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

        private async Task ShowLoadingScene(string worldName = null)
        {
            generalScene = SceneManager.GetSceneByName(generalSceneName);
            if (generalScene.isLoaded)
            {
                SceneManager.MoveGameObjectToScene(interactionManager, SceneManager.GetSceneByName("Bootstrap"));
                SceneManager.UnloadSceneAsync(generalScene);
                generalSceneLoaded = false;
            }

            // set the loading background texture properly
            if (worldName != null)
            {
            // TODO: for new game we need to load texture "LOADING.TGA"
                var textureString = "LOADING_" + worldName.Split('.')[0].ToUpper() + ".TGA";
                UIManager.I.setTexture(textureString, UIManager.I.GothicLoadingMenuMaterial);
            }

            var loadingScene = SceneManager.LoadScene("Loading", new LoadSceneParameters(LoadSceneMode.Additive));

            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) =>
            {
                if (scene == loadingScene)
                {
                    bar = loadingScene.GetRootGameObjects().FirstOrDefault(go => go.name == "VRPlayer_v4 (romey)").transform.Find("LoadingCanvas/LoadingImage/ProgressBackground/ProgressBar").gameObject;
                }
            };
            // Delay for magic number amount to make sure that bar can be found
            // 1 and 2 caused issues for the 3rd time showing the loading scene in editor
            await Task.Delay(5);
        }

        private void UpdateLoadingBar(float progress)
        {
            bar.GetComponent<Image>().fillAmount = progress;
        }

        private void HideLoadingScene()
        {
            SceneManager.UnloadSceneAsync("Loading");

            SceneManager.sceneUnloaded += (Scene scene) =>
            {
                //this is a fix to make sure that we load General Scene only once
                if (scene.name == "Loading" && !generalSceneLoaded)
                {
                    generalScene = SceneManager.LoadScene(generalSceneName, new LoadSceneParameters(LoadSceneMode.Additive));
                    generalSceneLoaded = true;
                }
            };

            SceneManager.sceneLoaded += (Scene scene, LoadSceneMode mode) =>
            {
                if (scene == generalScene)
                {
                    SceneManager.MoveGameObjectToScene(interactionManager, generalScene);

                    WorldCreator.I.PostCreate(interactionManager.GetComponent<XRInteractionManager>());

                    var playerParent = scene.GetRootGameObjects().FirstOrDefault(go => go.name == "PlayerController");
                    playerParent.transform.Find("VRPlayer_v4 (romey)").transform.position = startPoint.transform.position;

                    return;
                }
            };
        }
    }
}