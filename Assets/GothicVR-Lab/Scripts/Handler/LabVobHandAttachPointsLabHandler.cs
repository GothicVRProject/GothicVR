using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GVR.Caches;
using GVR.Creator;
using GVR.Creator.Meshes.V2;
using GVR.Data;
using GVR.Extensions;
using GVR.Globals;
using GVR.Vm;
using GVR.Vob;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using ZenKit.Daedalus;

namespace GVR.Lab.Handler
{
    public class LabVobHandAttachPointsLabHandler: MonoBehaviour, ILabHandler
    {
        public TMP_Dropdown vobCategoryDropdown;
        public TMP_Dropdown vobItemDropdown;
        public GameObject itemSpawnSlot;
        public Slider SliderPosX;
        public Slider SliderPosY;
        public Slider SliderPosZ;
        public Slider SliderRotX;
        public Slider SliderRotY;
        public Slider SliderRotZ;
        public Toggle dynamicAttachToggle;
        public TMP_Text copiedItemText;
        public TMP_Text currentItemText;

        private XRGrabInteractable xrGrabComp;

        private string currentItemName;
        private Transform attachPoint1;
        private Transform attachPoint2;

        private Dictionary<string, ItemInstance> items = new();
        private VobItemAttachPoints attachPoints;

        public void Bootstrap()
        {
            /*
             * 1. Load Vdfs
             * 2. Load VobItemAttachPoints json
             * 3. Load Vob name list
             * 4. Fill dropdown
             */
            var itemNames = GameData.GothicVm.GetInstanceSymbols("C_Item").Select(i => i.Name).ToList();

            items = itemNames
                .ToDictionary(itemName => itemName, AssetCache.TryGetItemData);

            vobCategoryDropdown.options = items
                .Select(item => ((VmGothicEnums.ItemFlags)item.Value.MainFlag).ToString())
                .Distinct()
                .Select(flag => new TMP_Dropdown.OptionData(flag))
                .ToList();

            CategoryDropdownValueChanged();

            var attachPointJson = Resources.Load<TextAsset>("Configuration/VobItemAttachPoints");
            attachPoints = JsonUtility.FromJson<VobItemAttachPoints>(attachPointJson.text);
        }

        public void CategoryDropdownValueChanged()
        {
            Enum.TryParse<VmGothicEnums.ItemFlags>(vobCategoryDropdown.options[vobCategoryDropdown.value].text, out var category);
            var items = this.items.Where(item => item.Value.MainFlag == (int)category).ToList();
            vobItemDropdown.options = items.Select(item => new TMP_Dropdown.OptionData(item.Key)).ToList();
        }

        public void LoadVobOnClick()
        {
            // We want to have one element only.
            if (itemSpawnSlot.transform.childCount != 0)
                Destroy(itemSpawnSlot.transform.GetChild(0).gameObject);

            StartCoroutine(LoadVobOnClickDelayed());
        }

        private IEnumerator LoadVobOnClickDelayed()
        {
            // Wait 1 frame for GOs to be destroyed.
            yield return null;

            currentItemName = vobItemDropdown.options[vobItemDropdown.value].text;
            var item = CreateItem(currentItemName);

            attachPoint1 = item.FindChildRecursively("AttachPoint1").transform;
            attachPoint2 = item.FindChildRecursively("AttachPoint2").transform;

            SetInitialItemValue();
        }

        private GameObject CreateItem(string itemName)
        {
            var itemPrefab = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobItem);
            var item = AssetCache.TryGetItemData(itemName);
            var mrm = AssetCache.TryGetMrm(item.Visual);
            var itemGo = MeshFactory.CreateVob(item.Visual, mrm, default, default, true, rootGo: itemPrefab, parent: itemSpawnSlot);

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

        public void SliderButtonPosMinus1Clicked(Slider slider) { slider.value -= 0.001f; }
        public void SliderButtonPosMinus2Clicked(Slider slider) { slider.value -= 0.01f; }
        public void SliderButtonPosMinus3Clicked(Slider slider) { slider.value -= 0.1f; }

        public void SliderButtonPosPlus1Clicked(Slider slider) { slider.value += 0.001f; }
        public void SliderButtonPosPlus2Clicked(Slider slider) { slider.value += 0.01f; }
        public void SliderButtonPosPlus3Clicked(Slider slider) { slider.value += 0.1f; }

        public void SliderResetClicked(Slider slider) { slider.value = 0f; }

        public void SliderPositionValueChanged()
        {
            attachPoint1.localPosition = new Vector3(SliderPosX.value, SliderPosY.value, SliderPosZ.value);
            SliderPosX.gameObject.FindChildRecursively("Value Text").GetComponent<TMP_Text>().text = SliderPosX.value.ToString();
            SliderPosY.gameObject.FindChildRecursively("Value Text").GetComponent<TMP_Text>().text = SliderPosY.value.ToString();
            SliderPosZ.gameObject.FindChildRecursively("Value Text").GetComponent<TMP_Text>().text = SliderPosZ.value.ToString();
        }

