using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Mesh;
using PxCs.Data.Model;
using PxCs.Data.Sound;
using PxCs.Data.Vm;
using PxCs.Interface;
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
        private Dictionary<string, PxMorphMeshData> mmbCache = new();

        private Dictionary<string, PxVmItemData> itemDataCache = new();
        private Dictionary<string, PxVmSfxData> sfxDataCache = new();

        private Dictionary<string, PxSoundData<float>> soundCache = new();


        public Texture2D TryGetTexture(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (textureCache.TryGetValue(preparedKey, out Texture2D data))
                return data;


            // FIXME - There might be more textures to load compressed. Please check for sake of performance!
            var pxTexture = PxTexture.GetTextureFromVfs(
                GameData.I.VfsPtr,
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

            var newData = PxModelScript.GetModelScriptFromVfs(GameData.I.VfsPtr, $"{preparedKey}.mds");
            mdsCache[preparedKey] = newData;

            return newData;
        }

        public PxModelHierarchyData TryGetMdh(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdhCache.TryGetValue(preparedKey, out PxModelHierarchyData data))
                return data;

            var newData = PxModelHierarchy.LoadFromVfs(GameData.I.VfsPtr, $"{preparedKey}.mdh");
            mdhCache[preparedKey] = newData;

            return newData;
        }

        public PxModelData TryGetMdl(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdlCache.TryGetValue(preparedKey, out PxModelData data))
                return data;

            var newData = PxModel.LoadModelFromVfs(GameData.I.VfsPtr, $"{preparedKey}.mdl");
            mdlCache[preparedKey] = newData;

            return newData;
        }

        public PxModelMeshData TryGetMdm(string key, params string[] attachmentKeys)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdmCache.TryGetValue(preparedKey, out PxModelMeshData data))
                return data;

            var newData = PxModelMesh.LoadModelMeshFromVfs(GameData.I.VfsPtr, $"{preparedKey}.mdm", attachmentKeys);
            mdmCache[preparedKey] = newData;

            return newData;
        }

        public PxMultiResolutionMeshData TryGetMrm(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mrmCache.TryGetValue(preparedKey, out PxMultiResolutionMeshData data))
                return data;

            var newData = PxMultiResolutionMesh.GetMRMFromVfs(GameData.I.VfsPtr, $"{preparedKey}.mrm");
            mrmCache[preparedKey] = newData;

            return newData;
        }

        public PxMorphMeshData TryGetMmb(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mmbCache.TryGetValue(preparedKey, out PxMorphMeshData data))
                return data;

            var newData = PxMorphMesh.LoadMorphMeshFromVfs(GameData.I.VfsPtr, $"{preparedKey}.mmb");
            mmbCache[preparedKey] = newData;

            return newData;
        }

        /// <summary>
        /// Hint: Instances only need to be initialized once on phoenix and don't need to be deleted during runtime.
        /// </summary>
        public PxVmItemData TryGetItemData(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (itemDataCache.TryGetValue(preparedKey, out PxVmItemData data))
                return data;

            var newData = PxVm.InitializeItem(GameData.I.VmGothicPtr, preparedKey);
            itemDataCache[preparedKey] = newData;

            return newData;
        }

        /// <summary>
        /// Hint: Instances only need to be initialized once on phoenix and don't need to be deleted during runtime.
        /// </summary>
        public PxVmSfxData TryGetSfxData(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (sfxDataCache.TryGetValue(preparedKey, out PxVmSfxData data))
                return data;

            var newData = PxVm.InitializeSfx(GameData.I.VmSfxPtr, preparedKey);
            sfxDataCache[preparedKey] = newData;

            return newData;
        }

        public PxSoundData<float> TryGetSound(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (soundCache.TryGetValue(preparedKey, out PxSoundData<float> data))
                return data;

            var wavFile = PxSound.GetSoundArrayFromVfs<float>(GameData.I.VfsPtr, $"{preparedKey}.wav");
            soundCache[preparedKey] = wavFile;

            return wavFile;
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
