using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GVR.Caches;
using GVR.Globals;
using UnityEngine;
using UnityEngine.Rendering;
using ZenKit;
using Material = UnityEngine.Material;
using Texture = UnityEngine.Texture;
using TextureFormat = UnityEngine.TextureFormat;

namespace GVR.Creator.Meshes.V2.Builder.Textures
{
    /// <summary>
    /// Create texture array for vob meshes. Basically no MeshBuilder,
    /// but we inherit the abstract builder to leverage some methods.
    /// </summary>
    public class VobTextureArrayBuilder : AbstractMeshBuilder
    {
        public override GameObject Build()
        {
            throw new NotImplementedException("Use BuildAsync instead.");
        }

        public async Task BuildAsync()
        {
            await TextureCache.BuildTextureArrays(TextureCache.TextureTypes.Vob);
            AssignTextureArrays();
            TextureCache.RemoveCachedTextureArrayData(TextureCache.TextureTypes.Vob);
        }

        private void AssignTextureArrays()
        {
            foreach (var mesh in TextureCache.VobMeshRenderersForTextureArray)
            {
                PrepareMeshRenderer(mesh.Renderer, mesh.Data.Mrm, mesh.Data.TextureArrayTypes);
            }
        }

        private void PrepareMeshRenderer(Renderer rend, IMultiResolutionMesh mrmData, List<TextureCache.TextureArrayTypes> textureArrayTypes)
        {
            if (null == mrmData)
            {
                Debug.LogError("No mesh data could be added to renderer: " + rend.transform.parent.name);
                return;
            }

            if (rend is MeshRenderer && !rend.GetComponent<MeshFilter>().sharedMesh)
            {
                Debug.LogError($"Null mesh on {rend.gameObject.name}");
                return;
            }

            List<Material> finalMaterials = new List<Material>(mrmData.SubMeshes.Count);
            int submeshCount = rend is MeshRenderer ? rend.GetComponent<MeshFilter>().sharedMesh.subMeshCount : mrmData.SubMeshCount;

            for (int i = 0; i < submeshCount; i++)
            {
                Texture texture = TextureCache.TextureArrays[TextureCache.TextureTypes.Vob][textureArrayTypes[i]];
                Material material = GetDefaultMaterial(texture && ((Texture2DArray)texture).format == TextureFormat.RGBA32);

                material.mainTexture = texture;
                rend.material = material;
                finalMaterials.Add(material);
            }

            rend.SetMaterials(finalMaterials);
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
