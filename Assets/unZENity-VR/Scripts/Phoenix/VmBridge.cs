using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;

namespace UZVR.Phoenix
{
    public class VmBridge
    {
        private const string DLLNAME = "phoenix-csharp-bridge";
        private const string G1DatDir = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Gothic\\_work\\DATA\\scripts\\_compiled\\";

        [DllImport(DLLNAME)] private static extern IntPtr createVM(string datFilePath);
        [DllImport(DLLNAME)] private static extern void disposeVM(IntPtr vm);

        private IntPtr vm;


        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        delegate void Callback(int value);

        [DllImport(DLLNAME)] private static extern void registerCallback([MarshalAs(UnmanagedType.FunctionPtr)] Callback callbackPointer);
        [DllImport(DLLNAME)] private static extern void triggerCallback(int value);

        public void registerCallback()
        {
            Callback callback =
            (value) =>
            {
                Debug.Log("Progress = " + value);
            };

            registerCallback(callback);
        }

        public void callCallback(int value)
        {
            triggerCallback(value);
        }



        public VmBridge(string datFilename)
        {
            var filePath = G1DatDir + datFilename;

            if (!File.Exists(filePath))
                throw new FileNotFoundException(filePath + " not found.");

            vm = createVM(G1DatDir + datFilename);
        }

        ~VmBridge()
        {
            disposeVM(vm);
        }
    }

}