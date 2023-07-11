using GVR.Util;
using GVR.Phoenix.Interface;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace GVR.Creator
{
    public class WorldCreator : SingletonBehaviour<WorldCreator>
    {

        /// <summary>
        /// Loads the world.
        /// </summary>
        /// <param name="vdfPtr">The VDF pointer.</param>
        /// <param name="zen">The name of the .zen world to load.</param>
        public void LoadWorld(IntPtr vdfPtr, string zen, string startVob)
        {
            var worldScene = SceneManager.GetSceneByName(zen);

            if (!worldScene.isLoaded)
            {
                // unload the current scene and load the new one
                if (SceneManager.GetActiveScene().name != "Bootstrap")
                    SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
                SceneManager.LoadScene(zen, LoadSceneMode.Additive);
                worldScene = SceneManager.GetSceneByName(zen); // we do this to reload the values for the new scene which are no updated for the above cast
            }

            var world = WorldBridge.LoadWorld(vdfPtr, $"{zen}.zen"); // world.zen -> G1, newworld.zen/oldworld.zen/addonworld.zen -> G2

            PhoenixBridge.VdfsPtr = vdfPtr;
            PhoenixBridge.World = world;

            var worldGo = new GameObject("World");

            // We use SampleScene because it contains all the VM pointers and asset cache necesarry to generate the world
            var sampleScene = SceneManager.GetSceneByName("Bootstrap");
            SceneManager.SetActiveScene(sampleScene);
            sampleScene.GetRootGameObjects().Append(worldGo);

            var worldMesh = SingletonBehaviour<MeshCreator>.GetOrCreate().Create(world, worldGo);
            SingletonBehaviour<VobCreator>.GetOrCreate().Create(worldGo, world);
            SingletonBehaviour<WaynetCreator>.GetOrCreate().Create(worldGo, world);
            SingletonBehaviour<WorldCreator>.GetOrCreate().PostCreate(worldMesh);

            SingletonBehaviour<DebugAnimationCreator>.GetOrCreate().Create();

            // move the world to the correct scene
            SceneManager.MoveGameObjectToScene(worldGo, worldScene);

            // Subscribe the SetActiveScene method to the sceneLoaded event
            // so that we can set the proper scene as active when the scene is finally loaded
            // is related to occlusion culling
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                SceneManager.SetActiveScene(scene);
            };

            // Subscribe the SetActiveScene method so wen can properly place the player in the correct spot
            SceneManager.activeSceneChanged += (oldScene, newScene) =>
            {
                if (newScene == worldScene)
                {
                    GameObject.Find("VRPlayer_v4 (romey)").transform.position = GameObject.Find(startVob).transform.position;
                }
            };
        }

        /// <summary>
        /// Logic to be called after world is fully loaded.
        /// </summary>
        public void PostCreate(GameObject worldMesh)
        {
            // If we load a new scene, just remove the existing one.
            if (worldMesh.TryGetComponent(out TeleportationArea teleportArea))
                Destroy(teleportArea);

            // We need to set the Teleportation area after adding mesh to world. Otherwise Awake() method is called too early.
            worldMesh.AddComponent<TeleportationArea>();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Loads the world for occlusion culling.
        /// </summary>
        /// <param name="vdfPtr">The VDF pointer.</param>
        /// <param name="zen">The name of the .zen world to load.</param>
        public void LoadEditorWorld(IntPtr vdfPtr, string zen)
        {
            var worldScene = EditorSceneManager.GetSceneByName(zen);

            if (!worldScene.isLoaded)
            {
                // unload the current scene and load the new one
                if (EditorSceneManager.GetActiveScene().name != "Bootstrap")
                    EditorSceneManager.UnloadSceneAsync(EditorSceneManager.GetActiveScene());
                EditorSceneManager.LoadScene(zen, LoadSceneMode.Additive);
                worldScene = EditorSceneManager.GetSceneByName(zen); // we do this to reload the values for the new scene which are no updated for the above cast
            }

            var world = WorldBridge.LoadWorld(vdfPtr, $"{zen}.zen"); // world.zen -> G1, newworld.zen/oldworld.zen/addonworld.zen -> G2

            PhoenixBridge.VdfsPtr = vdfPtr;
            PhoenixBridge.World = world;

            var worldGo = new GameObject("World");

            // We use SampleScene because it contains all the VM pointers and asset cache necesarry to generate the world
            var sampleScene = EditorSceneManager.GetSceneByName("Bootstrap");
            EditorSceneManager.SetActiveScene(sampleScene);
            sampleScene.GetRootGameObjects().Append(worldGo);

            // load only the world mesh
            SingletonBehaviour<MeshCreator>.GetOrCreate().Create(world, worldGo);

            // move the world to the correct scene
            EditorSceneManager.MoveGameObjectToScene(worldGo, worldScene);

            // Subscribe the SetActiveScene method to the sceneLoaded event
            // so that we can set the proper scene as active when the scene is finally loaded
            // is related to occlusion culling
            EditorSceneManager.sceneLoaded += (scene, mode) => EditorSceneManager.SetActiveScene(scene);
        }
#endif
    }
}