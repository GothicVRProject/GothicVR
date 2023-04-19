using PxCs.Data;
using System.Linq;

namespace UZVR.Phoenix.Util
{
    public static class PxMRMSubMeshDataExtension
    {
        public static int[] ToUnityTriangles(this PxMRMSubMeshData.Triangle[] triangles)
        {
            return triangles
                .SelectMany(item => new int[] { item.b, item.c, item.a })
                .ToArray();
        }
    }
}
