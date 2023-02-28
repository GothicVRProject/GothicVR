using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UZVR
{
    public class DllImportTest : MonoBehaviour
    {
        [DllImport("test_lib", EntryPoint = "count")]
        private static extern int count();

        void Start()
        {
            Debug.Log(count());
        }
    }
}