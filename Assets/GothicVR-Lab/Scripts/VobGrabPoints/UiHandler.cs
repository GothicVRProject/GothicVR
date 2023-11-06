using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GVR.Bootstrap;
using GVR.Caches;
using GVR.Creator;
using GVR.Creator.Meshes;
using GVR.Extensions;
using GVR.Manager;
using GVR.Manager.Settings;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Interface.Vm;
using GVR.Vob;
using PxCs.Data.Vob;
using PxCs.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

namespace GVR.Lab.VobGrabPoints
{
    public class UiHandler: MonoBehaviour
    {
        public TMP_Dropdown vobDropdown;
        public GameObject itemSpawnSlot;
        public Slider SliderPosX;
        public Slider SliderPosY;
        public Slider SliderPosZ;
        public Slider SliderRotX;
        public Slider SliderRotY;
        public Slider SliderRotZ;

        private XRGrabInteractable xrGrabComp;

        public void Start()
        {
            var x = GameObject.Find("MinMax Slider");
        }

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
            // We want to have one element only.
            if (itemSpawnSlot.transform.childCount != 0)
                Destroy(itemSpawnSlot.transform.GetChild(0).gameObject);

            var itemName = vobDropdown.options[vobDropdown.value].text;
            var item = CreateItem(itemName);
        }

        private GameObject CreateItem(string itemName)
        {
            var itemPrefab = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobItem);
            var pxItem = AssetCache.TryGetItemData(itemName);
            var mrm = AssetCache.TryGetMrm(pxItem.visual);
            var itemGo = VobMeshCreator.Create(pxItem.visual, mrm, default, default, true, rootGo: itemPrefab, parent: itemSpawnSlot);

            var itemGrabComp = itemGo.GetComponent<ItemGrabInteractable>();
            var colliderComp = itemGo.GetComponent<MeshCollider>();

            // Adding it now will set some default values for collider and grabbing now.
            // Easier than putting it on a prefab and updating it at runtime (as grabbing didn't work this way out-of-the-box).
            xrGrabComp = itemGo.AddComponent<XRGrabInteractable>();

            xrGrabComp.attachTransform = itemGrabComp.attachPoint1.transform;
            xrGrabComp.secondaryAttachTransform = itemGrabComp.attachPoint2.transform;

            colliderComp.convex = true;

            return gameObject;
        }

        public void SliderPositionValueChanged()
        {
            xrGrabComp.attachTransform.localPosition = new Vector3(SliderPosX.value, SliderPosY.value, SliderPosZ.value);
            SliderPosX.gameObject.FindChildRecursively("Value Text").GetComponent<TMP_Text>().text = SliderPosX.value.ToString();
            SliderPosY.gameObject.FindChildRecursively("Value Text").GetComponent<TMP_Text>().text = SliderPosY.value.ToString();
            SliderPosZ.gameObject.FindChildRecursively("Value Text").GetComponent<TMP_Text>().text = SliderPosZ.value.ToString();
        }

        public void SliderRotationValueChanged()
        {
            xrGrabComp.attachTransform.localRotation = Quaternion.Euler(SliderRotX.value, SliderRotY.value, SliderRotZ.value);
            SliderRotX.gameObject.FindChildRecursively("Value Text").GetComponent<TMP_Text>().text = SliderRotX.value.ToString();
            SliderRotY.gameObject.FindChildRecursively("Value Text").GetComponent<TMP_Text>().text = SliderRotY.value.ToString();
            SliderRotZ.gameObject.FindChildRecursively("Value Text").GetComponent<TMP_Text>().text = SliderRotZ.value.ToString();
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
