﻿using PxCs.Interface;
using System;
using UnityEngine;

namespace GVR.Extensions
{
    public static class PxTextureDataExtension
    {
        public static TextureFormat AsUnityTextureFormat(this PxTexture.Format format)
        {
            switch (format)
            {
                case PxTexture.Format.tex_dxt1:
                    return TextureFormat.DXT1;
                case PxTexture.Format.tex_dxt5:
                    return TextureFormat.DXT5;
                case PxTexture.Format.tex_B8G8R8A8:
                    return TextureFormat.RGBA32;
                default:
                    throw new NotSupportedException(
                        $"Format >{format}< is not supported or not yet tested to work with Unity."
                    );
            }
        }
    }
}