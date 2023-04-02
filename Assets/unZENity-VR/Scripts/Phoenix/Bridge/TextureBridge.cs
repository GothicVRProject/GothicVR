using System;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using UnityEngine;
using UZVR.World;
using static UZVR.World.BTexture;

namespace UZVR.Phoenix.Bridge
{
    public static class TextureBridge
    {
        private const string DLLNAME = PhoenixBridge.DLLNAME;
        [DllImport(DLLNAME)] private static extern IntPtr textureLoad(IntPtr vdfContainer, string textureFileName, out BTexture.BFormat format, out int mipmapCount);
        [DllImport(DLLNAME)] private static extern void textureGetMipmapInformation(IntPtr texture, int mipmapId, out uint width, out uint height, out UInt64 byteCount);
        [DllImport(DLLNAME)] private static extern byte textureGetByte(IntPtr texture, int mipmapId, UInt64 index);
        [DllImport(DLLNAME)] private static extern void textureDispose(IntPtr texture);


        public static BTexture LoadTexture(IntPtr vdfContainer, string name)
        {
            var texture = textureLoad(vdfContainer, name, out BTexture.BFormat format, out int mipmapCount);

            if (texture == IntPtr.Zero)
                return null;

            BTexture bTexture = null;

            switch (format)
            {
                case BFormat.tex_dxt1:
                case BFormat.tex_dxt5:
                    bTexture = LoadTextureCompressed(texture, format);
                    break;
                case BFormat.tex_dxt3:
                    bTexture = LoadTextureUncompressed(texture);
                    break;
                default:
                    // Two options here: 1/ we need to check if the format can be handled within Unity and just forward it to Texture2D, 2/ if not, we need to get raw uncompressed data from phoenix.
                    throw new ArgumentException("Not yet handled TextureFormat compression of " + Enum.GetName(typeof(BFormat), format));
            }

            return bTexture;
        }

        private static BTexture LoadTextureCompressed(IntPtr texture, BTexture.BFormat format)
        {
            // FIXME - Load other mipmaps with lower resolution as well (if supported by Unity of course...)            
            textureGetMipmapInformation(texture, 0, out uint width, out uint height, out UInt64 byteCount);

            if (byteCount > (UInt64)int.MaxValue) // I don't think this will ever happen, but you never know...
                throw new ArgumentOutOfRangeException("byteCount for given texture-mipmap is too high for an int value but need to fit within a List<>(int) size for texture.");

            var bTexture = new BTexture
            {
                format = format,
                width = width,
                height = height,
                data = new((int)byteCount)
            };

            for (UInt64 i = 0; i < byteCount; i++)
                bTexture.data.Add(textureGetByte(texture, 0, i));

            textureDispose(texture);

            return bTexture;
        }

        /// <summary>
        /// If Unity can't handle a certain texture compression, we need to ask phoenix to provide the uncompressed value and store it.
        /// </summary>
        private static BTexture LoadTextureUncompressed(IntPtr texture)
        {
            // FIXME - Implement!
            return null;
        }

    }
}