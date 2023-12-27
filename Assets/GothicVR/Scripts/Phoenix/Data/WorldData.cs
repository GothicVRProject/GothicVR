using System.Collections.Generic;
using UnityEngine;
using ZenKit;
using ZenKit.Vobs;

namespace GVR.Phoenix.Data
{
    /// <summary>
    /// Parsed Phoenix World data is arranged in a way to easily be usable by Unity objects.
    /// E.g. by providing submeshes.
    /// </summary>
    public class WorldData
    {
        // We need to store it as we need the pointer to it for load+save of un-cached vobs.
        public IWorld world;
        
        public Dictionary<int, SubMeshData> subMeshes;

        public class SubMeshData
        {
            public int materialIndex;
            public IMaterial material;

            public List<Vector3> vertices = new();
            public List<int> triangles = new();
            public List<Vector2> uvs = new() ;
            public List<Vector3> normals = new();
        }

        public List<IVirtualObject> vobs;
        public IWayNet wayNet;
    }
}
