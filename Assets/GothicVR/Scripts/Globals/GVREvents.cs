using UnityEngine.Events;

namespace GVR.Globals
{
    public static class GVREvents
    {
        public static readonly UnityEvent MainMenuSceneLoaded = new();
        public static readonly UnityEvent MainMenuSceneUnloaded = new();
        
        public static readonly UnityEvent LoadingSceneLoaded = new();
        public static readonly UnityEvent LoadingSceneUnloaded = new();

        public static readonly UnityEvent GeneralSceneLoaded = new();
        public static readonly UnityEvent GeneralSceneUnloaded = new();
        
        public static readonly UnityEvent WorldSceneLoaded = new();
        public static readonly UnityEvent WorldSceneUnloaded = new();

        
        public static void Dispose()
        {
            MainMenuSceneLoaded.RemoveAllListeners();
            MainMenuSceneUnloaded.RemoveAllListeners();
            
            LoadingSceneLoaded.RemoveAllListeners();
            LoadingSceneUnloaded.RemoveAllListeners();

            GeneralSceneLoaded.RemoveAllListeners();
            GeneralSceneUnloaded.RemoveAllListeners();
            
            WorldSceneLoaded.RemoveAllListeners();
            WorldSceneUnloaded.RemoveAllListeners();
        }
    }
}
