using System.Linq;
using GVR.Caches;
using GVR.Debugging;
using GVR.Util;
using UnityEngine.SceneManagement;

namespace GVR.Manager
{
    public class XRDeviceSimulatorManager: SingletonBehaviour<XRDeviceSimulatorManager>
    {

        public void PrepareForScene(Scene scene)
        {
#if !UNITY_EDITOR // Use this Feature only in Editor mode.
            return;
#endif

            if (!FeatureFlags.I.useXRDeviceSimulator)
                return;

            var simulator = PrefabCache.I.TryGetObject(PrefabCache.PrefabType.XRDeviceSimulator);
            scene.GetRootGameObjects().Append(simulator);
        }
        
    }
}