using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace UZVR
{
    public struct PCBridge_World
    {
        public List<Vector3> vertices;
        public List<PCBridge_Material> materials;
        public Dictionary<int, List<int>> triangles;

        public List<PCBridge_Waypoint> waypoints;
        public List<PCBridge_WaypointEdge> waypointEdges;
    }

    public struct PCBridge_Material
    {
        public string name;
        public Color color;
    }

    public struct PCBridge_Waypoint
    {
        public string name;
        public bool freePoint;
        public Vector3 position;
        public Vector3 direction;
        public bool underWater;
        public int waterDepth;
    }

    public struct PCBridge_WaypointEdge {
        public uint a;
        public uint b;
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
            world.waypoints = _GetWorldWaypoints(worldPtr);
            world.waypointEdges = _GetWorldWaypointEdges(worldPtr);

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
                vertices.Add(getWorldMeshVertex(worldPtr, i));
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

                StringBuilder name = new(getWorldMeshMaterialNameSize(worldPtr, i));
                getWorldMeshMaterialName(worldPtr, i, name);

                var m = new PCBridge_Material() {
                    name = name.ToString(),
                    color = new Color((float)r/255, (float)g /255, (float)b /255, (float)a /255)
                };
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
                getWorldMeshTriangle(worldPtr, i, out uint valueA, out uint valueB, out uint valueC);

                // We need to flip valueA with valueC to:
                // 1/ have the mesh elements shown (flipped surface) and
                // 2/ world mirrored right way.
                triangles[materialIndex].AddRange(new []{ (int)valueC, (int)valueB, (int)valueA });
            }

            return triangles;
        }


        private List<PCBridge_Waypoint> _GetWorldWaypoints(IntPtr worldPtr)
        {
            var waypointCount = getWorldWaynetWaypointCount(worldPtr);
            var waypoints = new List<PCBridge_Waypoint>(waypointCount);

            for(int i = 0; i < waypointCount; i++)
            {
                StringBuilder name = new(getWorldWaynetWaypointNameSize(worldPtr, i));

                getWorldWaynetWaypoint(worldPtr, i, name, out Vector3 position, out Vector3 direction, out bool freePoint, out bool underWater, out int waterDepth);

                var waypoint = new PCBridge_Waypoint()
                {
                    name = name.ToString(),
                    position = position,
                    direction = direction,
                    freePoint = freePoint,
                    underWater = underWater,
                    waterDepth = waterDepth
                };
                
                waypoints.Add(waypoint);
            }

            return waypoints;
        }

        private List<PCBridge_WaypointEdge> _GetWorldWaypointEdges(IntPtr worldPtr)
        {
            var edgeCount = getWorldWaynetEdgeCount(worldPtr);
            var edges = new List<PCBridge_WaypointEdge>(edgeCount);

            for (int i = 0; i < edgeCount; i++)
            {
                var edge = getWorldWaynetEdge(worldPtr, i);

                edges.Add(edge);
            }

            return edges;
        }
    }
}