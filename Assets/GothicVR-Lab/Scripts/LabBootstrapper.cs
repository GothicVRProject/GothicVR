using GVR.Globals;
using GVR.Manager;
using GVR.Manager.Settings;
using UnityEngine;

namespace GVR.GothicVR_Lab.Scripts
{
    public class LabBootstrapper : MonoBehaviour
    {
        private void Awake()
        {
            GvrBootstrapper.BootGothicVR(SettingsManager.GameSettings.GothicIPath);
        }

        private void OnDestroy()
        {
            GameData.Dispose();
        }
    }
}
