using GVR.Util;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace GVR.Creator
{
    public class WorldCreator : SingletonBehaviour<WorldCreator>
    {
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
    }
}