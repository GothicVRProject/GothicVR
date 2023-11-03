using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GVR.Extensions;
using GVR.Phoenix.Interface;
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
    public static class AssetCache
    {
        private static Dictionary<string, Texture2D> textureCache = new();
        private static Dictionary<string, PxModelScriptData> mdsCache = new();
        private static Dictionary<string, PxAnimationData> animCache = new();
        private static Dictionary<string, PxModelHierarchyData> mdhCache = new();
        private static Dictionary<string, PxModelData> mdlCache = new();
        private static Dictionary<string, PxModelMeshData> mdmCache = new();
        private static Dictionary<string, PxMultiResolutionMeshData> mrmCache = new();
        private static Dictionary<string, PxMorphMeshData> mmbCache = new();
        private static Dictionary<string, PxVmItemData> itemDataCache = new();
        private static Dictionary<string, PxVmMusicData> musicDataCache = new();
        private static Dictionary<string, PxVmSfxData> sfxDataCache = new();
        private static Dictionary<string, PxSoundData<float>> soundCache = new();
        private static Dictionary<string, PxFontData> fontCache = new();

        private static readonly string[] misplacedMdmArmors =
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

        public static Texture2D TryGetTexture(string key)
        {
            string preparedKey = GetPreparedKey(key);
            if (textureCache.ContainsKey(preparedKey) && textureCache[preparedKey])
            {
                return textureCache[preparedKey];
            }

            // FIXME - There might be more textures to load compressed. Please check for sake of performance!
            var pxTexture = PxTexture.GetTextureFromVfs(
                GameData.VfsPtr,
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
            if (pxTexture.mipmapCount == 1)
            {
                // Let Unity generate mips if not provided.
                if (format == TextureFormat.DXT1)
                {
                    // Unity doesn't want to create mips for DXT1 textures. Recreate them as RGB24.
                    Texture2D dxtTexture = new Texture2D((int)pxTexture.width, (int)pxTexture.height, format, false);
                    dxtTexture.SetPixelData(pxTexture.mipmaps[0].mipmap, 0);
                    dxtTexture.Apply(false);
                    texture = new Texture2D((int)pxTexture.width, (int)pxTexture.height, TextureFormat.RGB24, true);
                    texture.SetPixels(dxtTexture.GetPixels());
                    texture.Apply(true, true);
                    GameObject.Destroy(dxtTexture);
                }
                else
                {
                    texture = new Texture2D((int)pxTexture.width, (int)pxTexture.height, format, true);
                    texture.SetPixelData(pxTexture.mipmaps[0].mipmap, 0);
                    texture.Apply(true, true);
                }
            }
            else
            {
                // Use Gothic's mips if provided. We could also let Unity generate them here, though.
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

        public static PxModelScriptData TryGetMds(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdsCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxModelScript.GetModelScriptFromVfs(GameData.VfsPtr, $"{preparedKey}.mds");
            mdsCache[preparedKey] = newData;

            return newData;
        }

        public static PxAnimationData TryGetAnimation(string mdsKey, string animKey)
        {
            var preparedMdsKey = GetPreparedKey(mdsKey);
            var preparedAnimKey = GetPreparedKey(animKey);
            var preparedKey = preparedMdsKey + "-" + preparedAnimKey;
            if (animCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxAnimation.LoadFromVfs(GameData.VfsPtr, $"{preparedKey}.man");
            animCache[preparedKey] = newData;

            return newData;
        }

        public static PxModelHierarchyData TryGetMdh(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdhCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxModelHierarchy.LoadFromVfs(GameData.VfsPtr, $"{preparedKey}.mdh");
            mdhCache[preparedKey] = newData;

            return newData;
        }

        public static PxModelData TryGetMdl(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdlCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxModel.LoadModelFromVfs(GameData.VfsPtr, $"{preparedKey}.mdl");
            mdlCache[preparedKey] = newData;

            return newData;
        }

        public static PxModelMeshData TryGetMdm(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mdmCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxModelMesh.LoadModelMeshFromVfs(GameData.VfsPtr, $"{preparedKey}.mdm");
            mdmCache[preparedKey] = newData;

            FixArmorTriangles(preparedKey, newData);

            return newData;
        }

        /// <summary>
        /// Some armor mdm's have wrong triangles. This function corrects them hard coded until we find a proper solution.
        /// </summary>
        private static void FixArmorTriangles(string key, PxModelMeshData mdm)
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

        public static PxMultiResolutionMeshData TryGetMrm(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mrmCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxMultiResolutionMesh.GetMRMFromVfs(GameData.VfsPtr, $"{preparedKey}.mrm");
            mrmCache[preparedKey] = newData;

            return newData;
        }

        public static PxMorphMeshData TryGetMmb(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (mmbCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxMorphMesh.LoadMorphMeshFromVfs(GameData.VfsPtr, $"{preparedKey}.mmb");
            mmbCache[preparedKey] = newData;

            return newData;
        }

        public static PxVmMusicData TryGetMusic(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (musicDataCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxVm.InitializeMusic(GameData.VmMusicPtr, preparedKey);
            musicDataCache[preparedKey] = newData;

            return newData;
        }

        /// <summary>
        /// Hint: Instances only need to be initialized once on phoenix.
        /// There are two ways of getting Item data. Via INSTANCE name or symbolIndex inside VM.
        /// </summary>
        public static PxVmItemData TryGetItemData(uint instanceId)
        {
            var symbol = PxDaedalusScript.GetSymbol(GameData.VmGothicPtr, instanceId);

            if (symbol == null)
                return null;

            return TryGetItemData(symbol.name);
        }

        /// <summary>
        /// Hint: Instances only need to be initialized once on phoenix.
        /// There are two ways of getting Item data. Via INSTANCE name or symbolIndex inside VM.
        /// </summary>
        public static PxVmItemData TryGetItemData(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (itemDataCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxVm.InitializeItem(GameData.VmGothicPtr, preparedKey);
            itemDataCache[preparedKey] = newData;

            return newData;
        }

        /// <summary>
        /// Hint: Instances only need to be initialized once on phoenix and don't need to be deleted during runtime.
        /// </summary>
        public static PxVmSfxData TryGetSfxData(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (sfxDataCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxVm.InitializeSfx(GameData.VmSfxPtr, preparedKey);
            sfxDataCache[preparedKey] = newData;

            return newData;
        }

        public static PxSoundData<float> TryGetSound(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (soundCache.TryGetValue(preparedKey, out var data))
                return data;

            var wavFile = PxSound.GetSoundArrayFromVfs<float>(GameData.VfsPtr, $"{preparedKey}.wav");
            soundCache[preparedKey] = wavFile;

            return wavFile;
        }

        public static PxFontData TryGetFont(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (fontCache.TryGetValue(preparedKey, out var data))
                return data;

            var fontData = PxFont.LoadFont(GameData.VfsPtr, $"{preparedKey}.fnt");
            fontCache[preparedKey] = fontData;

            return fontData;
        }

        private static string GetPreparedKey(string key)
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
