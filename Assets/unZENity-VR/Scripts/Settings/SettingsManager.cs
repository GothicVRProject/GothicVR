using System;
using System.IO;
using UnityEditor;
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
            var settingsFilePath = $"{Application.streamingAssetsPath}/{SETTINGS_FILE_NAME}";
            if (!File.Exists(settingsFilePath))
                throw new ArgumentException("No >GameSettings.json< file exists. Can't load engine.");

            var settingsJson = File.ReadAllText(settingsFilePath);
            GameSettings = JsonUtility.FromJson<GameSettings>(settingsJson);

            var settingsDevFilePath = $"{Application.streamingAssetsPath}/{SETTINGS_FILE_NAME_DEV}";
            if (File.Exists(settingsDevFilePath))
            {
                var devJson = File.ReadAllText(settingsDevFilePath);
                EditorJsonUtility.FromJsonOverwrite(devJson, GameSettings);
            }
        }
    }
}
