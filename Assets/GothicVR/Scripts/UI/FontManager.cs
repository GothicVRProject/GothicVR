using System;
using System.Reflection;
using GVR.Caches;
using GVR.Util;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore;
using Constants = GVR.Globals.Constants;

namespace GVR.Manager
{
    public class FontManager : SingletonBehaviour<FontManager>
    {
        public TMP_FontAsset DefaultFont;

        public void Create()
        {
            TMP_Settings.defaultSpriteAsset = LoadFont("font_old_20_white.FNT");
            TMP_Settings.defaultFontAsset = DefaultFont;
        }

        public TMP_SpriteAsset LoadFont(string fontName)
        {
            if (LookupCache.fontCache.TryGetValue(fontName.ToUpper(), out TMP_SpriteAsset data))
                return data;
            var font = AssetCache.TryGetFont(fontName.ToUpper());
            var fontTexture = TextureCache.TryGetTexture(fontName);

            TMP_SpriteAsset spriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();

            for (int i = 0; i < font.Glyphs.Count; i++)
            {
                var x = font.Glyphs[i].topLeft.X * fontTexture.width;
                x = x < 0 ? 0 : x;
                var y = font.Glyphs[i].topLeft.Y * fontTexture.height;
                var w = font.Glyphs[i].width;
                var h = font.Height;
                Sprite newSprite = Sprite.Create(fontTexture, new Rect(x, y, w, h), new Vector2(font.Glyphs[i].topLeft.X, font.Glyphs[i].bottomRight.Y));

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
                        height = -h,
                        horizontalBearingY = 0,
                        horizontalBearingX = 0,
                        horizontalAdvance = w
                    },
                    index = (uint)i,
                    sprite = newSprite,
                    scale = -1
                };
                spriteAsset.spriteGlyphTable.Add(spriteGlyph);
                var spriteCharacter = new TMP_SpriteCharacter((uint)i, spriteGlyph);
                spriteAsset.spriteCharacterTable.Add(spriteCharacter);
            }
            spriteAsset.name = name;
            spriteAsset.material = GetDefaultSpriteMaterial(fontTexture);
            spriteAsset.spriteSheet = fontTexture;

            // Get the Type of the TMP_SpriteAsset
            Type spriteAssetType = spriteAsset.GetType();

            // Get the FieldInfo of the 'm_Version' field
            FieldInfo versionField = spriteAssetType.GetField("m_Version", BindingFlags.NonPublic | BindingFlags.Instance);

            versionField.SetValue(spriteAsset, "1.0.0"); // setting this as to skip "UpgradeSpriteAsset"

            spriteAsset.UpdateLookupTables();

            LookupCache.fontCache[fontName.ToUpper()] = spriteAsset;

            return spriteAsset;
        }

        static Material GetDefaultSpriteMaterial(Texture2D spriteSheet = null)
        {
            ShaderUtilities.GetShaderPropertyIDs();

            // Add a new material
            var shader = Constants.ShaderTMPSprite;
            var tempMaterial = new Material(shader);
            tempMaterial.SetTexture(ShaderUtilities.ID_MainTex, spriteSheet);

            return tempMaterial;
        }
    }
}
