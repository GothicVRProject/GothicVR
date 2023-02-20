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
        private const string UNITY_OUT_DIR = "Assets/unZENity-VR/Extracted~";

        void Start()
        {
            string[] files = Directory.GetFiles(G1_DIR, "*.vdf", SearchOption.AllDirectories);

            _ExtractVDFs(files);
        }

        private void _ExtractVDFs(string[] files)
        {
            Debug.Log(string.Format("Start importing vdf files from {0}", G1_DIR));

            foreach (var file in files)
            {
                var reader = new VdfsExtractor(file);

                reader.ExtractFiles(UNITY_OUT_DIR, ExtractOption.Hierarchy);
            }

            Debug.Log(string.Format("Import of {0} vdf files done.", files.Length));
        }

    }
}