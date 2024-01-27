using System.Linq;
using GVR.Caches;
using GVR.Debugging;
using GVR.Globals;
using GVR.Util;
using UnityEngine.SceneManagement;

namespace GVR.Manager
{
    public class XRDeviceSimulatorManager: SingletonBehaviour<XRDeviceSimulatorManager>
    {
        private void Start()
        {
            GvrEvents.GeneralSceneLoaded.AddListener(WorldLoaded);
            GvrEvents.MainMenuSceneLoaded.AddListener(WorldLoaded);
        }

        private void WorldLoaded()
        {
            if (!FeatureFlags.I.useXRDeviceSimulator)
                return;

            var simulator = PrefabCache.TryGetObject(PrefabCache.PrefabType.XRDeviceSimulator);
            SceneManager.GetActiveScene().GetRootGameObjects().Append(simulator);
        }
        
    }
}
