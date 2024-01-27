using System.Diagnostics;
using System.IO;
using System.Linq;
using GVR.Caches;
using GVR.Debugging;
using GVR.Globals;
using GVR.Manager.Settings;
using GVR.Util;
using GVR.Vm;
using UnityEngine;
using ZenKit;
using ZenKit.Daedalus;
using Debug = UnityEngine.Debug;
using Logger = ZenKit.Logger;

namespace GVR.Manager
{
    public class GvrBootstrapper : SingletonBehaviour<GvrBootstrapper>
    {
        private bool isBootstrapped;
        public GameObject invalidInstallationDirMessage;
        public GameObject filePickerButton;

        private void Start()
        {
            Logger.Set(FeatureFlags.I.zenKitLogLevel, ZenKitLoggerCallback);

            // Just in case we forgot to disable it in scene view. ;-)
            invalidInstallationDirMessage.SetActive(false);
        }

        private void OnApplicationQuit()
        {
            GameData.Dispose();
            AssetCache.Dispose();
            LookupCache.Dispose();
            PrefabCache.Dispose();
            MorphMeshCache.Dispose();
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
                
#pragma warning disable CS4014 // It's intended, that this async call is not awaited.
                GvrSceneManager.I.LoadStartupScenes();
#pragma warning restore CS4014
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
        
        public static void BootGothicVR(string g1Dir)
        {
            var watch = Stopwatch.StartNew();
            
            MountVfs(g1Dir);
            SetLanguage();
            LoadGothicVm(g1Dir);
            LoadDialogs();
            LoadSfxVm(g1Dir);
            LoadPfxVm(g1Dir);
            LoadMusicVm(g1Dir);
            LoadMusic();
            LoadFonts();
            
            watch.Stop();
            Debug.Log($"Time spent for Bootstrapping ZenKit: {watch.Elapsed}");

            GvrEvents.ZenKitBootstrapped.Invoke();
        }

        public static void ZenKitLoggerCallback(LogLevel level, string name, string message)
        {
            // Using fastest string concatenation as we might have a lot of logs here.
            var messageString = string.Concat("level=", level, ", name=", name, ", message=", message);
            
            switch (level)
            {
                case LogLevel.Error:
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
        public static void MountVfs(string g1Dir)
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
                    StringEncodingController.SetEncoding(StringEncoding.CentralEurope);
                    break;
                case "ru":
                    StringEncodingController.SetEncoding(StringEncoding.EastEurope);
                    break;
                case "de":
                case "en":
                case "es":
                case "fr":
                case "it":
                default:
                    StringEncodingController.SetEncoding(StringEncoding.WestEurope);
                    break;
            }
        }

        
        private static void LoadGothicVm(string g1Dir)
        {
            var fullPath = Path.GetFullPath(Path.Join(g1Dir, "/_work/DATA/scripts/_compiled/GOTHIC.DAT"));
            GameData.GothicVm = new DaedalusVm(fullPath);
            
            // If we don't set it early, other calls within VM will fail.
            // TODO - Could be moved to a separate Hero.cs class and set during GvrEvents.GeneralSceneLoaded event.
            var hero = GameData.GothicVm.InitInstance<NpcInstance>("hero");
            GameData.GothicVm.GlobalHero = hero;

            VmGothicExternals.RegisterExternals();
        }

        /// <summary>
        /// We load all dialogs once and assign them to the appropriate NPCs once spawned.
        /// </summary>
        private static void LoadDialogs()
        {
            var infoInstances = GameData.GothicVm.GetInstanceSymbols("C_Info")
                .Select(symbol => GameData.GothicVm.InitInstance<InfoInstance>(symbol))
                .ToList();

            infoInstances.ForEach(i => GameData.Dialogs.Instances.Add(i));
        }

        private static void LoadSfxVm(string g1Dir)
        {
            var fullPath = Path.GetFullPath(Path.Join(g1Dir, "/_work/DATA/scripts/_compiled/SFX.DAT"));
            GameData.SfxVm = new DaedalusVm(fullPath);
        }

        private static void LoadPfxVm(string g1Dir)
        {
            var fullPath = Path.GetFullPath(Path.Join(g1Dir, "/_work/DATA/scripts/_compiled/PARTICLEFX.DAT"));
            GameData.PfxVm = new DaedalusVm(fullPath);
        }

        private static void LoadMusicVm(string g1Dir)
        {
            var fullPath = Path.GetFullPath(Path.Join(g1Dir, "/_work/DATA/scripts/_compiled/MUSIC.DAT"));
            GameData.MusicVm = new DaedalusVm(fullPath);
        }

        private static void LoadMusic()
        {
            var music = MusicManager.I;
            music.Create();
            music.SetEnabled(FeatureFlags.I.enableMusic);
            music.SetMusic("SYS_MENU");
        }

        private static void LoadFonts()
        {
            FontManager.I.Create();
        }
    }
}
