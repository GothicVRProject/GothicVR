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

            // Load texture for the first time.
            if (!cachedTextures.TryGetValue(bMaterial.textureName, out Texture2D cachedTexture))
            {
                var bTexture = TextureBridge.LoadTexture(PhoenixBridge.VdfsPtr, bMaterial.textureName);

                if (bTexture == null)
                    return;

                var texture = new Texture2D((int)bTexture.width, (int)bTexture.height, bTexture.GetUnityTextureFormat(), false);

                texture.name = bMaterial.textureName;
                texture.LoadRawTextureData(bTexture.data.ToArray());

                texture.Apply();

                cachedTextures.Add(bMaterial.textureName, texture);
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
