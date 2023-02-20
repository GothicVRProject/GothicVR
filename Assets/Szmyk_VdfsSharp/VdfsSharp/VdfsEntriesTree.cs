using System;
using System.Text;
using System.Collections.Generic;

namespace VdfsSharp
{
    /// <summary>
    /// Represents hierarchical tree of VDFS entries.
    /// </summary>
    public class VdfsEntriesTree
    {
        public readonly VdfsEntry Entry;

        public readonly List<VdfsEntriesTree> Childrens = new List<VdfsEntriesTree>();

        /// <summary>
        /// Initializes a new instance of the <see cref="VdfsEntriesTree"/> class.
        /// </summary>
        public VdfsEntriesTree()
        {
            Entry = new VdfsEntry
            {
                Name = "(root)",
                Type = Vdfs.EntryType.Directory,
                Attributes = Vdfs.FileAttribute.Hidden
            };        
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VdfsEntriesTree"/> class.
        /// </summary>
        public VdfsEntriesTree(VdfsEntry entry)
        {
            Entry = entry;
        }

        /// <summary>
        /// Adds child node.
        /// </summary>
        public VdfsEntriesTree AddChild(VdfsEntry item)
        {
            var childNode = new VdfsEntriesTree(item);

            Childrens.Add(childNode);

            return childNode;
        }

        /// <summary>
        /// Gets hierarchical view of tree.
        /// </summary>
        public string GetTreeView()
        {
            return GetTreeView("", true, true);
        }

        /// <summary>
        /// Gets hierarchical view of tree.
        /// </summary>
        public string GetTreeView(string indent, bool isLast, bool isRoot)
        {
            var result = new StringBuilder();

            if (isRoot == false)
            {
                result.Append(indent);
                result.Append("+- ");
                result.Append(Entry.Name);
                result.Append(Environment.NewLine);

                indent += isLast ? "   " : "|  ";
            }

            for (int i = 0; i < Childrens.Count; i++)
            {
                result.Append(Childrens[i].GetTreeView(indent, i == Childrens.Count - 1, false));
            }

            return result.ToString();
        }
    }
}
