using System;
using System.Collections.Generic;
using UnityEngine;
using ZenKit;
using ZenKit.Util;
using TextureFormat = UnityEngine.TextureFormat;

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

        /// <summary>
        /// According to this blog post, we can transform 3x3 into 4x4 matrix:
        /// @see https://forum.unity.com/threads/convert-3x3-rotation-matrix-to-euler-angles.1086392/#post-7002275
        /// Hint: m33 needs to be 1 to work properly
        /// </summary>
        public static Quaternion ToUnityQuaternion(this Matrix3x3 matrix)
        {
            var unityMatrix = new Matrix4x4
            {
                m00 = matrix.M11,
                m01 = matrix.M12,
                m02 = matrix.M13,

                m10 = matrix.M21,
                m11 = matrix.M22,
                m12 = matrix.M23,

                m20 = matrix.M31,
                m21 = matrix.M32,
                m22 = matrix.M33,

                m33 = 1
            };

            return unityMatrix.rotation;
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

        public static BoneWeight ToBoneWeight(this List<SoftSkinWeightEntry> weights, List<int> nodeMapping)
        {
            if (weights == null)
                throw new ArgumentNullException("Weights are null.");
            if (weights.Count == 0 || weights.Count > 4)
                throw new ArgumentOutOfRangeException($"Only 1...4 weights are currently supported but >{weights.Count}< provided.");

            var data = new BoneWeight();

            for (var i = 0; i < weights.Count; i++)
            {
                var index = Array.IndexOf(nodeMapping.ToArray(), weights[i].NodeIndex);
                if (index == -1)
                    throw new ArgumentException($"No matching node index found in nodeMapping for weights[{i}].nodeIndex.");

                switch (i)
                {
                    case 0:
                        data.boneIndex0 = index;
                        data.weight0 = weights[i].Weight;
                        break;
                    case 1:
                        data.boneIndex1 = index;
                        data.weight1 = weights[i].Weight;
                        break;
                    case 2:
                        data.boneIndex2 = index;
                        data.weight2 = weights[i].Weight;
                        break;
                    case 3:
                        data.boneIndex3 = index;
                        data.weight3 = weights[i].Weight;
                        break;
                }
            }

            return data;
        }
    }
}
