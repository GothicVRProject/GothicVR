using Assets.unZENity_VR.Scripts.Phoenix.Util;
using PxCs;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UZVR.Phoenix.World;
using static UZVR.Phoenix.World.BWorld;

namespace UZVR.Phoenix.Bridge
{
    public static class WorldBridge
    {
        private const string DLLNAME = PhoenixBridge.DLLNAME;
        // Load
        [DllImport(DLLNAME)] private static extern IntPtr worldLoad(IntPtr vdfContainer, string worldFileName);

        // Dispose
        [DllImport(DLLNAME)] private static extern void worldDispose(IntPtr mesh);



        // Vertices; aka Vertexes ;-)
        [DllImport(DLLNAME)] private static extern int worldGetMeshVertexCount(IntPtr world);
        [DllImport(DLLNAME)] private static extern Vector3 worldGetMeshVertex(IntPtr world, int index);

        // Triangles
        [DllImport(DLLNAME)] private static extern ulong worldMeshVertexIndicesCount(IntPtr world);
        [DllImport(DLLNAME)] private static extern uint worldMeshVertexIndexGet(IntPtr world, ulong index);

        // Materials
        [DllImport(DLLNAME)] private static extern int worldGetMeshMaterialCount(IntPtr world);
        [DllImport(DLLNAME)] private static extern int worldGetMeshMaterialNameSize(IntPtr world, int index);
        [DllImport(DLLNAME)] private static extern void worldGetMeshMaterialName(IntPtr world, int index, StringBuilder name);
        [DllImport(DLLNAME)] private static extern void worldMeshMaterialGetTextureName(IntPtr world, int index, StringBuilder texture);
        [DllImport(DLLNAME)] private static extern void worldGetMeshMaterialColor(IntPtr world, int index, out byte r, out byte g, out byte b, out byte a);
        [DllImport(DLLNAME)] private static extern int worldGetMeshTriangleMaterialIndex(IntPtr world, ulong index);

        // Features
        [DllImport(DLLNAME)] private static extern ulong worldMeshFeatureIndicesCount(IntPtr world);
        [DllImport(DLLNAME)] private static extern uint worldMeshFeatureIndexGet(IntPtr world, ulong index);
        [DllImport(DLLNAME)] private static extern ulong worldMeshFeaturesCount(IntPtr world);
        [DllImport(DLLNAME)] private static extern Vector2 worldMeshFeatureTextureGet(IntPtr world, ulong index);
        [DllImport(DLLNAME)] private static extern Vector3 worldMeshFeatureNormalGet(IntPtr world, ulong index);

        // Waynet
        [DllImport(DLLNAME)] private static extern int worldGetWaynetWaypointCount(IntPtr world);

        // Fetch size of name string to allocate C# memory for the returned char*. Will only work this way.
        // @see https://limbioliong.wordpress.com/2011/11/01/using-the-stringbuilder-in-unmanaged-api-calls/
        [DllImport(DLLNAME)] private static extern int worldGetWaynetWaypointNameSize(IntPtr world, int index);

        // U1 to have bool as 1-byte from C++; https://learn.microsoft.com/en-us/visualstudio/code-quality/ca1414?view=vs-2022&tabs=csharp
        [DllImport(DLLNAME)] private static extern void worldGetWaynetWaypoint(IntPtr world, int index, StringBuilder name, out Vector3 position, out Vector3 direction, [MarshalAs(UnmanagedType.U1)] out bool freePoint, [MarshalAs(UnmanagedType.U1)] out bool underWater, out int waterDepth);
        [DllImport(DLLNAME)] private static extern int worldGetWaynetEdgeCount(IntPtr world);
        [DllImport(DLLNAME)] private static extern void worldGetWaynetEdge(IntPtr world, int index, out uint a, out uint b);


        public static BWorld LoadWorld(IntPtr vdfsPtr, string worldName)
        {
            var worldPtr = PxWorld.pxWorldLoadFromVdf(vdfsPtr, worldName);
            var worldMeshPtr = PxWorld.pxWorldGetMesh(worldPtr);

            BWorld world = new()
            {
                vertexIndices = PxMesh.GetPolygonVertexIndices(worldMeshPtr),
                materialIndices = PxMesh.GetPolygonMaterialIndices(worldMeshPtr),
                featureIndices = PxMesh.GetPolygonFeatureIndices(worldMeshPtr),

                vertices = PxMesh.GetVertices(worldMeshPtr),
                features = PxMesh.GetFeatures(worldMeshPtr),
                materials = PxMesh.GetMaterials(worldMeshPtr),

                waypoints = PxWorld.GetWayPoints(worldPtr),
                waypointEdges = PxWorld.GetWayEdges(worldPtr)
            };

            PxWorld.pxWorldDestroy(worldPtr);

            return world;
        }

        public static Dictionary<int, BSubMesh> CreateSubmeshesForUnity(BWorld world)
        {
            Dictionary<int, BSubMesh> subMeshes = new(world.materials.Length);
            var vertices = world.vertices;
            var vertexIndices = world.vertexIndices;
            var featureIndices = world.featureIndices;
            var features = world.features;

            // We need to put vertex_indices (aka triangles) in reversed order
            // to make Unity draw mesh elements right (instead of upside down)
            for (var loopVertexIndexId = vertexIndices.LongLength - 1; loopVertexIndexId >= 0; loopVertexIndexId--)
            {
                // For each 3 vertexIndices (aka each triangle) there's one materialIndex.
                var materialIndex = world.materialIndices[loopVertexIndexId / 3];

                // The materialIndex was never used before.
                if (!subMeshes.ContainsKey(materialIndex))
                {
                    var newSubMesh = new BSubMesh()
                    {
                        materialIndex = materialIndex,
                        material = world.materials[materialIndex]
                    };

                    subMeshes.Add(materialIndex, newSubMesh);
                }

                var currentSubMesh = subMeshes[materialIndex];
                var origVertexIndex = vertexIndices[loopVertexIndexId];

                // Gothic meshes are too big for Unity by factor 100.
                currentSubMesh.vertices.Add(vertices[(int)origVertexIndex].ToUnityVector() / 100);
                    
                var featureIndex = (int)featureIndices[loopVertexIndexId];
                currentSubMesh.uvs.Add(features[featureIndex].texture.ToUnityVector());
                currentSubMesh.normals.Add(features[featureIndex].normal.ToUnityVector());

                currentSubMesh.triangles.Add(currentSubMesh.vertices.Count - 1);
            }

            return subMeshes;
        }
    }
}