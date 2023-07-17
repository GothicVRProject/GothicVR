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
        public GameObject worldGo;
        
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
            LoadWorld(vdfPtr);
            LoadMusic();

            PxVm.CallFunction(PhoenixBridge.VmGothicPtr, "STARTUP_SUB_OLDCAMP"); // Goal: Spawn Bloodwyn ;-)        
            LoadFonts();

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

        private void LoadWorld(IntPtr vdfPtr)
        {
            var world = WorldBridge.LoadWorld(vdfPtr, "world.zen"); // world.zen -> G1, newworld.zen/oldworld.zen/addonworld.zen -> G2

            PhoenixBridge.VdfsPtr = vdfPtr;
            PhoenixBridge.World = world;


            var scene = SceneManager.GetSceneByName("SampleScene");
            scene.GetRootGameObjects().Append(worldGo);

            SingletonBehaviour<MeshCreator>.GetOrCreate().Create(world, worldGo);
            SingletonBehaviour<VobCreator>.GetOrCreate().Create(worldGo, world);
            SingletonBehaviour<WaynetCreator>.GetOrCreate().Create(worldGo, world);

            SingletonBehaviour<DebugAnimationCreator>.GetOrCreate().Create();
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
            int atlasPadding = 5;
            GlyphRenderMode renderMode = GlyphRenderMode.COLOR;
            int atlasWidth = 1024;
            int atlasHeight = 1024;
            
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
