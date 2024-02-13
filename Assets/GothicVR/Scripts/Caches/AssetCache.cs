using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GVR.Creator.Sounds;
using GVR.Data;
using GVR.Extensions;
using GVR.Globals;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using ZenKit;
using ZenKit.Daedalus;
using Font = ZenKit.Font;
using Mesh = ZenKit.Mesh;
using Object = UnityEngine.Object;

namespace GVR.Caches
{
    public static class AssetCache
    {
        public static Dictionary<TextureArrayTypes, UnityEngine.Texture> TextureArrays { get; private set; } = new();
        public static int ReferenceTextureSize = 256;

        private static readonly Dictionary<string, Texture2D> TextureCache = new();
        private static readonly Dictionary<string, ZenKit.Texture> TextureDataCache = new();
        private static readonly Dictionary<string, IMesh> MshCache = new();
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

        private static Dictionary<TextureArrayTypes, List<(string PreparedKey, ZenKit.Texture Texture)>> _arrayTexturesList = new();

        public enum TextureArrayTypes
        {
            Opaque,
            Transparent,
            Water
        }

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

        public static void GetTextureArrayIndex(IMaterial materialData, out TextureArrayTypes textureArrayType, out int arrayIndex, out Vector2 textureScale, out int maxMipLevel)
        {
            string key = materialData.Texture;
            ZenKit.Texture zenTextureData = GetZenTextureData(key);
            if (zenTextureData == null)
            {
                textureArrayType = default;
                arrayIndex = -1;
                textureScale = Vector2.zero;
                maxMipLevel = 0;
                return;
            }

            maxMipLevel = zenTextureData.MipmapCount - 1;
            UnityEngine.TextureFormat textureFormat = zenTextureData.Format.AsUnityTextureFormat();
            if (materialData.Group == MaterialGroup.Water)
            {
                textureArrayType = TextureArrayTypes.Water;
            }
            else
            {
                textureArrayType = textureFormat == UnityEngine.TextureFormat.DXT1 ? TextureArrayTypes.Opaque : TextureArrayTypes.Transparent;
            }
            textureScale = new Vector2((float)zenTextureData.Width / ReferenceTextureSize, (float)zenTextureData.Height / ReferenceTextureSize);
            if (!_arrayTexturesList.ContainsKey(textureArrayType))
            {
                _arrayTexturesList.Add(textureArrayType, new List<(string PreparedKey, ZenKit.Texture Texture)>());
            }

            (string, ZenKit.Texture) cachedTextureData = _arrayTexturesList[textureArrayType].FirstOrDefault(k => k.PreparedKey == key);
            if (cachedTextureData != default)
            {
                arrayIndex = _arrayTexturesList[textureArrayType].IndexOf(cachedTextureData);
            }
            else
            {
                _arrayTexturesList[textureArrayType].Add((key, zenTextureData));
                arrayIndex = _arrayTexturesList[textureArrayType].Count - 1;
            }
        }

        public static async Task BuildTextureArrays()
        {
            System.Diagnostics.Stopwatch stopwatch = new();
            stopwatch.Start();
            foreach (TextureArrayTypes textureArrayType in _arrayTexturesList.Keys)
            {
                // Create the texture array with the max size of the contained textures.
                int maxSize = _arrayTexturesList[textureArrayType].Max(p => p.Texture.Width);
                int index = _arrayTexturesList[textureArrayType].FindIndex(p => p.Texture.Width == maxSize);

                UnityEngine.TextureFormat textureFormat = UnityEngine.TextureFormat.RGBA32;
                if (textureArrayType == TextureArrayTypes.Opaque)
                {
                    textureFormat = UnityEngine.TextureFormat.DXT1;
                }
                UnityEngine.Texture texArray = null;
                if (textureArrayType != TextureArrayTypes.Water)
                {
                    texArray = new Texture2DArray(maxSize, maxSize, _arrayTexturesList[textureArrayType].Count, textureFormat, true, false, true)
                    {
                        filterMode = FilterMode.Trilinear,
                        wrapMode = TextureWrapMode.Repeat,
                    };
                }
                else
                {
                    texArray = new RenderTexture(maxSize, maxSize, 0, RenderTextureFormat.ARGB32, _arrayTexturesList[textureArrayType].Max(p => p.Texture.MipmapCount))
                    {
                        dimension = UnityEngine.Rendering.TextureDimension.Tex2DArray,
                        autoGenerateMips = false,
                        filterMode = FilterMode.Trilinear,
                        useMipMap = true,
                        volumeDepth = _arrayTexturesList[textureArrayType].Count,
                        wrapMode = TextureWrapMode.Repeat
                    };
                }

                // Copy all the textures and their mips into the array. Textures which are smaller are tiled so bilinear sampling isn't broken - this is why it's not possible to pack different textures together in the same slice.
                for (int i = 0; i < _arrayTexturesList[textureArrayType].Count; i++)
                {
                    Texture2D sourceTex = ImportZenTexture(_arrayTexturesList[textureArrayType][i].PreparedKey);
                    for (int mip = 0; mip < sourceTex.mipmapCount; mip++)
                    {
                        for (int x = 0; x < texArray.width / sourceTex.width; x++)
                        {
                            for (int y = 0; y < texArray.height / sourceTex.height; y++)
                            {
                                if (texArray is Texture2DArray)
                                {
                                    Graphics.CopyTexture(sourceTex, 0, mip, 0, 0, sourceTex.width >> mip, sourceTex.height >> mip, texArray, i, mip, (sourceTex.width >> mip) * x, (sourceTex.height >> mip) * y);
                                }
                                else
                                {
                                    CommandBuffer cmd = CommandBufferPool.Get();
                                    RenderTexture rt = (RenderTexture)texArray;
                                    cmd.SetRenderTarget(new RenderTargetBinding(new RenderTargetSetup(rt.colorBuffer, rt.depthBuffer, mip, CubemapFace.Unknown, i)));
                                    Vector2 scale = new Vector2((float)sourceTex.width / texArray.width, (float)sourceTex.height / texArray.height);
                                    Blitter.BlitQuad(cmd, sourceTex, new Vector4(1, 1, 0, 0), new Vector4(scale.x, scale.y, scale.x * x, scale.y * y), mip, false);
                                    Graphics.ExecuteCommandBuffer(cmd);
                                    cmd.Clear();
                                    CommandBufferPool.Release(cmd);
                                }
                            }
                        }
                    }
                    if (!Application.isPlaying)
                    {
                        Object.DestroyImmediate(sourceTex);
                    }
                    else
                    {
                        Object.Destroy(sourceTex);
                    }

                    if (i % 20 == 0)
                    {
                        await Task.Yield();
                    }
                }

                TextureArrays.Add(textureArrayType, texArray);
            }

            // Clear cached texture data.
            _arrayTexturesList.Clear();
            _arrayTexturesList.TrimExcess();

            stopwatch.Stop();
            Debug.Log($"Built tex array in {stopwatch.ElapsedMilliseconds / 1000f} s");
        }

