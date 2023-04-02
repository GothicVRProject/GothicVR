using System;
using System.Collections.Generic;
using UnityEngine;

namespace UZVR.World
{

    public class BTexture
    {
        public enum BFormat {
            // Values taken directly from https://github.com/lmichaelis/phoenix/blob/main/include/phoenix/texture.hh
            tex_B8G8R8A8 = 0x0, ///< \brief 32-bit ARGB pixel format with alpha, using 8 bits per channel
            tex_R8G8B8A8 = 0x1, ///< \brief 32-bit ARGB pixel format with alpha, using 8 bits per channel
            tex_A8B8G8R8 = 0x2, ///< \brief 32-bit ARGB pixel format with alpha, using 8 bits per channel
            tex_A8R8G8B8 = 0x3, ///< \brief 32-bit ARGB pixel format with alpha, using 8 bits per channel
            tex_B8G8R8 = 0x4,   ///< \brief 24-bit RGB pixel format with 8 bits per channel
            tex_R8G8B8 = 0x5,   ///< \brief 24-bit RGB pixel format with 8 bits per channel
            tex_A4R4G4B4 = 0x6, ///< \brief 16-bit ARGB pixel format with 4 bits for each channel
            tex_A1R5G5B5 = 0x7, ///< \brief 16-bit pixel format where 5 bits are reserved for each color, and 1 bit is reserved for alpha
            tex_R5G6B5 = 0x8,   ///< \brief 16-bit RGB pixel format with 5 bits for red, 6 bits for green, and 5 bits for blue
            tex_p8 = 0x9,       ///< \brief 8-bit color indexed
            tex_dxt1 = 0xA,     ///< \brief DXT1 compression texture format
            tex_dxt2 = 0xB,     ///< \brief DXT2 compression texture format
            tex_dxt3 = 0xC,     ///< \brief DXT3 compression texture format
            tex_dxt4 = 0xD,     ///< \brief DXT4 compression texture format
            tex_dxt5 = 0xE      ///< \brief DXT5 compression texture format
        }

        public TextureFormat GetUnityTextureFormat()
        {
            switch (format)
            {
                case BFormat.tex_dxt1:
                    return TextureFormat.DXT1;
                case BFormat.tex_dxt5:
                    return TextureFormat.DXT5;
                default:
                    throw new NotSupportedException("Format is not supported or not yet tested to work with Unity: " + Enum.GetName(typeof(BFormat), format));
            }
        }


        public BFormat format;
        public uint width;
        public uint height;
        public List<byte> data;
    }
}
