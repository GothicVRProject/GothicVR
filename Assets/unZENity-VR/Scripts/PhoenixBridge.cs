using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UZVR
{
    public struct PCBridge_World
    {
        public List<Vector3> vertices;
        public List<PCBridge_Material> materials;
        public Dictionary<int, List<int>> triangles;
    }

    public struct PCBridge_Material
    {
        public Color color;
    }


    public class PhoenixBridge
    {
        private const string DLLNAME = "phoenix-csharp-bridge";
        private const string G1Dir = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic\\";


        // Load
        [DllImport(DLLNAME)] private static extern IntPtr createVDFContainer();
        [DllImport(DLLNAME)] private static extern void addVDFToContainer(IntPtr vdfContainer, string vdfPath);
        [DllImport(DLLNAME)] private static extern IntPtr loadWorld(IntPtr vdfContainer, string worldFileName);

        // Dispose
        [DllImport(DLLNAME)] private static extern void disposeVDFContainer(IntPtr vdfContainer);
        [DllImport(DLLNAME)] private static extern void disposeWorld(IntPtr mesh);



        // Vertices; aka Vertexes ;-)
        [DllImport(DLLNAME)] private static extern int getWorldMeshVertexCount(IntPtr world);
        [DllImport(DLLNAME)] private static extern void getWorldMeshVertex(IntPtr world, int index, out float x, out float y, out float z);

        // Triangles
        [DllImport(DLLNAME)] private static extern int getWorldMeshTriangleCount(IntPtr world);
        [DllImport(DLLNAME)] private static extern void getWorldMeshTriangle(IntPtr world, int index, out UInt32 valueA, out UInt32 valueB, out UInt32 valueC);

        // Materials
        [DllImport(DLLNAME)] private static extern int getWorldMeshMaterialCount(IntPtr world);
        [DllImport(DLLNAME)] private static extern void getWorldMeshMaterialColor(IntPtr world, int index, out byte r, out byte g, out byte b, out byte a);
        [DllImport(DLLNAME)] private static extern int getWorldMeshTriangleMaterialIndex(IntPtr world, int index);


        private IntPtr _vdfContainer;

        ~PhoenixBridge()
        {
            disposeVDFContainer(_vdfContainer);
        }


        public PCBridge_World GetWorld()
        {
            _ParseVDFs();

            var world = new PCBridge_World();

            var worldPtr = loadWorld(_vdfContainer, "world.zen");

            world.vertices = _GetWorldVertices(worldPtr);
            world.materials = _GetWorldMaterials(worldPtr);
            world.triangles = _GetWorldTriangles(worldPtr, world.materials.Count);

            disposeWorld(worldPtr);

            return world;
        }

        private void _ParseVDFs()
        {
            if (_vdfContainer != IntPtr.Zero)
                return;

            _vdfContainer = createVDFContainer();

            var vdfPaths = Directory.GetFiles(G1Dir + "/Data", "*.vdf");

            foreach (var vdfPath in vdfPaths)
                addVDFToContainer(_vdfContainer, vdfPath);


        }

        private List<Vector3> _GetWorldVertices(IntPtr worldPtr)
        {
            List<Vector3> vertices = new();


            for (int i = 0; i < getWorldMeshVertexCount(worldPtr); i++)
            {
                getWorldMeshVertex(worldPtr, i, out float x, out float y, out float z);
                vertices.Add(new(x, y, z));
            }

            return vertices;
        }

        private List<PCBridge_Material> _GetWorldMaterials(IntPtr worldPtr)
        {
            int materialCount = getWorldMeshMaterialCount(worldPtr);
            var materials = new List<PCBridge_Material>(materialCount);

            for (var i=0; i<materialCount; i++)
            {
                getWorldMeshMaterialColor(worldPtr, i, out byte r, out byte g, out byte b, out byte a);
                // We need to convert uint8 (byte) to float for Unity.
                var m = new PCBridge_Material() { color = new Color((float)r/255, (float)g /255, (float)b /255, (float)a /255) };
                materials.Add(m);
            }

            return materials;
        }


        private Dictionary<int, List<int>> _GetWorldTriangles(IntPtr worldPtr, int materialCount)
        {
            Dictionary<int, List<int>> triangles = new();

            // FIXME We can optimize by cleaning up empty materials.
            // PERFORMANCE e.g. worlds.vdfs has 2263 materials, but only ~1300 of them have triangles attached to it.

            // Initialize array
            for (var i=0; i < materialCount; i++)
            {
                triangles.Add(i, new());
            }


            for (int i = 0; i < getWorldMeshTriangleCount(worldPtr); i++)
            {
                var materialIndex = getWorldMeshTriangleMaterialIndex(worldPtr, i);
                getWorldMeshTriangle(worldPtr, i, out UInt32 valueA, out UInt32 valueB, out UInt32 valueC);

                // We need to flip valueA with valueC to:
                // 1/ have the mesh elements shown (flipped surface) and
                // 2/ world mirrored right way.
                triangles[materialIndex].AddRange(new []{ (int)valueC, (int)valueB, (int)valueA });
            }

            return triangles;
        }
    }
}