        [CanBeNull]
        public static Texture2D TryGetTexture(string key)
        {
            string preparedKey = GetPreparedKey(key);
            if (TextureCache.ContainsKey(preparedKey) && TextureCache[preparedKey])
            {
                return TextureCache[preparedKey];
            }

            return ImportZenTexture(key);
        }

        public static ZenKit.Texture GetZenTextureData(string key)
        {
            if (TextureDataCache.ContainsKey(key))
            {
                return TextureDataCache[key];
            }

            try
            {
                string preparedKey = GetPreparedKey(key);
                TextureDataCache.Add(key, new ZenKit.Texture(GameData.Vfs, $"{preparedKey}-C.TEX"));
                return TextureDataCache[key];
            }
            catch (Exception)
            {
                Debug.LogWarning($"Texture {key} couldn't be found.");
                return null;
            }
        }

        private static Texture2D ImportZenTexture(string key)
        {
            string preparedKey = GetPreparedKey(key);
            ZenKit.Texture zkTexture = GetZenTextureData(key);
            if (zkTexture == null)
            {
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
                texture = new Texture2D(zkTexture.Width, zkTexture.Height, format, zkTexture.MipmapCount, false);
                for (var i = 0; i < zkTexture.MipmapCount; i++)
                {
                    if (format == UnityEngine.TextureFormat.RGBA32)
                    {
                        // RGBA is uncompressed format.
                        texture.SetPixelData(zkTexture.AllMipmapsRgba[i], i);
                    }
                    else
                    {
                        // Raw means "compressed data provided by Gothic texture"
                        texture.SetPixelData(zkTexture.AllMipmapsRaw[i], i);
                    }
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
        private static Texture2D GenerateDxt1Mipmaps(ZenKit.Texture zkTexture)
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

            IModelAnimation newData = null;
            try
            {
                newData = new ModelAnimation(GameData.Vfs, $"{preparedKey}.man").Cache();
            }
            catch (Exception)
            {
                // ignored
            }

            AnimCache[preparedKey] = newData;

            return newData;
        }

        [CanBeNull]
        public static IMesh TryGetMsh(string key)
        {
            var preparedKey = GetPreparedKey(key);
            if (MshCache.TryGetValue(preparedKey, out var data))
                return data;

            IMesh newData = null;
            try
            {
                newData = new Mesh(GameData.Vfs, $"{preparedKey}.msh").Cache();
            }
            catch (Exception)
            {
                // ignored
            }

            MshCache[preparedKey] = newData;

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

        /// <summary>
        /// MMS == MorphMesh
        /// e.g. face animations during dialogs.
        /// </summary>
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
        /// Hint: Instances only need to be initialized once in ZenKit.
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
        /// Hint: Instances only need to be initialized once in ZenKit.
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
        /// Hint: Instances only need to be initialized once in ZenKit and don't need to be deleted during runtime.
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
        /// Hint: Instances only need to be initialized once in ZenKit and don't need to be deleted during runtime.
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

        public static void Dispose()
        {
            TextureCache.Clear();
            TextureDataCache.Clear();
            MdsCache.Clear();
            AnimCache.Clear();
            MdhCache.Clear();
            MdlCache.Clear();
            MdmCache.Clear();
            MrmCache.Clear();
            MmbCache.Clear();
            ItemDataCache.Clear();
            MusiThemeCache.Clear();
            SfxDataCache.Clear();
            PfxDataCache.Clear();
            SoundCache.Clear();
            FontCache.Clear();
        }
    }
}
