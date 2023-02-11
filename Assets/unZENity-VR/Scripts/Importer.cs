using Assimp;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
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
            File.Copy(srcPath, destPath, true);

            _Scene.GetRootGameObjects().Append(new GameObject("Foo"));
        }


        bool done;
        public void Update()
        {
            if (done)
                return;


            AssimpContext importer = new AssimpContext();


            //var srcPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic\\_work\\DATA\\Worlds\\_work\\OLDCAMP.3DS";


            //var gObject = new GameObject("Oldcamp");
            //gObject.transform.localScale = new Vector3(0.001f, 0.001f, 0.001f);

            //var loader = gObject.AddComponent<Loader3DS>();
            //loader.modelPath = srcPath;

            //_Scene.GetRootGameObjects().Append(new GameObject("Foo2"));
            //_Scene.GetRootGameObjects().Append(gObject);

            //done = true;
        }
    }
}