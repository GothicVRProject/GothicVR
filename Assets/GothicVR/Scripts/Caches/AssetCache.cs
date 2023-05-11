using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Mesh;
using PxCs.Data.Model;
using PxCs.Interface;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GVR.Caches
{
    public class AssetCache : SingletonBehaviour<AssetCache>
    {
        private Dictionary<string, Texture2D> textureCache = new();

        private Dictionary<string, PxModelScriptData> mdsCache = new();
        private Dictionary<string, PxModelHierarchyData> mdhCache = new();
        private Dictionary<string, PxModelData> mdlCache = new();
        private Dictionary<string, PxModelMeshData> mdmCache = new();
        private Dictionary<string, PxMultiResolutionMeshData> mrmCache = new();


        public Texture2D TryGetTexture(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (textureCache.TryGetValue(preparedKey, out Texture2D data))
                return data;


            // FIXME - There might be more textures to load compressed. Please check for sake of performance!
            var pxTexture = PxTexture.GetTextureFromVdf(
                PhoenixBridge.VdfsPtr,
                key,
                PxTexture.Format.tex_dxt1, PxTexture.Format.tex_dxt5
            );

            // No texture found
            if (pxTexture == null)
            {
                Debug.LogWarning($"Texture {key} couldn't be found.");
                return null;
            }

            var format = pxTexture.format.AsUnityTextureFormat();
            var texture = new Texture2D((int)pxTexture.width, (int)pxTexture.height, format, (int)pxTexture.mipmapCount, false);
            texture.name = key;

            for (var i = 0u; i < pxTexture.mipmapCount; i++)
                texture.SetPixelData(pxTexture.mipmaps[i].mipmap, (int)i);

            texture.Apply();

            textureCache[preparedKey] = texture;
            return texture;
        }

        public PxModelScriptData TryGetMds(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdsCache.TryGetValue(preparedKey, out PxModelScriptData data))
                return data;

            var newData = PxModelScript.GetModelScriptFromVdf(PhoenixBridge.VdfsPtr, $"{GetPreparedKey(key)}.mds");
            mdsCache[preparedKey] = newData;

            return newData;
        }

        public PxModelHierarchyData TryGetMdh(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdhCache.TryGetValue(preparedKey, out PxModelHierarchyData data))
                return data;

            var newData = PxModelHierarchy.LoadFromVdf(PhoenixBridge.VdfsPtr, $"{GetPreparedKey(key)}.mdh");
            mdhCache[preparedKey] = newData;

            return newData;
        }

        public PxModelData TryGetMdl(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdlCache.TryGetValue(preparedKey, out PxModelData data))
                return data;

            var newData = PxModel.LoadModelFromVdf(PhoenixBridge.VdfsPtr, $"{GetPreparedKey(key)}.mdl");
            mdlCache[preparedKey] = newData;

            return newData;
        }

        public PxModelMeshData TryGetMdm(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdmCache.TryGetValue(preparedKey, out PxModelMeshData data))
                return data;

            var newData = PxModelMesh.LoadModelMeshFromVdf(PhoenixBridge.VdfsPtr, $"{GetPreparedKey(key)}.mdm");
            mdmCache[preparedKey] = newData;

            return newData;
        }

        public PxMultiResolutionMeshData TryGetMrm(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mrmCache.TryGetValue(preparedKey, out PxMultiResolutionMeshData data))
                return data;

            var newData = PxMultiResolutionMesh.GetMRMFromVdf(PhoenixBridge.VdfsPtr, $"{GetPreparedKey(key)}.mrm");
            mrmCache[preparedKey] = newData;

            return newData;
        }



        private string GetPreparedKey(string key)
        {
            var lowerKey = key.ToLower();
            var extension = Path.GetExtension(lowerKey);

            if (extension == string.Empty)
                return lowerKey;
            else
                return lowerKey.Replace(extension, "");
        }
    }
}
