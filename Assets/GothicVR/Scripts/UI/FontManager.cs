using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using GVR.Phoenix.Interface;
using GVR.Util;
using System;
using GVR.Caches;



namespace GVR.Manager
{
    public class FontManager : SingletonBehaviour <FontManager>
    {
        public Dictionary<string, TMP_FontAsset> fontDictionary = new Dictionary<string, TMP_FontAsset>(); // Dictionary to store font assets for each tag
        
        
        private bool fontloaded = false;

        //either do it like this or just have a public Create() for this
    
            public void Create()
            {
                //if (!fontloaded && GameData.I.VdfsPtr != IntPtr.Zero)
                //{

                    fontloaded = true;
                    // Populate the font dictionary with tag-fontAsset pairs
                    fontDictionary["MenuUI"] = GameData.I.GothicMenuFont;
                    fontDictionary["Subtitle"] = GameData.I.GothicSubtitleFont;

                    ChangeFont();

                    // Subscribe to the OnFontAssetChanged event to handle font changes for newly created UI elements
                    SceneManager.sceneLoaded += OnSceneLoaded;
               // }
            }
           
        

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ChangeFont();
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