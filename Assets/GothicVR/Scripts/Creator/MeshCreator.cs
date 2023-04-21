using PxCs;
using System.Collections.Generic;
using UnityEngine;
using GVR.Phoenix.Data;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Util;

namespace GVR.Creator
{
    public class MeshCreator: SingletonBehaviour<MeshCreator>
    {
        // Cache helped speed up loading of G1 world textures from 870ms to 230 (~75% speedup)
        private Dictionary<string, Texture2D> cachedTextures = new();

        public void Create(GameObject root, WorldData world)
        {
            var meshObj = new GameObject("Mesh");
            meshObj.transform.parent = root.transform;

            foreach (var subMesh in world.subMeshes.Values)
            {
                var subMeshObj = new GameObject(string.Format("submesh-{0}", subMesh.material.name));
                var meshFilter = subMeshObj.AddComponent<MeshFilter>();
                var meshRenderer = subMeshObj.AddComponent<MeshRenderer>();
                var meshCollider = subMeshObj.AddComponent<MeshCollider>();

                PrepareMeshRenderer(meshRenderer, subMesh);
                PrepareMeshFilter(meshFilter, subMesh);
                meshCollider.sharedMesh = meshFilter.mesh;

                subMeshObj.transform.parent = meshObj.transform;
            }

            // Currently we don't need to store cachedTextures once the world is loaded.
            cachedTextures.Clear();
        }

        private void PrepareMeshRenderer(MeshRenderer meshRenderer, WorldData.SubMeshData subMesh)
        {
            var standardShader = Shader.Find("Standard");
            var material = new Material(standardShader);
            var bMaterial = subMesh.material;

            meshRenderer.material = material;

            // No texture to add.
            if (bMaterial.texture == "")
                return;

            // Load texture for the first time.
            if (!cachedTextures.TryGetValue(bMaterial.texture, out Texture2D cachedTexture))
            {
                // FIXME - There might be more textures to load compressed. Please check for sake of performance!
                var pxTexture = PxTexture.GetTextureFromVdf(
                    PhoenixBridge.VdfsPtr,
                    bMaterial.texture,
                    PxTexture.Format.tex_dxt1, PxTexture.Format.tex_dxt5);

                // No texture found
                if (pxTexture == null)
                {
                    Debug.LogWarning($"Texture {bMaterial.texture} couldn't be found.");
                    return;
                }

                var format = pxTexture.format.AsUnityTextureFormat();
                if (format == 0)
                {
                    Debug.LogWarning($"Format >{pxTexture.format}< is not supported or not yet tested to work with Unity:");
                    return;
                }

                var texture = new Texture2D((int)pxTexture.width, (int)pxTexture.height, format, (int)pxTexture.mipmapCount, false);

                texture.name = bMaterial.texture;

                for (var i = 0u; i < pxTexture.mipmapCount; i++)
                {
                    texture.SetPixelData(pxTexture.mipmaps[i].mipmap, (int)i);
                }

                texture.Apply();

                cachedTextures.Add(bMaterial.texture, texture);
                cachedTexture = texture;
            }

            material.mainTexture = cachedTexture;
        }

        private void PrepareMeshFilter(MeshFilter meshFilter, WorldData.SubMeshData subMesh)
        {
            var mesh = new Mesh();
            meshFilter.mesh = mesh;

            mesh.SetVertices(subMesh.vertices);
            mesh.SetTriangles(subMesh.triangles, 0);
            mesh.SetUVs(0, subMesh.uvs);
        }
    }
}
