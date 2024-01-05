using System.Threading.Tasks;
using GVR.Extensions;
using GVR.Manager;
using GVR.World;
using UnityEditor;
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
            int numSubMeshes = world.SubMeshes.Values.Count;
            int meshesCreated = 0;

            foreach (var subMesh in world.SubMeshes.Values)
            {
                // No texture to add.
                // For G1 this is: material.name == [KEINE, KEINETEXTUREN, DEFAULT, BRETT2, BRETT1, SUMPFWAASER, S:PSIT01_ABODEN]
                // Removing these removes tiny slices of walls on the ground. If anyone finds them, I owe them a beer. ;-)
                if (subMesh.Material.Texture.IsEmpty() || subMesh.Triangles.IsEmpty())
                    continue;

                var subMeshObj = new GameObject()
                {
                    name = subMesh.Material.Name,
                    isStatic = true
                };

                subMeshObj.SetParent(meshObj);

                var meshFilter = subMeshObj.AddComponent<MeshFilter>();
                var meshRenderer = subMeshObj.AddComponent<MeshRenderer>();

                Self.PrepareMeshRenderer(meshRenderer, subMesh);
                Self.PrepareMeshFilter(meshFilter, subMesh);
                Self.PrepareMeshCollider(subMeshObj, meshFilter.sharedMesh, subMesh.Material);

#if UNITY_EDITOR // Only needed for Occlusion Culling baking
                // Don't set transparent meshes as occluders.
                if (IsTransparentShader(meshRenderer.sharedMaterial.shader))
                    GameObjectUtility.SetStaticEditorFlags(subMeshObj, (StaticEditorFlags)(int.MaxValue & ~(int)StaticEditorFlags.OccluderStatic));
#endif

                if (LoadingManager.I)
                    LoadingManager.I.AddProgress(LoadingManager.LoadingProgressType.WorldMesh, 1f / numSubMeshes);

                if (++meshesCreated % meshesPerFrame == 0)
                    await Task.Yield(); // Yield to allow other operations to run in the frame
            }
        }

        protected void PrepareMeshRenderer(Renderer rend, WorldData.SubMeshData subMesh)
        {
            var bMaterial = subMesh.Material;
            var texture = GetTexture(bMaterial.Texture);

            if (null == texture)
            {
                if (bMaterial.Texture.EndsWithIgnoreCase(".TGA"))
                    Debug.LogError($"This is supposed to be a decal: ${bMaterial.Texture}");
                else
                    Debug.LogError($"Couldn't get texture from name: {bMaterial.Texture}");
            }

            Material material;
            switch (subMesh.Material.Group)
            {
                case MaterialGroup.Water:
                    material = GetWaterMaterial(subMesh.Material);
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

            if (subMesh.Triangles.Count % 3 != 0)
                Debug.LogError("Triangle count is not a multiple of 3");

            mesh.SetVertices(subMesh.Vertices);
            mesh.SetTriangles(subMesh.Triangles, 0);
            mesh.SetUVs(0, subMesh.Uvs);
        }
    }
}
