using PxCs.Interface;
using System;
using System.IO;

namespace GVR.Phoenix.Interface
{
    public static class VfsBridge
    {
        public static IntPtr LoadVfsInDirectory(string vfsDir)
        {
            var vfsPaths = Directory.GetFiles(vfsDir, "*.VDF");
            return PxVfs.LoadVfs(vfsPaths);
        }
    }

}