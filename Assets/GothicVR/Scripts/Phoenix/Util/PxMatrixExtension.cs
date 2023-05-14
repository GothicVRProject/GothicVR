using PxCs.Data.Struct;

namespace GVR.Phoenix.Util
{
    public static class PxMatrixExtension
    {

        /// <summary>
        /// According to this blog post, we can transform 3x3 into 4x4 matrix:
        /// @see https://forum.unity.com/threads/convert-3x3-rotation-matrix-to-euler-angles.1086392/#post-7002275
        /// Hint 1: The matrix is transposed, i.e. we needed to change e.g. m01=[0,1] to m01=[1,0]
        /// Hint 2: m33 needs to be 1
        /// </summary>
        public static UnityEngine.Matrix4x4 ToUnityMatrix(this PxMatrix3x3Data matrix)
        {
            return new()
            {
                m00 = matrix.m00,
                m01 = matrix.m10,
                m02 = matrix.m20,

                m10 = matrix.m01,
                m11 = matrix.m11,
                m12 = matrix.m21,

                m20 = matrix.m02,
                m21 = matrix.m12,
                m22 = matrix.m22,

                m33 = 1
            };
        }


        public static UnityEngine.Matrix4x4 ToUnityMatrix(this PxMatrix4x4Data matrix)
        {
            return new()
            {
                m00 = matrix.m00,
                m01 = matrix.m10,
                m02 = matrix.m20,
                m03 = matrix.m30,

                m10 = matrix.m01,
                m11 = matrix.m11,
                m12 = matrix.m21,
                m13 = matrix.m31,

                m20 = matrix.m02,
                m21 = matrix.m12,
                m22 = matrix.m22,
                m23 = matrix.m32,

                m30 = matrix.m03,
                m31 = matrix.m13,
                m32 = matrix.m23,
                m33 = matrix.m33
            };
        }
    }
}