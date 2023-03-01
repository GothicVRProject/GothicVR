using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UZVR
{
    public class DllImportTest : MonoBehaviour
    {
        [DllImport("phoenix-csharp-wrapper")]
        private static extern IntPtr getVDFHeader();
        [DllImport("phoenix-csharp-wrapper")]
        private static extern string getHeaderComment(IntPtr vdfHeader);

        void Start()
        {
            IntPtr vdfHeader = getVDFHeader();
            var comment = getHeaderComment(vdfHeader);

            Debug.Log(comment);
        }
    }
}