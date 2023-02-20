using System;

namespace VdfsSharp
{
    /// <summary>
    /// Represents VDFS archive header.
    /// </summary>
    public struct VdfsHeader
    {
        /// <summary>
        /// Gets or sets the name of entry.
        /// </summary>
        public string Comment;

        /// <summary>
        /// Gets or set the signature of archive.
        /// </summary>
        public string Signature;

        /// <summary>
        /// Gets or set the number of all entries in archive.
        /// </summary>
        public uint EntryCount;

        /// <summary>
        /// Gets or set the number of files entries in archive.
        /// </summary>
        public uint FileCount;

        /// <summary>
        /// Gets or set the time in which the archive was built.
        /// </summary>
        public DateTime TimeStamp;

        /// <summary>
        /// Gets or set the size of archive.
        /// </summary>
        public uint DataSize;

        /// <summary>
        /// Gets or set the offset at which entries table starts.
        /// </summary>
        public uint RootOffset;

        /// <summary>
        /// Gets or set the size in bytes of entry header.
        /// </summary>
        public int EntrySize;
    }
}
