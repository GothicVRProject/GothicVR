using GVR.Phoenix.Interface;
using GVR.Util;
using System.Collections.Generic;
using TMPro;

namespace GVR.Manager
{
    public class FontManager : SingletonBehaviour<FontManager>
    {
        public Dictionary<string, TMP_FontAsset> fontDictionary = new Dictionary<string, TMP_FontAsset>(); // Dictionary to store font assets for each tag

        public void Create()
        {
            // Populate the font dictionary with tag-fontAsset pairs
            fontDictionary["MenuUI"] = GameData.I.GothicMenuFont;
            fontDictionary["Subtitle"] = GameData.I.GothicSubtitleFont;
        }

        public void ChangeFont()
        {
            // Get all the TextMeshPro components in the scene
            TMP_Text[] textComponents = FindObjectsOfType<TextMeshProUGUI>();

            // Change the font of the TextMeshPro components based on their tags
            foreach (TMP_Text textComponent in textComponents)
            {
                if (fontDictionary.ContainsKey(textComponent.tag))
                {
                    textComponent.font = fontDictionary[textComponent.tag];
                }
            }
        }
    }
}