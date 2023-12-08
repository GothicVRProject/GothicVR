using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GVR.Extensions;
using GVR.Phoenix.Interface;
using JetBrains.Annotations;
using PxCs.Data.Animation;
using PxCs.Data.Mesh;
using PxCs.Data.Model;
using PxCs.Data.Sound;
using PxCs.Data.Vm;
using PxCs.Interface;
using UnityEngine;
using ZenKit.Materialized;
using ModelScript = ZenKit.Materialized.ModelScript;
using Font = ZenKit.Materialized.Font;
using Object = UnityEngine.Object;
using Texture = ZenKit.Texture;
using TextureFormat = UnityEngine.TextureFormat;

namespace GVR.Caches
{
    public static class AssetCache
    {
        private static readonly Dictionary<string, Texture2D> TextureCache = new();
        private static readonly Dictionary<string, ModelScript> MdsCache = new();
        private static readonly Dictionary<string, ModelAnimation> AnimCache = new();
        private static readonly Dictionary<string, PxModelHierarchyData> MdhCache = new();
        private static readonly Dictionary<string, PxModelData> MdlCache = new();
        private static readonly Dictionary<string, PxModelMeshData> MdmCache = new();
        private static readonly Dictionary<string, PxMultiResolutionMeshData> MrmCache = new();
        private static readonly Dictionary<string, PxMorphMeshData> MmbCache = new();
        private static readonly Dictionary<string, PxVmItemData> ItemDataCache = new();
        private static readonly Dictionary<string, PxVmMusicData> MusicDataCache = new();
        private static readonly Dictionary<string, PxVmSfxData> SfxDataCache = new();
        private static readonly Dictionary<string, PxVmPfxData> PfxDataCache = new();
        private static readonly Dictionary<string, PxSoundData<float>> SoundCache = new();
        private static readonly Dictionary<string, Font> FontCache = new();

        private static readonly string[] MisplacedMdmArmors =
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

        [CanBeNull]
        public static Texture2D TryGetTexture(string key)
        {
            string preparedKey = GetPreparedKey(key);
            if (TextureCache.ContainsKey(preparedKey) && TextureCache[preparedKey])
            {
                return TextureCache[preparedKey];
            }

            Texture zkTexture;
            try
            {
                zkTexture = new Texture(GameData.Vfs, $"{preparedKey}-C.TEX");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Texture {key} couldn't be found.");
                return null;
            }

            Texture2D texture;

            // Workaround for Unity and DXT1 Mipmaps.
            if (zkTexture.Format == ZenKit.TextureFormat.Dxt1 && zkTexture.MipmapCount == 1)
            {
                texture = GenerateDxt1Mipmaps(zkTexture);
            }
            else
            {
                var format = zkTexture.Format.AsUnityTextureFormat();
                var updateMipmaps = zkTexture.MipmapCount == 1; // Let Unity generate Mipmaps if they aren't provided by Gothic texture itself.

                // Use Gothic's mips if provided.
                texture = new Texture2D((int)zkTexture.Width, (int)zkTexture.Height, format, (int)zkTexture.MipmapCount, false);
                for (var i = 0; i < zkTexture.MipmapCount; i++)
                {
                    if (format == TextureFormat.RGBA32)
                        // RGBA is uncompressed format.
                        texture.SetPixelData(zkTexture.AllMipmapsRgba[i], i);
                    else
                        // Raw means "compressed data provided by Gothic texture"
                        texture.SetPixelData(zkTexture.AllMipmapsRaw[i], i);
                }

                texture.Apply(updateMipmaps, true);
            }

            texture.filterMode = FilterMode.Trilinear;
            texture.name = key;
            TextureCache[preparedKey] = texture;

            return texture;
        }

        /// <summary>
        /// Unity doesn't want to create mips for DXT1 textures. Recreate them as RGB24.
        /// </summary>
        private static Texture2D GenerateDxt1Mipmaps(Texture zkTexture)
        {
            var dxtTexture = new Texture2D((int)zkTexture.Width, (int)zkTexture.Height, TextureFormat.DXT1, false);
            dxtTexture.SetPixelData(zkTexture.AllMipmapsRaw[0], 0);
            dxtTexture.Apply(false);

            var texture = new Texture2D((int)zkTexture.Width, (int)zkTexture.Height, TextureFormat.RGB24, true);
            texture.SetPixels(dxtTexture.GetPixels());
            texture.Apply(true, true);
            Object.Destroy(dxtTexture);

            return texture;
        }

