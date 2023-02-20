using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using VdfsSharp;

namespace UZVR
{
    public class TestSzmykVdfsSharp : MonoBehaviour
    {
        private const string G1_DIR = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic";

        void Start()
        {
            string[] files = Directory.GetFiles(G1_DIR, "*.vdf", SearchOption.AllDirectories);

            _ExtractVDFs(files);
        }

        private void _ExtractVDFs(string[] files)
        {
            foreach (var file in files)
            {
                var reader = new VdfsExtractor(file);

                reader.ExtractFiles("Assets/unZENity-VR/Extracted~", ExtractOption.Hierarchy);
                
                int a = 2;
            }
        }

    }
}