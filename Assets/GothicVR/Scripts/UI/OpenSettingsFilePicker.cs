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

            if (paths != null && paths.Length > 0)
            {
                GameSettings gameSettings = SettingsManager.I.LoadGameSettings();
                gameSettings.GothicIPath = paths[0];
                SettingsManager.I.SaveGameSettings(gameSettings);
                if (SettingsManager.I.CheckIfGothic1InstallationExists())
                {
                    PhoenixBootstrapper.I.BootGothicVR(SettingsManager.I.GameSettings.GothicIPath);
                }
            }
        }
    }
}
