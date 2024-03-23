using System;
using System.Threading.Tasks;
using GVR.Caches;
using GVR.Globals;
using GVR.World;
using UnityEngine;
using UnityEngine.Rendering;
using ZenKit;
using Material = UnityEngine.Material;

namespace GVR.Creator.Meshes.V2.Builder.Textures
{
    /// <summary>
    /// Create texture array for world meshes. Basically no MeshBuilder,
    /// but we inherit the abstract builder to leverage some methods.
    /// </summary>
    public class WorldTextureArrayBuilder : AbstractMeshBuilder
    {
        public override GameObject Build()
        {
            throw new NotImplementedException("Use BuildAsync instead.");
        }

        public async Task BuildAsync()
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
                material = GetDefaultMaterial(subMesh.TextureArrayType == TextureCache.TextureArrayTypes.Transparent);
            }
            material.mainTexture = texture;
            rend.material = material;
        }

        protected override Material GetDefaultMaterial(bool isAlphaTest)
        {
            var shader = isAlphaTest ? Constants.ShaderLitAlphaToCoverage : Constants.ShaderWorldLit;
            var material = new Material(shader);

            if (isAlphaTest)
            {
                // Manually correct the render queue for alpha test, as Unity doesn't want to do it from the shader's render queue tag.
                material.renderQueue = (int)RenderQueue.AlphaTest;
            }

            return material;
        }
    }
}
