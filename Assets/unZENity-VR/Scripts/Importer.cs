using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UZVR
{
    public class Importer : MonoBehaviour
    {
        private Scene _Scene;

        void Start()
        {
            _Scene = SceneManager.GetSceneByName("Importer");

            _ImportMap();
        }

        private void _ImportMap()
        {
            var srcPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic\\_work\\DATA\\Worlds\\_work\\OLDCAMP.3DS";
            var destPath = Application.dataPath + "/OLDCAMP.3DS";
            var relDestPath = "Assets/OLDCAMP.3DS";
            File.Copy(srcPath, destPath, true);

            AssetDatabase.ImportAsset(relDestPath);

            var obj = AssetDatabase.LoadAssetAtPath(relDestPath, typeof(Mesh));

            _Scene.GetRootGameObjects().Append(new GameObject("Foo"));
            _Scene.GetRootGameObjects().Append(Object.Instantiate(obj));

            int a = 2;
        }

        bool done;
        public void Update()
        {
            if (done)
                return;

            var relDestPath = "Assets/OLDCAMP.3DS";

            Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(relDestPath);

            var gObject = new GameObject("Oldcamp");

            var meshFilter = gObject.AddComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            var meshRenderer = gObject.AddComponent<MeshRenderer>();

            _Scene.GetRootGameObjects().Append(new GameObject("Foo2"));
            _Scene.GetRootGameObjects().Append(gObject);

            done = true;

        }
    }
}