using System;
using System.Threading.Tasks;
using GVR.Caches;
using GVR.Extensions;
using GVR.Globals;
using GVR.Manager;
using GVR.World;
using UnityEditor;
using UnityEngine;
using ZenKit;
using Mesh = UnityEngine.Mesh;

namespace GVR.Creator.Meshes
{

    [Obsolete("Use MeshFactory and *MeshBuilder instead.")]
    public class WorldMeshCreator : AbstractMeshCreator
    {
        // As we subclass the main Mesh Creator, we need to have a parent-child inheritance instance.
        // Needed e.g. for overwriting PrepareMeshRenderer() to change specific behaviour.
        private static readonly WorldMeshCreator Self = new();

        public static async Task CreateAsync(WorldData world, GameObject parent, int meshesPerFrame)
        {
            var meshObj = new GameObject()
            {
                name = "Mesh",
                isStatic = true
            };
            meshObj.SetParent(parent);

            // Track the progress of each sub-mesh creation separately
            int numSubMeshes = world.SubMeshes.Count;
            int meshesCreated = 0;

            foreach (WorldData.SubMeshData subMesh in world.SubMeshes)
            {
                // No texture to add.
                // For G1 this is: material.name == [KEINE, KEINETEXTUREN, DEFAULT, BRETT2, BRETT1, SUMPFWAASER, S:PSIT01_ABODEN]
                // Removing these removes tiny slices of walls on the ground. If anyone finds them, I owe them a beer. ;-)
                if (subMesh.Material.Texture.IsEmpty() || subMesh.Triangles.IsEmpty())
                {
                    continue;
                }

                var subMeshObj = new GameObject()
                {
                    name = $"{subMesh.TextureArrayType} world chunk",
                    isStatic = true
                };

                subMeshObj.SetParent(meshObj);

                MeshFilter meshFilter = subMeshObj.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = subMeshObj.AddComponent<MeshRenderer>();

                meshRenderer.material = Constants.LoadingMaterial;
                TextureCache.WorldMeshRenderersForTextureArray.Add((meshRenderer, subMesh));
                Self.PrepareMeshFilter(meshFilter, subMesh);
                Self.PrepareMeshCollider(subMeshObj, meshFilter.sharedMesh, subMesh.Material);

#if UNITY_EDITOR // Only needed for Occlusion Culling baking
                // Don't set transparent meshes as occluders.
                if (IsTransparentShader(subMesh))
                {
                    GameObjectUtility.SetStaticEditorFlags(subMeshObj, (StaticEditorFlags)(int.MaxValue & ~(int)StaticEditorFlags.OccluderStatic));
                }
#endif

                if (LoadingManager.I)
                {
                    LoadingManager.I.AddProgress(LoadingManager.LoadingProgressType.WorldMesh, 1f / numSubMeshes);
                }

                if (++meshesCreated % meshesPerFrame == 0)
                {
                    await Task.Yield(); // Yield to allow other operations to run in the frame
                }
            }
        }

        private void PrepareMeshFilter(MeshFilter meshFilter, WorldData.SubMeshData subMesh)
        {
            var mesh = new Mesh();
            meshFilter.sharedMesh = mesh;
            mesh.SetVertices(subMesh.Vertices);
            mesh.SetTriangles(subMesh.Triangles, 0);
            mesh.SetUVs(0, subMesh.Uvs);
            mesh.SetNormals(subMesh.Normals);
            mesh.SetColors(subMesh.BakedLightColors);
            if (subMesh.Material.Group == MaterialGroup.Water)
            {
                mesh.SetUVs(1, subMesh.TextureAnimations);
            }
        }

        private static bool IsTransparentShader(WorldData.SubMeshData subMeshData)
        {
            if (subMeshData == null)
            {
                return false;
            }

            return subMeshData.TextureArrayType != TextureCache.TextureArrayTypes.Opaque;
        }
    }
}
