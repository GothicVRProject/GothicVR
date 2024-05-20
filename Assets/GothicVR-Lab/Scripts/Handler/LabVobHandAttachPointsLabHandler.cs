using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Caches;
using GVR.Context;
using GVR.Creator.Meshes.V2;
using GVR.Globals;
using GVR.Vm;
using TMPro;
using UnityEngine;
using ZenKit.Daedalus;

namespace GVR.Lab.Handler
{
    public class LabVobHandAttachPointsLabHandler: MonoBehaviour, ILabHandler
    {
        public TMP_Dropdown vobCategoryDropdown;
        public TMP_Dropdown vobItemDropdown;
        public GameObject itemSpawnSlot;

        private Dictionary<string, ItemInstance> items = new();

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

            var currentItemName = vobItemDropdown.options[vobItemDropdown.value].text;
            var item = CreateItem(currentItemName);
        }

        private GameObject CreateItem(string itemName)
        {
            var itemPrefab = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobItem);
            var item = AssetCache.TryGetItemData(itemName);
            var mrm = AssetCache.TryGetMrm(item.Visual);
            var itemGo = MeshFactory.CreateVob(item.Visual, mrm, default, default, true, rootGo: itemPrefab, parent: itemSpawnSlot, useTextureArray: false);

            GVRContext.InteractionAdapter.AddItemComponent(itemGo, true);

            return gameObject;
        }
    }
}
