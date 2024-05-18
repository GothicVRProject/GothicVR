using GVR.Context.Controls;
using GVR.Manager;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GVR.OXR
{
    public class OXRPlayerControllerAdapter : IPlayerControllerAdapter
    {
        public void CreatePlayerController(Scene scene)
        {
            string prefabName;
            string goName;

            if (GvrSceneManager.DefaultSceneNames.MainMenu.ToString() == scene.name)
            {
                prefabName = "OXR/Prefabs/VRPlayer-MainMenu";
                goName = "VRPlayer - OpenXR - (legacy) - MainMenu";
            }
            else
            {
                prefabName = "OXR/Prefabs/VRPlayer";
                goName = "VRPlayer - OpenXR - (legacy)";
            }

            var newPrefab = Resources.Load<GameObject>(prefabName);
            var go = Object.Instantiate(newPrefab);
            go.name = goName;

            // During normal gameplay, we need to move the VRPlayer to General scene. Otherwise, it will be created inside
            // world scene and removed whenever we change the world.
            SceneManager.MoveGameObjectToScene(go, scene);
        }
    }
}
