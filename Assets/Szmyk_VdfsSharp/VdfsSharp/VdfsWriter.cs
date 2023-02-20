using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace VdfsSharp
{
    public enum GothicVersion
    {
        Gothic1,
        Gothic2
    }

    public class VdfsWriter : IDisposable
    {
        VdfsHeader _header = new VdfsHeader();

        BinaryWriter _writer;

        public List<VdfsEntry> Entries = new List<VdfsEntry>();

        GothicVersion _gothicVersion;

        public VdfsWriter(string path, string comment, GothicVersion gothicVersion)
        {
            var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);

            _writer = new BinaryWriter(fileStream);

            _header.Comment = comment;

            _gothicVersion = gothicVersion;
        }

        public void AddDirectories(string[] paths, string searchPattern)
        {
            foreach (var path in paths)
            {
                Entries.Add(new VdfsEntry()
                {
                    Name = Path.GetFileName(path).ToUpper(),
                    Type = Vdfs.EntryType.Directory,
                    Attributes = 0,
                    Size = 0
                });
            }

            Entries.Last().Type = Entries.Last().Type | Vdfs.EntryType.Last;

            for (int i = 0; i < paths.Length; i++)
            {
                Entries.ElementAt(i).Offset = (uint)(Entries.Count);

                AddDirectory(paths[i], searchPattern);
            }          
        }

        public void AddDirectory(string path)
        {
            AddDirectory(path, "*");
        }

        public void AddDirectory(string path, string searchPattern)
        {
            var directories = Directory.GetDirectories(path, "*", SearchOption.TopDirectoryOnly).Where((x) =>
            {
                return Directory.GetFiles(x, searchPattern, SearchOption.AllDirectories).Length > 0;
            }).ToArray();

            var files = Directory.GetFiles(path, searchPattern, SearchOption.TopDirectoryOnly);

            uint offset = (uint)(Entries.Count + directories.Length + files.Length);

            for (int i = 0; i < directories.Length; i++)
            {
                var filesInside = Directory.GetFiles(directories[i], searchPattern, SearchOption.AllDirectories).Length;

                if (filesInside == 0)
                {
                    continue;
                }

                Entries.Add(new VdfsEntry()
                {
                    Name = Path.GetFileName(directories[i]).ToUpper(),
                    Type = Vdfs.EntryType.Directory,
                    Attributes = 0,
                    Size = 0,
                    Offset = offset
                });

                var directoriesInside = Directory.GetDirectories(directories[i], "*", SearchOption.AllDirectories).Length;

                offset += (uint)(filesInside + directoriesInside);
            }

            for (int i = 0; i < files.Length; i++)
            {
                var content = File.ReadAllBytes(files[i]);

                var entry = new VdfsEntry()
                {
                    Name = Path.GetFileName(files[i]).ToUpper(),
                    Content = content,
                    Size = (uint)content.Length
                };

                var fileAttributes = File.GetAttributes(files[i]);

                if (fileAttributes.HasFlag(FileAttributes.Archive))
                {
                    entry.Attributes = entry.Attributes | Vdfs.FileAttribute.Archive;
                }

                Entries.Add(entry);
            }

            if (Entries.Count > 0)
            {
                Entries.Last().Type = Entries.Last().Type | Vdfs.EntryType.Last;
            }

            for (int i = 0; i < directories.Length; i++)
            {
                if (Directory.GetFiles(directories[i], searchPattern, SearchOption.AllDirectories).Length > 0)
                {
                    AddDirectory(directories[i], searchPattern);
                }
            }
        }

        private string fillWithCharacter(string text, int length, char filler)
        {
            return text.PadRight(length, filler);
        }

        private int toDosDateTime(DateTime dateTime)
        {
            var years = dateTime.Year - 1980;
            var months = dateTime.Month;
            var days = dateTime.Day;
            var hours = dateTime.Hour;
            var minutes = dateTime.Minute;
            var seconds = dateTime.Second;

            var date = ( years << 9 ) | ( months << 5 ) | days;
            var time = ( hours << 11 ) | ( minutes << 5 ) | ( seconds << 1 );

            return ( date << 16 ) | time;
        }

        public void Save()
        {
            _writer.BaseStream.Position = 0;

            _writer.Write(Encoding.ASCII.GetBytes(fillWithCharacter(_header.Comment, 256, Vdfs.CommentFillByte)), 0, 256);

            if (_gothicVersion == GothicVersion.Gothic1)
            {
                _writer.Write(Encoding.ASCII.GetBytes(Vdfs.VersionGothic1), 0, 16);
            }
            else if (_gothicVersion == GothicVersion.Gothic2)
            {
                _writer.Write(Encoding.ASCII.GetBytes(Vdfs.VersionGothic2), 0, 16);
            }

            _writer.Write(BitConverter.GetBytes(Entries.Count), 0, 4);
            _writer.Write(BitConverter.GetBytes(Entries.Where(x => x.Type != Vdfs.EntryType.Directory).ToList().Count), 0, 4);
            _writer.Write(BitConverter.GetBytes(toDosDateTime(DateTime.Now)), 0, 4);
            _writer.Write(BitConverter.GetBytes(Entries.Where(x => x.Type != Vdfs.EntryType.Directory && x.Content != null).Sum(x => x.Content.Length)), 0, 4);
            _writer.Write(BitConverter.GetBytes(_writer.BaseStream.Position + 8), 0, 4);
            _writer.Write(BitConverter.GetBytes(80), 0, 4);

            var position = _writer.BaseStream.Position + ( Entries.Count * 80 );

            foreach (var entry in Entries)
            {
                _writer.Write(Encoding.ASCII.GetBytes(fillWithCharacter(entry.Name.ToUpper(), 64, ' ')), 0, 64);
                
                if (entry.Content == null)
                {
                    _writer.Write(BitConverter.GetBytes(entry.Offset), 0, 4);
                    _writer.Write(BitConverter.GetBytes(0), 0, 4);
                }
                else
                {               
                    _writer.Write(BitConverter.GetBytes(position), 0, 4);
                    _writer.Write(BitConverter.GetBytes(entry.Content.Length), 0, 4);

                    position += entry.Content.Length;
                }

                _writer.Write(BitConverter.GetBytes((int)entry.Type), 0, 4);
                _writer.Write(BitConverter.GetBytes((int)entry.Attributes), 0, 4);
            }

            foreach (var entry in Entries.Where(x => x.Content != null))
            {
                _writer.Write(entry.Content, 0, entry.Content.Length);
            }
        }

        public void Dispose()
        {
            _writer.Dispose();
        }
    }
}
