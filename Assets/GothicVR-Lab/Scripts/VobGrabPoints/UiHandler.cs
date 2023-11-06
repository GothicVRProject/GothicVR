using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GVR.Manager.Settings;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Interface.Vm;
using PxCs.Interface;
using TMPro;
using UnityEngine;

namespace GVR.Lab.VobGrabPoints
{
    public class UiHandler: MonoBehaviour
    {
        public TMP_Dropdown vobDropdown;

        private IntPtr vfsPtr;
        private IntPtr vmPtr;

        public void InitializeOnClick()
        {
            /*
             * 1. Load Vdfs
             * 2. Load Vob name list
             * 3. Fill dropdown
             */

            var vdfsPath = Path.GetFullPath($"{SettingsManager.GameSettings.GothicIPath}/Data");
            vfsPtr = VfsBridge.LoadVfsInDirectory(vdfsPath);

            var gothicVmPath = Path.GetFullPath($"{SettingsManager.GameSettings.GothicIPath}/_work/DATA/scripts/_compiled/GOTHIC.DAT");
            vmPtr = VmGothicExternals.LoadVm(gothicVmPath);


            List<string> itemNames = new();
            PxVm.pxVmEnumerateInstancesByClassName(vmPtr, "C_Item", (string name) => itemNames.Add(name));


            vobDropdown.options = itemNames.Select(name => new TMP_Dropdown.OptionData(name)).ToList();
        }

        public void LoadVobOnClick()
        {
            Debug.Log("Load Vob");
        }

        public void SaveVobOnClick()
        {
            Debug.Log("Save Vob");
        }

        private void OnDestroy()
        {
            if (vfsPtr != IntPtr.Zero)
            {
                PxVfs.pxVfsDestroy(vfsPtr);
                vfsPtr = IntPtr.Zero;
            }

            if (vmPtr != IntPtr.Zero)
            {
                PxVm.pxVmDestroy(vmPtr);
                vmPtr = IntPtr.Zero;
            }
        }
    }
}
