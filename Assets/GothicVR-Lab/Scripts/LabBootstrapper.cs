using GVR.Caches;
using GVR.Globals;
using GVR.Lab.Handler;
using GVR.Manager;
using GVR.Manager.Settings;
using UnityEngine;

namespace GVR.GothicVR_Lab.Scripts
{
    public class LabBootstrapper : MonoBehaviour
    {
        [Header("Bootstrapping")]
        public bool bootNpcHandler;
        public bool bootLockableHandler;
        public bool bootAttachPointHandler;
        
        public LabNpcLabHandler npcLabHandler;
        public LabLockableLabHandler lockableLabHandler;
        public LabVobHandAttachPointsLabHandler vobHandAttachPointsLabHandler;
        
        private bool isBooted;
        /// <summary>
        /// It's easiest to wait for Start() to initialize all the MonoBehaviours first.
        /// </summary>
        private void Update()
        {
            if (isBooted)
                return;
            isBooted = true;
            
            GvrBootstrapper.BootGothicVR(SettingsManager.GameSettings.GothicIPath);

            if (bootNpcHandler)
                npcLabHandler.Bootstrap();
            if (bootLockableHandler)
                lockableLabHandler.Bootstrap();
            if (bootAttachPointHandler)
                vobHandAttachPointsLabHandler.Bootstrap();
        }

        private void OnDestroy()
        {
            GameData.Dispose();
            AssetCache.Dispose();
            LookupCache.Dispose();
            PrefabCache.Dispose();
            MorphMeshCache.Dispose();
        }
    }
}
