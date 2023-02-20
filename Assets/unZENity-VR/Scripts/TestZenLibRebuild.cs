using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using VdfsSharp;

namespace UZVR
{
    public class TestZenLibRebuild : MonoBehaviour
    {
        private const string FREEMINE_ZEN = "Assets/unZENity-VR/Extracted~/_WORK/DATA/WORLDS/FREEMINE.ZEN";

        void Start()
        {
            _ReadZen(FREEMINE_ZEN);
        }

        private void _ReadZen(string path)
        {
            new ZenParser(FREEMINE_ZEN).Parse();
        }

    }
}