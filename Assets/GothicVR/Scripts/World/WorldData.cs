﻿using GVR.Caches;
using System.Collections.Generic;
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
        // ReSharper disable once NotAccessedField.Global
        public global::ZenKit.World World;
        public List<IVirtualObject> Vobs;
        public CachedWayNet WayNet;
        
        public List<SubMeshData> SubMeshes;

        public class SubMeshData
        {
            public IMaterial Material;
            public AssetCache.TextureArrayTypes TextureArrayType;

            public readonly List<Vector3> Vertices = new();
            public readonly List<int> Triangles = new();
            public readonly List<Vector4> Uvs = new() ;
            public readonly List<Vector3> Normals = new();
            public readonly List<Color32> Light = new();
            public readonly List<Vector2> TextureAnimation = new(0);
        }

    }
}
