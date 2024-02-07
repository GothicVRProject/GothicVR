using System;
using UnityEngine.Events;

namespace GVR.Globals
{
    /// <summary>
    /// Loading/Unloading order of scenes:
    /// https://github.com/GothicVRProject/GothicVR/blob/main/Docs/development/diagrams/SceneLoading.drawio.png
    /// </summary>
    public static class GvrEvents
    {
        public static readonly UnityEvent ZenKitBootstrapped = new();

        public static readonly UnityEvent MainMenuSceneLoaded = new();
        public static readonly UnityEvent MainMenuSceneUnloaded = new();
        
        public static readonly UnityEvent LoadingSceneLoaded = new();
        public static readonly UnityEvent LoadingSceneUnloaded = new();

        // Hint: Scene general is always loaded >after< world is fully filled with vobs etc.
        public static readonly UnityEvent GeneralSceneLoaded = new();
        public static readonly UnityEvent GeneralSceneUnloaded = new();
        
        public static readonly UnityEvent WorldSceneLoaded = new();
        public static readonly UnityEvent WorldSceneUnloaded = new();
        
        public static readonly UnityEvent<DateTime> GameTimeSecondChangeCallback = new();
        public static readonly UnityEvent<DateTime> GameTimeMinuteChangeCallback = new();
        public static readonly UnityEvent<DateTime> GameTimeHourChangeCallback = new();
    }
}
