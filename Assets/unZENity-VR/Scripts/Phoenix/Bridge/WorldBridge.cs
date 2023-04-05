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
    public class WorldBridge
    {

        public IntPtr WorldPtr { get; private set; } = IntPtr.Zero;

        public BWorld World { get; private set; } = new();

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


        public WorldBridge(VdfsBridge vdfs, string worldName)
        {
            WorldPtr = worldLoad(vdfs.VdfsPtr, worldName);

            SetWorldVertices(World);
            World.materials = _GetWorldMaterials();
            SetWorldTriangles(World, World.materials.Count);

            World.featureIndices = GetWorldFeatureIndices();
            SetWorldFeatures(World);

            World.waypoints = _GetWorldWaypoints();
            World.waypointEdges = _GetWorldWaypointEdges();
        }





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
                featureNormals = LoadFeatureNormals(worldPtr)                
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

            // Store temporarily if a specific vertex is already in the material. Reference new vertex_indices to it then.
            // dict<materialId, dict<originalVertexId, newVertexIdInList>
            Dictionary<int, Dictionary<uint, int>> tempVerticesMapping = new();

            for (int loopVertexIndexId = 0; loopVertexIndexId < vertexIndices.Count; loopVertexIndexId++)
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
                    tempVerticesMapping.Add(materialIndex, new());
                }

                var currentSubMesh = subMeshes[materialIndex];
                var curTempVerticesMapping = tempVerticesMapping[materialIndex];

                var origVertexIndex = vertexIndices[loopVertexIndexId];
                if (!curTempVerticesMapping.ContainsKey(origVertexIndex))
                {
                    // Gothic meshes are too big for Unity by factor 100.
                    currentSubMesh.vertices.Add(vertices[(int)origVertexIndex] / 100);
                    
                    // Temporarily store vertexIndex as it can be referenced by other
                    // vertexIndices (aka triangles) within the same SubMesh.
                    curTempVerticesMapping.Add(origVertexIndex, curTempVerticesMapping.Count);

                    currentSubMesh.debugTextureIndices.Add((int)featureIndices[loopVertexIndexId]);

                    var featureIndex = (int)featureIndices[loopVertexIndexId];
                    currentSubMesh.uvs.Add(featureTextures[featureIndex]);
                    currentSubMesh.normals.Add(featureNormals[featureIndex]);
                }

                var newVertexIndex = curTempVerticesMapping[origVertexIndex];

                currentSubMesh.triangles.Add(newVertexIndex);

                // We need to flip valueA with valueC to have the mesh elements
                // shown upside down (original data shows flipped surface in Unity)
                var triangleCount = currentSubMesh.triangles.Count;
                if (triangleCount > 0 && triangleCount % 3 == 0)
                {
                    // Flip n-1 with n-3
                    (currentSubMesh.triangles[triangleCount - 1], currentSubMesh.triangles[triangleCount - 3]) =
                        (currentSubMesh.triangles[triangleCount - 3], currentSubMesh.triangles[triangleCount - 1]);
                }
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



        // TODO: Check when the class is disposed to free memory within DLL.
        // If happening too late, then free it manually earlier.
        ~WorldBridge()
        {
            worldDispose(WorldPtr);
            WorldPtr = IntPtr.Zero;
        }



        private void SetWorldVertices(BWorld world)
        {
            List<Vector3> vertices = new();

            for (int i = 0; i < worldGetMeshVertexCount(WorldPtr); i++)
            {
                var vertex = worldGetMeshVertex(WorldPtr, i);

                vertices.Add(vertex / 100); // Gothic coordinates are too big by factor 100
            }

            world.vertices = vertices;
        }

        private List<BMaterial> _GetWorldMaterials()
        {
            int materialCount = worldGetMeshMaterialCount(WorldPtr);
            var materials = new List<BMaterial>(materialCount);

            for (var i=0; i<materialCount; i++)
            {
                StringBuilder name = new(worldGetMeshMaterialNameSize(WorldPtr, i));
                worldGetMeshMaterialName(WorldPtr, i, name);

                StringBuilder textureName = new(255);
                worldMeshMaterialGetTextureName(WorldPtr, i, textureName);

                worldGetMeshMaterialColor(WorldPtr, i, out byte r, out byte g, out byte b, out byte a);

                var m = new BMaterial() {
                    name = name.ToString(),
                    textureName = textureName.ToString(),
                    // We need to convert uint8 (byte) to float for Unity.
                    color = new Color((float)r/255, (float)g /255, (float)b /255, (float)a /255)
                };
                materials.Add(m);
            }

            return materials;
        }


        private void SetWorldTriangles(BWorld world, int materialCount)
        {
            Dictionary<int, List<uint>> materializedTriangles = new();
            List<uint> triangles = new();
            List<int> materialIndices = new();

            // FIXME We can optimize by cleaning up empty materials.
            // PERFORMANCE e.g. worlds.vdfs has 2263 materials, but only ~1300 of them have triangles attached to it.

            // Initialize arrays
            for (var i=0; i < materialCount; i++)
            {
                materializedTriangles.Add(i, new());
            }

            for (int i = 0; i < (int)worldMeshVertexIndicesCount(WorldPtr); i+=3)
            {
                var vertexIndex1 = worldMeshVertexIndexGet(WorldPtr, (ulong)i);
                var vertexIndex2 = worldMeshVertexIndexGet(WorldPtr, (ulong)i + 1);
                var vertexIndex3 = worldMeshVertexIndexGet(WorldPtr, (ulong)i + 2);

                // only 1/3 is the count of materials. Aka every 1st of 3 triangle indices to check for its value.
                var materialIndex = worldGetMeshTriangleMaterialIndex(WorldPtr, (ulong)i / 3);
                materialIndices.Add(materialIndex);

                materializedTriangles[materialIndex].AddRange(new[] { vertexIndex1, vertexIndex2, vertexIndex3 });
                triangles.AddRange(new[] { vertexIndex1, vertexIndex2, vertexIndex3 });
            }

            world.materialIndices = materialIndices;

            world.materializedTriangles = materializedTriangles;
            world.vertexIndices = triangles;
        }

        private List<uint> GetWorldFeatureIndices()
        {
            List<uint> featureIndices = new();

            for (ulong i = 0; i < worldMeshFeatureIndicesCount(WorldPtr); i++)
            {
                featureIndices.Add(worldMeshFeatureIndexGet(WorldPtr, i));
            }

            return featureIndices;
        }

        private void SetWorldFeatures(BWorld world)
        {
            var featureCount = worldMeshFeaturesCount(WorldPtr);

            world.featureTextures = new((int)featureCount);
            world.featureNormals = new((int)featureCount);

            for (ulong i = 0; i < worldMeshFeaturesCount(WorldPtr); i++)
            {
                world.featureTextures.Add(worldMeshFeatureTextureGet(WorldPtr, i));
                world.featureNormals.Add(worldMeshFeatureNormalGet(WorldPtr, i));
            }
        }


    private List<BWaypoint> _GetWorldWaypoints()
        {
            var waypointCount = worldGetWaynetWaypointCount(WorldPtr);
            var waypoints = new List<BWaypoint>(waypointCount);

            for(int i = 0; i < waypointCount; i++)
            {
                StringBuilder name = new(worldGetWaynetWaypointNameSize(WorldPtr, i));

                worldGetWaynetWaypoint(WorldPtr, i, name, out Vector3 position, out Vector3 direction, out bool freePoint, out bool underWater, out int waterDepth);

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

        private List<BWaypointEdge> _GetWorldWaypointEdges()
        {
            var edgeCount = worldGetWaynetEdgeCount(WorldPtr);
            var edges = new List<BWaypointEdge>(edgeCount);

            for (int i = 0; i < edgeCount; i++)
            {
                worldGetWaynetEdge(WorldPtr, i, out uint a, out uint b);

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