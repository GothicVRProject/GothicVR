using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UZVR
{
    public class DllImportTest : MonoBehaviour
    {
        private const string DLLNAME = "phoenix-csharp-wrapper";
        private const string G1Dir = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic\\";


        [DllImport(DLLNAME)] private static extern IntPtr getVDFHeader(string vdfPath);
        [DllImport(DLLNAME)] private static extern string getHeaderComment(IntPtr vdfHeader);
        [DllImport(DLLNAME)] private static extern void disposeHeader(IntPtr vdfHeader);
        

        void Start()
        {
            var vdfPaths = GetVDFPaths();

            foreach (var vdfPath in vdfPaths)
            {
               HandleVDF(vdfPath);
            }
        }



        private string[] GetVDFPaths()
        {
            return Directory.GetFiles(G1Dir + "/Data", "*.vdf");
        }


        private void HandleVDF(string vdfPath)
        {
            IntPtr vdfHeader = getVDFHeader(vdfPath);
            var comment = getHeaderComment(vdfHeader);
            // FIXME When reading a c_str/char* from DLL, the dispose will kill Unity app. Why?
            // disposeHeader(vdfHeader);

            Debug.Log(comment);
        }
    }
}