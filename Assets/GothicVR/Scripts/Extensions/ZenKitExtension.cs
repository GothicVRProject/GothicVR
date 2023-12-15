using UnityEngine;

namespace GVR.Extensions
{
    public static class ZenKitExtension
    {
        public static TextureFormat AsUnityTextureFormat(this ZenKit.TextureFormat format)
        {
            return format switch
            {
                ZenKit.TextureFormat.Dxt1 => TextureFormat.DXT1,
                ZenKit.TextureFormat.Dxt5 => TextureFormat.DXT5,
                _ => TextureFormat.RGBA32 // Everything else we need to use uncompressed for Unity (e.g. DXT3).
            };
        }

        public static Quaternion ToUnityQuaternion(this System.Numerics.Quaternion numericsQuaternion)
        {
            return new Quaternion(numericsQuaternion.X, numericsQuaternion.Y, numericsQuaternion.Z, numericsQuaternion.W);
        }
    }
}
