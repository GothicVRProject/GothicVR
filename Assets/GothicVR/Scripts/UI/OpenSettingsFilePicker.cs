using GVR.Manager;
using GVR.Manager.Settings;
using SFB;
using UnityEngine;

namespace GVR
{
    public class OpenSettingsFilePicker : MonoBehaviour
    {
        public static void OpenGothicInstallationFilePicker()
        {
            var paths = StandaloneFileBrowser.OpenFolderPanel("Select Gothic installation","", false);

            if (paths == null || paths.Length == 0)
                return;
            
            SettingsManager.LoadGameSettings();
            SettingsManager.GameSettings.GothicIPath = paths[0];
            SettingsManager.SaveGameSettings(SettingsManager.GameSettings);
            
            if (SettingsManager.CheckIfGothic1InstallationExists())
            {
                GvrBootstrapper.I.invalidInstallationDirMessage.SetActive(false);
                GvrBootstrapper.BootGothicVR(SettingsManager.GameSettings.GothicIPath);
            }
        }
    }
}
