using PxCs.Data;
using System.Collections.Generic;
using UnityEngine;

namespace UZVR.Phoenix.World
{
    public class BWorld
    {
        public int[] vertexIndices; // index to vertices. 3 indices form one triangle.
        public int[] materialIndices; // each key (index) of a vertex_index has a material index in here.
        public int[] featureIndices; // Each vertex_index has a feature index.

        public System.Numerics.Vector3[] vertices;
        public PxFeatureData[] features;
        public PxMaterialData[] materials;

        public Dictionary<int, BSubMesh> subMeshes;

        public class BSubMesh
        {
            public int materialIndex;
            public PxMaterialData material;

            public List<Vector3> vertices = new();
            public List<int> triangles = new();
            public List<Vector2> uvs = new() ;
            public List<Vector3> normals = new();
        }

        public PxWayPointData[] waypoints;
        public PxWayEdgeData[] waypointEdges;

    }
}
