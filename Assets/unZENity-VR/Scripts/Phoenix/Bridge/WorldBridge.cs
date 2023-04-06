using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Unity.VisualScripting;
using UnityEditor;
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
            IntPtr worldPtr = worldLoad(vdfsPtr, worldName);

            BWorld world = new()
            {
                vertexIndices = LoadVertexIndices(worldPtr),
                materialIndices = LoadMaterialIndices(worldPtr),
                featureIndices = LoadFeatureIndices(worldPtr),

                vertices = LoadVertices(worldPtr),
                materials = LoadMaterials(worldPtr),
                featureTextures = LoadFeatureTextures(worldPtr),
                featureNormals = LoadFeatureNormals(worldPtr),

                waypoints = LoadWorldWaypoints(worldPtr),
                waypointEdges = LoadWorldWaypointEdges(worldPtr)
            };

            worldDispose(worldPtr);

            return world;
        }

        public static Dictionary<int, BSubMesh> CreateSubmeshesForUnity(BWorld world)
        {
            Dictionary<int, BSubMesh> subMeshes = new(world.materials.Count);
            var vertices = world.vertices;
            var vertexIndices = world.vertexIndices;
            var featureIndices = world.featureIndices;
            var featureTextures = world.featureTextures;
            var featureNormals = world.featureNormals;

            // We need to put vertex_indices (aka triangles) in reversed order
            // to make Unity draw mesh elements right (instead of upside down)
            for (int loopVertexIndexId = vertexIndices.Count-1; loopVertexIndexId >= 0; loopVertexIndexId--)
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
                currentSubMesh.vertices.Add(vertices[(int)origVertexIndex] / 100);
                    
                var featureIndex = (int)featureIndices[loopVertexIndexId];
                currentSubMesh.uvs.Add(featureTextures[featureIndex]);
                currentSubMesh.normals.Add(featureNormals[featureIndex]);

                currentSubMesh.triangles.Add(currentSubMesh.vertices.Count - 1);
            }

            return subMeshes;
        }

        private static List<uint> LoadVertexIndices(IntPtr worldPtr)
        {
            var size = worldMeshVertexIndicesCount(worldPtr);
            List<uint> vertexIndices = new((int)size);

            for (ulong i = 0; i < size; i++)
            {
                vertexIndices.Add(
                    worldMeshVertexIndexGet(worldPtr, i)
                );
            }

            return vertexIndices;
        }

        private static List<int> LoadMaterialIndices(IntPtr worldPtr)
        {
            // Every 3 vertices (==1 triangle) have 1 material. It means 1/3 of vertexIndices count.
            var size = worldMeshVertexIndicesCount(worldPtr) / 3ul;
            List<int> materialIndices = new((int)size);

            for (ulong i = 0; i < size; i++)
            {
                var materialIndex = worldGetMeshTriangleMaterialIndex(worldPtr, i);
                materialIndices.Add(materialIndex);
            }

            return materialIndices;
        }

        private static List<uint> LoadFeatureIndices(IntPtr worldPtr)
        {
            var size = worldMeshFeatureIndicesCount(worldPtr);
            List<uint> featureIndices = new((int)size);

            for (ulong i = 0; i < size; i++)
            {
                featureIndices.Add(worldMeshFeatureIndexGet(worldPtr, i));
            }

            return featureIndices;
        }


        private static List<Vector3> LoadVertices(IntPtr worldPtr)
        {
            var size = worldGetMeshVertexCount(worldPtr);
            List<Vector3> vertices = new();

            for (int i = 0; i < size; i++)
            {
                vertices.Add(
                    worldGetMeshVertex(worldPtr, i)
                );
            }

            return vertices;
        }

        private static List<BMaterial> LoadMaterials(IntPtr worldPtr)
        {
            int materialCount = worldGetMeshMaterialCount(worldPtr);
            var materials = new List<BMaterial>(materialCount);

            for (var i = 0; i < materialCount; i++)
            {
                StringBuilder name = new(worldGetMeshMaterialNameSize(worldPtr, i));
                worldGetMeshMaterialName(worldPtr, i, name);

                StringBuilder textureName = new(255);
                worldMeshMaterialGetTextureName(worldPtr, i, textureName);

                worldGetMeshMaterialColor(worldPtr, i, out byte r, out byte g, out byte b, out byte a);

                var m = new BMaterial()
                {
                    name = name.ToString(),
                    textureName = textureName.ToString(),
                    // We need to convert uint8 (byte) to float for Unity.
                    color = new Color((float)r / 255, (float)g / 255, (float)b / 255, (float)a / 255)
                };
                materials.Add(m);
            }

            return materials;
        }

        private static List<Vector2> LoadFeatureTextures(IntPtr worldPtr)
        {
            var featureCount = worldMeshFeaturesCount(worldPtr);

            List<Vector2> featureTextures = new((int)featureCount);

            for (ulong i = 0; i < worldMeshFeaturesCount(worldPtr); i++)
            {
                featureTextures.Add(worldMeshFeatureTextureGet(worldPtr, i));
            }

            return featureTextures;
        }

        private static List<Vector3> LoadFeatureNormals(IntPtr worldPtr)
        {
            var featureCount = worldMeshFeaturesCount(worldPtr);

            List<Vector3> featureNormals = new((int)featureCount);

            for (ulong i = 0; i < worldMeshFeaturesCount(worldPtr); i++)
            {
                featureNormals.Add(
                    worldMeshFeatureNormalGet(worldPtr, i)
                );
            }

            return featureNormals;
        }

    private static List<BWaypoint> LoadWorldWaypoints(IntPtr worldPtr)
        {
            var waypointCount = worldGetWaynetWaypointCount(worldPtr);
            var waypoints = new List<BWaypoint>(waypointCount);

            for(int i = 0; i < waypointCount; i++)
            {
                StringBuilder name = new(worldGetWaynetWaypointNameSize(worldPtr, i));

                worldGetWaynetWaypoint(worldPtr, i, name, out Vector3 position, out Vector3 direction, out bool freePoint, out bool underWater, out int waterDepth);

                var waypoint = new BWaypoint()
                {
                    name = name.ToString(),
                    position = position / 100, // Gothic coordinates are too big by factor 100
                    direction = direction,
                    freePoint = freePoint,
                    underWater = underWater,
                    waterDepth = waterDepth
                };
                
                waypoints.Add(waypoint);
            }

            return waypoints;
        }

        private static List<BWaypointEdge> LoadWorldWaypointEdges(IntPtr worldPtr)
        {
            var edgeCount = worldGetWaynetEdgeCount(worldPtr);
            var edges = new List<BWaypointEdge>(edgeCount);

            for (int i = 0; i < edgeCount; i++)
            {
                worldGetWaynetEdge(worldPtr, i, out uint a, out uint b);

                var edge = new BWaypointEdge()
                {
                    a = a,
                    b = b
                };

                edges.Add(edge);
            }

            return edges;
        }
    }
}