using GVR.Util;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Caches;
using GVR.Manager;
using PxCs.Interface;
using UnityEngine;
using UnityEngine.UI;
using System;
using PxCs.Data.Vm;
using TMPro;


namespace GVR.Creator
{
    public class MenuCreator : SingletonBehaviour<MenuCreator>
    {
        private float scriptDiv = 8120f;
        private float multiplerFactor = 6f;

        public void Create(string menuName, int xOffset)
        {
            PxVmMenuData menu = new();
            try
            {
                menu = PxVm.InitializeMenu(GameData.I.VmMenuPtr, menuName);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            var root = new GameObject($"MenuRoot - {menuName}");

            root.transform.position = new Vector3(xOffset, 1501, 5);
            root.transform.rotation = Quaternion.AngleAxis(180, Vector3.right);

            var canvasGO = new GameObject("Canvas");
            canvasGO.SetParent(root, true, true);
            var canvas = canvasGO.AddComponent<Canvas>();

            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            var back_pic = new GameObject("back_pic");
            back_pic.SetParent(canvasGO, true, true);
            var image = back_pic.AddComponent<Image>();
            var back_picMaterial = TextureManager.I.GetEmptyMaterial();
            back_picMaterial.mainTexture = AssetCache.I.TryGetTexture(menu.backPic.ToUpper());

            image.material = back_picMaterial;

            var dimX = menu.dimX / scriptDiv * multiplerFactor;
            var dimY = menu.dimY / scriptDiv * multiplerFactor;

            image.rectTransform.sizeDelta = new Vector2(dimX, dimY);

            var itemRoot = new GameObject("Items");
            itemRoot.SetParent(root, true, true);

            var menuInfoX = 1000f / scriptDiv * multiplerFactor;
            var menuInfoY = 7500f / scriptDiv * multiplerFactor;

            var menuInfoXPtr = PxVm.pxScriptGetSymbolByName(GameData.I.VmMenuPtr, "MENU_INFO_X");
            var menuInfoYPtr = PxVm.pxScriptGetSymbolByName(GameData.I.VmMenuPtr, "MENU_INFO_Y");

            if (menuInfoXPtr != IntPtr.Zero && menuInfoYPtr != IntPtr.Zero)
            {
                menuInfoX = PxVm.pxScriptSymbolGetInt(menuInfoXPtr, 0) / scriptDiv * multiplerFactor;
                menuInfoY = PxVm.pxScriptSymbolGetInt(menuInfoYPtr, 0) / scriptDiv * multiplerFactor;

            }

            var extraText = new GameObject("ExtraText");
            extraText.SetParent(root, true);

            var text = extraText.AddComponent<TextMeshPro>();

            text.fontSizeMin = 1;
            text.enableAutoSizing = true;
            text.alignment = TextAlignmentOptions.Center;
            text.font = GameData.I.GothicMenuFont;

            text.rectTransform.localPosition = new Vector3(0, (dimY / 2 - menuInfoX), 0.02f);
            text.rectTransform.sizeDelta = new Vector2(dimX, (menuInfoX));

            // Debug.Log($"menuInfoX {menuInfoX} menuInfoY {menuInfoY} dimX {dimX} dimY {dimY}");
            itemRoot.transform.localPosition = new Vector3(-(dimX / 2), -(dimY / 2), 0.02f);

            foreach (var item in menu.items)
            {
                if (item != string.Empty)
                    CreateMenuItem(itemRoot, item);
            }
        }

        public void CreateMenuItem(GameObject root, string name)
        {
            PxVmMenuItemData menuItem = new();
            try
            {
                menuItem = PxVm.InitializeMenuItem(GameData.I.VmMenuPtr, name);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            var itemRoot = new GameObject($"ItemRoot - {name}");
            itemRoot.SetParent(root, true, true);

            var dimX = menuItem.dimX / scriptDiv * multiplerFactor;
            var dimY = menuItem.dimY / scriptDiv * multiplerFactor;
            var posX = menuItem.posX / scriptDiv * multiplerFactor;
            var posY = menuItem.posY / scriptDiv * multiplerFactor;

            if (menuItem.backpic != string.Empty)
            {

                Debug.Log($"Creating {name} of type {menuItem.type}");

                var canvasGO = new GameObject("Canvas");
                canvasGO.SetParent(itemRoot, true, true);
                var canvas = canvasGO.AddComponent<Canvas>();
                var back_pic = new GameObject("back_pic");
                back_pic.SetParent(canvasGO, true, true);
                var image = back_pic.AddComponent<Image>();
                var back_picMaterial = TextureManager.I.GetEmptyMaterial();
                back_picMaterial.mainTexture = AssetCache.I.TryGetTexture(menuItem.backpic.ToUpper());

                image.material = back_picMaterial;

                image.rectTransform.sizeDelta = new Vector2(dimX, dimY);
                if (name.EndsWith("2"))
                {
                    image.rectTransform.localPosition = new Vector3((posX + dimX) / 2, (posY + dimY * 1.25f) / 2, 0.01f);
                }
                else
                {
                    image.rectTransform.localPosition = new Vector3((posX + dimX) / 2, (posY + dimY * 1.25f) / 2, 0);
                }
            }
            else
            {
                if (menuItem.type == (int)PxVm.PxVmCMenuItemType.PxVmCMenuItemTypeText)
                {
                    var label = new GameObject("Label");
                    label.SetParent(itemRoot, true);

                    var text = label.AddComponent<TextMeshPro>();
                    text.text = menuItem.text[0] != string.Empty ? menuItem.text[0] : "---";

                    text.font = GameData.I.GothicMenuFont;

                    if ((menuItem.flags & (uint)PxVm.PxVmCMenuItemFlags.Centered) != 0)
                        text.alignment = TextAlignmentOptions.BaselineGeoAligned;
                    else
                    {
                        text.alignment = TextAlignmentOptions.TopLeft;
                    }

                    var defaultDimX = menuItem.dimX != -1 ? menuItem.dimX : scriptDiv;
                    var defaultDimY = menuItem.dimY != -1 ? menuItem.dimY : 750f;

                    text.rectTransform.sizeDelta = new Vector2(defaultDimX / scriptDiv * multiplerFactor, defaultDimY / scriptDiv * multiplerFactor);
                    text.rectTransform.localPosition = new Vector3((posX + (defaultDimX / scriptDiv * multiplerFactor)) / 2, posY + (defaultDimY / scriptDiv * multiplerFactor), 0);

                    text.fontSizeMin = 1;
                    text.enableAutoSizing = true;
                }
                // text.autoSizeTextContainer = true;
            }
        }
    }
}
