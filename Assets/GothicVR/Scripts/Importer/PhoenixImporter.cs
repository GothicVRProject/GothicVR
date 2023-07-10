using AOT;
using GVR.Creator;
using GVR.Demo;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Interface.Vm;
using GVR.Settings;
using GVR.Util;
using PxCs.Interface;
using System;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;

namespace GVR.Importer
{
    public class PhoenixImporter : SingletonBehaviour<PhoenixImporter>
    {
        private bool _loaded = false;
        private static DebugSettings _debugSettings;

        private void Start()
        {
            _debugSettings = SingletonBehaviour<DebugSettings>.GetOrCreate();

            VmGothicBridge.DefaultExternalCallback.AddListener(MissingVmExternalCall);
            PxLogging.pxLoggerSet(PxLoggerCallback);
        }

        private void Update()
        {
            // Load after Start() so that other MonoBehaviours can subscribe to DaedalusVM events.
            if (_loaded)
                return;
            _loaded = true;

            var watch = System.Diagnostics.Stopwatch.StartNew();

            var G1Dir = SingletonBehaviour<SettingsManager>.GetOrCreate().GameSettings.GothicIPath;

            var fullPath = Path.GetFullPath(Path.Join(G1Dir, "Data"));
            var vdfPtr = VdfsBridge.LoadVdfsInDirectory(fullPath);

            LoadGothicVM(G1Dir);
            LoadSfxVM(G1Dir);
            LoadMusicVM(G1Dir);
            LoadWorld(vdfPtr, "world");
            LoadMusic();

            // PxVm.CallFunction(PhoenixBridge.VmGothicPtr, "STARTUP_SUB_OLDCAMP"); // Goal: Spawn Bloodwyn ;-)        
            //LoadFonts();

            watch.Stop();
            Debug.Log($"Time spent for loading world + VM + npc loading: {watch.Elapsed}");
        }


        public static void MissingVmExternalCall(IntPtr vmPtr, string missingCallbackName)
        {
            Debug.LogWarning($"Method >{missingCallbackName}< not yet implemented in DaedalusVM.");
        }

        [MonoPInvokeCallback(typeof(PxLogging.PxLogCallback))]
        public static void PxLoggerCallback(PxLogging.Level level, string message)
        {
            switch (level)
            {
                case PxLogging.Level.warn:
                    Debug.LogWarning(message);
                    break;
                case PxLogging.Level.error:
                    bool isVdfMessage = message.StartsWith("failed to find vdf entry");
                    if (isVdfMessage && !_debugSettings.ShowVdfsFileNotFoundErrors)
                        break;

                    Debug.LogError(message);
                    break;
            }
        }

        /// <summary>
        /// Loads the world.
        /// </summary>
        /// <param name="vdfPtr">The VDF pointer.</param>
        /// <param name="zen">The name of the .zen world to load.</param>
        public void LoadWorld(IntPtr vdfPtr, string zen)
        {
            var worldScene = SceneManager.GetSceneByName(zen);

            if (!worldScene.isLoaded)
            {
                // unload the current scene and load the new one
                SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
                SceneManager.LoadScene(zen, LoadSceneMode.Additive);
                worldScene = SceneManager.GetSceneByName(zen); // we do this to reload the values for the new scene which are no updated for the above cast
            }

            var world = WorldBridge.LoadWorld(vdfPtr, $"{zen}.zen"); // world.zen -> G1, newworld.zen/oldworld.zen/addonworld.zen -> G2

            PhoenixBridge.VdfsPtr = vdfPtr;
            PhoenixBridge.World = world;

            var worldGo = new GameObject("World");

            // We use SampleScene because it contains all the VM pointers and asset cache necesarry to generate the world
            var sampleScene = SceneManager.GetSceneByName("SampleScene");
            SceneManager.SetActiveScene(sampleScene);
            sampleScene.GetRootGameObjects().Append(worldGo);

            var worldMesh = SingletonBehaviour<MeshCreator>.GetOrCreate().Create(world, worldGo);
            SingletonBehaviour<VobCreator>.GetOrCreate().Create(worldGo, world);
            SingletonBehaviour<WaynetCreator>.GetOrCreate().Create(worldGo, world);
            SingletonBehaviour<WorldCreator>.GetOrCreate().PostCreate(worldMesh);

            SingletonBehaviour<DebugAnimationCreator>.GetOrCreate().Create();

            // move the world to the correct scene
            SceneManager.MoveGameObjectToScene(worldGo, worldScene);

            // Subscribe the SetActiveScene method to the sceneLoaded event
            // so that we can set the proper scene as active when the scene is finally loaded
            // is related to occlusion culling
            SceneManager.sceneLoaded += SetActiveScene;
        }
                // if the world scene is already loaded it could mean that we have a world loaded in
                // so we delete everything :D
                if (worldScene.GetRootGameObjects().Length != 0)
                    foreach (var item in worldScene.GetRootGameObjects())
                    {
                        GameObject.Destroy(item);
                    }
            }

            var world = WorldBridge.LoadWorld(vdfPtr, $"{zen}.zen"); // world.zen -> G1, newworld.zen/oldworld.zen/addonworld.zen -> G2

            PhoenixBridge.VdfsPtr = vdfPtr;
            PhoenixBridge.World = world;


            var scene = SceneManager.GetSceneByName("SampleScene");
            scene.GetRootGameObjects().Append(worldGo);
            
