using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Mesh;
using PxCs.Data.Model;
using PxCs.Interface;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GVR.Creator
{
    public class AssetCache : SingletonBehaviour<AssetCache>
    {
        private Dictionary<string, Texture2D> textureCache = new();

        private Dictionary<string, PxModelScriptData> mdsCache = new();
        private Dictionary<string, PxModelHierarchyData> mdhCache = new();
        private Dictionary<string, PxModelData> mdlCache = new();
        private Dictionary<string, PxModelMeshData> mdmCache = new();
        private Dictionary<string, PxMultiResolutionMeshData> mrmCache = new();


        public bool TryGetTexture(string key, out Texture2D texture)
        {
            return textureCache.TryGetValue(GetPreparedKey(key), out texture);
        }

        public bool TryGetMds(string key, out PxModelScriptData mds)
        {
            return mdsCache.TryGetValue(GetPreparedKey(key), out mds);
        }

        public bool TryGetMdh(string key, out PxModelHierarchyData mdh)
        {
            return mdhCache.TryGetValue(GetPreparedKey(key), out mdh);
        }

        public bool TryGetMdl(string key, out PxModelData mdl)
        {
            return mdlCache.TryGetValue(GetPreparedKey(key), out mdl);
        }

        public bool TryGetMdm(string key, out PxModelMeshData mdm)
        {
            return mdmCache.TryGetValue(GetPreparedKey(key), out mdm);
        }

        public bool TryGetMrm(string key, out PxMultiResolutionMeshData mrm)
        {
            return mrmCache.TryGetValue(GetPreparedKey(key), out mrm);
        }




        public Texture2D TryAddTexture(string key)
        {
            if (TryGetTexture(key, out Texture2D data))
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
            if (format == 0)
            {
                Debug.LogWarning($"Format >{pxTexture.format}< is not supported or not yet tested to work with Unity:");
                return null;
            }

            var texture = new Texture2D((int)pxTexture.width, (int)pxTexture.height, format, (int)pxTexture.mipmapCount, false);
            texture.name = key;

            for (var i = 0u; i < pxTexture.mipmapCount; i++)
                texture.SetPixelData(pxTexture.mipmaps[i].mipmap, (int)i);

            texture.Apply();

            textureCache[GetPreparedKey(key)] = texture;
            return texture;
        }

        public PxModelScriptData TryAddMds(string key)
        {
            if (TryGetMds(key, out PxModelScriptData data))
                return data;

            var newData = PxModelScript.GetModelScriptFromVdf(PhoenixBridge.VdfsPtr, $"{GetPreparedKey(key)}.mds");
            mdsCache[GetPreparedKey(key)] = newData;

            return newData;
        }

        public PxModelHierarchyData TryAddMdh(string key)
        {
            if (TryGetMdh(key, out PxModelHierarchyData data))
                return data;

            var newData = PxModelHierarchy.LoadFromVdf(PhoenixBridge.VdfsPtr, $"{GetPreparedKey(key)}.mdh");
            mdhCache[GetPreparedKey(key)] = newData;

            return newData;
        }

        public PxModelData TryAddMdl(string key)
        {
            if (TryGetMdl(key, out PxModelData data))
                return data;

            var newData = PxModel.LoadModelFromVdf(PhoenixBridge.VdfsPtr, $"{GetPreparedKey(key)}.mdl");
            mdlCache[GetPreparedKey(key)] = newData;

            return newData;
        }

        public PxModelMeshData TryAddMdm(string key)
        {
            if (TryGetMdm(key, out PxModelMeshData data))
                return data;

            var newData = PxModelMesh.LoadModelMeshFromVdf(PhoenixBridge.VdfsPtr, $"{GetPreparedKey(key)}.mdm");
            mdmCache[GetPreparedKey(key)] = newData;

            return newData;
        }

        public PxMultiResolutionMeshData TryAddMrm(string key)
        {
            if (TryGetMrm(key, out PxMultiResolutionMeshData data))
                return data;

            var newData = PxMultiResolutionMesh.GetMRMFromVdf(PhoenixBridge.VdfsPtr, $"{GetPreparedKey(key)}.mrm");
            mrmCache[GetPreparedKey(key)] = newData;

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
