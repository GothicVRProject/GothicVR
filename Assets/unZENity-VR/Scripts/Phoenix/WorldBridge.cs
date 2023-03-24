using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace UZVR.Phoenix
{
    public class PCBridge_World
    {
        public List<Vector3> vertices;
        public List<PCBridge_Material> materials;
        public Dictionary<int, List<int>> triangles;

        public List<PCBridge_Waypoint> waypoints;
        public List<PCBridge_WaypointEdge> waypointEdges;
    }

    public class PCBridge_Material
    {
        public string name;
        public Color color;
    }

    public class PCBridge_Waypoint
    {
        public string name;
        public bool freePoint;
        public Vector3 position;
        public Vector3 direction;
        public bool underWater;
        public int waterDepth;
    }

    // Reason to be a struct is to have auto-marshalling for PInvoke
    public struct PCBridge_WaypointEdge {
        public uint a;
        public uint b;
    }


    public class WorldBridge
    {
        private const string DLLNAME = "phoenix-csharp-bridge";

        public IntPtr WorldPtr { get; private set; } = IntPtr.Zero;

        public PCBridge_World World { get; private set; } = new PCBridge_World();


        // Load
        [DllImport(DLLNAME)] private static extern IntPtr loadWorld(IntPtr vdfContainer, string worldFileName);

        // Dispose
        [DllImport(DLLNAME)] private static extern void disposeWorld(IntPtr mesh);



        // Vertices; aka Vertexes ;-)
        [DllImport(DLLNAME)] private static extern int getWorldMeshVertexCount(IntPtr world);
        [DllImport(DLLNAME)] private static extern Vector3 getWorldMeshVertex(IntPtr world, int index);

        // Triangles
        [DllImport(DLLNAME)] private static extern int getWorldMeshTriangleCount(IntPtr world);
        [DllImport(DLLNAME)] private static extern void getWorldMeshTriangle(IntPtr world, int index, out UInt32 valueA, out UInt32 valueB, out UInt32 valueC);

        // Materials
        [DllImport(DLLNAME)] private static extern int getWorldMeshMaterialCount(IntPtr world);
        [DllImport(DLLNAME)] private static extern int getWorldMeshMaterialNameSize(IntPtr world, int index);
        [DllImport(DLLNAME)] private static extern void getWorldMeshMaterialName(IntPtr world, int index, StringBuilder name);
        [DllImport(DLLNAME)] private static extern void getWorldMeshMaterialColor(IntPtr world, int index, out byte r, out byte g, out byte b, out byte a);

        [DllImport(DLLNAME)] private static extern int getWorldMeshTriangleMaterialIndex(IntPtr world, int index);

        // Waynet
        [DllImport(DLLNAME)] private static extern int getWorldWaynetWaypointCount(IntPtr world);

        // Fetch size of name string to allocate C# memory for the returned char*. Will only work this way.
        // @see https://limbioliong.wordpress.com/2011/11/01/using-the-stringbuilder-in-unmanaged-api-calls/
        [DllImport(DLLNAME)] private static extern int getWorldWaynetWaypointNameSize(IntPtr world, int index);

        // U1 to have bool as 1-byte from C++; https://learn.microsoft.com/en-us/visualstudio/code-quality/ca1414?view=vs-2022&tabs=csharp
        [DllImport(DLLNAME)] private static extern void getWorldWaynetWaypoint(IntPtr world, int index, StringBuilder name, out Vector3 position, out Vector3 direction, [MarshalAs(UnmanagedType.U1)] out bool freePoint, [MarshalAs(UnmanagedType.U1)] out bool underWater, out int waterDepth);
        [DllImport(DLLNAME)] private static extern int getWorldWaynetEdgeCount(IntPtr world);
        [DllImport(DLLNAME)] private static extern PCBridge_WaypointEdge getWorldWaynetEdge(IntPtr world, int index);


        public WorldBridge(VdfsBridge vdfs, string worldName)
        {
            WorldPtr = loadWorld(vdfs.VdfsPtr, worldName);

            World.vertices = _GetWorldVertices();
            World.materials = _GetWorldMaterials();
            World.triangles = _GetWorldTriangles(World.materials.Count);
            World.waypoints = _GetWorldWaypoints();
            World.waypointEdges = _GetWorldWaypointEdges();
        }

        // TODO: Check when the class is disposed to free memory within DLL.
        // If happening too late, then free it manually earlier.
        ~WorldBridge()
        {
            disposeWorld(WorldPtr);
            WorldPtr = IntPtr.Zero;
        }

        private List<Vector3> _GetWorldVertices()
        {
            List<Vector3> vertices = new();


            for (int i = 0; i < getWorldMeshVertexCount(WorldPtr); i++)
            {
                vertices.Add(getWorldMeshVertex(WorldPtr, i));
            }

            return vertices;
        }

        private List<PCBridge_Material> _GetWorldMaterials()
        {
            int materialCount = getWorldMeshMaterialCount(WorldPtr);
            var materials = new List<PCBridge_Material>(materialCount);

            for (var i=0; i<materialCount; i++)
            {
                getWorldMeshMaterialColor(WorldPtr, i, out byte r, out byte g, out byte b, out byte a);
                // We need to convert uint8 (byte) to float for Unity.

                StringBuilder name = new(getWorldMeshMaterialNameSize(WorldPtr, i));
                getWorldMeshMaterialName(WorldPtr, i, name);

                var m = new PCBridge_Material() {
                    name = name.ToString(),
                    color = new Color((float)r/255, (float)g /255, (float)b /255, (float)a /255)
                };
                materials.Add(m);
            }

            return materials;
        }


        private Dictionary<int, List<int>> _GetWorldTriangles(int materialCount)
        {
            Dictionary<int, List<int>> triangles = new();

            // FIXME We can optimize by cleaning up empty materials.
            // PERFORMANCE e.g. worlds.vdfs has 2263 materials, but only ~1300 of them have triangles attached to it.

            // Initialize array
            for (var i=0; i < materialCount; i++)
            {
                triangles.Add(i, new());
            }


            for (int i = 0; i < getWorldMeshTriangleCount(WorldPtr); i++)
            {
                var materialIndex = getWorldMeshTriangleMaterialIndex(WorldPtr, i);
                getWorldMeshTriangle(WorldPtr, i, out uint valueA, out uint valueB, out uint valueC);

                // We need to flip valueA with valueC to:
                // 1/ have the mesh elements shown (flipped surface) and
                // 2/ world mirrored right way.
                triangles[materialIndex].AddRange(new []{ (int)valueC, (int)valueB, (int)valueA });
            }

            return triangles;
        }


        private List<PCBridge_Waypoint> _GetWorldWaypoints()
        {
            var waypointCount = getWorldWaynetWaypointCount(WorldPtr);
            var waypoints = new List<PCBridge_Waypoint>(waypointCount);

            for(int i = 0; i < waypointCount; i++)
            {
                StringBuilder name = new(getWorldWaynetWaypointNameSize(WorldPtr, i));

                getWorldWaynetWaypoint(WorldPtr, i, name, out Vector3 position, out Vector3 direction, out bool freePoint, out bool underWater, out int waterDepth);

                var waypoint = new PCBridge_Waypoint()
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

        private List<PCBridge_WaypointEdge> _GetWorldWaypointEdges()
        {
            var edgeCount = getWorldWaynetEdgeCount(WorldPtr);
            var edges = new List<PCBridge_WaypointEdge>(edgeCount);

            for (int i = 0; i < edgeCount; i++)
            {
                var edge = getWorldWaynetEdge(WorldPtr, i);

                edges.Add(edge);
            }

            return edges;
        }
    }
}