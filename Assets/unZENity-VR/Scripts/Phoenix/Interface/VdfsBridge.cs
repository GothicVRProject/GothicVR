using PxCs;
using System;
using System.IO;

namespace UZVR.Phoenix.Interface
{
    public static class VdfsBridge
    {
        public static IntPtr LoadVdfsInDirectory(string vdfsDir)
        {
            var vdfsPtr = PxVdf.pxVdfNew("main");

            var vdfPaths = Directory.GetFiles(vdfsDir, "*.VDF");

            foreach (var vdfPath in vdfPaths)
            {
                var additionalVdf = PxVdf.pxVdfLoadFromFile(vdfPath);

                PxVdf.pxVdfMerge(vdfsPtr, additionalVdf, true);
                PxVdf.pxVdfDestroy(additionalVdf);
            }

            return vdfsPtr;
        }
    }

}