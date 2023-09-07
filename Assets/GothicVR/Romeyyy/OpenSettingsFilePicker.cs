using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SFB;
using GVR.Manager.Settings;

namespace GVR
{
    public class OpenSettingsFilePicker : MonoBehaviour
    {

        public static void OpenFilePicker()
        {
            var paths = StandaloneFileBrowser.OpenFolderPanel("Open File","", false);

            if (paths != null)
            {
                GameSettings updatedGameSettings = new GameSettings();
                updatedGameSettings.GothicIPath = "Test";

                //updatedGameSettings.GothicILanguage = "Test";
                //updatedGameSettings.LogLevel = "Test";
                //updatedGameSettings.GothicMenuFontPath = "Test";
                //updatedGameSettings.GothicSubtitleFontPath = "Test";

                SettingsManager.I.SaveGameSettings(updatedGameSettings);
            }
        }
    }
}
