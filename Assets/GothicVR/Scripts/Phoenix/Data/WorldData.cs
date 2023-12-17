using System.Collections.Generic;
using System.Numerics;
using ZenKit;
using ZenKit.Vobs;
using Material = ZenKit.Material;
using Vector2 = UnityEngine.Vector2;
using WayNet = ZenKit.Materialized.WayNet;
using WayPoint = ZenKit.WayPoint;

namespace GVR.Phoenix.Data
{
    /// <summary>
    /// Parsed Phoenix World data is arranged in a way to easily be usable by Unity objects.
    /// E.g. by providing submeshes.
    /// </summary>
    public class WorldData
    {
        public Dictionary<int, SubMeshData> subMeshes;

        public class SubMeshData
        {
            public int materialIndex;
            public Material material;

            public List<UnityEngine.Vector3> vertices = new();
            public List<int> triangles = new();
            public List<Vector2> uvs = new() ;
            public List<UnityEngine.Vector3> normals = new();
        }

        public List<VirtualObject> vobs;

        public WayNet wayNet;
        public List<WayPoint> waypoints;
        public WayEdge[] waypointEdges;
    }
}
