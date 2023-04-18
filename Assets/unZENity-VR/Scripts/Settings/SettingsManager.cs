using System;
using System.IO;
using UnityEngine;
using UZVR.Util;

namespace UZVR.Settings
{
    public class SettingsManager: SingletonBehaviour<SettingsManager>
    {
        public GameSettings GameSettings { get; private set; }

        private const string SETTINGS_FILE_NAME = "GameSettings.json";
        private const string SETTINGS_FILE_NAME_DEV = "GameSettings.dev.json";


        private void Start()
        {
            LoadGameSettings();
        }


        private void LoadGameSettings()
        {
            var settingsFilePath = $"{GetRootPath()}/{SETTINGS_FILE_NAME}";
            if (!File.Exists(settingsFilePath))
                throw new ArgumentException($"No >GameSettings.json< file exists at >{settingsFilePath}<. Can't load Gothic1.");

            var settingsJson = File.ReadAllText(settingsFilePath);
            GameSettings = JsonUtility.FromJson<GameSettings>(settingsJson);

            var settingsDevFilePath = $"{GetRootPath()}/{SETTINGS_FILE_NAME_DEV}";
            if (File.Exists(settingsDevFilePath))
            {
                var devJson = File.ReadAllText(settingsDevFilePath);
                JsonUtility.FromJsonOverwrite(devJson, GameSettings);
            }

            CheckIfGothicIDirectoryExists();
        }

        /// <summary>
        /// Return path of settings file based on target architecture.
        /// As there is no "folder" for an Android build (as it's a packaged .apk file), we need to check within user directory.
        /// </summary>
        /// <returns></returns>
        private string GetRootPath()
        {
            if (Application.platform == RuntimePlatform.Android)
                // https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html
                // Will be: /storage/emulated/<userid>/Android/data/<packagename>/files
                return Application.persistentDataPath;
            else
                // https://docs.unity3d.com/ScriptReference/Application-streamingAssetsPath.html
                // Will be:
                // 1. Editor: Assets\StreamingAssets\
                // 2. Standalone: Build\GothicVR_Data\StreamingAssets\
                return Application.streamingAssetsPath;
        }

        private void CheckIfGothicIDirectoryExists()
        {
            if (!Directory.Exists(GameSettings.GothicIPath))
                throw new ArgumentException(
                    $"GothicI installation path wasn't found at >{GameSettings.GothicIPath}<." +
                    $"Please put in the right absolute path to your local installation.");
        }

        /// <summary>
        /// Import the settings file from streamingAssetPath to persistentDataPath.
        /// Since the settings file is in streamingAssetPath, we need to use UnityWebRequest to move it so we can have access to it
        /// as detailed here https://docs.unity3d.com/ScriptReference/Application-streamingAssetsPath.html
        /// </summary>
        private void Importer()
        {
            string GameSettingsPath = System.IO.Path.Combine(Application.streamingAssetsPath, $"{SETTINGS_FILE_NAME}");
            string result = "";
            if (GameSettingsPath.Contains("://") || GameSettingsPath.Contains(":///"))
            {
                UnityEngine.Networking.UnityWebRequest www = UnityEngine.Networking.UnityWebRequest.Get(GameSettingsPath);
                www.SendWebRequest();
                // Wait until async download is done
                while (!www.isDone) { }
                result = www.downloadHandler.text;
            }
            else
                result = System.IO.File.ReadAllText(GameSettingsPath);

            string FinalPath = System.IO.Path.Combine(Application.persistentDataPath, $"{SETTINGS_FILE_NAME}");
            File.WriteAllText(FinalPath, result);
        }
    }
}
