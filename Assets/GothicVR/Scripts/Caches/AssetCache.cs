using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GVR.Creator.Sounds;
using GVR.Data;
using GVR.Extensions;
using GVR.Globals;
using JetBrains.Annotations;
using PxCs.Data.Sound;
using PxCs.Interface;
using UnityEngine;
using ZenKit;
using ZenKit.Daedalus;
using Font = ZenKit.Font;
using Object = UnityEngine.Object;
using Texture = ZenKit.Texture;
using TextureFormat = ZenKit.TextureFormat;

namespace GVR.Caches
{
    public static class AssetCache
    {
        private static readonly Dictionary<string, Texture2D> TextureCache = new();
        private static readonly Dictionary<string, IModelScript> MdsCache = new();
        private static readonly Dictionary<string, IModelAnimation> AnimCache = new();
        private static readonly Dictionary<string, IModelHierarchy> MdhCache = new();
        private static readonly Dictionary<string, IModel> MdlCache = new();
        private static readonly Dictionary<string, IModelMesh> MdmCache = new();
        private static readonly Dictionary<string, IMultiResolutionMesh> MrmCache = new();
        private static readonly Dictionary<string, IMorphMesh> MmbCache = new();
        private static readonly Dictionary<string, ItemInstance> ItemDataCache = new();
        private static readonly Dictionary<string, MusicThemeInstance> MusiThemeCache = new();
        private static readonly Dictionary<string, SoundEffectInstance> SfxDataCache = new();
        private static readonly Dictionary<string, ParticleEffectInstance> PfxDataCache = new();
        private static readonly Dictionary<string, SoundData> SoundCache = new();
        private static readonly Dictionary<string, IFont> FontCache = new();

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
            catch (Exception)
            {
                Debug.LogWarning($"Texture {key} couldn't be found.");
                return null;
            }

            Texture2D texture;

            // Workaround for Unity and DXT1 Mipmaps.
            if (zkTexture.Format == TextureFormat.Dxt1 && zkTexture.MipmapCount == 1)
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
                    if (format == UnityEngine.TextureFormat.RGBA32)
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
            var dxtTexture = new Texture2D((int)zkTexture.Width, (int)zkTexture.Height, UnityEngine.TextureFormat.DXT1, false);
            dxtTexture.SetPixelData(zkTexture.AllMipmapsRaw[0], 0);
            dxtTexture.Apply(false);

            var texture = new Texture2D((int)zkTexture.Width, (int)zkTexture.Height, UnityEngine.TextureFormat.RGB24, true);
            texture.SetPixels(dxtTexture.GetPixels());
            texture.Apply(true, true);
            Object.Destroy(dxtTexture);

            return texture;
        }

        public static IModelScript TryGetMds(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MdsCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = new ModelScript(GameData.Vfs, $"{preparedKey}.mds").Cache();
            MdsCache[preparedKey] = newData;

            return newData;
        }

        public static IModelAnimation TryGetAnimation(string mdsKey, string animKey)
        {
            var preparedMdsKey = GetPreparedKey(mdsKey);
            var preparedAnimKey = GetPreparedKey(animKey);
            var preparedKey = $"{preparedMdsKey}-{preparedAnimKey}";
            if (AnimCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = new ModelAnimation(GameData.Vfs, $"{preparedKey}.man").Cache();
            AnimCache[preparedKey] = newData;

            return newData;
        }

        [CanBeNull]
        public static IModelHierarchy TryGetMdh(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MdhCache.TryGetValue(preparedKey, out var data))
                return data;

            IModelHierarchy newData = null;
            try
            {
                newData = new ModelHierarchy(GameData.Vfs, $"{preparedKey}.mdh").Cache();
            }
            catch (Exception)
            {
                // ignored
            }

            MdhCache[preparedKey] = newData;

            return newData;
        }

        [CanBeNull]
        public static IModel TryGetMdl(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MdlCache.TryGetValue(preparedKey, out var data))
                return data;

            IModel newData = null;
            try
            {
                newData = new Model(GameData.Vfs, $"{preparedKey}.mdl").Cache();
            }
            catch (Exception)
            {
                // ignored
            }

            MdlCache[preparedKey] = newData;

