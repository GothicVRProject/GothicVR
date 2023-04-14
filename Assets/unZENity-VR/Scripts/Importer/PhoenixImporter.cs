using PxCs;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UZVR.Phoenix.Interface;
using UZVR.Phoenix.Interface.Vm;
using UZVR.Util;
using UZVR.Creator;
using UZVR.Settings;

namespace UZVR.Importer
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

            var G1Dir = SingletonBehaviour<SettingsManager>.GetOrCreate().GameSettings.G1_path;

            var fullPath = Path.GetFullPath(Path.Join(G1Dir, "Data"));
            var vdfPtr = VdfsBridge.LoadVdfsInDirectory(fullPath);

            LoadWorld(vdfPtr);
            LoadGothicVM(G1Dir);
        }


        public static void MissingVmExternalCall(IntPtr vmPtr, string missingCallbackName)
        {
            Debug.LogWarning($"Method >{missingCallbackName}< not yet implemented in DaedalusVM.");
        }

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

            SingletonBehaviour<MeshCreator>.GetOrCreate().Create(root, world);
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