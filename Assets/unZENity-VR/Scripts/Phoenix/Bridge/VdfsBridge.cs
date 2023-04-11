using PxCs;
using System;
using System.IO;

namespace UZVR.Phoenix.Bridge
{
    public static class VdfsBridge
    {
        public static IntPtr LoadVdfsInDirectory(string vdfsDir)
        {
            var vdfsPtr = PxVdf.pxVdfNew("main");

            var vdfPaths = Directory.GetFiles(vdfsDir, "*.vdf");

            foreach (var vdfPath in vdfPaths)
            {
                var additionalVdf = PxVdf.pxVdfLoadFromFile(vdfPath);

                PxVdf.pxVdfMerge(vdfsPtr, additionalVdf, true);
                PxVdf.pxVdfDestroy(additionalVdf);
            }

            return vdfsPtr;
        }

        public static void DestroyVdfs(IntPtr vdfsPtr)
        {
            PxVdf.pxVdfDestroy(vdfsPtr);
        }
    }

}