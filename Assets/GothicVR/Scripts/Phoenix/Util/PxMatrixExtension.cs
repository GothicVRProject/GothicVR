using PxCs.Data.Struct;

namespace GVR.Phoenix.Util
{
    public static class PxMatrixExtension
    {
        public static UnityEngine.Matrix4x4 ToUnityMatrix(this PxMatrix4x4Data matrix)
        {
            return new()
            {
                // Check if column with row switch helps.
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


                //m00 = matrix.m00,
                //m01 = matrix.m01,
                //m02 = matrix.m02,
                //m03 = matrix.m03,

                //m10 = matrix.m10,
                //m11 = matrix.m11,
                //m12 = matrix.m12,
                //m13 = matrix.m13,

                //m20 = matrix.m20,
                //m21 = matrix.m21,
                //m22 = matrix.m22,
                //m23 = matrix.m23,

                //m30 = matrix.m30,
                //m31 = matrix.m31,
                //m32 = matrix.m32,
                //m33 = matrix.m33
            };
        }
    }
}