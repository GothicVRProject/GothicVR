using System.Numerics;

namespace UZVR.Util
{
    public static class NumericsExtension
    {
        public static UnityEngine.Vector2 ToUnityVector(this Vector2 vector2)
        {
            return new()
            {
                x = vector2.X,
                y = vector2.Y,
            };
        }

        public static UnityEngine.Vector3 ToUnityVector(this Vector3 vector3)
        {
            return new()
            {
                x = vector3.X,
                y = vector3.Y,
                z = vector3.Z
            };
        }

    }
}
