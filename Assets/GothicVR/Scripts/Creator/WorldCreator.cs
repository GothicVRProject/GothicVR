using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GVR.Creator.Meshes;
using GVR.Debugging;
using GVR.Extensions;
using GVR.Globals;
using GVR.Manager;
using GVR.Phoenix.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using ZenKit;
using Vector3 = System.Numerics.Vector3;
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

        static WorldCreator()
        {
            GvrEvents.GeneralSceneLoaded.AddListener(WorldLoaded);
        }
        
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

            if (FeatureFlags.I.createWorldMesh)
                await WorldMeshCreator.CreateAsync(world, teleportGo, Constants.MeshPerFrame);
    
            if (FeatureFlags.I.createVobs)
                await VobCreator.CreateAsync(teleportGo, nonTeleportGo, world, Constants.VObPerFrame);
    
            WaynetCreator.Create(worldGo, world);
            
            // Set the global variable to the result of the coroutine
            LoadingManager.I.SetProgress(LoadingManager.LoadingProgressType.NPC, 1f);
        }

        private static WorldData LoadWorld(string worldName)
        {
            var zkWorld = new ZenKit.World(GameData.Vfs, worldName);
            var zkMesh = zkWorld.Mesh.Cache();
            var zkBspTree = zkWorld.BspTree.Cache();
            var zkWayNet = zkWorld.WayNet.Cache();

            if (zkWorld.RootObjects.IsEmpty())
                throw new ArgumentException($"World >{worldName}< couldn't be found.");
            if (zkMesh.Polygons.IsEmpty())
                throw new ArgumentException($"No mesh in world >{worldName}< found.");

            WorldData world = new()
            {
                World = zkWorld,
                Vobs = zkWorld.RootObjects,
                WayNet = (CachedWayNet)zkWayNet
            };

            var subMeshes = CreateSubMeshesForUnity(zkMesh, zkBspTree);
            world.SubMeshes = subMeshes;
            
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
        private static Dictionary<int, WorldData.SubMeshData> CreateSubMeshesForUnity(IMesh zkMesh, IBspTree zkBspTree)
        {
            var zkMaterials = zkMesh.Materials;
            var zkPolygons = zkMesh.Polygons;
            var zkPositions = zkMesh.Positions;
            var zkFeatures = zkMesh.Features;

            // As we know the exact size of SubMeshes (aka size of Materials), we will prefill them now.
            Dictionary<int, WorldData.SubMeshData> subMeshes = new(zkMaterials.Count);
            for (var materialIndex = 0; materialIndex < zkMaterials.Count; materialIndex++)
            {
                subMeshes.Add(materialIndex, new()
                {
                    Material = zkMaterials[materialIndex]
                });
            }

            // LeafPolygonIndices aren't distinct. We therefore need to rearrange them this way.
            // Alternatively we could also loop through all Nodes and fetch where Front==Back==-1 (aka Leaf)
            foreach (var leafPolygonIndex in zkBspTree.LeafPolygonIndices.Distinct())
            {
                var polygon = zkPolygons[leafPolygonIndex];
                var currentSubMesh = subMeshes[polygon.MaterialIndex];

                if (polygon.IsPortal)
                    continue;
                
                // As we always use element 0 and i+1, we skip it in the loop.
                for (var i=1; i < polygon.PositionIndices.Count - 1; i++)
                {
                    // Triangle Fan - We need to add element 0 (A) before every triangle 2 elements.
                    AddEntry(zkPositions, zkFeatures, polygon, currentSubMesh, 0);
                    AddEntry(zkPositions, zkFeatures, polygon, currentSubMesh, i);
                    AddEntry(zkPositions, zkFeatures, polygon, currentSubMesh, i+1);
                }
            }

            // To have easier to read code above, we reverse the arrays now at the end.
            foreach (var subMesh in subMeshes)
            {
                subMesh.Value.Vertices.Reverse();
                subMesh.Value.Uvs.Reverse();
                subMesh.Value.Normals.Reverse();
            }

            return subMeshes;
        }

        private static void AddEntry(List<Vector3> zkPositions, List<Vertex> features, IPolygon polygon, WorldData.SubMeshData currentSubMesh, int index)
        {
            // For every vertexIndex we store a new vertex. (i.e. no reuse of Vector3-vertices for later texture/uv attachment)
            var positionIndex = polygon.PositionIndices[index];
            currentSubMesh.Vertices.Add(zkPositions[(int)positionIndex].ToUnityVector());

            // This triangle (index where Vector 3 lies inside vertices, points to the newly added vertex (Vector3) as we don't reuse vertices.
            currentSubMesh.Triangles.Add(currentSubMesh.Vertices.Count - 1);

            var featureIndex = polygon.FeatureIndices[index];
            var feature = features[(int)featureIndex];
            currentSubMesh.Uvs.Add(feature.Texture.ToUnityVector());
            currentSubMesh.Normals.Add(feature.Normal.ToUnityVector());
        }

        private static void WorldLoaded()
        {
            // As we already added stored world mesh and waypoints in Unity GOs, we can safely remove them to free MBs.
            GameData.World.SubMeshes = null;
            
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
            RenderSettings.skybox = TextureManager.I.skyMaterial;
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// Loads the world for occlusion culling.
        /// </summary>
        public static async void LoadEditorWorld()
        {
            Scene worldScene = SceneManager.GetActiveScene();
            if (Path.GetDirectoryName(worldScene.path) != "Assets\\GothicVR\\Scenes\\Worlds")
            {
                Debug.LogWarning($"Open a world scene, from Assets/GothicVR/Scenes/Worlds.");
                return;
            }

            GameData.World = LoadWorld(worldScene.name);

            await WorldMeshCreator.CreateAsync(GameData.World, new GameObject("World"), Constants.MeshPerFrame);
        }
#endif
    }
}
