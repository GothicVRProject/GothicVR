using GVR.Caches;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Interface;
using PxCs.Data.Vm;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using UnityEngine.TextCore;
using System.Reflection;

namespace GVR.Creator
{
    public class MenuCreator : SingletonBehaviour<MenuCreator>
    {
        private float scriptDiv = 8120f;
        private float multiplierFactor = 6f;

        [SerializeField] private TMP_FontAsset emptyFont;

        public void Create(string menuName, int xOffset)
        {
            PxVmMenuData menu = new PxVmMenuData();
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

            canvasGO.AddComponent<Canvas>();
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            var backPic = new GameObject("BackPic");
            backPic.SetParent(canvasGO, true, true);

            var image = backPic.AddComponent<Image>();
            var backPicMaterial = TextureManager.I.GetEmptyMaterial();
            backPicMaterial.mainTexture = AssetCache.I.TryGetTexture(menu.backPic.ToUpper());
            image.material = backPicMaterial;

            var dimX = menu.dimX / scriptDiv * multiplierFactor;
            var dimY = menu.dimY / scriptDiv * multiplierFactor;
            image.rectTransform.sizeDelta = new Vector2(dimX, dimY);

            var itemRoot = new GameObject("Items");
            itemRoot.SetParent(root, true, true);

            var menuInfoX = 1000f;
            var menuInfoY = 7500f;

            var menuInfoXPtr = PxVm.pxScriptGetSymbolByName(GameData.I.VmMenuPtr, "MENU_INFO_X");
            var menuInfoYPtr = PxVm.pxScriptGetSymbolByName(GameData.I.VmMenuPtr, "MENU_INFO_Y");

            if (menuInfoXPtr != IntPtr.Zero && menuInfoYPtr != IntPtr.Zero)
            {
                menuInfoX = PxVm.pxScriptSymbolGetInt(menuInfoXPtr, 0);
                menuInfoY = PxVm.pxScriptSymbolGetInt(menuInfoYPtr, 0);
            }

            menuInfoX = menuInfoX / scriptDiv * multiplierFactor;
            menuInfoY = menuInfoY / scriptDiv * multiplierFactor;

            if ((menu.flags & 1 << 6) != 0) //show info
            {

                var extraText = new GameObject("ExtraText");
                extraText.SetParent(root, true);
                var text = extraText.AddComponent<TextMeshPro>();

                text.fontSizeMin = 1;
                text.enableAutoSizing = true;
                text.alignment = TextAlignmentOptions.Center;
                text.font = GameData.I.GothicMenuFont;

                text.text = "Lorem ipsum dolor sit amet, consectetur adipiscing elit";

                text.rectTransform.localPosition = new Vector3(0, (dimY / 2 - menuInfoX), 0.02f);
                text.rectTransform.sizeDelta = new Vector2(dimX, (menuInfoX));
            }

            itemRoot.transform.localPosition = new Vector3(-(dimX / 2), -(dimY / 2), 0.02f);

            foreach (var item in menu.items)
            {
                if (!string.IsNullOrEmpty(item))
                    CreateMenuItem(itemRoot, item);
            }
            // LoadFont();
        }

