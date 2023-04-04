﻿using System.Collections.Generic;
using UnityEngine;

namespace UZVR.Phoenix.World
{
    public class BWorld
    {
        public class BFeature
        {
            public Vector2 texture; // uv
            public Vector3 normal;
        }

        public List<Vector3> vertices;
        public List<BMaterial> materials;
        public Dictionary<int, List<uint>> triangles;

        public List<uint> featureIndices;
        public List<BFeature> features;

        public List<BWaypoint> waypoints;
        public List<BWaypointEdge> waypointEdges;
    }
}
