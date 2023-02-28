using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace UZVR
{
    /// <summary>
    /// CSharp rebuild of ataulien/Zenlib
    /// </summary>
    public class ZenParser
    {
        private string path;
        private BinaryReader reader;

        private ZenFile zen;

        public ZenParser(string path)
        {
            this.path = path;
            zen = new();
        }

        public void Parse()
        {
            using (FileStream fs = File.OpenRead(path))
            using (BinaryReader rdr = new BinaryReader(fs, Encoding.ASCII))
            {
                reader = rdr;
                _ParseHeader();
                _ParseImplementationHeader();
            }

            Debug.Log(zen);
        }

        private void _ParseHeader()
        {
            var foo = new BinaryReader(reader.BaseStream);
            reader.ReadLine(); // ZenGin Archive
            zen.header.version = int.Parse(reader.ReadLine().Replace("ver ", "")); // ver X
            reader.ReadLine(); // archiver type
            zen.header.type = reader.ReadLine(); // ASCII|BINARY|BIN_SAFE
            zen.header.isSaveGame = reader.ReadLine().Replace("saveGame ", "") == "1"; // saveGame 0|1
            zen.header.date = reader.ReadLine().Replace("date ", ""); // date xyz
            zen.header.user = reader.ReadLine().Replace("user ", ""); // user x
            var end = reader.ReadLine(); // END

            if (end != "END")
                throw new Exception("Zen Header broken.");

            if (zen.header.type != "BIN_SAFE")
                throw new Exception("Only BINARY_SAFE as Zen type is implemented so far.");
        }

        private void _ParseImplementationHeader()
        {
            var version = reader.ReadUInt32();
            var objectCount = reader.ReadUInt32();
            var hashTableOffset = reader.ReadUInt32();
        }
    }
}