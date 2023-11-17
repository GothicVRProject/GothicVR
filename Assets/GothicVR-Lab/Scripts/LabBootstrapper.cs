using System;
using System.IO;
using GVR.Bootstrap;
using GVR.Manager.Settings;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Interface.Vm;
using UnityEngine;

namespace GVR.GothicVR_Lab.Scripts
{
    public class LabBootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            PhoenixBootstrapper.SetLanguage();

            var vdfsPath = Path.GetFullPath($"{SettingsManager.GameSettings.GothicIPath}/Data");
            GameData.VfsPtr = VfsBridge.LoadVfsInDirectory(vdfsPath);

            var gothicVmPath = Path.GetFullPath($"{SettingsManager.GameSettings.GothicIPath}/_work/DATA/scripts/_compiled/GOTHIC.DAT");
            GameData.VmGothicPtr = VmGothicExternals.LoadVm(gothicVmPath);
        }
    }
}
