using System;
using System.IO;
using System.Linq;

namespace VdfsSharp
{
    /// <summary>
    /// Specifies how to extract files.
    /// </summary>
    public enum ExtractOption
    {
        /// <summary>
        /// Extract files with directories structure.
        /// </summary>
        Hierarchy,

        /// <summary>
        /// Extract files without directories structure.
        /// </summary>
        NoHierarchy
    }

    /// <summary>
    /// Provides extracting files from VDFS archive.
    /// </summary>
    public class VdfsExtractor : IDisposable
    {
        readonly VdfsReader _vdfsReader;

        /// <summary>
        /// Initializes a new instance of the <see cref="VdfsExtractor"/> class.
        /// </summary>
        /// <param name="vdfFilePath">Path of VDFS archive.</param>
        public VdfsExtractor(string vdfFilePath)
        {
            _vdfsReader = new VdfsReader(vdfFilePath);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VdfsExtractor"/> class.
        /// </summary>
        /// <param name="vdfsReader"> An instance of <see cref="VdfsReader"/>. </param>
        public VdfsExtractor(VdfsReader vdfsReader)
        {
            _vdfsReader = vdfsReader;
        }

        /// <summary>
        /// Releases all resources used by the current instance of the <see cref="VdfsExtractor"/> class.
        /// </summary>
        public void Dispose()
        {
            _vdfsReader.Dispose();
        }

        /// <summary>
        /// Extracts specific file.
        /// </summary>
        public void ExtractFile(string fileName, string outputFile)
        {
            var entry = _vdfsReader.ReadEntries(false).Where(x => x.Name == fileName).First();

            entry.Content = _vdfsReader.ReadEntryContent(entry);

            entry.SaveToFile(outputFile);
        }

        /// <summary>
        /// Extracts all entries to directory.
        /// </summary>
        /// <param name="outputDirectory">Directory to extract files.</param>
        /// <param name="options">Specifies extracting options. </param>
        public void ExtractFiles(string outputDirectory, ExtractOption options)
        {
            var entries = _vdfsReader.ReadEntries(true).ToArray();

            if (options == ExtractOption.Hierarchy)
            {
                var tree = new VdfsEntriesTreeGenerator(entries).Generate();

                saveFiles(tree, outputDirectory);
            }
            else
            {
                saveFiles(entries, outputDirectory);
            }                 
        }

        private void saveFiles(VdfsEntry[] entries, string outputDirectory)
        {
            Directory.CreateDirectory(outputDirectory);

            foreach (var entry in entries.Where(x => x.Type.HasFlag(Vdfs.EntryType.Directory) == false))
            {
                var output = Path.Combine(outputDirectory, entry.Name);

                entry.SaveToFile(output);
            }
        }

        private void saveFiles(VdfsEntriesTree tree, string outputDirectory)
        {
            Directory.CreateDirectory(outputDirectory);

            for (int i = 0; i < tree.Childrens.Count; i++)
            {
                var output = Path.Combine(outputDirectory, tree.Childrens[i].Entry.Name);

                if (tree.Childrens[i].Entry.Type.HasFlag(Vdfs.EntryType.Directory))
                {
                    saveFiles(tree.Childrens[i], output);
                }
                else
                {
                    tree.Childrens[i].Entry.SaveToFile(output);
                }
            }
        }
    }
}
