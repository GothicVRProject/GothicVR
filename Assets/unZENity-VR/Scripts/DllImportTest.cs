using System;
using System.Collections.Generic;
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
        [DllImport(DLLNAME)] private static extern void disposeVDFContainer(IntPtr vdfContainer);

        [DllImport(DLLNAME)] private static extern IntPtr loadWorld(IntPtr vdfContainer, string worldFileName, MyVector3[] vectors);
        
        
        private IntPtr vdfContainer;
        private void OnDestroy() { disposeVDFContainer(vdfContainer); }


        [StructLayout(LayoutKind.Sequential)]
        struct MyVector3
        {
            float x;
            float y;
            float z;
        }

        void Start()
        {
            vdfContainer = createVDFContainer();

            var vdfPaths = Directory.GetFiles(G1Dir + "/Data", "*.vdf");

            foreach (var vdfPath in vdfPaths)
                addVDFToContainer(vdfContainer, vdfPath);

            var vectors = new MyVector3[1];

            loadWorld(vdfContainer, "world.zen", vectors);
        }
    }
}