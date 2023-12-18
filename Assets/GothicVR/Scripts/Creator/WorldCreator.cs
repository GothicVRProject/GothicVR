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

            // We need to put vertex_indices (aka triangles) in reversed order
            // to make Unity draw mesh elements right (instead of upside down)
            foreach (var leafPolygonIndex in zkBspTree.LeafPolygonIndices.Distinct())
            {
                var polygon = zkMesh.Polygons[(int)leafPolygonIndex];
                var materialIndex = (int)polygon.MaterialIndex;

                var currentSubMesh = subMeshes[materialIndex];

                for (var polygonIndices = 0; polygonIndices < polygon.PositionIndices.Length; polygonIndices++)
                {
                    var origVertexIndex = polygon.PositionIndices[polygonIndices];

                    // For every vertexIndex we store a new vertex. (i.e. no reuse of Vector3-vertices for later texture/uv attachment)
                    currentSubMesh.vertices.Add(zkMesh.Positions[origVertexIndex].ToUnityVector());

                    var featureIndex = polygon.FeatureIndices[polygonIndices];
                    currentSubMesh.uvs.Add(zkMesh.Features[featureIndex].Texture.ToUnityVector());
                    currentSubMesh.normals.Add(zkMesh.Features[featureIndex].Normal.ToUnityVector());

                    currentSubMesh.triangles.Add(currentSubMesh.vertices.Count - 1);
                }
            }

            return subMeshes;
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
