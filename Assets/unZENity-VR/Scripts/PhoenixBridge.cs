using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UZVR
{
    public class PhoenixBridge
    {
        private const string DLLNAME = "phoenix-csharp-wrapper";
        private const string G1Dir = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic\\";


        [DllImport(DLLNAME)] private static extern IntPtr createVDFContainer();
        [DllImport(DLLNAME)] private static extern void addVDFToContainer(IntPtr vdfContainer, string vdfPath);
        [DllImport(DLLNAME)] private static extern void disposeVDFContainer(IntPtr vdfContainer);

        [DllImport(DLLNAME)] private static extern IntPtr loadWorldMesh(IntPtr vdfContainer, string worldFileName);
        [DllImport(DLLNAME)] private static extern int getWorldVerticesCount(IntPtr worldContainer);
        [DllImport(DLLNAME)] private static extern void getMeshVertex(IntPtr mesh, int index, out float x, out float y, out float z);
        [DllImport(DLLNAME)] private static extern void disposeMesh(IntPtr mesh);


        private IntPtr vdfContainer;

        ~PhoenixBridge()
        {
            disposeVDFContainer(vdfContainer);
        }


        public List<Vector3> GetWorldVertices()
        {
            List<Vector3> vertices = new();

            vdfContainer = createVDFContainer();

            var vdfPaths = Directory.GetFiles(G1Dir + "/Data", "*.vdf");

            foreach (var vdfPath in vdfPaths)
                addVDFToContainer(vdfContainer, vdfPath);

            var mesh = loadWorldMesh(vdfContainer, "world.zen");

            for (int i = 0; i < getWorldVerticesCount(mesh); i++)
            {
                getMeshVertex(mesh, i, out float x, out float y, out float z);
                vertices.Add(new(x, y, z));
            }

            disposeMesh(mesh);

            return vertices;
        }
    }
}