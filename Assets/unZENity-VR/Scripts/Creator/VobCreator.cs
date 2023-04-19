using PxCs;
using PxCs.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UZVR.Demo;
using UZVR.Phoenix.Data;
using UZVR.Phoenix.Interface;
using UZVR.Phoenix.Util;
using UZVR.Util;

namespace UZVR.Creator
{
    public class VobCreator: SingletonBehaviour<VobCreator>
    {
        // Needs to be changed to the real value. (But for now: Without it potions are big as houses. :-) )
        private const int DEBUG_ITEM_SCALE = 45;


        // Cache helped speed up loading of G1 world textures from 870ms to 230 (~75% speedup)
        private Dictionary<string, Texture2D> cachedTextures = new();


        public void Create(GameObject root, WorldData world)
        {
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().CreateVobs)
                return;

            var itemVobs = GetFlattenedVobsByType(world.vobs, PxWorld.PxVobType.PxVob_oCItem);
            var vobRootObj = new GameObject("Vobs");
            vobRootObj.transform.parent = root.transform;

            foreach (var vob in itemVobs)
            {
                // FIXME: Add caching of MRM as object will be created multiple times inside a scene.
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
                    meshCollider.sharedMesh = meshFilter.mesh;
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Debug.LogError(e.Message);
                    Destroy(meshObj);
                    continue;
                }

                meshObj.transform.position = vob.position.ToUnityVector();
                meshObj.transform.localScale /= DEBUG_ITEM_SCALE;
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

                var texture = new Texture2D((int)pxTexture.width, (int)pxTexture.height, format, (int)pxTexture.mipmapCount, false);

                texture.name = materialData.texture;

                for (var i = 0u; i < pxTexture.mipmapCount; i++)
                {
                    texture.SetPixelData(pxTexture.mipmaps[i].mipmap, (int)i);
                }

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

            var triangles = mrmData.subMeshes.First().triangles;
            var wedges = mrmData.subMeshes.First().wedges;


            // We need to flip [a,b,c] => [c,b,a]. Otherwise mesh is drawn wrong side.
            var preparedVertices = mrmData.positions.ToUnityArray();
            var preparedTriangles = triangles.SelectMany(i => new int[]{wedges[i.c].index, wedges[i.b].index, wedges[i.a].index}).ToArray();
            var preparedUVs = triangles.SelectMany(i => new Vector2[] { wedges[i.c].texture.ToUnityVector(), wedges[i.b].texture.ToUnityVector(), wedges[i.a].texture.ToUnityVector() }).ToArray();
            
            mesh.SetVertices(preparedVertices);
            mesh.SetTriangles(preparedTriangles, 0);
            // FIXME - We need to check how uv==vertices count was set within world meshes (I think we needed to create a new vertex for every triangle. No reuse!).
            mesh.SetUVs(0, preparedUVs);
        }
    }
}
