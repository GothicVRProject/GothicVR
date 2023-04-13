using PxCs;
using System;
using System.Collections.Generic;
using UnityEngine;
using UZVR.Phoenix.Bridge;
using UZVR.Phoenix.World;
using UZVR.Util;
using static UZVR.Phoenix.World.BWorld;

namespace UZVR.WorldCreator
{
    public class MeshCreator: SingletonBehaviour<MeshCreator>
    {
        // Cache helped speed up loading of G1 world textures from 870ms to 230 (~75% speedup)
        private Dictionary<string, Texture2D> cachedTextures = new();

        public void Create(GameObject root, BWorld world)
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

        private void PrepareMeshRenderer(MeshRenderer meshRenderer, BSubMesh subMesh)
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
                var pxTexture = PxTexture.GetTextureFromVdf(PhoenixBridge.VdfsPtr, bMaterial.texture);

                // No texture found
                if (pxTexture == null)
                {
                    Debug.LogWarning($"Texture {bMaterial.texture} couldn't be found.");
                    return;
                }

                var format = pxTexture.GetUnityTextureFormat();
                if (format == 0)
                {
                    Debug.LogWarning("Format is not supported or not yet tested to work with Unity:" +
                        Enum.GetName(typeof(PxTexture.Format), format)
                    );
                    return;
                }

                var texture = new Texture2D((int)pxTexture.width, (int)pxTexture.height, format, false);

                texture.name = bMaterial.texture;
                texture.LoadRawTextureData(pxTexture.mipmaps[0].mipmap);

                texture.Apply();

                cachedTextures.Add(bMaterial.texture, texture);
                cachedTexture = texture;
            }

            material.mainTexture = cachedTexture;
        }

        private void PrepareMeshFilter(MeshFilter meshFilter, BSubMesh subMesh)
        {
            var mesh = new Mesh();
            meshFilter.mesh = mesh;

            mesh.SetVertices(subMesh.vertices);
            mesh.SetTriangles(subMesh.triangles, 0);
            mesh.SetUVs(0, subMesh.uvs);
        }
    }
}
