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
            var pxTexture = PxTexture.GetTextureFromVdf(
                GameData.I.VdfsPtr,
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

        public async Task<Texture2D> TryGetTextureAsync(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (textureCache.TryGetValue(preparedKey, out Texture2D data))
                return data;

            var pxTexture = await Task.Run(() => PxTexture.GetTextureFromVdf(
                GameData.I.VdfsPtr,
                key,
                PxTexture.Format.tex_dxt1, PxTexture.Format.tex_dxt5
            ));

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

            var newData = PxModelScript.GetModelScriptFromVdf(GameData.I.VdfsPtr, $"{preparedKey}.mds");
            mdsCache[preparedKey] = newData;

            return newData;
        }
        public async Task<PxModelScriptData> TryGetMdsAsync(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdsCache.TryGetValue(preparedKey, out PxModelScriptData data))
                return data;

            var newData = await Task.Run(() => PxModelScript.GetModelScriptFromVdf(GameData.I.VdfsPtr, $"{GetPreparedKey(key)}.mds"));
            mdsCache[preparedKey] = newData;

            return newData;
        }

        public PxModelHierarchyData TryGetMdh(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdhCache.TryGetValue(preparedKey, out PxModelHierarchyData data))
                return data;

            var newData = PxModelHierarchy.LoadFromVdf(GameData.I.VdfsPtr, $"{preparedKey}.mdh");
            mdhCache[preparedKey] = newData;

            return newData;
        }

        public async Task<PxModelHierarchyData> TryGetMdhAsync(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdhCache.TryGetValue(preparedKey, out PxModelHierarchyData data))
                return data;

            var newData = await Task.Run(() => PxModelHierarchy.LoadFromVdf(GameData.I.VdfsPtr, $"{GetPreparedKey(key)}.mdh"));
            mdhCache[preparedKey] = newData;

            return newData;
        }

        public PxModelData TryGetMdl(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdlCache.TryGetValue(preparedKey, out PxModelData data))
                return data;

            var newData = PxModel.LoadModelFromVdf(GameData.I.VdfsPtr, $"{preparedKey}.mdl");
            mdlCache[preparedKey] = newData;

            return newData;
        }

        public async Task<PxModelData> TryGetMdlAsync(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdlCache.TryGetValue(preparedKey, out PxModelData data))
                return data;

            var newData = await Task.Run(() => PxModel.LoadModelFromVdf(GameData.I.VdfsPtr, $"{GetPreparedKey(key)}.mdl"));
            mdlCache[preparedKey] = newData;

            return newData;
        }

        public PxModelMeshData TryGetMdm(string key, params string[] attachmentKeys)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdmCache.TryGetValue(preparedKey, out PxModelMeshData data))
                return data;

            var newData = PxModelMesh.LoadModelMeshFromVdf(GameData.I.VdfsPtr, $"{preparedKey}.mdm", attachmentKeys);
            mdmCache[preparedKey] = newData;

            return newData;
        }

        public async Task<PxModelMeshData> TryGetMdmAsync(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdmCache.TryGetValue(preparedKey, out PxModelMeshData data))
                return data;

            var newData = await Task.Run(() => PxModelMesh.LoadModelMeshFromVdf(GameData.I.VdfsPtr, $"{GetPreparedKey(key)}.mdm"));
            mdmCache[preparedKey] = newData;

            return newData;
        }

        public PxMultiResolutionMeshData TryGetMrm(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mrmCache.TryGetValue(preparedKey, out PxMultiResolutionMeshData data))
                return data;

            var newData = PxMultiResolutionMesh.GetMRMFromVdf(GameData.I.VdfsPtr, $"{preparedKey}.mrm");
            mrmCache[preparedKey] = newData;

            return newData;
        }

        public async Task<PxMultiResolutionMeshData> TryGetMrmAsync(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mrmCache.TryGetValue(preparedKey, out PxMultiResolutionMeshData data))
                return data;

            var newData = await Task.Run(() => PxMultiResolutionMesh.GetMRMFromVdf(GameData.I.VdfsPtr, $"{GetPreparedKey(key)}.mrm"));
            mrmCache[preparedKey] = newData;

            return newData;
        }

        public PxMorphMeshData TryGetMmb(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mmbCache.TryGetValue(preparedKey, out PxMorphMeshData data))
                return data;

            var newData = PxMorphMesh.LoadMorphMeshFromVdf(GameData.I.VdfsPtr, $"{preparedKey}.mmb");
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
        public async Task<PxVmItemData> TryGetItemDataAsync(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (itemDataCache.TryGetValue(preparedKey, out PxVmItemData data))
                return data;

            var newData = await Task.Run(() => PxVm.InitializeItem(GameData.I.VmGothicPtr, preparedKey));
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

        public async Task<PxVmSfxData> TryGetSfxDataAsync(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (sfxDataCache.TryGetValue(preparedKey, out PxVmSfxData data))
                return data;

            var newData = await Task.Run(() => PxVm.InitializeSfx(GameData.I.VdfsPtr, $"{GetPreparedKey(key)}.sfx"));
            sfxDataCache[preparedKey] = newData;

            return newData;
        }

        public PxSoundData<float> TryGetSound(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (soundCache.TryGetValue(preparedKey, out PxSoundData<float> data))
                return data;

            var wavFile = PxSound.GetSoundArrayFromVDF<float>(GameData.I.VdfsPtr, $"{preparedKey}.wav");
            soundCache[preparedKey] = wavFile;

            return wavFile;
        }

        public async Task<PxSoundData<float>> TryGetSoundAsync(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (soundCache.TryGetValue(preparedKey, out PxSoundData<float> data))
                return data;

            var newData = await Task.Run(() => PxSound.GetSoundArrayFromVDF<float>(GameData.I.VdfsPtr, $"{GetPreparedKey(key)}.sound"));
            soundCache[preparedKey] = newData;

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
