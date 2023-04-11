using PxCs;
using PxCs.Data;
using UnityEngine;

namespace UZVR.Util
{
    public static class PxTextureDataExtension
    {
        public static TextureFormat GetUnityTextureFormat(this PxTextureData obj)
        {
            switch (obj.format)
            {
                case PxTexture.Format.tex_dxt1:
                    return TextureFormat.DXT1;
                case PxTexture.Format.tex_dxt5:
                    return TextureFormat.DXT5;
                default:
                    return 0;
                    //throw new NotSupportedException(
                    //    "Format is not supported or not yet tested to work with Unity: "
                    //    + Enum.GetName(typeof(PxTexture.Format), obj.format)
                    //);
            }
        }
    }
}
