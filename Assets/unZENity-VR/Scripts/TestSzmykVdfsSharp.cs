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

            //_ExtractVDFs();
            _ExtractZens();
        }

        private void _ExtractVDFs()
        {
            Debug.Log(string.Format("Start extracting vdf files from {0}", G1_DIR));

            string[] files = Directory.GetFiles(G1_DIR, "*.vdf", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var reader = new VdfsExtractor(file);

                reader.ExtractFiles(UNITY_OUT_DIR, ExtractOption.Hierarchy);
            }

            Debug.Log(string.Format("Extracting of {0} vdf files done.", files.Length));
        }

        private void _ExtractZens()
        {
            Debug.Log(string.Format("Start extracting zen files from {0}", G1_DIR));

            string[] files = Directory.GetFiles(UNITY_OUT_DIR, "*.zen", SearchOption.AllDirectories);

            Debug.Log(string.Format("Extracting of {0} zen files done.", files.Length));
        }

    }
}