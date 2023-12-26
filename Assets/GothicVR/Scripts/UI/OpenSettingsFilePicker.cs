using UnityEngine;
using SFB;
using GVR.Manager.Settings;
using GVR.Bootstrap;

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
                GVRBootstrapper.I.invalidInstallationDirMessage.SetActive(false);
                GVRBootstrapper.I.BootGothicVR(SettingsManager.GameSettings.GothicIPath);
            }
        }
    }
}
