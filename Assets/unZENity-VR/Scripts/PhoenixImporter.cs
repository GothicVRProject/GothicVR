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

            var vdfsBridge = new VdfsBridge(G1Dir + "/Data");

            LoadWorld(vdfsBridge);
            LoadGothicVM();
        }

        private void LoadWorld(VdfsBridge vdfsBridge)
        {
            var worldBridge = new WorldBridge(vdfsBridge, "world.zen");
            PhoenixBridge.WorldBridge = worldBridge;

            var root = new GameObject("World");

            var scene = SceneManager.GetSceneByName("SampleScene");
            scene.GetRootGameObjects().Append(root);

            SingletonBehaviour<MeshCreator>.GetOrCreate().Create(root, worldBridge.World);
            SingletonBehaviour<WaynetCreator>.GetOrCreate().Create(root, worldBridge.World);
        }

        private void LoadGothicVM()
        {
            var vmGothicBridge = new VmGothicBridge(G1Dir + "/_work/DATA/scripts/_compiled/GOTHIC.DAT");

            PhoenixBridge.VmGothicBridge = vmGothicBridge;
            PhoenixBridge.VmGothicNpcBridge = new(vmGothicBridge);

            vmGothicBridge.CallFunction("STARTUP_SUB_OLDCAMP"); // Goal: Spawn Bloodwyn ;-)
        }
    }
}