        public void SliderButtonRotMinus1Clicked(Slider slider) { slider.value -= 0.01f; }
        public void SliderButtonRotMinus2Clicked(Slider slider) { slider.value -= 0.1f; }
        public void SliderButtonRotMinus3Clicked(Slider slider) { slider.value -= 1f; }

        public void SliderButtonRotPlus1Clicked(Slider slider) { slider.value += 0.01f; }
        public void SliderButtonRotPlus2Clicked(Slider slider) { slider.value += 0.1f; }
        public void SliderButtonRotPlus3Clicked(Slider slider) { slider.value += 1f; }

        public void SliderRotationValueChanged()
        {
            attachPoint1.localRotation = Quaternion.Euler(SliderRotX.value, SliderRotY.value, SliderRotZ.value);
            SliderRotX.gameObject.FindChildRecursively("Value Text").GetComponent<TMP_Text>().text = SliderRotX.value.ToString();
            SliderRotY.gameObject.FindChildRecursively("Value Text").GetComponent<TMP_Text>().text = SliderRotY.value.ToString();
            SliderRotZ.gameObject.FindChildRecursively("Value Text").GetComponent<TMP_Text>().text = SliderRotZ.value.ToString();
        }


        private void SetInitialItemValue()
        {
            var savedPoint = attachPoints.points.FirstOrDefault(p => p.name == currentItemName);
            if (savedPoint != null)
            {
                attachPoint1.localPosition = savedPoint.position;
                attachPoint1.localRotation = Quaternion.Euler(savedPoint.rotation);
                xrGrabComp.useDynamicAttach = savedPoint.isDynamicAttach;
            }

            var pos1 = attachPoint1.localPosition;
            var rot1 = attachPoint1.localRotation.eulerAngles;

            SliderPosX.value = pos1.x;
            SliderPosY.value = pos1.y;
            SliderPosZ.value = pos1.z;
            SliderRotX.value = rot1.x;
            SliderRotY.value = rot1.y;
            SliderRotZ.value = rot1.z;

            dynamicAttachToggle.isOn = xrGrabComp.useDynamicAttach;

            ItemValueChanged();
        }

        public void ItemValueChanged()
        {
            currentItemText.text = $"Name: {currentItemName} \n";
            currentItemText.text += $"Position: {SliderPosX.value}, {SliderPosY.value}, {SliderPosZ.value}\n";
            currentItemText.text += $"Rotation: {SliderRotX.value}, {SliderRotY.value}, {SliderRotZ.value}";
        }

        public void DynamicAttachToggleValueChanged()
        {
            xrGrabComp.useDynamicAttach = dynamicAttachToggle.isOn;
        }

        private Vector3 copyPosition1;
        private Vector3 copyRotation1;
        private bool copyDynamicAttach;

        public void CopyCurrentItemClick()
        {
            copyPosition1 = attachPoint1.localPosition;
            copyRotation1 = attachPoint1.localRotation.eulerAngles;
            copyDynamicAttach = dynamicAttachToggle.isOn;

            copiedItemText.text = $"Name: {currentItemName} \n";
            copiedItemText.text += $"Is Dynamic Attach: {dynamicAttachToggle.isOn} \n";
            copiedItemText.text += $"Position: {copyPosition1.x}, {copyPosition1.y}, {copyPosition1.z}\n";
            copiedItemText.text += $"Rotation: {copyRotation1.x}, {copyRotation1.y}, {copyRotation1.z}";
        }

        public void ApplyCopyItemClick()
        {
            if (copyPosition1 == default && copyRotation1 == default)
                return;

            attachPoint1.localPosition = copyPosition1;
            attachPoint1.localRotation = Quaternion.Euler(copyRotation1);
            dynamicAttachToggle.isOn = copyDynamicAttach;

            SetInitialItemValue();
        }

        public void SaveVobOnClick()
        {
            var item = attachPoints.points.FirstOrDefault(p => p.name == currentItemName);
            if (item == null)
            {
                attachPoints.points.Add(new()
                {
                    name = currentItemName,
                    isDynamicAttach = dynamicAttachToggle.isOn,
                    position = attachPoint1.localPosition,
                    rotation = attachPoint1.localRotation.eulerAngles
                });
            }
            else
            {
                item.isDynamicAttach = dynamicAttachToggle.isOn;
                item.position = attachPoint1.localPosition;
                item.rotation = attachPoint1.localRotation.eulerAngles;
            }

            var content = JsonUtility.ToJson(attachPoints, true);
            File.WriteAllText($"{Application.dataPath}/GothicVR/Resources/Configuration/VobItemAttachPoints.json", content);
        }
    }
}
