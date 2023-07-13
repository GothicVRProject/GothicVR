using GVR.Util;
using GVR.Phoenix.Interface;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using GVR.Phoenix.Data;
using GVR.Phoenix.Util;
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
        public void Create(string worldName)
        {
            var world = LoadWorld($"{worldName}.zen");
            GameData.I.World = world;

            var worldGo = new GameObject("World");

            GameData.I.WorldScene!.Value.GetRootGameObjects().Append(worldGo);

            worldMesh = MeshCreator.I.Create(world, worldGo);
            SingletonBehaviour<VobCreator>.GetOrCreate().Create(worldGo, world);
            SingletonBehaviour<WaynetCreator>.GetOrCreate().Create(worldGo, world);

            SingletonBehaviour<DebugAnimationCreator>.GetOrCreate().Create();
            WorldCreator.I.PostCreate();


            SceneManager.MoveGameObjectToScene(worldGo, SceneManager.GetSceneByName(worldName));
        }

        /// <summary>
        /// Logic to be called after world is fully loaded.
        /// </summary>
        public void PostCreate()
        {
            // If we load a new scene, just remove the existing one.
            if (worldMesh.TryGetComponent(out TeleportationArea teleportArea))
                Destroy(teleportArea);

            // We need to set the Teleportation area after adding mesh to world. Otherwise Awake() method is called too early.
            var teleportationArea = worldMesh.AddComponent<TeleportationArea>();
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

            var world = LoadWorld($"{zen}.zen");

            // FIXME - Might not work as we have no context inside Editor mode. Need to test and find alternative.
            GameData.I.VdfsPtr = vdfPtr;
            GameData.I.World = world;

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