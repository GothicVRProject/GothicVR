using System.Diagnostics;
using System.IO;
using AOT;
using GVR.Debugging;
using GVR.Extensions;
using GVR.Globals;
using GVR.Manager.Settings;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Interface.Vm;
using GVR.Util;
using PxCs.Helper;
using PxCs.Interface;
using UnityEngine;
using ZenKit;
using Debug = UnityEngine.Debug;
using Logger = ZenKit.Logger;

namespace GVR.Manager
{
    public class GVRBootstrapper : SingletonBehaviour<GVRBootstrapper>
    {
        private bool isBootstrapped;
        public GameObject invalidInstallationDirMessage;
        public GameObject filePickerButton;

        private void Start()
        {
            PxLogging.pxLoggerSet(PxLoggerCallback);
            Logger.Set(FeatureFlags.I.zenKitLogLevel, ZenKitLoggerCallback);

            // Just in case we forgot to disable it in scene view. ;-)
            invalidInstallationDirMessage.SetActive(false);
        }

        private void OnApplicationQuit()
        {
            GameData.Dispose();
        }

        private void Update()
        {
            // Load after Start() so that other MonoBehaviours can subscribe to DaedalusVM events.
            if (isBootstrapped)
                return;
            isBootstrapped = true;

            var g1Dir = SettingsManager.GameSettings.GothicIPath;

            if (SettingsManager.CheckIfGothic1InstallationExists())
            {
                BootGothicVR(g1Dir);
            }
            else
            {
                //Show the startup config message, show filepicker for PCVR but not for Android standalone
                invalidInstallationDirMessage.SetActive(true);
                if (Application.platform == RuntimePlatform.Android)
                    filePickerButton.SetActive(false);
                else
                    filePickerButton.SetActive(true);
            }
        }
        
        public void BootGothicVR(string g1Dir)
        {
            var watch = Stopwatch.StartNew();
            
            // FIXME - We currently don't load from within _WORK directory which is required for e.g. mods who use it.
            var fullPath = Path.GetFullPath(Path.Join(g1Dir, "Data"));

            // Holy grail of everything! If this pointer is zero, we have nothing but a plain empty wormhole.
            GameData.VfsPtr = VfsBridge.LoadVfsInDirectory(fullPath);

            MountVfs(g1Dir);
            SetLanguage();
            LoadGothicVM(g1Dir);
            LoadSfxVM(g1Dir);
            LoadPfxVm(g1Dir);
            LoadMusicVM(g1Dir);
            LoadMusic();
            LoadFonts();
            watch.Stop();
            Debug.Log($"Time spent for Bootstrapping Phoenix: {watch.Elapsed}");

#pragma warning disable CS4014 // It's intended, that this async call is not awaited.
            GvrSceneManager.I.LoadStartupScenes();
#pragma warning restore CS4014
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
                    var isVfsMessage = message.ContainsIgnoreCase("failed to find vfs entry");
                    if (isVfsMessage && !FeatureFlags.I.showPhoenixVfsFileNotFoundErrors)
                        break;

                    Debug.LogError(message);
                    break;
                default:
                    if (!FeatureFlags.I.showPhoenixDebugMessages)
                        break;

                    Debug.Log(message);
                    break;
            }
        }

        [MonoPInvokeCallback(typeof(Logger.Callback))]
        public static void ZenKitLoggerCallback(LogLevel level, string name, string message)
        {
            // Using fastest string concatenation as we might have a lot of logs here.
            var messageString = string.Concat("level=", level, ", name=", name, ", message=", message);
            
            switch (level)
            {
                case LogLevel.Error:
                    var isVfsMessage = message.ContainsIgnoreCase("failed to find vfs entry");
                    if (isVfsMessage && !FeatureFlags.I.showPhoenixVfsFileNotFoundErrors)
                        break;

                    Debug.LogError(messageString);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(messageString);
                    break;
                default:
                    Debug.Log(messageString);
                    break;
            }
        }

        /// <summary>
        /// Holy grail of everything! If this pointer is zero, we have nothing but a plain empty wormhole.
        /// </summary>
        private static void MountVfs(string g1Dir)
        {
            GameData.Vfs = new Vfs();

            // FIXME - We currently don't load from within _WORK directory which is required for e.g. mods who use it.
            var fullPath = Path.GetFullPath(Path.Join(g1Dir, "Data"));

            var vfsPaths = Directory.GetFiles(fullPath, "*.VDF", SearchOption.AllDirectories);

            foreach (var path in vfsPaths)
            {
                GameData.Vfs.MountDisk(path, VfsOverwriteBehavior.Older);
            }
        }

        public static void SetLanguage()
        {
            var g1Language = SettingsManager.GameSettings.GothicILanguage;

            switch (g1Language?.Trim().ToLower())
            {
                case "cs":
                case "pl":
                    PxEncoding.SetEncoding(PxEncoding.SupportedEncodings.CentralEurope);
                    StringEncodingController.SetEncoding(StringEncoding.CentralEurope);
                    break;
                case "ru":
                    PxEncoding.SetEncoding(PxEncoding.SupportedEncodings.EastEurope);
                    StringEncodingController.SetEncoding(StringEncoding.EastEurope);
                    break;
                case "de":
                case "en":
                case "es":
                case "fr":
                case "it":
                default:
                    PxEncoding.SetEncoding(PxEncoding.SupportedEncodings.WestEurope);
                    StringEncodingController.SetEncoding(StringEncoding.WestEurope);
                    break;
            }
        }

        
        private void LoadGothicVM(string g1Dir)
        {
            var fullPath = Path.GetFullPath(Path.Join(g1Dir, "/_work/DATA/scripts/_compiled/GOTHIC.DAT"));
            var vmPtr = VmGothicExternals.LoadVm(fullPath);
            GameData.VmGothicPtr = vmPtr;

            GameData.GothicVm = new DaedalusVm(fullPath);

            VmGothicExternals.RegisterExternals();
        }

        private void LoadSfxVM(string G1Dir)
        {
            var fullPath = Path.GetFullPath(Path.Join(G1Dir, "/_work/DATA/scripts/_compiled/SFX.DAT"));
            var vmPtr = VmGothicExternals.LoadVm(fullPath);
            GameData.VmSfxPtr = vmPtr;
        }

        private static void LoadPfxVm(string g1Dir)
        {
            var fullPath = Path.GetFullPath(Path.Join(g1Dir, "/_work/DATA/scripts/_compiled/PARTICLEFX.DAT"));
            var vmPtr = VmGothicExternals.LoadVm(fullPath);
            GameData.VmPfxPtr = vmPtr;
        }

        private void LoadMusicVM(string G1Dir)
        {
            var fullPath = Path.GetFullPath(Path.Join(G1Dir, "/_work/DATA/scripts/_compiled/MUSIC.DAT"));
            var vmPtr = VmGothicExternals.LoadVm(fullPath);
            GameData.VmMusicPtr = vmPtr;
        }

        private void LoadMusic()
        {
            var music = MusicManager.I;
            music.Create();
            music.SetEnabled(FeatureFlags.I.enableMusic);
            music.SetMusic("SYS_MENU");
        }

        private void LoadFonts()
        {
            FontManager.I.Create();
        }
    }
}
