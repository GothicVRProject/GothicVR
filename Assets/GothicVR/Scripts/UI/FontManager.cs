using GVR.Phoenix.Interface;
using GVR.Util;
using System.Collections.Generic;
using TMPro;
using GVR.Caches;
using GVR.Phoenix.Util;
using UnityEngine;
using System;
using UnityEngine.TextCore;
using System.Reflection;

namespace GVR.Manager
{
    public class FontManager : SingletonBehaviour<FontManager>
    {

        public void Create()
        {
            TMP_Settings.defaultSpriteAsset = LoadFont("font_old_20_white.FNT");
            TMP_Settings.defaultFontAsset = GameData.I.EmptyFont;
        }

        public void ChangeFont()
        {
            // Get all the TextMeshPro components in the scene
            TMP_Text[] textComponents = FindObjectsOfType<TextMeshProUGUI>();

            foreach (TMP_Text textComponent in textComponents)
            {
                textComponent.font = GameData.I.EmptyFont;

                // texture is rotated 180 degrees
                textComponent.rectTransform.localRotation = Quaternion.Euler(180, 0, 0);
            }
        }

        public TMP_SpriteAsset LoadFont(string fontName)
        {
            if (LookupCache.I.fontCache.TryGetValue(fontName.ToUpper(), out TMP_SpriteAsset data))
                return data;
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

            LookupCache.I.fontCache[fontName.ToUpper()] = spriteAsset;

            return spriteAsset;
        }

        static Material GetDefaultSpriteMaterial(Texture2D spriteSheet = null)
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