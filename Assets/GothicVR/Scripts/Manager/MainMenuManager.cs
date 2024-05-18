using System;
using GVR.Context;
using GVR.Globals;
using GVR.Util;
using UnityEngine.SceneManagement;

namespace GVR.Manager
{
    /// <summary>
    /// We need to reference this class inside Bootstrap scene, otherwise it won't get called by Unity during gameplay.
    /// </summary>
    public class MainMenuManager : SingletonBehaviour<MainMenuManager>
    {
        private void Start()
        {
            GvrEvents.MainMenuSceneLoaded.AddListener(delegate
            {
                GVRContext.PlayerControllerAdapter.CreatePlayerController(SceneManager.GetActiveScene());
            });
        }
    }
}
