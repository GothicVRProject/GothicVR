﻿using PxCs.Data;
using PxCs.Data.Mesh;
using PxCs.Data.Vob;
using PxCs.Data.WayNet;
using System;
using System.Collections.Generic;
using UnityEngine;
using ZenKit;
using ZenKit.Vobs.Materialized;
using Material = ZenKit.Materialized.Material;
using WayPoint = ZenKit.Materialized.WayPoint;

namespace GVR.Phoenix.Data
{
    /// <summary>
    /// Parsed Phoenix World data is arranged in a way to easily be usable by Unity objects.
    /// E.g. by providing submeshes.
    /// </summary>
    public class WorldData
    {
        public int[] vertexIndices; // index to vertices. 3 indices form one triangle.
        public int[] materialIndices; // each key (index) of a vertex_index has a material index in here.
        public int[] featureIndices; // Each vertex_index has a feature index.

        public System.Numerics.Vector3[] vertices;
        public Vertex[] features;
        public List<Material> materials;

        public Dictionary<int, List<SubMeshData>> subMeshes;

        public class SubMeshData
        {
            public int materialIndex;
            public Material material;

            public List<Vector3> vertices = new();
            public List<int> triangles = new();
            public List<Vector2> uvs = new() ;
            public List<Vector3> normals = new();
        }

        public List<VirtualObject> vobs;

        public List<WayPoint> waypoints;
        public WayEdge[] waypointEdges;
    }
}
