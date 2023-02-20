using System;
using System.IO;
using UnityEngine;

namespace UZVR
{
    /// <summary>
    /// CSharp rebuild of ataulien/Zenlib
    /// </summary>
    public class ZenParser
    {
        private string path;
        private StreamReader reader;

        private ZenFile zen;

        public ZenParser(string path)
        {
            this.path = path;
            zen = new();
        }

        public void Parse()
        {
            _ReadFile();
            _ParseHeader();

            Debug.Log(zen);
        }

        private void _ReadFile()
        {
            reader = File.OpenText(path);
        }

        private void _ParseHeader()
        {
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
        }
    }
}