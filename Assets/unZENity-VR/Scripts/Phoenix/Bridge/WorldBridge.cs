using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using UnityEditor;
using UnityEngine;
using UZVR.Phoenix.World;

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
            World.triangles = GetWorldTriangles(World.materials.Count);

            World.featureIndices = GetWorldFeatureIndices();
            World.features = GetWorldFeatures();

            World.waypoints = _GetWorldWaypoints();
            World.waypointEdges = _GetWorldWaypointEdges();
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


        private Dictionary<int, List<uint>> GetWorldTriangles(int materialCount)
        {
            Dictionary<int, List<uint>> triangles = new();

            // FIXME We can optimize by cleaning up empty materials.
            // PERFORMANCE e.g. worlds.vdfs has 2263 materials, but only ~1300 of them have triangles attached to it.

            // Initialize arrays
            for (var i=0; i < materialCount; i++)
            {
                triangles.Add(i, new());
            }

            var size = worldMeshVertexIndicesCount(WorldPtr);

            for (int i = 0; i < (int)worldMeshVertexIndicesCount(WorldPtr); i+=3)
            {
                // only 1/3 is the count of materials. Aka every 1st of 3 triangle indices to check for its value.
                var materialIndex = worldGetMeshTriangleMaterialIndex(WorldPtr, (ulong)i / 3);
                triangles[materialIndex].AddRange(new[]
                {
                    worldMeshVertexIndexGet(WorldPtr, (ulong) i),
                    worldMeshVertexIndexGet(WorldPtr, (ulong) i+1),
                    worldMeshVertexIndexGet(WorldPtr, (ulong) i+2)
                });

                // We need to flip valueA with valueC to:
                // 1/ have the mesh elements shown (flipped surface) and
                // 2/ world mirrored right way.
            }

            return triangles;
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

        private List<BWorld.BFeature> GetWorldFeatures()
        {
            List<BWorld.BFeature> features = new();

            for (ulong i = 0; i < worldMeshFeaturesCount(WorldPtr); i++)
            {
                features.Add(new()
                {
                    texture = worldMeshFeatureTextureGet(WorldPtr, i),
                    normal = worldMeshFeatureNormalGet(WorldPtr, i)
                });
            }

            return features;
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