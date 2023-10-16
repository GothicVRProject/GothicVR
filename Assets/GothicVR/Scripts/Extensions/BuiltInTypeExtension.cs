using System;

namespace GVR.Extensions
{
    public static class BuiltInTypeExtension
    {
        public static bool EqualsIgnoreCase(this string self, string other)
        {
            return self.Equals(other, StringComparison.OrdinalIgnoreCase);
        }
        
        public static bool StartsWithIgnoreCase(this string self, string other)
        {
            return self.StartsWith(other, StringComparison.OrdinalIgnoreCase);
        }

        public static bool ContainsIgnoreCase(this string self, string other)
        {
            return self.Contains(other, StringComparison.OrdinalIgnoreCase);
        }

        public static bool EndsWithIgnoreCase(this string self, string other)
        {
            return self.EndsWith(other, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Phoenix delivers values mostly in cm. Convenient method to move to Meter.
        /// </summary>
        public static int ToMeter(this int cmValue)
        {
            return cmValue / 100;
        }

        public static float ToMeter(this float cmValue)
        {
            return cmValue / 100;
        }
        
        public static int ToCentimeter(this int cmValue)
        {
            return cmValue * 100;
        }
        
        public static float ToCentimeter(this float cmValue)
        {
            return cmValue * 100;
        }
    }
}