using System;
using System.IO;
using System.Runtime.InteropServices;

namespace UZVR.Phoenix
{
    public class VdfsBridge
    {      
        public IntPtr VdfsPtr
        {
            get;
            private set;
        }

        private const string DLLNAME = PhoenixBridge.DLLNAME;
        [DllImport(DLLNAME)] private static extern IntPtr createVDFContainer();
        [DllImport(DLLNAME)] private static extern void addVDFToContainer(IntPtr vdfContainer, string vdfPath);
        [DllImport(DLLNAME)] private static extern void disposeVDFContainer(IntPtr vdfContainer);


        public VdfsBridge(string vdfsDir)
        {
            VdfsPtr = createVDFContainer();

            _ParseVDFs(vdfsDir);
        }

        private void _ParseVDFs(string vdfsDir)
        {
            VdfsPtr = createVDFContainer();

            var vdfPaths = Directory.GetFiles(vdfsDir, "*.vdf");

            foreach (var vdfPath in vdfPaths)
                addVDFToContainer(VdfsPtr, vdfPath);
        }

        // TODO: Check when the class is disposed to free memory within DLL.
        // If happening too late, then free it manually earlier.
        ~VdfsBridge()
        {
            disposeVDFContainer(VdfsPtr);
            VdfsPtr = IntPtr.Zero;
        }

    }

}