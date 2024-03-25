using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GVR.Extensions;
using GVR.World;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using ZenKit;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;
using Texture = UnityEngine.Texture;
using TextureFormat = ZenKit.TextureFormat;

namespace GVR.Caches
{
    public static class TextureCache
    {
        public static int ReferenceTextureSize = 256;

        public static Dictionary<TextureTypes, Dictionary<TextureArrayTypes, Texture>> TextureArrays { get; } = new()
        {
            { TextureTypes.World, new() },
            { TextureTypes.Vob, new() }
        };

        public static List<(MeshRenderer Renderer, WorldData.SubMeshData SubmeshData)> WorldMeshRenderersForTextureArray = new();
        public static List<(MeshRenderer Renderer, (IMultiResolutionMesh Mrm, List<TextureArrayTypes> TextureArrayTypes) Data)> VobMeshRenderersForTextureArray = new();

        private static readonly Dictionary<string, Texture2D> Texture2DCache = new();
        private static readonly Dictionary<TextureTypes, Dictionary<TextureArrayTypes, List<(string PreparedKey, ITexture Texture)>>> _arrayTexturesList = new()
        {
            { TextureTypes.World, new() },
            { TextureTypes.Vob, new() }
        };


        public enum TextureTypes
        {
            World,
            Vob
        }

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
            if (Texture2DCache.TryGetValue(preparedKey, out var cachedTexture))
            {
                return cachedTexture;
            }

            ITexture zkTexture = AssetCache.TryGetTexture(key);
            if (zkTexture == null)
            {
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

            Texture2DCache[preparedKey] = texture;

            return texture;
        }

        public static void GetTextureArrayIndex(TextureTypes type, IMaterial materialData, out TextureArrayTypes textureArrayType, out int arrayIndex, out Vector2 textureScale, out int maxMipLevel)
        {
            Dictionary<TextureArrayTypes, List<(string PreparedKey, ITexture Texture)>> textureDict = _arrayTexturesList[type];

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
            if (!textureDict.ContainsKey(textureArrayType))
            {
                textureDict.Add(textureArrayType, new List<(string PreparedKey, ITexture Texture)>());
            }

            (string, ITexture) cachedTextureData = textureDict[textureArrayType].FirstOrDefault(k => k.PreparedKey == key);
            if (cachedTextureData != default)
            {
                arrayIndex = textureDict[textureArrayType].IndexOf(cachedTextureData);
            }
            else
            {
                textureDict[textureArrayType].Add((key, zenTextureData));
                arrayIndex = textureDict[textureArrayType].Count - 1;
            }
        }

        public static async Task BuildTextureArrays(TextureTypes type)
        {
            Dictionary<TextureArrayTypes, List<(string PreparedKey, ITexture Texture)>> textureDict = _arrayTexturesList[type];

            Stopwatch stopwatch = new();
            stopwatch.Start();
            foreach (TextureArrayTypes textureArrayType in textureDict.Keys)
            {
                // Create the texture array with the max size of the contained textures.
                int maxSize = textureDict[textureArrayType].Max(p => p.Texture.Width);

                UnityEngine.TextureFormat textureFormat = UnityEngine.TextureFormat.RGBA32;
                if (textureArrayType == TextureArrayTypes.Opaque)
                {
                    textureFormat = UnityEngine.TextureFormat.DXT1;
                }

                Texture texArray;
                if (textureArrayType != TextureArrayTypes.Water)
                {
                    texArray = new Texture2DArray(maxSize, maxSize, textureDict[textureArrayType].Count, textureFormat, true, false, true)
                    {
                        filterMode = FilterMode.Trilinear,
                        wrapMode = TextureWrapMode.Repeat,
                    };
                }
                else
                {
                    texArray = new RenderTexture(maxSize, maxSize, 0, RenderTextureFormat.ARGB32, textureDict[textureArrayType].Max(p => p.Texture.MipmapCount))
                    {
                        dimension = TextureDimension.Tex2DArray,
                        autoGenerateMips = false,
                        filterMode = FilterMode.Trilinear,
                        useMipMap = true,
                        volumeDepth = textureDict[textureArrayType].Count,
                        wrapMode = TextureWrapMode.Repeat
                    };
                }

                // Copy all the textures and their mips into the array. Textures which are smaller are tiled so bilinear sampling isn't broken - this is why it's not possible to pack different textures together in the same slice.
                for (int i = 0; i < textureDict[textureArrayType].Count; i++)
                {
                    Texture2D sourceTex = TryGetTexture(textureDict[textureArrayType][i].PreparedKey);
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

                    if (i % 20 == 0)
                    {
                        await Task.Yield();
                    }
                }

                TextureArrays[type].Add(textureArrayType, texArray);
            }

            // Clear cached texture data to save storage.
            foreach (var textureList in textureDict.Values)
            {
                textureList.Clear();
                textureList.TrimExcess();
            }

            stopwatch.Stop();
            Debug.Log($"Built tex array in {stopwatch.ElapsedMilliseconds / 1000f} s");
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

        /// <summary>
        /// Once a TextureArray is build and assigned to renderers, we can safely remove the
        /// "renderers in need for texture array data" from memory.
        /// </summary>
        public static void RemoveCachedTextureArrayData(TextureTypes type)
        {
            switch (type)
            {
                case TextureTypes.World:
                    TextureArrays[TextureTypes.World].Clear();
                    TextureArrays[TextureTypes.World].TrimExcess();

                    _arrayTexturesList[TextureTypes.World].Clear();
                    _arrayTexturesList[TextureTypes.World].TrimExcess();

                    WorldMeshRenderersForTextureArray.Clear();
                    WorldMeshRenderersForTextureArray.TrimExcess();
                    break;
                case TextureTypes.Vob:
                    TextureArrays[TextureTypes.Vob].Clear();
                    TextureArrays[TextureTypes.Vob].TrimExcess();

                    _arrayTexturesList[TextureTypes.Vob].Clear();
                    _arrayTexturesList[TextureTypes.Vob].TrimExcess();

                    VobMeshRenderersForTextureArray.Clear();
                    VobMeshRenderersForTextureArray.TrimExcess();
                    break;
                default:
                    throw new NotImplementedException($"Type {type} not yet implemented for cleanup.");
            }
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
            Texture2DCache.Clear();

            foreach (var textureArray in TextureArrays.Values)
            {
                textureArray.Clear();
                textureArray.TrimExcess();
            }

            foreach (var textureList in _arrayTexturesList.Values)
            {
                textureList.Clear();
                textureList.TrimExcess();
            }
        }
    }
}
