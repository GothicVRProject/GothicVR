using System;
using System.IO;

namespace VdfsSharp
{
    /// <summary>
    /// Provide definition of VDFS file format.
    /// </summary>
    public static class Vdfs
    {
        /// <summary>
        /// Signature of archive header in case of Gothic 1 archive.
        /// </summary>
        public const string VersionGothic1 = "PSVDSC_V2.00\r\n\r\n";

        /// <summary>
        /// Signature of archive header in case of Gothic 2 archive.
        /// </summary>
        public const string VersionGothic2 = "PSVDSC_V2.00\n\r\n\r";

        /// <summary>
        /// Character that is used to fill up archive's comment.
        /// </summary>
        public const char CommentFillByte = '\x1A';

        /// <summary>
        /// Size of entry header in bytes.
        /// </summary>
        public const int EntrySize = 0x00000050;

        /// <summary>
        /// Maximum length of entry name in bytes.
        /// </summary>
        public const int MaxEntryNameLength = 0x40;

        /// <summary>
        /// Type of entry.
        /// </summary>
        [Flags]
        public enum EntryType : uint
        {
            Directory = 0x80000000,
            Last = 0x40000000
        }

        /// <summary>
        /// File attribute of entry.
        /// </summary>
        [Flags]
        public enum FileAttribute
        {
            Readonly = FileAttributes.ReadOnly,
            Hidden = FileAttributes.Hidden,
            System = FileAttributes.System,
            Archive = FileAttributes.Archive,
        }
    }
}
