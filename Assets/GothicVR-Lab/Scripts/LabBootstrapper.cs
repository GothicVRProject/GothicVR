using GVR.Caches;
using GVR.Context;
using GVR.Globals;
using GVR.Lab.Handler;
using GVR.Manager;
using GVR.Manager.Settings;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GVR.GothicVR_Lab.Scripts
{
    public class LabBootstrapper : MonoBehaviour
    {
        public LabMusicHandler labMusicHandler;
        public LabNpcDialogHandler npcDialogHandler;
        public LabLockableLabHandler lockableLabHandler;
        public LabLadderLabHandler ladderLabHandler;
        public LabVobHandAttachPointsLabHandler vobHandAttachPointsLabHandler;
        public LabNpcAnimationHandler labNpcAnimationHandler;

        private bool _isBooted;
        /// <summary>
        /// It's easiest to wait for Start() to initialize all the MonoBehaviours first.
        /// </summary>
        private void Update()
        {
            if (_isBooted)
                return;
            _isBooted = true;

            GvrBootstrapper.BootGothicVR(SettingsManager.GameSettings.GothicIPath);

            BootLab();

            labNpcAnimationHandler.Bootstrap();
            labMusicHandler.Bootstrap();
            npcDialogHandler.Bootstrap();
            lockableLabHandler.Bootstrap();
            ladderLabHandler.Bootstrap();
            vobHandAttachPointsLabHandler.Bootstrap();
        }

        private void BootLab()
        {
            GVRContext.PlayerControllerAdapter.CreatePlayerController(SceneManager.GetActiveScene());
            NpcHelper.CacheHero();
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
