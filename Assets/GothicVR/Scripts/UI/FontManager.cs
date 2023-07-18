using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using GVR.Phoenix.Interface;
using GVR.Util;


namespace GVR.Manager
{
    public class FontManager : SingletonBehaviour <FontManager>
    {
        public Dictionary<string, TMP_FontAsset> fontDictionary = new Dictionary<string, TMP_FontAsset>(); // Dictionary to store font assets for each tag

        //either do it like this or just have a public Create() for this
        private void Start()
        {
            // Populate the font dictionary with tag-fontAsset pairs
            fontDictionary["MenuUI"] = GameData.I.GothicMenuFont;
            fontDictionary["Subtitle"] = GameData.I.GothicSubtitleFont;

            ChangeFont();

            // Subscribe to the OnFontAssetChanged event to handle font changes for newly created UI elements
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ChangeFont();
        }

        private void ChangeFont()
        {
            // Get all the TextMeshPro components in the scene
            TMP_Text[] textComponents = FindObjectsOfType<TMP_Text>();

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