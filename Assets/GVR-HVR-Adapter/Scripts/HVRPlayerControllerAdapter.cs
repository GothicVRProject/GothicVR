#if GVR_HVR_INSTALLED
using GVR.Context.Controls;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GVR.HVR
{
    public class HVRPlayerControllerAdapter : IPlayerControllerAdapter
    {
        public void CreatePlayerController(Scene scene)
        {
            var newPrefab = Resources.Load<GameObject>("HVR/Prefabs/VRPlayer");
            var go = Object.Instantiate(newPrefab);
            go.name = "VRPlayer - HVR";

            // During normal gameplay, we need to move the VRPlayer to General scene. Otherwise, it will be created inside
            // world scene and removed whenever we change the world.
            SceneManager.MoveGameObjectToScene(go, scene);
        }
    }
}
#endif