        public void CreateMenuItem(GameObject root, string name)
        {
            PxVmMenuItemData menuItem = new PxVmMenuItemData();
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

            var dimX = menuItem.dimX / scriptDiv * multiplierFactor;
            var dimY = menuItem.dimY / scriptDiv * multiplierFactor;
            var posX = menuItem.posX / scriptDiv * multiplierFactor;
            var posY = menuItem.posY / scriptDiv * multiplierFactor;

            if (!string.IsNullOrEmpty(menuItem.backpic))
            {
                var canvasGO = new GameObject("Canvas");
                canvasGO.SetParent(itemRoot, true, true);
                canvasGO.AddComponent<Canvas>();

                var backPic = new GameObject("BackPic");
                backPic.SetParent(canvasGO, true, true);

                var image = backPic.AddComponent<Image>();
                var backPicMaterial = TextureManager.I.GetEmptyMaterial();
                backPicMaterial.mainTexture = AssetCache.I.TryGetTexture(menuItem.backpic.ToUpper());
                image.material = backPicMaterial;

                image.rectTransform.sizeDelta = new Vector2(dimX, dimY);

                if (name.EndsWith("2")) // the secong heading is the actual logo, the first is just the shadow
                {
                    image.rectTransform.localPosition = new Vector3((menuItem.posX + menuItem.dimX) / scriptDiv * multiplierFactor / 2, (menuItem.posY + menuItem.dimY * 1.25f) / scriptDiv * multiplierFactor / 2, 0.01f);
                }
                else
                {
                    image.rectTransform.localPosition = new Vector3((menuItem.posX + menuItem.dimX) / scriptDiv * multiplierFactor / 2, (menuItem.posY + menuItem.dimY * 1.25f) / scriptDiv * multiplierFactor / 2, 0);
                }
            }
            else
            {
                // Debug.Log($"Menu item {name} has flags {menuItem.flags}");
                if (menuItem.type == (int)PxVm.PxVmCMenuItemType.PxVmCMenuItemTypeText)
                {
                    var label = new GameObject("Label");
                    label.SetParent(itemRoot, true, true);

                    var text = label.AddComponent<TextMeshPro>();
                    text.text = !string.IsNullOrEmpty(menuItem.text[0]) ? menuItem.text[0] : "---";

                    text.spriteAsset = LoadFont(menuItem.fontname);
                    text.font = emptyFont;

                    if ((menuItem.flags & (uint)PxVm.PxVmCMenuItemFlags.Centered) != 0)
                        text.alignment = TextAlignmentOptions.Center;
                    else
                        text.alignment = TextAlignmentOptions.TopLeft;

                    if ((menuItem.flags & (uint)PxVm.PxVmCMenuItemFlags.OnlyIngame) != 0)
                        text.color = new Color(1, 1, 1, 0.1f);
                    // else
                    // {
                        // text.color = new Color(1, 1, 0, 1);
                    // }

                    var defaultDimX = menuItem.dimX != -1 ? menuItem.dimX : scriptDiv;
                    var defaultDimY = menuItem.dimY != -1 ? menuItem.dimY : 750f;

                    // defaultDimX = defaultDimX / scriptDiv * multiplierFactor;
                    // defaultDimY = defaultDimY / scriptDiv * multiplierFactor;

                    // Debug.Log($"Text for {name} has posX {menuItem.posX} posY {menuItem.posY} defaultDimX {defaultDimX} defaultDimY {defaultDimY}");

                    text.rectTransform.sizeDelta = new Vector2(defaultDimX / scriptDiv * multiplierFactor, defaultDimY / scriptDiv * multiplierFactor);
                    text.rectTransform.localPosition = new Vector3(((menuItem.posX + defaultDimX) / scriptDiv * multiplierFactor) / 2, (menuItem.posY + defaultDimY) / scriptDiv * multiplierFactor, 0);

                    text.fontSizeMin = 1;
                    text.enableAutoSizing = true;
                }
            }
        }

        private TMP_SpriteAsset LoadFont(string fontName)
        {
            var fontData = AssetCache.I.TryGetFont(fontName.ToUpper());

            var format = fontData.texture.format.AsUnityTextureFormat();
            var texture = new Texture2D((int)fontData.texture.width, (int)fontData.texture.height, format, (int)fontData.texture.mipmapCount, false);
            texture.name = fontData.name.ToUpper();

            for (var i = 0u; i < fontData.texture.mipmapCount; i++)
                texture.SetPixelData(fontData.texture.mipmaps[i].mipmap, (int)i);

            texture.Apply();

            TMP_SpriteAsset spriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();

            for (int i = 0; i < fontData.glyphs.Length; i++)
            {
                var x = fontData.glyphs[i].upper.X * texture.width;
                x = x < 0 ? 0 : x;
                var y = fontData.glyphs[i].upper.Y * texture.height;
                var w = fontData.glyphs[i].width;
                var h = fontData.height;
                Sprite newSprite = Sprite.Create(texture, new Rect(x, y, w, h), new Vector2(fontData.glyphs[i].upper.X, fontData.glyphs[i].lower.Y));

                var spriteGlyph = new TMP_SpriteGlyph
                {
                    glyphRect = new GlyphRect
                    {
                        width = (int)w,
                        height = (int)h,
                        x = (int)x,
                        y = (int)y
                    },
                    metrics = new GlyphMetrics
                    {
                        width = w,
                        height = h,
                        horizontalBearingY = h,
                        horizontalBearingX = 0,
                        horizontalAdvance = w
                    },
                    index = (uint)i,
                    sprite = newSprite
                };
                spriteAsset.spriteGlyphTable.Add(spriteGlyph);
                var spriteCharacter = new TMP_SpriteCharacter((uint)i, spriteGlyph);
                spriteAsset.spriteCharacterTable.Add(spriteCharacter);
            }
            spriteAsset.name = name;
            spriteAsset.material = GetDefaultSpriteMaterial(texture);
            spriteAsset.spriteSheet = texture;

            // Get the Type of the TMP_SpriteAsset
            Type spriteAssetType = spriteAsset.GetType();

            // Get the FieldInfo of the 'm_Version' field
            FieldInfo versionField = spriteAssetType.GetField("m_Version", BindingFlags.NonPublic | BindingFlags.Instance);

            versionField.SetValue(spriteAsset, "1.0.0"); // setting this as to skip "UpgradeSpriteAsset"

            spriteAsset.UpdateLookupTables();

            return spriteAsset;
        }

        Material GetDefaultSpriteMaterial(Texture2D spriteSheet = null)
        {
            ShaderUtilities.GetShaderPropertyIDs();

            // Add a new material
            Shader shader = Shader.Find("TextMeshPro/Sprite");
            Material tempMaterial = new Material(shader);
            tempMaterial.SetTexture(ShaderUtilities.ID_MainTex, spriteSheet);

            return tempMaterial;
        }
    }
}
