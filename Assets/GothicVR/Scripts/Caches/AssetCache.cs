using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Animation;
using PxCs.Data.Font;
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
        private Dictionary<string, PxAnimationData> animCache = new();
        private Dictionary<string, PxModelHierarchyData> mdhCache = new();
        private Dictionary<string, PxModelData> mdlCache = new();
        private Dictionary<string, PxModelMeshData> mdmCache = new();
        private Dictionary<string, PxMultiResolutionMeshData> mrmCache = new();
        private Dictionary<string, PxMorphMeshData> mmbCache = new();

        private Dictionary<string, PxVmItemData> itemDataCache = new();
        private Dictionary<string, PxVmMusicData> musicDataCache = new();
        private Dictionary<string, PxVmSfxData> sfxDataCache = new();
        private Dictionary<string, PxSoundData<float>> soundCache = new();
        private Dictionary<string, PxFontData> fontCache = new();

        private readonly string[] misplacedMdmArmors =
        {
            "Hum_GrdS_Armor",
            "Hum_GrdM_Armor",
            "Hum_GrdL_Armor",
            "Hum_NovM_Armor",
            "Hum_TplL_Armor",
            "Hum_Body_Cooksmith",
            "Hum_VlkL_Armor",
            "Hum_VlkM_Armor",
            "Hum_KdfS_Armor"
        };

        public Texture2D TryGetTexture(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (textureCache.TryGetValue(preparedKey, out var data))
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

            Texture2D texture = null;
            if (pxTexture.format == PxTexture.Format.tex_B8G8R8A8)
            {
                // Let Unity generate mips for textures with alpha, as the game doesn't provide them.
                texture = new Texture2D((int)pxTexture.width, (int)pxTexture.height, format, true);
                texture.SetPixelData(pxTexture.mipmaps[0].mipmap, 0);
                texture.Apply(true, true);
            }
            else
            {
                // Use Gothic's mips for opaque textures. We could also let Unity generate them here, though.
                texture = new Texture2D((int)pxTexture.width, (int)pxTexture.height, format, (int)pxTexture.mipmapCount, false);
                for (int i = 0; i < pxTexture.mipmapCount; i++)
                {
                    texture.SetPixelData(pxTexture.mipmaps[i].mipmap, i);
                }
                texture.Apply(false, true);
            }

            texture.filterMode = FilterMode.Trilinear;
            texture.name = key;
            textureCache[preparedKey] = texture;
            return texture;
        }

        public PxModelScriptData TryGetMds(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdsCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxModelScript.GetModelScriptFromVfs(GameData.I.VfsPtr, $"{preparedKey}.mds");
            mdsCache[preparedKey] = newData;

            return newData;
        }

        public PxAnimationData TryGetAnimation(string mdsKey, string animKey)
        {
            var preparedMdsKey = GetPreparedKey(mdsKey);
            var preparedAnimKey = GetPreparedKey(animKey);
            var preparedKey = preparedMdsKey + "-" + preparedAnimKey;
            if (animCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxAnimation.LoadFromVfs(GameData.I.VfsPtr, $"{preparedKey}.man");
            animCache[preparedKey] = newData;

            return newData;
        }

        public PxModelHierarchyData TryGetMdh(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdhCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxModelHierarchy.LoadFromVfs(GameData.I.VfsPtr, $"{preparedKey}.mdh");
            mdhCache[preparedKey] = newData;

            return newData;
        }

        public PxModelData TryGetMdl(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdlCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxModel.LoadModelFromVfs(GameData.I.VfsPtr, $"{preparedKey}.mdl");
            mdlCache[preparedKey] = newData;

            return newData;
        }

        public PxModelMeshData TryGetMdm(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdmCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxModelMesh.LoadModelMeshFromVfs(GameData.I.VfsPtr, $"{preparedKey}.mdm");
            mdmCache[preparedKey] = newData;

            FixArmorTriangles(preparedKey, newData);

            return newData;
        }

        /// <summary>
        /// Some armor mdm's have wrong triangles. This function corrects them hard coded until we find a proper solution.
        /// </summary>
        private void FixArmorTriangles(string key, PxModelMeshData mdm)
        {
            if (!misplacedMdmArmors.Contains(key, StringComparer.OrdinalIgnoreCase))
                return;

            foreach (var mesh in mdm.meshes!)
            {
                for (var i = 0; i < mesh.mesh!.positions!.Length; i++)
                {
                    var curPos = mesh.mesh.positions[i];
                    mesh.mesh.positions[i] = new(curPos.X + 0.5f, curPos.Y - 0.5f, curPos.Z + 13f);
                }
            }
        }

        public PxMultiResolutionMeshData TryGetMrm(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mrmCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxMultiResolutionMesh.GetMRMFromVfs(GameData.I.VfsPtr, $"{preparedKey}.mrm");
            mrmCache[preparedKey] = newData;

            return newData;
        }

        public PxMorphMeshData TryGetMmb(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mmbCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxMorphMesh.LoadMorphMeshFromVfs(GameData.I.VfsPtr, $"{preparedKey}.mmb");
            mmbCache[preparedKey] = newData;

            return newData;
        }

        public PxVmMusicData TryGetMusic(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (musicDataCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxVm.InitializeMusic(GameData.I.VmMusicPtr, preparedKey);
            musicDataCache[preparedKey] = newData;

            return newData;
        }

        /// <summary>
        /// Hint: Instances only need to be initialized once on phoenix.
        /// There are two ways of getting Item data. Via INSTANCE name or symbolIndex inside VM.
        /// </summary>
        public PxVmItemData TryGetItemData(uint instanceId)
        {
            var symbol = PxDaedalusScript.GetSymbol(GameData.I.VmGothicPtr, instanceId);

            if (symbol == null)
                return null;

            return TryGetItemData(symbol.name);
        }

        /// <summary>
        /// Hint: Instances only need to be initialized once on phoenix.
        /// There are two ways of getting Item data. Via INSTANCE name or symbolIndex inside VM.
        /// </summary>
        public PxVmItemData TryGetItemData(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (itemDataCache.TryGetValue(preparedKey, out var data))
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
            if (sfxDataCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxVm.InitializeSfx(GameData.I.VmSfxPtr, preparedKey);
            sfxDataCache[preparedKey] = newData;

            return newData;
        }

        public PxSoundData<float> TryGetSound(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (soundCache.TryGetValue(preparedKey, out var data))
                return data;

            var wavFile = PxSound.GetSoundArrayFromVfs<float>(GameData.I.VfsPtr, $"{preparedKey}.wav");
            soundCache[preparedKey] = wavFile;

            return wavFile;
        }

        public PxFontData TryGetFont(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (fontCache.TryGetValue(preparedKey, out var data))
                return data;

            var fontData = PxFont.LoadFont(GameData.I.VfsPtr, $"{preparedKey}.fnt");
            fontCache[preparedKey] = fontData;

            return fontData;
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
