using System.Diagnostics;
using System.IO;
using AOT;
using GVR.Creator;
using GVR.Debugging;
using GVR.Manager;
using GVR.Manager.Settings;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Interface.Vm;
using GVR.Util;
using PxCs.Helper;
using PxCs.Interface;
using UnityEngine.XR;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace GVR.Bootstrap
{
    public class PhoenixBootstrapper : SingletonBehaviour<PhoenixBootstrapper>
    {
        private bool _loaded = false;
        public GameObject installationFilePicker;

        private void Start()
        {
            PxLogging.pxLoggerSet(PxLoggerCallback);

            // Just in case we forgot to disable it in scene view. ;-)
            installationFilePicker.SetActive(false);
        }

        private void Update()
        {
            // Load after Start() so that other MonoBehaviours can subscribe to DaedalusVM events.
            if (_loaded)
                return;
            _loaded = true;

            var g1Dir = SettingsManager.I.GameSettings.GothicIPath;

            if(SettingsManager.I.CheckIfGothic1InstallationExists())
                BootGothicVR(g1Dir);
            else
                installationFilePicker.SetActive(true);
        }

        public void BootGothicVR(string g1Dir)
        {
            var watch = Stopwatch.StartNew();

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
                    var isVfsMessage = message.StartsWith("failed to find vfs entry");
                    if (isVfsMessage && !FeatureFlags.I.ShowPhoenixVfsFileNotFoundErrors)
                        break;

                    Debug.LogError(message);
                    break;
                default:
                    if (!FeatureFlags.I.ShowPhoenixDebugMessages)
                        break;

                    Debug.Log(message);
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
            var vmPtr = VmGothicExternals.LoadVm(fullPath);

            VmGothicExternals.RegisterExternals(vmPtr);

            GameData.I.VmGothicPtr = vmPtr;
        }

        private void LoadSfxVM(string G1Dir)
        {
            var fullPath = Path.GetFullPath(Path.Join(G1Dir, "/_work/DATA/scripts/_compiled/SFX.DAT"));
            var vmPtr = VmGothicExternals.LoadVm(fullPath);
            GameData.I.VmSfxPtr = vmPtr;
        }

        private void LoadMusicVM(string G1Dir)
        {
            var fullPath = Path.GetFullPath(Path.Join(G1Dir, "/_work/DATA/scripts/_compiled/MUSIC.DAT"));
            var vmPtr = VmGothicExternals.LoadVm(fullPath);
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

        private void LoadFonts()
        {
            FontManager.I.Create();
        }
    }
}
