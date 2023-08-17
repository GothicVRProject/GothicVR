using PxCs.Interface;
using System;
using System.IO;

namespace GVR.Phoenix.Interface
{
    public static class VfsBridge
    {
        public static IntPtr LoadVfsInDirectory(string vfsDir)
        {
            var vfsPaths = Directory.GetFiles(vfsDir, "*.VDF", SearchOption.AllDirectories);
            return PxVfs.LoadVfs(vfsPaths, PxVfs.PxVfsOverwriteBehavior.PxVfsOverwrite_Older);
        }
    }
}