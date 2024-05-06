using System.Collections.Generic;
using GVR.Caches;
using UnityEngine;
using ZenKit;
using ZenKit.Vobs;

namespace GVR.World
{
    /// <summary>
    /// Parsed ZenKit World data is arranged in a way to easily be usable by Unity objects.
    /// E.g. by providing sub meshes.
    /// </summary>
    public class WorldData
    {
        // We need to store it as we need the pointer to it for load+save of un-cached vobs.
        public IWorld World;
        public List<IVirtualObject> Vobs;
        public IWayNet WayNet;
        
        public List<SubMeshData> SubMeshes;

        public class SubMeshData
        {
            public IMaterial Material;
            public TextureCache.TextureArrayTypes TextureArrayType;

            public readonly List<Vector3> Vertices = new();
            public readonly List<int> Triangles = new();
            public readonly List<Vector4> Uvs = new() ;
            public readonly List<Vector3> Normals = new();
            public readonly List<Color32> BakedLightColors = new();
            public readonly List<Vector2> TextureAnimations = new();
        }

    }
}
