using System;

namespace GVR.Extensions
{
    public static class StringExtension
    {
        public static bool EqualsIgnoreCase(this string self, string other)
        {
            return self.Equals(other, StringComparison.OrdinalIgnoreCase);
        }
    }
}