using System.Collections.Generic;
using UnityEngine;

namespace UZVR.Phoenix.World
{
    public class PBWorld
    {
        public List<Vector3> vertices;
        public List<PBMaterial> materials;
        public Dictionary<int, List<int>> triangles;

        public List<PBWaypoint> waypoints;
        public List<PBWaypointEdge> waypointEdges;
    }
}
