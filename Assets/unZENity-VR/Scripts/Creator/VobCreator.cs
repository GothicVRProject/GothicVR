using PxCs;
using PxCs.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UZVR.Phoenix.Data;
using UZVR.Phoenix.Interface;
using UZVR.Phoenix.Util;
using UZVR.Util;

namespace UZVR.Creator
{
    public class VobCreator: SingletonBehaviour<VobCreator>
    {
        // Cache helped speed up loading of G1 world textures from 870ms to 230 (~75% speedup)
        private Dictionary<string, Texture2D> cachedTextures = new();


        public void Create(GameObject root, WorldData world)
        {
            var itemVobs = GetFlattenedVobsByType(world.vobs, PxWorld.PxVobType.PxVob_oCItem);
            var vobRootObj = new GameObject("Vobs");
            vobRootObj.transform.parent = root.transform;

            foreach (var vob in itemVobs)
            {
                // FIXME: Add caching of MRM as object will be most likely be created multiple times inside scene.
                var mrm = PxMRM.GetMRMFromVdf(PhoenixBridge.VdfsPtr, $"{vob.vobName}.MRM");

                if (mrm == null)
                {
                    Debug.LogError($"MultiResolutionModel (MRM) >{vob.vobName}.MRM< not found.");
                    continue;
                }

                var meshObj = new GameObject(string.Format("vob-{0}", vob.vobName));
                var meshFilter = meshObj.AddComponent<MeshFilter>();
                var meshRenderer = meshObj.AddComponent<MeshRenderer>();
                var meshCollider = meshObj.AddComponent<MeshCollider>();

                try
                {
                    PrepareMeshRenderer(meshRenderer, mrm);
                    PrepareMeshFilter(meshFilter, mrm);
                    //                meshCollider.sharedMesh = meshFilter.mesh;
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Debug.LogError(e.Message);
                    Destroy(meshObj);
                    continue;
                }

                meshObj.transform.parent = vobRootObj.transform;
            }

            // Currently we don't need to store cachedTextures once the world is loaded.
            cachedTextures.Clear();
        }

        /// <summary>
        /// Convenient method to return specific vob elements in recursive list of PxVobData.childVobs...
        /// </summary>
        /// <param name="vobsToFilter"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private List<PxVobData> GetFlattenedVobsByType(PxVobData[] vobsToFilter, PxWorld.PxVobType type)
        {
            var returnVobs = new List<PxVobData>();
            for (var i = 0; i < vobsToFilter.Length; i++)
            {
                var curVob = vobsToFilter[i];
                if (curVob.type == type)
                    returnVobs.Add(curVob);

                returnVobs.AddRange(GetFlattenedVobsByType(curVob.childVobs, type));
            }

            return returnVobs;
        }

        private void PrepareMeshRenderer(MeshRenderer meshRenderer, PxMRMData mrmData)
        {
            if (mrmData.materials.Length != 1)
                throw new ArgumentOutOfRangeException("Currently it's only supported to have exact 1 material for VobMRMs.");

            var standardShader = Shader.Find("Standard");
            var material = new Material(standardShader);
            var materialData = mrmData.materials.First();

            meshRenderer.material = material;

            // No texture to add.
            if (materialData.texture == "")
                return;

            // Load texture for the first time.
            if (!cachedTextures.TryGetValue(materialData.texture, out Texture2D cachedTexture))
            {
                // FIXME - There might be more textures to load compressed. Please check for sake of performance!
                var pxTexture = PxTexture.GetTextureFromVdf(
                    PhoenixBridge.VdfsPtr,
                    materialData.texture,
                    PxTexture.Format.tex_dxt1, PxTexture.Format.tex_dxt5);

                // No texture found
                if (pxTexture == null)
                {
                    Debug.LogWarning($"Texture {materialData.texture} couldn't be found.");
                    return;
                }

                var format = pxTexture.format.AsUnityTextureFormat();
                if (format == 0)
                {
                    Debug.LogWarning($"Format >{pxTexture.format}< is not supported or not yet tested to work with Unity:");
                    return;
                }

                var texture = new Texture2D((int)pxTexture.width, (int)pxTexture.height, format, false);

                texture.name = materialData.texture;
                texture.LoadRawTextureData(pxTexture.mipmaps[0].mipmap);

                texture.Apply();

                cachedTextures.Add(materialData.texture, texture);
                cachedTexture = texture;
            }

            material.mainTexture = cachedTexture;
        }

        private void PrepareMeshFilter(MeshFilter meshFilter, PxMRMData mrmData)
        {
            var mesh = new Mesh();
            meshFilter.mesh = mesh;

            if (mrmData.subMeshes.Length != 1)
                throw new ArgumentOutOfRangeException("Currently it's only supported to have exact 1 subMesh for VobMRMs.");

            mesh.SetVertices(mrmData.positions.ToUnityArray());
            mesh.SetTriangles(mrmData.subMeshes.First().triangles.ToUnityTriangles(), 0);

//            mesh.SetTriangles(mrmData.triangles, 0);
//            mesh.SetTriangles(mrmData.triangles, 0);
//            mesh.SetUVs(0, mrmData.uvs);
        }
    }
}
