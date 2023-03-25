using System;
using System.IO;
using System.Runtime.InteropServices;

namespace UZVR.Phoenix.Bridge
{
    public class VdfsBridge
    {      
        public IntPtr VdfsPtr
        {
            get;
            private set;
        }

        private const string DLLNAME = PhoenixBridge.DLLNAME;
        [DllImport(DLLNAME)] private static extern IntPtr vdfCreateContainer();
        [DllImport(DLLNAME)] private static extern void vdfAddToContainer(IntPtr vdfContainer, string vdfPath);
        [DllImport(DLLNAME)] private static extern void vdfDisposeContainer(IntPtr vdfContainer);


        public VdfsBridge(string vdfsDir)
        {
            VdfsPtr = vdfCreateContainer();

            _ParseVDFs(vdfsDir);
        }

        private void _ParseVDFs(string vdfsDir)
        {
            VdfsPtr = vdfCreateContainer();

            var vdfPaths = Directory.GetFiles(vdfsDir, "*.vdf");

            foreach (var vdfPath in vdfPaths)
                vdfAddToContainer(VdfsPtr, vdfPath);
        }

        // TODO: Check when the class is disposed to free memory within DLL.
        // If happening too late, then free it manually earlier.
        ~VdfsBridge()
        {
            vdfDisposeContainer(VdfsPtr);
            VdfsPtr = IntPtr.Zero;
        }

    }

}