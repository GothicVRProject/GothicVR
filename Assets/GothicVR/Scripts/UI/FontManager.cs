using GVR.Phoenix.Interface;
using GVR.Util;
using System.Collections.Generic;
using TMPro;

namespace GVR.Manager
{
    public class FontManager : SingletonBehaviour<FontManager>
    {
        public Dictionary<string, TMP_FontAsset> fontDictionary = new(); // Dictionary to store font assets for each tag

        public void Create()
        {
            fontDictionary[ConstantsManager.I.MenuFontTag] = GameData.I.GothicMenuFont;
            fontDictionary[ConstantsManager.I.SubtitleFontTag] = GameData.I.GothicSubtitleFont;
        }

        public void ChangeFont()
        {
            // Get all the TextMeshPro components in the scene
            TMP_Text[] textComponents = FindObjectsOfType<TextMeshProUGUI>();

            // Change the font of the TextMeshPro components based on their tags
            foreach (TMP_Text textComponent in textComponents)
            {
                if (fontDictionary.TryGetValue(textComponent.tag, out var value))
                    textComponent.font = value;
                
                // On Main Menu text is very small if gothic font is used
                if(textComponent.tag == ConstantsManager.I.MenuFontTag && GameData.I.GothicMenuFont != null)
                {
                    textComponent.fontSize = 75;
                }
            }
        }
    }
}