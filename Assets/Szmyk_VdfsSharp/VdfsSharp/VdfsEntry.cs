using System;
using System.IO;
using System.Linq;

namespace VdfsSharp
{
    /// <summary>
    /// Represents VDFS archive entry.
    /// </summary>
    public class VdfsEntry
    {
        /// <summary>
        /// Gets or sets the name of entry.
        /// </summary>
        public string Name;

        /// <summary>
        /// Gets or sets the index of first entry if entry is directory or offset of the first byte of content if entry is file.
        /// </summary>
        public uint Offset;

        /// <summary>
        /// Gets or sets the size in bytes of the entry content.
        /// </summary>
        public uint Size;

        /// <summary>
        /// Gets or sets the type of entry.
        /// </summary>
        public Vdfs.EntryType Type;

        /// <summary>
        /// Gets or sets the attributes of entry.
        /// </summary>
        public Vdfs.FileAttribute Attributes;

        /// <summary>
        /// Gets or sets the content of entry.
        /// </summary>
        public byte[] Content;

        /// <summary>
        /// Saves content of entry to file.
        /// </summary>
        /// <param name="path">Path of output file.</param>
        public void SaveToFile(string path)
        {
            File.WriteAllBytes(path, Content);
        }

        /// <summary>
        /// Determines whether this instance and a specified object, which must also be a <seealso cref="VdfsEntry"/>, have the same values of all properties and the same content.
        /// </summary>
        public override bool Equals(Object obj)
        {
            VdfsEntry other = obj as VdfsEntry;

            if (other == null)  
            {
                return false;
            }
            else
            {
                bool isContentEquals;

                if (Content == null && other.Content == null)
                {
                    isContentEquals = true;
                }
                else if (Content == null || other.Content == null)
                {
                    return false;
                }
                else
                {
                    isContentEquals = Content.SequenceEqual(other.Content);
                }

                return Name.Equals(other.Name)
                    && Offset.Equals(other.Offset)
                    && Type.Equals(other.Type)
                    && Size.Equals(other.Size)                    
                    && Attributes.Equals(other.Attributes)
                    && isContentEquals;
            }
        }

        /// <summary>
        /// Returns joined hash code of all properties of this instance.
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = 0;

                result = (result * 397) ^ Type.GetHashCode();
                result = (result * 397) ^ Attributes.GetHashCode();
                result = (result * 397) ^ Offset.GetHashCode();
                result = (result * 397) ^ Content.GetHashCode();
                result = (result * 397) ^ Name.GetHashCode();

                return result;
            }
        }     
    }
}
