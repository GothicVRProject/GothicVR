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
        
        public static Matrix4x4 ToUnityMatrix(this System.Numerics.Matrix4x4 matrix)
        {
            return new()
            {
                m00 = matrix.M11,
                m01 = matrix.M12,
                m02 = matrix.M13,
                m03 = matrix.M14,

                m10 = matrix.M21,
                m11 = matrix.M22,
                m12 = matrix.M23,
                m13 = matrix.M24,

                m20 = matrix.M31,
                m21 = matrix.M32,
                m22 = matrix.M33,
                m23 = matrix.M34,

                m30 = matrix.M41,
                m31 = matrix.M42,
                m32 = matrix.M43,
                m33 = matrix.M44
            };
        }

    }
}
