using System.Collections.Generic;
using UnityEngine;

namespace UZVR.Phoenix.World
{
    public class BWorld
    {
        public List<Vector3> vertices;
        public List<Vector2> textures;
        public List<Vector3> normals;
        public List<BMaterial> materials;
        public Dictionary<int, List<int>> triangles;

        public List<BWaypoint> waypoints;
        public List<BWaypointEdge> waypointEdges;
    }
}
