using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UZVR
{
    public static class BinaryReaderExtensions
    {
        public static String ReadLine(this BinaryReader reader)
        {
            if (reader is null) throw new ArgumentNullException(nameof(reader));
            if (reader.IsEndOfStream()) return null;

            StringBuilder sb = new StringBuilder();

            while (ReadChar(reader, out Char c))
            {
                if (c == '\r' || c == '\n')
                {
                    return sb.ToString();
                }
                else
                {
                    sb.Append(c);
                }
            }

            if (sb.Length > 0) return sb.ToString();

            return null;
        }

        private static bool ReadChar(BinaryReader reader, out Char c)
        {
            c = Char.MinValue;

            if (reader.IsEndOfStream()) return false;

            c = reader.ReadChar();
            return true;
        }

        public static Boolean IsEndOfStream(this BinaryReader reader)
        {
            return reader.BaseStream.Position == reader.BaseStream.Length;
        }
    }
}
