using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using GVR.Creator;
using GVR.Debugging;
using GVR.Phoenix.Interface;
using GVR.Util;
using PxCs.Interface;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using Debug = UnityEngine.Debug;

namespace GVR.Manager
{
    public class GvrSceneManager : SingletonBehaviour<GvrSceneManager>
    {
        public static UnityEvent StartWorldLoading = new(); // Basically to clear caches etc.

        public GameObject interactionManager;
        
        private const string generalSceneName = "General";
        private const int ensureLoadingBarDelayMilliseconds = 5;

        private string newWorldName;
        private string startVobAfterLoading;
        private Scene generalScene;
        private bool generalSceneLoaded;
        private GameObject startPoint;
        private GameObject player;
        
        
        protected override void Awake()
        {
            base.Awake();

            SceneManager.sceneLoaded += OnWorldSceneLoaded;
            SceneManager.sceneUnloaded += OnLoadingSceneUnloaded;
        }

        /// <summary>
        /// Called once after bootstrapping scene is done.
        /// Then load either menu or a world defined inside DebugSettings.
        /// </summary>
        public async Task LoadStartupScenes()
        {
            try
            {
                if (FeatureFlags.I.SkipMainMenu)
                    await LoadWorld(ConstantsManager.I.selectedWorld, ConstantsManager.I.selectedWaypoint);
                else
                    await LoadMainMenu();

                if (FeatureFlags.I.CreateOcNpcs)
                    PxVm.CallFunction(GameData.I.VmGothicPtr, "STARTUP_SUB_OLDCAMP");

                if (FeatureFlags.I.CreateDebugIdleAnimations)
                    NpcCreator.I.DebugAddIdleAnimationToAllNpc();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }

        private async Task LoadMainMenu()
        {
            TextureManager.I.LoadLoadingDefaultTextures();
            await LoadNewWorldScene("MainMenu");
        }

        public async Task LoadWorld(string worldName, string startVob)
        {
            startVobAfterLoading = startVob;
            
            if (worldName == newWorldName)
            {
                SetSpawnPoint(SceneManager.GetSceneByName(newWorldName));
                TeleportPlayerToSpot();
                return;
            }
            
            newWorldName = worldName;
            MusicCreator.I.setMusic("SYS_LOADING");
            var watch = Stopwatch.StartNew();

            StartWorldLoading.Invoke();
            
            await ShowLoadingScene(worldName);
            var newWorldScene = await LoadNewWorldScene(newWorldName);
            await WorldCreator.I.CreateAsync(newWorldName);
            SetSpawnPoint(newWorldScene);

            HideLoadingScene();
            watch.Stop();
            Debug.Log($"Time spent for loading {worldName}: {watch.Elapsed}");
        }

        private async Task<Scene> LoadNewWorldScene(string worldName)
        {
            var newWorldScene = SceneManager.LoadScene(worldName, new LoadSceneParameters(LoadSceneMode.Additive));

            // Delay for at least one frame to allow the scene to be set active successfully
            // i.e. created GOs will be automatically put to right scene afterwards.
            await Task.Yield();

            // Remove previous scene if it exists
            if (GameData.I.WorldScene.HasValue)
                SceneManager.UnloadSceneAsync(GameData.I.WorldScene.Value);

            GameData.I.WorldScene = newWorldScene;
            return newWorldScene;
        }

        /// <summary>
        /// Create loading scene and wait for a few milliseconds to go on, ensuring loading bar is selectable.
        /// Async: execute in sync, but whole process can be paused for x amount of frames.
        /// </summary>
        private async Task ShowLoadingScene(string worldName = null)
        {
            TextureManager.I.LoadLoadingDefaultTextures();

            generalScene = SceneManager.GetSceneByName(generalSceneName);
            if (generalScene.isLoaded)
            {
                SceneManager.MoveGameObjectToScene(interactionManager, SceneManager.GetSceneByName("Bootstrap"));
                SceneManager.UnloadSceneAsync(generalScene);
                generalSceneLoaded = false;
            }

            SetLoadingTextureForWorld(worldName);

            SceneManager.LoadScene("Loading", new LoadSceneParameters(LoadSceneMode.Additive));

            // Delay for magic number amount to make sure that bar can be found
            // 1 and 2 caused issues for the 3rd time showing the loading scene in editor
            await Task.Delay(ensureLoadingBarDelayMilliseconds);
        }

        private void SetLoadingTextureForWorld(string worldName)
        {
            if (worldName == null)
                return;

            // set the loading background texture properly
            // TODO: for new game we need to load texture "LOADING.TGA"
            var textureString = "LOADING_" + worldName.Split('.')[0].ToUpper() + ".TGA";
            TextureManager.I.SetTexture(textureString, TextureManager.I.GothicLoadingMenuMaterial);
        }

        private void HideLoadingScene()
        {
            SceneManager.UnloadSceneAsync("Loading");

            LoadingManager.I.ResetProgress();
        }

        private void OnWorldSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Loading")
            {
                LoadingManager.I.SetBarFromScene(scene);
                LoadingManager.I.SetMaterialForLoading(scene);
                AudioSourceManager.I.ResetDictionaries();
            }
            else if (scene == generalScene)
            {
                AudioSourceManager.I.SetAudioListener(Camera.main!.GetComponent<AudioListener>());

                SceneManager.MoveGameObjectToScene(interactionManager, generalScene);

                WorldCreator.I.PostCreate(interactionManager.GetComponent<XRInteractionManager>());

                TeleportPlayerToSpot();
            }
            else if (scene.name == "MainMenu")
            {
                var sphere = scene.GetRootGameObjects().FirstOrDefault(go => go.name == "LoadingSphere");
                sphere.GetComponent<MeshRenderer>().material = TextureManager.I.LoadingSphereMaterial;
                SceneManager.SetActiveScene(scene);
            }
            else
            {
                SceneManager.SetActiveScene(scene);
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

        private void SetSpawnPoint(Scene worldScene)
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

        public void MoveToWorldScene(GameObject go)
        {
            GameData.I.WorldScene!.Value.GetRootGameObjects().Append(go);
            SceneManager.MoveGameObjectToScene(go, SceneManager.GetSceneByName(GameData.I.WorldScene.Value.name));
        }

        private void SetPlayer()
        {
            player = generalScene.GetRootGameObjects().FirstOrDefault(go => go.name == "PlayerController").transform.Find("VRPlayer").gameObject;
        }

        public void TeleportPlayerToSpot()
        {
            if (player == null)
                SetPlayer();

            if (startPoint != null)
            {
                player.transform.position = startPoint.transform.position;
                player.transform.rotation = startPoint.transform.rotation;
            }
        }
    }
}