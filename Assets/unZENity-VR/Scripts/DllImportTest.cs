using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UZVR.Phoenix;

namespace UZVR
{
    public class DllImportTest : MonoBehaviour
    {
        private const string G1Dir = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic";

        void Start()
        {
            var vdfsBridge = new VdfsBridge(G1Dir + "/Data");

            TestWorld(vdfsBridge);
            TestVM();
        }

        private void TestWorld(VdfsBridge vdfsBridge)
        {
            var worldBridge = new WorldBridge(vdfsBridge, "world.zen");

            var root = new GameObject("World");

            var scene = SceneManager.GetSceneByName("SampleScene");
            scene.GetRootGameObjects().Append(root);

            new MeshCreator().Create(root, worldBridge.World);
            new WaynetCreator().Create(root, worldBridge.World);

            PhoenixBridge.WorldBridge = worldBridge;
        }

        private void TestVM()
        {
            var vm = new VmBridge(G1Dir + "/_work/DATA/scripts/_compiled/GOTHIC.DAT");

            PhoenixBridge.VMBridge = vm;

            vm.CallFunction("STARTUP_SUB_OLDCAMP"); // Goal: Spawn Bloodwyn ;-)
        }
    }
}