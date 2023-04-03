using System.Collections.Generic;
using UnityEngine;

namespace UZVR.Phoenix.World
{
    public class BWorld
    {
        public List<Vector3> vertices;
        public List<BMaterial> materials;
        public Dictionary<int, List<int>> triangles;
        public List<BWaypoint> waypointsList;
        public Dictionary<string, BWaypoint> waypointsDict;
        public List<BWaypointEdge> waypointEdges;
    }
}
