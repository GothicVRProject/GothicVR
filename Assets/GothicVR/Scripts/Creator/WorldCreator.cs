using GVR.Manager;
using GVR.Phoenix.Data;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PxCs.Data.WayNet;
using PxCs.Interface;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace GVR.Creator
{
    public class WorldCreator : SingletonBehaviour<WorldCreator>
    {

        private GameObject worldMesh;

        public async Task<GameObject> Create(string worldName)
        {
            var world = LoadWorld(worldName);
            GameData.I.World = world;

            var worldGo = new GameObject("World");

            GameData.I.WorldScene!.Value.GetRootGameObjects().Append(worldGo);

            worldMesh = await MeshCreator.I.Create(world, worldGo);
            await VobCreator.I.Create(worldGo, world);
            WaynetCreator.I.Create(worldGo, world);

            DebugAnimationCreator.I.Create();

            LoadingManager.I.SetProgress(LoadingManager.LoadingProgressType.NPC, 1f);

            return worldGo;
        }

        /// <summary>
        /// Logic to be called after world is fully loaded.
        /// </summary>
        public void PostCreate(XRInteractionManager interactionManager)
        {
            // If we load a new scene, just remove the existing one.
            if (worldMesh.TryGetComponent(out TeleportationArea teleportArea))
                Destroy(teleportArea);

            // We need to set the Teleportation area after adding mesh to world. Otherwise Awake() method is called too early.
            var teleportationArea = worldMesh.AddComponent<TeleportationArea>();
            if (interactionManager != null)
            {
                teleportationArea.interactionManager = interactionManager;
            }
        }

        private WorldData LoadWorld(string worldName)
        {
            var worldPtr = PxWorld.pxWorldLoadFromVdf(GameData.I.VdfsPtr, worldName);
            if (worldPtr == IntPtr.Zero)
                throw new ArgumentException($"World >{worldName}< couldn't be found.");

            var worldMeshPtr = PxWorld.pxWorldGetMesh(worldPtr);
            if (worldMeshPtr == IntPtr.Zero)
                throw new ArgumentException($"No mesh in world >{worldName}< found.");

            var vertexIndices = PxMesh.GetPolygonVertexIndices(worldMeshPtr);
            var materialIndices = PxMesh.GetPolygonMaterialIndices(worldMeshPtr);
            var featureIndices = PxMesh.GetPolygonFeatureIndices(worldMeshPtr);

            var vertices = PxMesh.GetVertices(worldMeshPtr);
            var features = PxMesh.GetFeatures(worldMeshPtr);
            var materials = PxMesh.GetMaterials(worldMeshPtr);

            var waypoints = PxWorld.GetWayPoints(worldPtr);
            var waypointsDict = new Dictionary<string, PxWayPointData>();
            foreach (var waypoint in waypoints)
            {
                waypointsDict[waypoint.name] = waypoint;
            }
            var waypointEdges = PxWorld.GetWayEdges(worldPtr);

            WorldData world = new()
            {
                vertexIndices = vertexIndices,
                materialIndices = materialIndices,
                featureIndices = featureIndices,

                vertices = vertices,
                features = features,
                materials = materials,


                vobs = PxWorld.GetVobs(worldPtr),

                waypoints = waypoints,
                waypointsDict = waypointsDict,
                waypointEdges = waypointEdges
            };

            var subMeshes = CreateSubmeshesForUnity(world);
            world.subMeshes = subMeshes;

            PxWorld.pxWorldDestroy(worldPtr);

            return world;
        }

        private Dictionary<int, WorldData.SubMeshData> CreateSubmeshesForUnity(WorldData world)
        {
            Dictionary<int, WorldData.SubMeshData> subMeshes = new(world.materials.Length);
            var vertices = world.vertices;
            var vertexIndices = world.vertexIndices;
            var featureIndices = world.featureIndices;
            var features = world.features;

            // We need to put vertex_indices (aka triangles) in reversed order
            // to make Unity draw mesh elements right (instead of upside down)
            for (var loopVertexIndexId = vertexIndices.LongLength - 1; loopVertexIndexId >= 0; loopVertexIndexId--)
            {
                // For each 3 vertexIndices (aka each triangle) there's one materialIndex.
                var materialIndex = world.materialIndices[loopVertexIndexId / 3];

                // The materialIndex was never used before.
                if (!subMeshes.ContainsKey(materialIndex))
                {
                    var newSubMesh = new WorldData.SubMeshData()
                    {
                        materialIndex = materialIndex,
                        material = world.materials[materialIndex]
                    };

                    subMeshes.Add(materialIndex, newSubMesh);
                }

                var currentSubMesh = subMeshes[materialIndex];
                var origVertexIndex = vertexIndices[loopVertexIndexId];

                // For every vertexIndex we store a new vertex. (i.e. no reuse of Vector3-vertices for later texture/uv attachment)
                currentSubMesh.vertices.Add(vertices[origVertexIndex].ToUnityVector());

                var featureIndex = featureIndices[loopVertexIndexId];
                currentSubMesh.uvs.Add(features[featureIndex].texture.ToUnityVector());
                currentSubMesh.normals.Add(features[featureIndex].normal.ToUnityVector());

                currentSubMesh.triangles.Add(currentSubMesh.vertices.Count - 1);
            }

            return subMeshes;
        }


#if UNITY_EDITOR
        /// <summary>
        /// Loads the world for occlusion culling.
        /// </summary>
        /// <param name="vdfPtr">The VDF pointer.</param>
        /// <param name="zen">The name of the .zen world to load.</param>
        public async void LoadEditorWorld(IntPtr vdfPtr, string zen)
        {
            var worldScene = EditorSceneManager.GetSceneByName(zen);

            if (!worldScene.isLoaded)
            {
                // unload the current scene and load the new one
                if (EditorSceneManager.GetActiveScene().name != "Bootstrap")
                    EditorSceneManager.UnloadSceneAsync(EditorSceneManager.GetActiveScene());
                EditorSceneManager.OpenScene(zen);
                worldScene = EditorSceneManager.GetSceneByName(zen); // we do this to reload the values for the new scene which are no updated for the above cast
            }

            var world = LoadWorld(zen);

            // FIXME - Might not work as we have no context inside Editor mode. Need to test and find alternative.
            GameData.I.VdfsPtr = vdfPtr;
            GameData.I.World = world;

            var worldGo = new GameObject("World");

            // We use SampleScene because it contains all the VM pointers and asset cache necesarry to generate the world
            var sampleScene = EditorSceneManager.GetSceneByName("Bootstrap");
            EditorSceneManager.SetActiveScene(sampleScene);
            sampleScene.GetRootGameObjects().Append(worldGo);

            // load only the world mesh
            await MeshCreator.I.Create(world, worldGo);

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