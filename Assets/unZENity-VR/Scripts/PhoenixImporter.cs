using PxCs;
using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UZVR.Phoenix.Bridge;
using UZVR.Phoenix.Bridge.Vm;
using UZVR.Util;
using UZVR.WorldCreator;

namespace UZVR
{
    public class PhoenixImporter : SingletonBehaviour<PhoenixImporter>
    {
        private const string G1Dir = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic";

        private bool _loaded = false;
        void Update()
        {
            // Load after Start() so that other MonoBehaviours can subscribe to DaedalusVM events.
            if (_loaded) return;
                _loaded = true;

            var fullPath = Path.GetFullPath(Path.Join(G1Dir, "Data"));
            var vdfPtr = VdfsBridge.LoadVdfsInDirectory(fullPath);

            LoadWorld(vdfPtr);
            LoadGothicVM();
        }

        private void LoadWorld(IntPtr vdfPtr)
        {
            var world = WorldBridge.LoadWorld(vdfPtr, "world.zen");

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

        private void LoadGothicVM()
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