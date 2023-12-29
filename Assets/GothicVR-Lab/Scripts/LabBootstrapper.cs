using System;
using System.IO;
using GVR.Caches;
using GVR.Globals;
using GVR.Manager;
using GVR.Manager.Settings;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Interface.Vm;
using PxCs.Interface;
using UnityEngine;

namespace GVR.GothicVR_Lab.Scripts
{
    public class LabBootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            GVRBootstrapper.SetLanguage();

            var vdfsPath = Path.GetFullPath($"{SettingsManager.GameSettings.GothicIPath}/Data");
            GameData.VfsPtr = VfsBridge.LoadVfsInDirectory(vdfsPath);

            var gothicVmPath = Path.GetFullPath($"{SettingsManager.GameSettings.GothicIPath}/_work/DATA/scripts/_compiled/GOTHIC.DAT");
            GameData.VmGothicPtr = VmGothicExternals.LoadVm(gothicVmPath);
        }


        private void OnDestroy()
        {
            if (GameData.VfsPtr != IntPtr.Zero)
            {
                PxVfs.pxVfsDestroy(GameData.VfsPtr);
                GameData.VfsPtr = IntPtr.Zero;
            }

            if (GameData.VmGothicPtr != IntPtr.Zero)
            {
                PxVm.pxVmDestroy(GameData.VmGothicPtr);
                GameData.VmGothicPtr = IntPtr.Zero;
            }

            GameData.FreePoints.Clear();
            LookupCache.NpcCache.Clear();
        }
    }
}
