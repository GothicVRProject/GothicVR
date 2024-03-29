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
        /// cmScale - Gothic positions are in cm, but Unity in m. (factor 100). Most of the time we just transform it directly.
        /// </summary>
        public static UnityEngine.Vector3 ToUnityVector(this Vector3 vector3, bool cmScale = true)
        {
            var vector = new UnityEngine.Vector3
            {
                x = vector3.X,
                y = vector3.Y,
                z = vector3.Z
            };

            if (cmScale)
            {
                return vector / 100;
            }
            else
            {
                return vector;
            }
        }

        public static UnityEngine.Bounds ToUnityBounds(this ZenKit.AxisAlignedBoundingBox bounds)
        {
            UnityEngine.Vector3 max = bounds.Max.ToUnityVector();
            UnityEngine.Vector3 min = bounds.Min.ToUnityVector();

            UnityEngine.Vector3 boundsChord = max  - min;
            UnityEngine.Bounds unityBounds = new UnityEngine.Bounds(min + boundsChord.normalized * boundsChord.magnitude * .5f, 
                new UnityEngine.Vector3(UnityEngine.Mathf.Abs(max.x - min.x),
                                        UnityEngine.Mathf.Abs(max.y - min.y),
                                        UnityEngine.Mathf.Abs(max.z - min.z)));

            return unityBounds;
        }

        public static UnityEngine.Color ToUnityColor(this Vector3 vector3, float alpha = 1)
        {
            return new UnityEngine.Color()
            {
                r = vector3.X,
                g = vector3.Y,
                b = vector3.Z,
                a = alpha
            };
        }

        public static UnityEngine.Color ToUnityColor(this UnityEngine.Vector3 vector3, float alpha = 1)
        {
            return new UnityEngine.Color()
            {
                r = vector3.x,
                g = vector3.y,
                b = vector3.z,
                a = alpha
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
