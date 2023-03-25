using System;
using System.Runtime.InteropServices;
using UZVR.Phoenix.Vm.Gothic.Externals;

namespace UZVR.Phoenix.Bridge.Vm
{
    /// <summary>
    /// Contains basic methods only available in Gothic Daedalus module.
    /// </summary>
    public class VmGothicBridge : VmBridge
    {
        private const string DLLNAME = PhoenixBridge.DLLNAME;

        public VmGothicBridge(string datFilePath) : base(datFilePath)
        {
            RegisterDefaultCallbacks(VmPtr);
        }


#region Externals
        delegate void DefaultExternal(string value);

        [DllImport(DLLNAME)] private static extern void vmRegisterDefaultExternal(IntPtr vm, DefaultExternal callbackPointer);

        private void RegisterDefaultCallbacks(IntPtr vmPtr)
        {
            vmRegisterDefaultExternal(vmPtr, DefaultExternals.NotImplementedCallback);
        }
#endregion
    }
}
