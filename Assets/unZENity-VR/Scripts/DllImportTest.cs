using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UZVR.Phoenix;

namespace UZVR
{
    public class DllImportTest : MonoBehaviour
    {
        void Start()
        {
            //TestWorld();
            TestVM();
        }

        private void TestWorld()
        {
            var world = new WorldBridge().GetWorld();

            var root = new GameObject("World");

            var scene = SceneManager.GetSceneByName("SampleScene");
            scene.GetRootGameObjects().Append(root);

            new MeshCreator().Create(root, world);
            //new WaynetCreator().Create(root, world);
        }

        private void TestVM()
        {
            var vm = new VmBridge("GOTHIC.DAT");

            vm.CallFunction("STARTUP_WORLD"); // Works: STARTUP_ORCGRAVEYARD

            //vm.registerCallback();

            //vm.callCallback(1);
            //vm.callCallback(2);
            //vm.callCallback(3);

        }
    }
}