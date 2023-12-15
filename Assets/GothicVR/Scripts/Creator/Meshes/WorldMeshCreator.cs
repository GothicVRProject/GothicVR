using System.Collections.Generic;
using System.Threading.Tasks;
using GVR.Extensions;
using GVR.Manager;
using GVR.Manager.Culling;
using GVR.Phoenix.Data;
using PxCs.Interface;
using UnityEngine;

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

                    Self.PrepareMeshRenderer(meshRenderer, subSubMesh);
                    Self.PrepareMeshFilter(meshFilter, subSubMesh);
                    Self.PrepareMeshCollider(subSubMeshObj, meshFilter.sharedMesh, subSubMesh.material);

#if UNITY_EDITOR
                    // Don't set alpha clipped as occluders.
                    if (meshRenderer.sharedMaterial.shader.name == AlphaToCoverageShaderName)
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
        }

        protected void PrepareMeshRenderer(Renderer rend, WorldData.SubMeshData subMesh)
        {
            var bMaterial = subMesh.material;
            var texture = GetTexture(bMaterial.texture);

            if (null == texture)
            {
                if (bMaterial.texture.EndsWith(".TGA"))
                    Debug.LogError("This is supposed to be a decal: " + bMaterial.texture);
                else
                    Debug.LogError("Couldn't get texture from name: " + bMaterial.texture);
            }

            Material material;
            switch (subMesh.material.group)
            {
                case PxMaterial.PxMaterialGroup.PxMaterialGroup_Water:
                    material = GetWaterMaterial(subMesh.material);
                    break;
                default:
                    material = GetDefaultMaterial(texture != null && texture.format == TextureFormat.RGBA32);
                    break;
            }

            rend.material = material;

            // No texture to add.
            if (bMaterial.texture == "")
            {
                Debug.LogWarning("No texture was set for: " + bMaterial.name);
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
