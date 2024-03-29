using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GVR.Extensions
{
    public static class BuiltInTypeExtension
    {
        public static bool IsEmpty(this string self)
        {
            return !self.Any();
        }

        public static bool IsEmpty<T>(this IEnumerable<T> self)
        {
            return !self.Any();
        }
        
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
        /// For huge lists and elements where data is (e.g.) structs, it makes sense to set Capacity=0,
        /// which releases the elements finally. If you have a smaller Collection with references only, it's not needed.
        ///
        /// Please judge for yourself.
        /// </summary>
        public static void ClearAndReleaseMemory<TKey>(this List<TKey> self)
        {
            self.Clear();
            self.TrimExcess(); // Sets Capacity=0 and releases the "empty" objects from memory.
        }

        /// <summary>
        /// For huge lists and elements where data is (e.g.) structs, it makes sense to set Capacity=0,
        /// which releases the elements finally. If you have a smaller Collection with references only, it's not needed.
        ///
        /// Please judge for yourself.
        /// </summary>
        public static void ClearAndReleaseMemory<TKey, TValue>(this Dictionary<TKey, TValue> self)
        {
            self.Clear();
            self.TrimExcess(); // Sets Capacity=0 and releases the "empty" objects from memory.
        }
        
        /// <summary>
        /// ZenKit delivers values mostly in cm. Convenient method to move to Meter.
        /// </summary>
        public static int ToMeter(this int cmValue)
        {
            return cmValue / 100;
        }

        /// <summary>
        /// ZenKit delivers values mostly in cm. Convenient method to move to Meter.
        /// </summary>
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
