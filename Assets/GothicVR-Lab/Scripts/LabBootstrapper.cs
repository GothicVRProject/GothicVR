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
        
        public NpcHandler npcHandler;
        public LockableHandler lockableHandler;
        public VobHandAttachPointsHandler vobHandAttachPointsHandler;
        
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
                npcHandler.Bootstrap();
            if (bootLockableHandler)
                lockableHandler.Bootstrap();
            if (bootAttachPointHandler)
                vobHandAttachPointsHandler.Bootstrap();
        }

        private void OnDestroy()
        {
            GameData.Dispose();
        }
    }
}
