using System.Collections.Generic;
using System.Threading.Tasks;
using GVR.Extensions;
using GVR.Manager;
using GVR.Manager.Culling;
using GVR.Phoenix.Data;
using PxCs.Interface;
using UnityEngine;
using ZenKit;
using Material = UnityEngine.Material;
using Mesh = UnityEngine.Mesh;
using TextureFormat = UnityEngine.TextureFormat;

namespace GVR.Creator.Meshes
{
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
            int numSubMeshes = world.subMeshes.Values.Count;
            int meshesCreated = 0;

            foreach (var subMesh in world.subMeshes.Values)
            {
                // No texture to add.
                // For G1 this is: material.name == [KEINE, KEINETEXTUREN, DEFAULT, BRETT2, BRETT1, SUMPFWAASER, S:PSIT01_ABODEN]
                // Removing these removes tiny slices of walls on the ground. If anyone finds them, I owe them a beer.
                if (subMesh.material.Texture.IsEmpty())
                    continue;

                var subMeshObj = new GameObject()
                {
                    name = subMesh.material.Name,
                    isStatic = true
                };

                subMeshObj.SetParent(meshObj);

                var meshFilter = subMeshObj.AddComponent<MeshFilter>();
                var meshRenderer = subMeshObj.AddComponent<MeshRenderer>();

                Self.PrepareMeshRenderer(meshRenderer, subMesh);
                Self.PrepareMeshFilter(meshFilter, subMesh);
                Self.PrepareMeshCollider(subMeshObj, meshFilter.sharedMesh, subMesh.material);

#if UNITY_EDITOR
                // Don't set alpha clipped as occluders.
                if (meshRenderer.sharedMaterial.shader.name == AlphaToCoverageShaderName)
                {
                    UnityEditor.GameObjectUtility.SetStaticEditorFlags(subMeshObj, (UnityEditor.StaticEditorFlags)(int.MaxValue & ~(int)UnityEditor.StaticEditorFlags.OccluderStatic));
                }
#endif

                if (LoadingManager.I)
                {
                    LoadingManager.I.AddProgress(LoadingManager.LoadingProgressType.WorldMesh, 1f / numSubMeshes);
                }

                if (++meshesCreated % meshesPerFrame == 0)
                    await Task.Yield(); // Yield to allow other operations to run in the frame
            }
        }

        protected void PrepareMeshRenderer(Renderer rend, WorldData.SubMeshData subMesh)
        {
            var bMaterial = subMesh.material;
            var texture = GetTexture(bMaterial.Texture);

            if (null == texture)
            {
                if (bMaterial.Texture.EndsWithIgnoreCase(".TGA"))
                    Debug.LogError($"This is supposed to be a decal: ${bMaterial.Texture}");
                else
                    Debug.LogError($"Couldn't get texture from name: {bMaterial.Texture}");
            }

            Material material;
            switch (subMesh.material.Group)
            {
                case MaterialGroup.Water:
                    material = GetWaterMaterial(subMesh.material);
                    break;
                default:
                    material = GetDefaultMaterial(texture != null && texture.format == TextureFormat.RGBA32);
                    break;
            }

            rend.material = material;

            // No texture to add.
            if (bMaterial.Texture.IsEmpty())
            {
                Debug.LogWarning($"No texture was set for: {bMaterial.Name}");
                return;
            }

            material.mainTexture = texture;
        }

        private void PrepareMeshFilter(MeshFilter meshFilter, WorldData.SubMeshData subMesh)
        {
            var mesh = new Mesh();
            meshFilter.sharedMesh = mesh;

            mesh.SetVertices(subMesh.vertices);
            mesh.SetTriangles(subMesh.triangles, 0);
            mesh.SetUVs(0, subMesh.uvs);
        }
    }
}
