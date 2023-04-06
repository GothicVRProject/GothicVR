using System.Collections.Generic;
using UnityEngine;

namespace UZVR.Phoenix.World
{
    public class BWorld
    {
        public List<uint> vertexIndices; // index to vertices. 3 indices form one triangle.
        public List<int> materialIndices; // each key (index) of a vertex_index has a material index in here.
        public List<uint> featureIndices; // Each vertex_index has a feature index.

        public List<Vector3> vertices;
        public List<BMaterial> materials;
        public List<Vector2> featureTextures; // Used as uv within Unity
        public List<Vector3> featureNormals;

        public Dictionary<int, BSubMesh> subMeshes;

        public class BSubMesh
        {
            public int materialIndex;
            public BMaterial material;

            public List<Vector3> vertices = new();
            public List<int> triangles = new();
            public List<Vector2> uvs = new() ;
            public List<Vector3> normals = new();
        }

        public List<BWaypoint> waypoints;
        public List<BWaypointEdge> waypointEdges;

    }
}
