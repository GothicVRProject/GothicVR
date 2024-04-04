using GVR.Caches;
using GVR.Globals;
using GVR.Lab.Handler;
using GVR.Manager;
using GVR.Manager.Settings;
using UnityEngine;
using UnityEngine.Serialization;

namespace GVR.GothicVR_Lab.Scripts
{
    public class LabBootstrapper : MonoBehaviour
    {
        [Header("Bootstrapping")]
        public bool bootLabMusicHandler;
        public bool bootNpcHandler;
        public bool bootLockableHandler;
        public bool bootLadderHandler;
        public bool bootAttachPointHandler;

        public LabMusicHandler labMusicHandler;
        public LabNpcDialogHandler npcDialogHandler;
        public LabLockableLabHandler lockableLabHandler;
        public LabLadderLabHandler ladderLabHandler;
        public LabVobHandAttachPointsLabHandler vobHandAttachPointsLabHandler;

        public LabNpcAnimationHandler LabNpcAnimationHandler;

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

            if (bootLabMusicHandler)
                labMusicHandler.Bootstrap();
            if (bootNpcHandler)
                npcDialogHandler.Bootstrap();
            if (bootLockableHandler)
                lockableLabHandler.Bootstrap();
            if (bootLadderHandler)
                ladderLabHandler.Bootstrap();
            if (bootAttachPointHandler)
                vobHandAttachPointsLabHandler.Bootstrap();
        }

        public void LoadNpcAnimationHandlerClicked()
        {
            LabNpcAnimationHandler.Bootstrap();
        }

        private void OnDestroy()
        {
            GameData.Dispose();
            AssetCache.Dispose();
            TextureCache.Dispose();
            LookupCache.Dispose();
            PrefabCache.Dispose();
            MorphMeshCache.Dispose();
        }
    }
}
