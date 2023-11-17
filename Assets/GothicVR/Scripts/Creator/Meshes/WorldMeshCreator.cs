using System.Collections.Generic;
using System.Threading.Tasks;
using GVR.Extensions;
using GVR.Manager;
using GVR.Manager.Culling;
using GVR.Phoenix.Data;
using UnityEngine;

namespace GVR.Creator.Meshes
{
    public abstract class WorldMeshCreator : MeshCreator
    {
        public static async Task<GameObject> CreateAsync(WorldData world, GameObject parent, int meshesPerFrame)
        {
            var meshObj = new GameObject()
            {
                name = "Mesh",
                isStatic = true
            };
            meshObj.SetParent(parent);

            // Track the progress of each sub-mesh creation separately
            int numSubMeshes = world.subMeshes.Values.Count;
            int meshesCreated = 0;
            var worldMeshesForCulling = new List<GameObject>();

            foreach (var subMesh in world.subMeshes.Values)
            {
                // No texture to add.
                // For G1 this is: material.name == [KEINE, KEINETEXTUREN, DEFAULT, BRETT2, BRETT1, SUMPFWAASER, S:PSIT01_ABODEN]
                // Removing these removes tiny slices of walls on the ground. If anyone finds them, I owe them a beer.
                if (subMesh[0].material.texture == "")
                {
                    continue;
                }

                var subMeshObj = new GameObject()
                {
                    name = subMesh[0].material.name!,
                    isStatic = true
                };

                subMeshObj.SetParent(meshObj);

                var i = 0;
                foreach (var subSubMesh in subMesh)
                {
                    var subSubMeshObj = new GameObject()
                    {
                        name = i++.ToString(),
                        isStatic = true
                    };

                    var meshFilter = subSubMeshObj.AddComponent<MeshFilter>();
                    var meshRenderer = subSubMeshObj.AddComponent<MeshRenderer>();

                    PrepareMeshRenderer(meshRenderer, subSubMesh);
                    PrepareMeshFilter(meshFilter, subSubMesh);
                    PrepareMeshCollider(subSubMeshObj, meshFilter.sharedMesh, subSubMesh.material);

#if UNITY_EDITOR
                    // Don't set transparent meshes as occluders.
                    if (IsTransparentShader(meshRenderer.sharedMaterial.shader))
                    {
                        UnityEditor.GameObjectUtility.SetStaticEditorFlags(subSubMeshObj, (UnityEditor.StaticEditorFlags)(int.MaxValue & ~(int)UnityEditor.StaticEditorFlags.OccluderStatic));
                    }
#endif

                    subSubMeshObj.SetParent(subMeshObj);
                    worldMeshesForCulling.Add(subSubMeshObj);

                    if (LoadingManager.I)
                    {
                        LoadingManager.I.AddProgress(LoadingManager.LoadingProgressType.WorldMesh, 1f / numSubMeshes);
                    }

                    if (++meshesCreated % meshesPerFrame == 0)
                        await Task.Yield(); // Yield to allow other operations to run in the frame  
                }
            }

            if (WorldCullingManager.I)
            {
                WorldCullingManager.I.PrepareWorldCulling(worldMeshesForCulling);
            }

            return meshObj;
        }
    }
}