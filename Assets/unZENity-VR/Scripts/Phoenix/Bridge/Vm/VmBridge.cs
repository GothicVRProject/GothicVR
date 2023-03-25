using System;
using System.IO;
using System.Runtime.InteropServices;

namespace UZVR.Phoenix.Bridge.Vm
{
    /// <summary>
    /// Contains basic methods available in all Daedalus modules.
    /// </summary>
    public abstract class VmBridge
    {
        public IntPtr VmPtr { get; private set; } = IntPtr.Zero;

        private const string DLLNAME = PhoenixBridge.DLLNAME;
        // Generic functions
        [DllImport(DLLNAME)] private static extern IntPtr vmCreate(string datFilePath);
        [DllImport(DLLNAME)] private static extern void vmDispose(IntPtr vm);
        [DllImport(DLLNAME)] private static extern void vmCallFunctionByName(IntPtr vm, string functionName);


        public VmBridge(string datFilePath)
        {
            _CreateVm(datFilePath);
        }

        private void _CreateVm(string datFilePath)
        {
            if (!File.Exists(datFilePath))
                throw new FileNotFoundException(datFilePath + " not found.");

            VmPtr = vmCreate(datFilePath);
        }

        public void CallFunction(string functionName)
        {
            vmCallFunctionByName(VmPtr, functionName);
        }

        ~VmBridge()
        {
            vmDispose(VmPtr);
        }
    }

}