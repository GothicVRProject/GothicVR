using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UZVR
{
    public class DllImportTest : MonoBehaviour
    {
        [DllImport("test_lib")]
        private static extern IntPtr getVDFHeader();
        [DllImport("test_lib")]
        private static extern string getHeaderComment(IntPtr vdfHeader);

        void Start()
        {
            IntPtr vdfHeader = getVDFHeader();
            var comment = getHeaderComment(vdfHeader);

            Debug.Log(comment);
        }
    }
}