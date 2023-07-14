using System;
using System.Diagnostics;
using System.IO;
using AOT;
using GVR.Creator;
using GVR.Debugging;
using GVR.Demo;
using GVR.Manager;
using GVR.Manager.Settings;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Interface.Vm;
using GVR.Util;
using PxCs.Interface;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.TextCore.LowLevel;
using Debug = UnityEngine.Debug;

namespace GVR.Bootstrap
{
    public class PhoenixBootstrapper : SingletonBehaviour<PhoenixBootstrapper>
    {
        private bool _loaded = false;

        private void Start()
        {
            VmGothicBridge.DefaultExternalCallback.AddListener(MissingVmExternalCall);
            PxLogging.pxLoggerSet(PxLoggerCallback);
        }

        private void Update()
        {
            // Load after Start() so that other MonoBehaviours can subscribe to DaedalusVM events.
            if (_loaded)
                return;
            _loaded = true;

            var watch = Stopwatch.StartNew();

            var g1Dir = SettingsManager.Instance.GameSettings.GothicIPath;

            var fullPath = Path.GetFullPath(Path.Join(g1Dir, "Data"));

            // Holy grail of everything! If this pointer is zero, we have nothing but a plain empty wormhole.
            GameData.I.VdfsPtr = VdfsBridge.LoadVdfsInDirectory(fullPath);

            LoadGothicVM(g1Dir);
            LoadSfxVM(g1Dir);
            LoadMusicVM(g1Dir);
            LoadMusic();
            LoadFonts();

            watch.Stop();
            Debug.Log($"Time spent for Bootstrapping Phoenix: {watch.Elapsed}");

            GvrSceneManager.Instance.LoadStartupScenes();
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
                    var isVdfMessage = message.StartsWith("failed to find vdf entry");
                    if (isVdfMessage && !FeatureFlags.I.ShowVdfsFileNotFoundErrors)
                        break;

                    Debug.LogError(message);
                    break;
            }
        }

        private void LoadGothicVM(string G1Dir)
        {
            var fullPath = Path.GetFullPath(Path.Join(G1Dir, "/_work/DATA/scripts/_compiled/GOTHIC.DAT"));
            var vmPtr = VmGothicBridge.LoadVm(fullPath);

            VmGothicBridge.RegisterExternals(vmPtr);

            GameData.Instance.VmGothicPtr = vmPtr;
        }

        private void LoadSfxVM(string G1Dir)
        {
            var fullPath = Path.GetFullPath(Path.Join(G1Dir, "/_work/DATA/scripts/_compiled/SFX.DAT"));
            var vmPtr = VmGothicBridge.LoadVm(fullPath);
            GameData.Instance.VmSfxPtr = vmPtr;
        }

        private void LoadMusicVM(string G1Dir)
        {
            var fullPath = Path.GetFullPath(Path.Join(G1Dir, "/_work/DATA/scripts/_compiled/MUSIC.DAT"));
            var vmPtr = VmGothicBridge.LoadVm(fullPath);
            GameData.Instance.VmMusicPtr = vmPtr;
        }

        private void LoadMusic()
        {
            if (!SingletonBehaviour<FeatureFlags>.GetOrCreate().EnableMusic)
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
                GameData.Instance.GothicMenuFont = TMP_FontAsset.CreateFontAsset(menuFontPath, faceIndex, samplingPointSize, atlasPadding, renderMode, atlasWidth, atlasHeight);

            if (File.Exists(subtitleFontPath))
                GameData.I.GothicSubtitleFont = TMP_FontAsset.CreateFontAsset(subtitleFontPath, faceIndex, samplingPointSize, atlasPadding, renderMode, atlasWidth, atlasHeight);
        }
    }
}
