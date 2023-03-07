using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

namespace UZVR
{
    public struct World
    {
        public List<Vector3> vertices;
        public List<int> triangles;
    }


    public class PhoenixBridge
    {
        private const string DLLNAME = "phoenix-csharp-bridge";
        private const string G1Dir = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic\\";


        [DllImport(DLLNAME)] private static extern IntPtr createVDFContainer();
        [DllImport(DLLNAME)] private static extern void addVDFToContainer(IntPtr vdfContainer, string vdfPath);

        [DllImport(DLLNAME)] private static extern IntPtr loadWorldMesh(IntPtr vdfContainer, string worldFileName);
        [DllImport(DLLNAME)] private static extern int getWorldVerticesCount(IntPtr worldContainer);
        [DllImport(DLLNAME)] private static extern void getWorldMeshVertex(IntPtr mesh, int index, out float x, out float y, out float z);
        [DllImport(DLLNAME)] private static extern int getWorldMeshVertexIndicesCount(IntPtr worldContainer);
        [DllImport(DLLNAME)] private static extern void getWorldMeshVertexIndex(IntPtr mesh, int index, out UInt32 value);

        [DllImport(DLLNAME)] private static extern void disposeVDFContainer(IntPtr vdfContainer);
        [DllImport(DLLNAME)] private static extern void disposeWorldMesh(IntPtr mesh);


        private IntPtr vdfContainer;

        ~PhoenixBridge()
        {
            disposeVDFContainer(vdfContainer);
        }


        public World GetWorld()
        {
            World world = new();
            vdfContainer = createVDFContainer();

            var vdfPaths = Directory.GetFiles(G1Dir + "/Data", "*.vdf");

            foreach (var vdfPath in vdfPaths)
                addVDFToContainer(vdfContainer, vdfPath);

            var mesh = loadWorldMesh(vdfContainer, "world.zen");

            world.vertices = _GetWorldVertices(mesh);
            world.triangles = _GetWorldTriangles(mesh);

            disposeWorldMesh(mesh);
// FIXME kills Unity. NPE?
//          disposeVDFContainer(vdfContainer);

            return world;
        }

        private List<Vector3> _GetWorldVertices(IntPtr mesh)
        {
            List<Vector3> vertices = new();


            for (int i = 0; i < getWorldVerticesCount(mesh); i++)
            {
                getWorldMeshVertex(mesh, i, out float x, out float y, out float z);
                vertices.Add(new(x, y, z));
            }

            return vertices;
        }

        private List<int> _GetWorldTriangles(IntPtr mesh)
        {
            List<int> triangles = new();


            for (int i = 0; i < getWorldMeshVertexIndicesCount(mesh); i++)
            {
                getWorldMeshVertexIndex(mesh, i, out UInt32 value);
                triangles.Add((int)value);
            }

            return triangles;
        }
    }
}