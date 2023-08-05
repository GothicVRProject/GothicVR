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
using PxCs.Helper;
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

            var g1Dir = SettingsManager.I.GameSettings.GothicIPath;
            
            // FIXME - We currently don't load from within _WORK directory which is required for e.g. mods who use it.
            var fullPath = Path.GetFullPath(Path.Join(g1Dir, "Data"));

            // Holy grail of everything! If this pointer is zero, we have nothing but a plain empty wormhole.
            GameData.I.VfsPtr = VfsBridge.LoadVfsInDirectory(fullPath);

            
            SetLanguage();
            LoadGothicVM(g1Dir);
            LoadSfxVM(g1Dir);
            LoadMusicVM(g1Dir);
            LoadMusic();
            LoadFonts();
            watch.Stop();
            Debug.Log($"Time spent for Bootstrapping Phoenix: {watch.Elapsed}");

            GvrSceneManager.I.LoadStartupScenes();
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
                    var isVfsMessage = message.StartsWith("failed to find vfs entry");
                    if (isVfsMessage && !FeatureFlags.I.ShowVfsFileNotFoundErrors)
                        break;

                    Debug.LogError(message);
                    break;
            }
        }

        private void SetLanguage()
        {
            var g1Language = SettingsManager.I.GameSettings.GothicILanguage;

            switch (g1Language?.Trim().ToLower())
            {
                case "cs":
                case "pl":
                    PxEncoding.SetEncoding(PxEncoding.SupportedEncodings.CentralEurope);
                    break;
                case "ru":
                    PxEncoding.SetEncoding(PxEncoding.SupportedEncodings.EastEurope);
                    break;
                case "de":
                case "en":
                case "es":
                case "fr":
                case "it":
                default:
                    PxEncoding.SetEncoding(PxEncoding.SupportedEncodings.WestEurope);
                    break;
            }
        }

        
        private void LoadGothicVM(string G1Dir)
        {
            var fullPath = Path.GetFullPath(Path.Join(G1Dir, "/_work/DATA/scripts/_compiled/GOTHIC.DAT"));
            var vmPtr = VmGothicBridge.LoadVm(fullPath);

            VmGothicBridge.RegisterExternals(vmPtr);

            GameData.I.VmGothicPtr = vmPtr;
        }

        private void LoadSfxVM(string G1Dir)
        {
            var fullPath = Path.GetFullPath(Path.Join(G1Dir, "/_work/DATA/scripts/_compiled/SFX.DAT"));
            var vmPtr = VmGothicBridge.LoadVm(fullPath);
            GameData.I.VmSfxPtr = vmPtr;
        }

        private void LoadMusicVM(string G1Dir)
        {
            var fullPath = Path.GetFullPath(Path.Join(G1Dir, "/_work/DATA/scripts/_compiled/MUSIC.DAT"));
            var vmPtr = VmGothicBridge.LoadVm(fullPath);
            GameData.I.VmMusicPtr = vmPtr;
        }

        private void LoadMusic()
        {
            if (!FeatureFlags.I.EnableMusic)
                return;
            var music = MusicCreator.I;
            music.Create();
            music.setEnabled(true);
            music.setMusic("SYS_MENU");
            Debug.Log("Loading music");
        }

        /// <summary>
        /// If there are Gothic ttf fonts stored on the current system, we will use them.
        /// If not, we will stick with a default font.
        /// </summary>
        private void LoadFonts()
        {
            var menuFontPath = SettingsManager.I.GameSettings.GothicMenuFontPath;
            var subtitleFontPath = SettingsManager.I.GameSettings.GothicSubtitleFontPath;

            // FIXME: These values are debug values. They need to be adjusted for optimized results.
            int faceIndex = 0;
            int samplingPointSize = 100;
            int atlasPadding = 0;
            GlyphRenderMode renderMode = GlyphRenderMode.COLOR;
            int atlasWidth = 100;
            int atlasHeight = 100;

            if (File.Exists(menuFontPath))
                GameData.I.GothicMenuFont = TMP_FontAsset.CreateFontAsset(menuFontPath, faceIndex, samplingPointSize, atlasPadding, renderMode, atlasWidth, atlasHeight);

            if (File.Exists(subtitleFontPath))
                GameData.I.GothicSubtitleFont = TMP_FontAsset.CreateFontAsset(subtitleFontPath, faceIndex, samplingPointSize, atlasPadding, renderMode, atlasWidth, atlasHeight);

            FontManager.I.Create();
        }
    }
}