            return newData;
        }

        [CanBeNull]
        public static IModelMesh TryGetMdm(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MdmCache.TryGetValue(preparedKey, out var data))
                return data;

            IModelMesh newData = null;
            try
            {
                newData = new ModelMesh(GameData.Vfs, $"{preparedKey}.mdm").Cache();
            }
            catch (Exception)
            {
                // ignored
            }

            MdmCache[preparedKey] = newData;

            FixArmorTriangles(preparedKey, newData);

            return newData;
        }

        /// <summary>
        /// Some armor mdm's have wrong triangles. This function corrects them hard coded until we find a proper solution.
        /// </summary>
        private static void FixArmorTriangles(string key, IModelMesh mdm)
        {
            if (!MisplacedMdmArmors.Contains(key, StringComparer.OrdinalIgnoreCase))
                return;

            foreach (var mesh in mdm.Meshes)
            {
                for (var i = 0; i < mesh.Mesh.Positions.Count; i++)
                {
                    var curPos = mesh.Mesh.Positions[i];
                    mesh.Mesh.Positions[i] = new(curPos.X + 0.5f, curPos.Y - 0.5f, curPos.Z + 13f);
                }
            }
        }

        public static IMultiResolutionMesh TryGetMrm(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MrmCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = new MultiResolutionMesh(GameData.Vfs, $"{preparedKey}.mrm").Cache();
            MrmCache[preparedKey] = newData;

            return newData;
        }

        public static IMorphMesh TryGetMmb(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MmbCache.TryGetValue(preparedKey, out var data))
                return data;

            var newData = new MorphMesh(GameData.Vfs, $"{preparedKey}.mmb").Cache();
            MmbCache[preparedKey] = newData;

            return newData;
        }

        public static MusicThemeInstance TryGetMusic(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MusiThemeCache.TryGetValue(preparedKey, out var data))
                return data;

            MusicThemeInstance newData = null;
            try
            {
                newData = GameData.MusicVm.InitInstance<MusicThemeInstance>(preparedKey);
            }
            catch (Exception)
            {
                // ignored
            }
            MusiThemeCache[preparedKey] = newData;

            return newData;
        }

        /// <summary>
        /// Hint: Instances only need to be initialized once on phoenix.
        /// There are two ways of getting Item data. Via INSTANCE name or symbolIndex inside VM.
        /// </summary>
        public static ItemInstance TryGetItemData(int instanceId)
        {
            var symbol = GameData.GothicVm.GetSymbolByIndex(instanceId);

            if (symbol == null)
                return null;

            return TryGetItemData(symbol.Name);
        }

        /// <summary>
        /// Hint: Instances only need to be initialized once on phoenix.
        /// There are two ways of getting Item data. Via INSTANCE name or symbolIndex inside VM.
        /// </summary>
        [CanBeNull]
        public static ItemInstance TryGetItemData(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (ItemDataCache.TryGetValue(preparedKey, out var data))
                return data;

            ItemInstance newData = null;
            try
            {
                newData = GameData.GothicVm.InitInstance<ItemInstance>(preparedKey);
            }
            catch (Exception)
            {
                // ignored
            }
            ItemDataCache[preparedKey] = newData;

            return newData;
        }

        /// <summary>
        /// Hint: Instances only need to be initialized once on phoenix and don't need to be deleted during runtime.
        /// </summary>
        [CanBeNull]
        public static SoundEffectInstance TryGetSfxData(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (SfxDataCache.TryGetValue(preparedKey, out var data))
                return data;

            SoundEffectInstance newData = null;
            try
            {
                newData = GameData.SfxVm.InitInstance<SoundEffectInstance>(preparedKey);
            }
            catch (Exception)
            {
                // ignored
            }
            SfxDataCache[preparedKey] = newData;

            return newData;
        }

        /// <summary>
        /// Hint: Instances only need to be initialized once on phoenix and don't need to be deleted during runtime.
        /// </summary>
        public static ParticleEffectInstance TryGetPfxData(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (PfxDataCache.TryGetValue(preparedKey, out var data))
                return data;

            ParticleEffectInstance newData = null;
            try
            {
                newData = GameData.PfxVm.InitInstance<ParticleEffectInstance>(preparedKey);
            }
            catch (Exception)
            {
                // ignored
            }
            PfxDataCache[preparedKey] = newData;

            return newData;
        }

        public static SoundData TryGetSound(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (SoundCache.TryGetValue(preparedKey, out var data))
                return data;
            
            var newData = SoundCreator.GetSoundArrayFromVfs($"{preparedKey}.wav");
            SoundCache[preparedKey] = newData;

            return newData;
        }

        public static IFont TryGetFont(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (FontCache.TryGetValue(preparedKey, out var data))
                return data;
            
            var fontData = new Font(GameData.Vfs, $"{preparedKey}.fnt").Cache();
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