            SingletonBehaviour<MeshCreator>.GetOrCreate().Create(world);
            SingletonBehaviour<VobCreator>.GetOrCreate().Create(worldGo, world);
            SingletonBehaviour<WaynetCreator>.GetOrCreate().Create(worldGo, world);
            SingletonBehaviour<WorldCreator>.GetOrCreate().PostCreate();

            SingletonBehaviour<DebugAnimationCreator>.GetOrCreate().Create();

            // we are creating the objects directly in SampleScene as we have the asset cache 
            // and everything we need to generate the world
            // and after that move everything to world scene
            SceneManager.MoveGameObjectToScene(root, SceneManager.GetSceneByName("world"));

        private void SetActiveScene(Scene scene, LoadSceneMode mode)
        {
            // just start position to for each world
            // as we need to have a starting position in the new world
            var startPosition = "";
            if (scene.name == "world")
            {
                startPosition = "ENTRANCE_SURFACE_OLDMINE";
            }
            if (scene.name == "oldmine" || scene.name == "freemine")
            {
                startPosition = "ENTRANCE_OLDMINE_SURFACE";
            }
            if (scene.name == "orcgraveyard")
            {
                startPosition = "ENTRANCE_ORCGRAVEYARD_SURFACE";
            }
            if (scene.name == "orctempel")
            {
                startPosition = "ENTRANCE_ORCTEMPLE_SURFACE";
            }
            GameObject.Find("VRPlayer_v4 (romey)").transform.position = GameObject.Find(startPosition).transform.position;

            Debug.Log(scene.name + " " + startPosition);

            SceneManager.SetActiveScene(scene);
        }

        private void LoadGothicVM(string G1Dir)
        {
            var fullPath = Path.GetFullPath(Path.Join(G1Dir, "/_work/DATA/scripts/_compiled/GOTHIC.DAT"));
            var vmPtr = VmGothicBridge.LoadVm(fullPath);

            VmGothicBridge.RegisterExternals(vmPtr);

            PhoenixBridge.VmGothicPtr = vmPtr;
        }

        private void LoadSfxVM(string G1Dir)
        {
            var fullPath = Path.GetFullPath(Path.Join(G1Dir, "/_work/DATA/scripts/_compiled/SFX.DAT"));
            var vmPtr = VmGothicBridge.LoadVm(fullPath);
            PhoenixBridge.VmSfxPtr = vmPtr;
        }

        private void LoadMusicVM(string G1Dir)
        {
            var fullPath = Path.GetFullPath(Path.Join(G1Dir, "/_work/DATA/scripts/_compiled/MUSIC.DAT"));
            var vmPtr = VmGothicBridge.LoadVm(fullPath);
            PhoenixBridge.VmMusicPtr = vmPtr;
        }

        private void LoadMusic()
        {
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().EnableMusic)
                return;
            var music = SingletonBehaviour<MusicCreator>.GetOrCreate();
            music.Create();
            music.setEnabled(true);
            music.setMusic("SYS_LOADING");
            Debug.Log("Loading music");
        }

        /// <summary>
        /// If there are Gothic ttf fonts stored on the current system, we will use them.
        /// If not, we will stick with a default font.
        /// </summary>
        private void LoadFonts()
        {
            var menuFontPath = SingletonBehaviour<SettingsManager>.GetOrCreate().GameSettings.GothicMenuFontPath;
            var subtitleFontPath = SingletonBehaviour<SettingsManager>.GetOrCreate().GameSettings.GothicSubtitleFontPath;

            // FIXME: These values are debug values. They need to be adjusted for optimized results.
            int faceIndex = 0;
            int samplingPointSize = 100;
            int atlasPadding = 0;
            GlyphRenderMode renderMode = GlyphRenderMode.COLOR;
            int atlasWidth = 100;
            int atlasHeight = 100;
            
            if (File.Exists(menuFontPath))
                PhoenixBridge.GothicMenuFont = TMP_FontAsset.CreateFontAsset(menuFontPath, faceIndex, samplingPointSize, atlasPadding, renderMode, atlasWidth, atlasHeight);

            if (File.Exists(subtitleFontPath))
                PhoenixBridge.GothicSubtitleFont = TMP_FontAsset.CreateFontAsset(subtitleFontPath, faceIndex, samplingPointSize, atlasPadding, renderMode, atlasWidth, atlasHeight);
        }


        // FIXME: This destructor is called multiple times when starting Unity game (Also during start of game)
        // FIXME: We need to check why and improve!
        // Destroy memory on phoenix DLL when game closes.
        ~PhoenixImporter()
        {
            if (PhoenixBridge.VdfsPtr != IntPtr.Zero)
            {
                PxVdf.pxVdfDestroy(PhoenixBridge.VdfsPtr);
                PhoenixBridge.VdfsPtr = IntPtr.Zero;
            }

            if (PhoenixBridge.VmGothicPtr != IntPtr.Zero)
            {
                PxVm.pxVmDestroy(PhoenixBridge.VmGothicPtr);
                PhoenixBridge.VmGothicPtr = IntPtr.Zero;
            }

            if (PhoenixBridge.VmSfxPtr != IntPtr.Zero)
            {
                PxVm.pxVmDestroy(PhoenixBridge.VmSfxPtr);
                PhoenixBridge.VmSfxPtr = IntPtr.Zero;
            }
            if (PhoenixBridge.VmMusicPtr != IntPtr.Zero)
            {
                PxVm.pxVmDestroy(PhoenixBridge.VmMusicPtr);
                PhoenixBridge.VmMusicPtr = IntPtr.Zero;
            }
        }
    }
}
