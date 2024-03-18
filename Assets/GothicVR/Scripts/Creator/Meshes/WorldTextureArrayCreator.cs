using System.Threading.Tasks;
using GVR.Caches;
using GVR.World;
using UnityEngine;
using ZenKit;
using Material = UnityEngine.Material;

namespace GVR.Creator.Meshes
{
    public class WorldTextureArrayCreator : AbstractMeshCreator
    {
        public async Task BuildWorldTextureArray()
        {
            await TextureCache.BuildTextureArrays(TextureCache.TextureTypes.World);
            AssignTextureArrays();
            TextureCache.RemoveCachedTextureArrayData(TextureCache.TextureTypes.World);
        }

        private void AssignTextureArrays()
        {
            foreach (var rendererData in TextureCache.WorldMeshRenderersForTextureArray)
            {
                PrepareMeshRenderer(rendererData.Renderer, rendererData.SubmeshData);
            }
        }

        private void PrepareMeshRenderer(Renderer rend, WorldData.SubMeshData subMesh)
        {
            UnityEngine.Texture texture = TextureCache.TextureArrays[TextureCache.TextureTypes.World][subMesh.TextureArrayType];
            Material material;
            if (subMesh.Material.Group == MaterialGroup.Water)
            {
                material = GetWaterMaterial();
            }
            else
            {
                material = GetDefaultMaterial(subMesh.TextureArrayType == TextureCache.TextureArrayTypes.Transparent, true);
            }
            material.mainTexture = texture;
            rend.material = material;
        }
    }
}
