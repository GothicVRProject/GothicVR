using AOT;
using GVR.Creator;
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


        private void Start()
        {
            VmGothicBridge.DefaultExternalCallback.AddListener(MissingVmExternalCall);
            PxLogging.pxLoggerSet(PxLoggerCallback);
        }

        private void Update()
        {
            // Load after Start() so that other MonoBehaviours can subscribe to DaedalusVM events.
            if (_loaded) return;
            _loaded = true;

            var G1Dir = SingletonBehaviour<SettingsManager>.GetOrCreate().GameSettings.GothicIPath;

            var fullPath = Path.GetFullPath(Path.Join(G1Dir, "Data"));
            var vdfPtr = VdfsBridge.LoadVdfsInDirectory(fullPath);

            LoadWorld(vdfPtr);
            LoadGothicVM(G1Dir);
            LoadFonts();
        }


        public static void MissingVmExternalCall(IntPtr vmPtr, string missingCallbackName)
        {
            Debug.LogWarning($"Method >{missingCallbackName}< not yet implemented in DaedalusVM.");
        }

        [MonoPInvokeCallback(typeof(PxLogging.PxLogCallback))]
        public static void PxLoggerCallback(PxLogging.Level level, string message)
        {
            switch(level)
            {
                case PxLogging.Level.warn:
                    Debug.LogWarning(message);
                    break;
                case PxLogging.Level.error:
                    Debug.LogError(message);
                    break;
            }
        }

        private void LoadWorld(IntPtr vdfPtr)
        {
            var world = WorldBridge.LoadWorld(vdfPtr, "world.zen"); // world.zen -> G1, newworld.zen/oldworld.zen/addonworld.zen -> G2

            var subMeshes = WorldBridge.CreateSubmeshesForUnity(world);
            world.subMeshes = subMeshes;


            PhoenixBridge.VdfsPtr = vdfPtr;
            PhoenixBridge.World = world;


            var root = new GameObject("World");

            var scene = SceneManager.GetSceneByName("SampleScene");
            scene.GetRootGameObjects().Append(root);

            SingletonBehaviour<MeshCreator>.GetOrCreate().Create(world, root);
            SingletonBehaviour<VobCreator>.GetOrCreate().Create(root, world);
            SingletonBehaviour<WaynetCreator>.GetOrCreate().Create(root, world);
        }

        private void LoadGothicVM(string G1Dir)
        {
            var fullPath = Path.GetFullPath(Path.Join(G1Dir, "/_work/DATA/scripts/_compiled/GOTHIC.DAT"));
            var vmPtr = VmGothicBridge.LoadVm(fullPath);

            VmGothicBridge.RegisterExternals(vmPtr);

            PhoenixBridge.VmGothicPtr = vmPtr;

            PxVm.CallFunction(PhoenixBridge.VmGothicPtr, "STARTUP_SUB_OLDCAMP"); // Goal: Spawn Bloodwyn ;-)
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
                PhoenixBridge.GothicMenuFont = TMP_FontAsset.CreateFontAsset(subtitleFontPath, faceIndex, samplingPointSize, atlasPadding, renderMode, atlasWidth, atlasHeight);

            // DEBUG - Example to show how the font is being picked.
            var obj = GameObject.Find("HelloWorld");
            var textMesh = obj.GetComponent<TMP_Text>();

            textMesh.fontSize = 50;
            textMesh.autoSizeTextContainer = true;

            textMesh.text = "Is it Gothic font?";

            if (PhoenixBridge.GothicMenuFont)
                textMesh.font = PhoenixBridge.GothicMenuFont;
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
        }
    }
}