using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace VdfsSharp
{
    /// <summary>
    /// Provides a reader of VDFS archives.
    /// </summary>
    public class VdfsReader : IDisposable
    {
        /// <summary>
        /// Gets or sets the header of archive.
        /// </summary>
        public readonly VdfsHeader Header;

        readonly BinaryReader _reader;

        /// <summary>
        /// Initializes a new instance of the <see cref="VdfsReader"/> class.
        /// </summary>
        public VdfsReader(string vdfFilePath)
        {
            var fileStream = new FileStream(vdfFilePath, FileMode.Open, FileAccess.Read);

            _reader = new BinaryReader(fileStream);

            Header = readHeader();
        }

        /// <summary>
        /// Reads all entries from archive and return as <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <param name="readContent">Specifies whether to read entry content.</param>
        public IEnumerable<VdfsEntry> ReadEntries(bool readContent)
        {
            _reader.BaseStream.Seek(Header.RootOffset, SeekOrigin.Begin);

            for (uint i = 0; i < Header.EntryCount; i++)
            {
                var entry = new VdfsEntry()
                {
                    Name = decodeBytesToString(_reader.ReadBytes(64)).TrimEnd(' '),
                    Offset = _reader.ReadUInt32(),
                    Size = _reader.ReadUInt32(),
                    Type = (Vdfs.EntryType)_reader.ReadUInt32(),
                    Attributes = (Vdfs.FileAttribute)_reader.ReadUInt32(),
                };

                if ((entry.Type.HasFlag(Vdfs.EntryType.Directory) == false) && (readContent))
                {
                    entry.Content = ReadEntryContent(entry);
                }             

                yield return entry;
            }
        }

        /// <summary>
        /// Reads entry content from archive.
        /// </summary>
        public byte[] ReadEntryContent (VdfsEntry entry)
        {
            long positonBackup = _reader.BaseStream.Position;

            _reader.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);

            var toReturn =_reader.ReadBytes((int)entry.Size);

            _reader.BaseStream.Position = positonBackup;

            return toReturn;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="VdfsReader"/> class.
        /// </summary>
        public void Dispose()
        {
            _reader.Dispose();
        }

        /// <summary>
        /// Reads header from archive.
        /// </summary>
        private VdfsHeader readHeader()
        {
            return new VdfsHeader
            {
                Comment = decodeBytesToString(_reader.ReadBytes(256)).TrimEnd(Vdfs.CommentFillByte),
                Signature = decodeBytesToString(_reader.ReadBytes(16)),
                EntryCount = _reader.ReadUInt32(),
                FileCount = _reader.ReadUInt32(),
                TimeStamp = msDosToDateTime(_reader.ReadInt32()),
                DataSize = _reader.ReadUInt32(),
                RootOffset = _reader.ReadUInt32(),
                EntrySize = _reader.ReadInt32()
            };
        }

        /// <summary>
        /// Converts bytes array to 8-bit ASCII string.
        /// </summary>
        private string decodeBytesToString (byte[] bytes)
        {
            return Encoding.GetEncoding("iso-8859-1").GetString(bytes);
        }

        /// <summary>
        /// Converts MS DOS timestamp to <see cref="DateTime"/>
        /// </summary>
        private DateTime msDosToDateTime (long timestamp)
        {
            var seconds = (int)( timestamp & 0x1F );
            var minutes = (int)( ( timestamp & 0x7E0 ) >> 5 );
            var hours = (int)( ( timestamp & 0xF800 ) >> 11 );

            var day = (int)( ( timestamp & 0x1F0000 ) >> 16 );
            var month = (int)( ( timestamp & 0x1E00000 ) >> 21 );
            var years = (int)( ( timestamp & 0xFE000000 ) >> 25 ) + 1980;

            try
            {
                return new DateTime(years, month, day, hours, minutes, seconds);
            }
            catch
            {
                return new DateTime();
            }
        }
    }
}
