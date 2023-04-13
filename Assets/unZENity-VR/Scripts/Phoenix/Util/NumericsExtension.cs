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

        /// <summary>
        /// Transform Vector3 to UnitVector3.
        /// optional: Adjust value by factor 100 if it's a position from original gothic.
        /// </summary>
        public static UnityEngine.Vector3 ToUnityVector(this Vector3 vector3, bool correctGothicFactor = true)
        {
            var vector = new UnityEngine.Vector3()
            {
                x = vector3.X,
                y = vector3.Y,
                z = vector3.Z
            };

            // Gothic positions are too big for Unity. (factor 100)
            if (correctGothicFactor)
                return vector / 100;
            else
                return vector;
        }

    }
}
