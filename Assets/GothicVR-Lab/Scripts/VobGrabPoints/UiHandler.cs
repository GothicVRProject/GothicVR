using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GVR.Bootstrap;
using GVR.Caches;
using GVR.Creator;
using GVR.Creator.Meshes;
using GVR.Extensions;
using GVR.Manager.Settings;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Interface.Vm;
using GVR.Vob;
using PxCs.Data.Vob;
using PxCs.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace GVR.Lab.VobGrabPoints
{
    public class UiHandler: MonoBehaviour
    {
        public TMP_Dropdown vobDropdown;
        public GameObject itemSpawnSlot;

        public void InitializeOnClick()
        {
            /*
             * 1. Load Vdfs
             * 2. Load Vob name list
             * 3. Fill dropdown
             */
            PhoenixBootstrapper.SetLanguage();

            var vdfsPath = Path.GetFullPath($"{SettingsManager.GameSettings.GothicIPath}/Data");
            GameData.VfsPtr = VfsBridge.LoadVfsInDirectory(vdfsPath);

            var gothicVmPath = Path.GetFullPath($"{SettingsManager.GameSettings.GothicIPath}/_work/DATA/scripts/_compiled/GOTHIC.DAT");
            GameData.VmGothicPtr = VmGothicExternals.LoadVm(gothicVmPath);

            List<string> itemNames = new();
            PxVm.pxVmEnumerateInstancesByClassName(GameData.VmGothicPtr, "C_Item", (string name) => itemNames.Add(name));
            vobDropdown.options = itemNames.Select(name => new TMP_Dropdown.OptionData(name)).ToList();
        }

        public void LoadVobOnClick()
        {
            var itemName = vobDropdown.options[vobDropdown.value].text;
            var item = CreateItem(itemName);
        }


        private GameObject CreateItem(string itemName)
        {
            var pxItem = AssetCache.TryGetItemData(itemName);
            var mrm = AssetCache.TryGetMrm(pxItem.visual);
            var item = VobMeshCreator.Create(pxItem.visual, mrm, default, default, true, parent: itemSpawnSlot);

            var grabComp = item.AddComponent<XRGrabInteractable>();
            var eventComp = item.GetComponent<ItemGrabInteractable>();
            var colliderComp = item.GetComponent<MeshCollider>();
            colliderComp.convex = true;

            return gameObject;
        }

        public void SaveVobOnClick()
        {
            Debug.Log("Save Vob");
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
        }
    }
}
