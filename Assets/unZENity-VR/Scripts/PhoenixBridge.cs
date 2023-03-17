using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UZVR
{
    public struct World
    {
        public List<Vector3> vertices;
        public List<PC_Material> materials;
        public Dictionary<int, List<int>> triangles;
    }

    public struct PC_Material
    {
        public Color color;
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
        [DllImport(DLLNAME)] private static extern int getWorldMeshTrianglesCount(IntPtr worldContainer);
        [DllImport(DLLNAME)] private static extern void getWorldMeshTriangleIndex(IntPtr mesh, int index, out UInt32 valueA, out UInt32 valueB, out UInt32 valueC);

        [DllImport(DLLNAME)] private static extern int getWorldMeshMaterialsCount(IntPtr mesh);
        [DllImport(DLLNAME)] private static extern void getWorldMeshMaterialColor(IntPtr mesh, int index, out byte r, out byte g, out byte b, out byte a);
        [DllImport(DLLNAME)] private static extern int getWorldMeshMaterialIndex(IntPtr mesh, int index);

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
            world.materials = _GetWorldMaterials(mesh);
            world.triangles = _GetWorldTriangles(mesh, world.materials.Count);

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

        private List<PC_Material> _GetWorldMaterials(IntPtr mesh)
        {
            var materials = new List<PC_Material>();

            for (var i = 0; i < getWorldMeshMaterialsCount(mesh); i++)
            {
                getWorldMeshMaterialColor(mesh, i, out byte r, out byte g, out byte b, out byte a);
                // We need to convert uint8 (byte) to float for Unity.
                var m = new PC_Material() { color = new Color((float)r/255, (float)g /255, (float)b /255, (float)a /255) };
                materials.Add(m);
            }

            return materials;
        }


        private Dictionary<int, List<int>> _GetWorldTriangles(IntPtr mesh, int materialCount)
        {
            Dictionary<int, List<int>> triangles = new();

            // FIXME We can optimize by cleaning up empty materials.
            // PERFORMANCE e.g. worlds.vdfs has 2263 materials, but only ~1300 of them have triangles attached to it.

            // Initialize array
            for (var i=0; i < materialCount; i++)
            {
                triangles.Add(i, new());
            }


            for (int i = 0; i < getWorldMeshTrianglesCount(mesh); i++)
            {
                var materialIndex = getWorldMeshMaterialIndex(mesh, i);
                getWorldMeshTriangleIndex(mesh, i, out UInt32 valueA, out UInt32 valueB, out UInt32 valueC);

                triangles[materialIndex].AddRange(new []{ (int)valueA, (int)valueB, (int)valueC });
            }

            return triangles;
        }
    }
}