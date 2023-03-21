using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UZVR
{
    public class DllImportTest : MonoBehaviour
    {
        void Start()
        {
            var world = new PhoenixBridge().GetWorld();


            var root = new GameObject("World");

            var scene = SceneManager.GetSceneByName("SampleScene");
            scene.GetRootGameObjects().Append(root);

            new MeshCreator().Create(root, world);
            //new WaynetCreator().Create(root, world);
        }
    }
}