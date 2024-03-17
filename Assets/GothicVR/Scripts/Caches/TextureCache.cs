using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GVR.Extensions;
using GVR.Globals;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using ZenKit;
using Object = UnityEngine.Object;

namespace GVR.Caches
{
    public static class TextureCache
    {
        public static int ReferenceTextureSize = 256;

        public static Dictionary<TextureArrayTypes, UnityEngine.Texture> TextureArrays { get; private set; } = new();

        private static readonly Dictionary<string, Texture2D> TextureDataCache = new();
        private static readonly Dictionary<TextureArrayTypes, List<(string PreparedKey, ITexture Texture)>> _arrayTexturesList = new();

        public enum TextureArrayTypes
        {
            Opaque,
            Transparent,
            Water
        }

        [CanBeNull]
        public static Texture2D TryGetTexture(string key)
        {
            string preparedKey = GetPreparedKey(key);
            if (TextureDataCache.ContainsKey(preparedKey) && TextureDataCache[preparedKey])
            {
                return TextureDataCache[preparedKey];
            }

            return ImportZenTexture(key);
        }

        public static void GetTextureArrayIndex(IMaterial materialData, out TextureArrayTypes textureArrayType, out int arrayIndex, out Vector2 textureScale, out int maxMipLevel)
        {
            string key = materialData.Texture;
            ITexture zenTextureData = AssetCache.TryGetTexture(key);

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
                _arrayTexturesList.Add(textureArrayType, new List<(string PreparedKey, ITexture Texture)>());
            }

            (string, ITexture) cachedTextureData = _arrayTexturesList[textureArrayType].FirstOrDefault(k => k.PreparedKey == key);
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

                UnityEngine.Texture texArray;
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
                        dimension = TextureDimension.Tex2DArray,
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

        private static Texture2D ImportZenTexture(string key)
        {
            string preparedKey = GetPreparedKey(key);
            ITexture zkTexture = AssetCache.TryGetTexture(key);
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
            TextureDataCache[preparedKey] = texture;

            return texture;
        }

        /// <summary>
        /// Unity doesn't want to create mips for DXT1 textures. Recreate them as RGB24.
        /// </summary>
        private static Texture2D GenerateDxt1Mipmaps(ITexture zkTexture)
        {
            var dxtTexture = new Texture2D(zkTexture.Width, zkTexture.Height, UnityEngine.TextureFormat.DXT1, false);
            dxtTexture.SetPixelData(zkTexture.AllMipmapsRaw[0], 0);
            dxtTexture.Apply(false);

            var texture = new Texture2D(zkTexture.Width, zkTexture.Height, UnityEngine.TextureFormat.RGB24, true);
            texture.SetPixels(dxtTexture.GetPixels());
            texture.Apply(true, true);
            Object.Destroy(dxtTexture);

            return texture;
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
            TextureDataCache.Clear();
            TextureArrays.Clear();
            _arrayTexturesList.TrimExcess();
        }
    }
}
