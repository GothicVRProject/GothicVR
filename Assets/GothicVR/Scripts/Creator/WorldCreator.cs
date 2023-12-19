using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GVR.Creator.Meshes;
using GVR.Extensions;
using GVR.Manager;
using GVR.Phoenix.Data;
using GVR.Phoenix.Interface;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using ZenKit.Materialized;
using Mesh = ZenKit.Materialized.Mesh;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace GVR.Creator
{
    public static class WorldCreator
    {
        private static GameObject worldGo;
        private static GameObject teleportGo;
        private static GameObject nonTeleportGo;

        public static async Task CreateAsync(string worldName)
        {
            var world = LoadWorld(worldName);
            GameData.World = world;
            worldGo = new GameObject("World");

            // Interactable Vobs (item, ladder, ...) have their own collider Components
            // AND we don't want to teleport on top of them. We therefore exclude them from being added to teleport.
            teleportGo = new GameObject("Teleport");
            nonTeleportGo = new GameObject("NonTeleport");
            teleportGo.SetParent(worldGo);
            nonTeleportGo.SetParent(worldGo);

            await WorldMeshCreator.CreateAsync(world, teleportGo, ConstantsManager.MeshPerFrame);
            await VobCreator.CreateAsync(teleportGo, nonTeleportGo, world, ConstantsManager.VObPerFrame);
            WaynetCreator.Create(worldGo, world);

            // Set the global variable to the result of the coroutine
            LoadingManager.I.SetProgress(LoadingManager.LoadingProgressType.NPC, 1f);
        }


        /// <summary>
        /// Logic to be called after world (i.e. general scene) is fully loaded.
        /// </summary>
        public static void PostCreate()
        {
            var interactionManager = GvrSceneManager.I.interactionManager.GetComponent<XRInteractionManager>();

            // If we load a new scene, just remove the existing one.
            if (worldGo.TryGetComponent(out TeleportationArea teleportArea))
                GameObject.Destroy(teleportArea);

            // We need to set the Teleportation area after adding mesh to world. Otherwise Awake() method is called too early.
            var teleportationArea = teleportGo.AddComponent<TeleportationArea>();
            if (interactionManager != null)
            {
                teleportationArea.interactionManager = interactionManager;
            }

            // TODO - For some reason the referenced skybox in scene is reset to default once game starts.
            // We therefore need to reset it now again.
            RenderSettings.skybox = TextureManager.I.skymaterial;
        }

        private static WorldData LoadWorld(string worldName)
        {
            var zkWorld = new ZenKit.World(GameData.Vfs, worldName);
            var zkMesh = zkWorld.Mesh.Materialize();
            var zkBspTree = zkWorld.BspTree.Materialize();
            var zkWayNet = zkWorld.WayNet.Materialize();

            if (zkWorld.RootObjects.IsEmpty())
                throw new ArgumentException($"World >{worldName}< couldn't be found.");
            if (zkMesh.Polygons.IsEmpty())
                throw new ArgumentException($"No mesh in world >{worldName}< found.");

            WorldData world = new()
            {
                vobs = zkWorld.RootObjects,
                wayNet = zkWayNet
            };

            var subMeshes = CreateSubMeshesForUnity(zkMesh, zkBspTree);
            world.subMeshes = subMeshes;

            return world;
        }

        /// <summary>
        /// If we keep Polygons/Vertices like they're provided by Gothic, then we would have hundreds of thousands of small meshes.
        /// We therefore merge them into blobs grouped by materials.
        ///
        /// Gothic provides Polygons as Triangle Fans. As Unity can't handle them out-of-the-box, we just map them
        /// (every 4th element - aka every new triangle -  is dependent on element 0 (A).
        /// @see https://en.wikipedia.org/wiki/Triangle_fan
        ///
        /// We also need to put the triangle indices in in Reverse() order to make Unity
        /// draw mesh elements right (instead of upside down)
        /// </summary>
        private static Dictionary<int, WorldData.SubMeshData> CreateSubMeshesForUnity(Mesh zkMesh, BspTree zkBspTree)
        {
            // As we know the exact size of Submeshes (aka size of Materials), we will prefill them now.
            Dictionary<int, WorldData.SubMeshData> subMeshes = new(zkMesh.Materials.Count);
            for (int materialIndex = 0; materialIndex < zkMesh.Materials.Count; materialIndex++)
            {
                subMeshes.Add(materialIndex, new()
                {
                    materialIndex = materialIndex,
                    material = zkMesh.Materials[materialIndex]
                });
            }

            foreach (var leafPolygonIndex in zkBspTree.LeafPolygonIndices.Distinct())
            {
                var polygon = zkMesh.Polygons[(int)leafPolygonIndex];
                var currentSubMesh = subMeshes[(int)polygon.MaterialIndex];


                // As we always use element 0 and i+1, we skip it in the loop.
                for (var i=1; i < polygon.PositionIndices.Length - 1; i++)
                {
                    // Triangle Fan - We need to add element 0 (A) before every triangle 2 elements.
                    AddEntry(zkMesh, polygon, currentSubMesh, 0);
                    AddEntry(zkMesh, polygon, currentSubMesh, i);
                    AddEntry(zkMesh, polygon, currentSubMesh, i+1);
                }
            }

            return subMeshes;
        }

        private static void AddEntry(Mesh zkMesh, Polygon polygon, WorldData.SubMeshData currentSubMesh, int index)
        {
            try
            {
                // For every vertexIndex we store a new vertex. (i.e. no reuse of Vector3-vertices for later texture/uv attachment)
                currentSubMesh.vertices.Add(zkMesh.Positions[index].ToUnityVector());
                // This triangle (index where Vector 3 lies inside vertices, points to the newly added vertex (Vector3) as we don't reuse vertices.
                currentSubMesh.triangles.Add(currentSubMesh.vertices.Count - 1);

                var featureIndex = polygon.FeatureIndices[index];
                var feature = zkMesh.Features[featureIndex];
                currentSubMesh.uvs.Add(feature.Texture.ToUnityVector());
                currentSubMesh.normals.Add(feature.Normal.ToUnityVector());
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
        }


#if UNITY_EDITOR
        /// <summary>
        /// Loads the world for occlusion culling.
        /// </summary>
        public static async void LoadEditorWorld(IntPtr vfsPtr)
        {
            Scene worldScene = EditorSceneManager.GetActiveScene();
            if (Path.GetDirectoryName(worldScene.path) != "Assets\\GothicVR\\Scenes\\Worlds")
            {
                Debug.LogWarning($"Open a world scene, from Assets/GothicVR/Scenes/Worlds.");
                return;
            }

            GameData.VfsPtr = vfsPtr;
            GameData.World = LoadWorld(worldScene.name);

            await WorldMeshCreator.CreateAsync(GameData.World, new GameObject("World"), ConstantsManager.MeshPerFrame);
        }
#endif
    }
}
