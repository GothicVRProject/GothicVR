using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UZVR
{
    public class DllImportTest : MonoBehaviour
    {
        private const string DLLNAME = "phoenix-csharp-wrapper";
        private const string G1Dir = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic\\";


        [DllImport(DLLNAME)] private static extern IntPtr createVDFContainer();
        [DllImport(DLLNAME)] private static extern void addVDFToContainer(IntPtr vdfContainer, string vdfPath);
        [DllImport(DLLNAME)] private static extern IntPtr getVDFEntry(IntPtr vdfContainer, string name);
        [DllImport(DLLNAME)] private static extern void disposeVDFContainer(IntPtr vdfContainer);

        private IntPtr vdfContainer;

        void Start()
        {
            vdfContainer = createVDFContainer();

            var vdfPaths = GetVDFPaths();

            foreach (var vdfPath in vdfPaths)
            {
               HandleVDF(vdfPath);
            }

            var found = findVDFByName("world.zen");

            disposeVDFContainer(vdfContainer);
        }



        private string[] GetVDFPaths()
        {
            return Directory.GetFiles(G1Dir + "/Data", "*.vdf");
        }


        private void HandleVDF(string vdfPath)
        {
            addVDFToContainer(vdfContainer, vdfPath);
        }

        private bool findVDFByName(string name)
        {
            IntPtr found = getVDFEntry(vdfContainer, name);

            return found != IntPtr.Zero;
        }
    }
}