        public static ModelScript TryGetMds(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MdsCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = new ZenKit.ModelScript(GameData.Vfs, $"{preparedKey}.mds").Materialize();
            MdsCache[preparedKey] = newData;

            return newData;
        }

        public static ModelAnimation TryGetAnimation(string mdsKey, string animKey)
        {
            var preparedMdsKey = GetPreparedKey(mdsKey);
            var preparedAnimKey = GetPreparedKey(animKey);
            var preparedKey = preparedMdsKey + "-" + preparedAnimKey;
            if (AnimCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = new ZenKit.ModelAnimation(GameData.Vfs, $"{preparedKey}.man").Materialize();
            AnimCache[preparedKey] = newData;

            return newData;
        }

        public static PxModelHierarchyData TryGetMdh(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MdhCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxModelHierarchy.LoadFromVfs(GameData.VfsPtr, $"{preparedKey}.mdh");
            MdhCache[preparedKey] = newData;

            return newData;
        }

        public static PxModelData TryGetMdl(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MdlCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxModel.LoadModelFromVfs(GameData.VfsPtr, $"{preparedKey}.mdl");
            MdlCache[preparedKey] = newData;

            return newData;
        }

        public static PxModelMeshData TryGetMdm(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MdmCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxModelMesh.LoadModelMeshFromVfs(GameData.VfsPtr, $"{preparedKey}.mdm");
            MdmCache[preparedKey] = newData;

            FixArmorTriangles(preparedKey, newData);

            return newData;
        }

        /// <summary>
        /// Some armor mdm's have wrong triangles. This function corrects them hard coded until we find a proper solution.
        /// </summary>
        private static void FixArmorTriangles(string key, PxModelMeshData mdm)
        {
            if (!MisplacedMdmArmors.Contains(key, StringComparer.OrdinalIgnoreCase))
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
            if (MrmCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxMultiResolutionMesh.GetMRMFromVfs(GameData.VfsPtr, $"{preparedKey}.mrm");
            MrmCache[preparedKey] = newData;

            return newData;
        }

        public static PxMorphMeshData TryGetMmb(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MmbCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxMorphMesh.LoadMorphMeshFromVfs(GameData.VfsPtr, $"{preparedKey}.mmb");
            MmbCache[preparedKey] = newData;

            return newData;
        }

        public static PxVmMusicData TryGetMusic(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MusicDataCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxVm.InitializeMusic(GameData.VmMusicPtr, preparedKey);
            MusicDataCache[preparedKey] = newData;

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
            if (ItemDataCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxVm.InitializeItem(GameData.VmGothicPtr, preparedKey);
            ItemDataCache[preparedKey] = newData;

            return newData;
        }

        /// <summary>
        /// Hint: Instances only need to be initialized once on phoenix and don't need to be deleted during runtime.
        /// </summary>
        public static PxVmSfxData TryGetSfxData(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (SfxDataCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxVm.InitializeSfx(GameData.VmSfxPtr, preparedKey);
            SfxDataCache[preparedKey] = newData;

            return newData;
        }

        /// <summary>
        /// Hint: Instances only need to be initialized once on phoenix and don't need to be deleted during runtime.
        /// </summary>
        public static PxVmPfxData TryGetPfxData(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (PfxDataCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = PxVm.InitializePfx(GameData.VmPfxPtr, preparedKey);
            PfxDataCache[preparedKey] = newData;

            return newData;
        }

        public static PxSoundData<float> TryGetSound(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (SoundCache.TryGetValue(preparedKey, out var data))
                return data;

            var wavFile = PxSound.GetSoundArrayFromVfs<float>(GameData.VfsPtr, $"{preparedKey}.wav");
            SoundCache[preparedKey] = wavFile;

            return wavFile;
        }

        public static Font TryGetFont(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (FontCache.TryGetValue(preparedKey, out var data))
                return data;
            
            var fontData = new ZenKit.Font(GameData.Vfs, $"{preparedKey}.fnt").Materialize();
            FontCache[preparedKey] = fontData;

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
