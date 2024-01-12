using System;
using System.Numerics;

namespace GVR.Extensions
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

        /// <summary>
        /// Transform Vector3 to Unity Vector3.
        /// </summary>
        public static UnityEngine.Vector3 ToUnityVector(this Vector3 vector3)
        {
            var vector = new UnityEngine.Vector3()
            {
                x = vector3.X,
                y = vector3.Y,
                z = vector3.Z
            };

            // Gothic positions are in cm, but Unity in m. (factor 100)
            return vector / 100;
        }

        public static UnityEngine.Color ToUnityColor(this Vector3 vector3)
        {
            return new UnityEngine.Color()
            {
                r = vector3.X,
                g = vector3.Y,
                b = vector3.Z
            };
        }

        public static UnityEngine.Color ToUnityColor(this UnityEngine.Vector3 vector3)
        {
            return new UnityEngine.Color()
            {
                r = vector3.x,
                g = vector3.y,
                b = vector3.z
            };
        }

        public static UnityEngine.Vector3[] ToUnityArray(this Vector3[] array)
        {
            return Array.ConvertAll(array, item => new UnityEngine.Vector3()
            {
                x = item.X,
                y = item.Y,
                z = item.Z
            });
        }

        public static UnityEngine.Vector2[] ToUnityArray(this Vector2[] array)
        {
            return Array.ConvertAll(array, item => new UnityEngine.Vector2()
            {
                x = item.X,
                y = item.Y
            });
        }
    }
}
