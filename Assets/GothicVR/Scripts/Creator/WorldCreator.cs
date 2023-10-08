using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GVR.Creator.Meshes;
using GVR.Debugging;
using GVR.Manager;
using GVR.Phoenix.Data;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.WayNet;
using PxCs.Interface;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace GVR.Creator
{
    public class WorldCreator : SingletonBehaviour<WorldCreator>
    {
        private GameObject worldGo;
        private GameObject teleportGo;
        private GameObject nonTeleportGo;

        public async Task CreateAsync(string worldName)
        {
            var world = LoadWorld(worldName);
            GameData.I.World = world;
            worldGo = new GameObject("World");

            // Interactable Vobs (item, ladder, ...) have their own collider Components
            // AND we don't want to teleport on top of them. We therefore exclude them from being added to teleport.
            teleportGo = new GameObject("Teleport");
            nonTeleportGo = new GameObject("NonTeleport");
            teleportGo.SetParent(worldGo);
            nonTeleportGo.SetParent(worldGo);

            await WorldMeshCreator.I.CreateAsync(world, teleportGo, ConstantsManager.I.MeshPerFrame);
            await VobCreator.I.CreateAsync(teleportGo, nonTeleportGo, world, ConstantsManager.I.VObPerFrame);
            WaynetCreator.I.Create(worldGo, world);

            DebugAnimationCreator.I.Create(worldName);
            DebugAnimationCreatorBSFire.I.Create(worldName);
            DebugAnimationCreatorVelaya.I.Create(worldName);
            DebugAnimationCreatorBloodwyn.I.Create();

            // Set the global variable to the result of the coroutine
            LoadingManager.I.SetProgress(LoadingManager.LoadingProgressType.NPC, 1f);
        }


        /// <summary>
        /// Logic to be called after world is fully loaded.
        /// </summary>
        public void PostCreate(XRInteractionManager interactionManager)
        {
            // If we load a new scene, just remove the existing one.
            if (worldGo.TryGetComponent(out TeleportationArea teleportArea))
                Destroy(teleportArea);

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

        private WorldData LoadWorld(string worldName)
        {
            var worldPtr = PxWorld.pxWorldLoadFromVfs(GameData.I.VfsPtr, worldName);
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

            if (FeatureFlags.I.enableLegacyBigWorldMeshCreation)
            {
                var subMeshes = CreateSubmeshesForUnity_Old(world);
                world.subMeshes = subMeshes;
            }
            else
            {
                var subMeshes = CreateSubmeshesForUnity_New(world);
                world.subMeshes = subMeshes;
            }

            PxWorld.pxWorldDestroy(worldPtr);

            return world;
        }
        
        private Dictionary<int, List<WorldData.SubMeshData>> CreateSubmeshesForUnity_Old(WorldData world)
        {
            Dictionary<int, List<WorldData.SubMeshData>> subMeshes = new(world.materials.Length);
            var vertices = world.vertices;
            var vertexIndices = world.vertexIndices;
            var featureIndices = world.featureIndices;
            var features = world.features;

            // We need to put vertex_indices (aka triangles) in reversed order
            // to make Unity draw mesh elements right (instead of upside down)
            for (var loopVertexIndexId = vertexIndices.LongLength - 1; loopVertexIndexId >= 0; loopVertexIndexId-=3)
            {
                // For each 3 vertexIndices (aka each triangle) there's one materialIndex.
                var materialIndex = world.materialIndices[loopVertexIndexId / 3];
                
                // if (materialIndex != 60)
                //     continue;
                
                // The materialIndex was never used before.
                if (!subMeshes.ContainsKey(materialIndex))
                {
                    var newSubMesh = new List<WorldData.SubMeshData>()
                    {
                        new WorldData.SubMeshData()
                        {
                            materialIndex = materialIndex,
                            material = world.materials[materialIndex]
                        }
                    };

                    subMeshes.Add(materialIndex, newSubMesh);
                }
                
                var currentSubMesh = subMeshes[materialIndex];
                var currentSubMeshFirstListItem = currentSubMesh.First();

                for (var subVertexIndexId = loopVertexIndexId; subVertexIndexId > loopVertexIndexId - 3; subVertexIndexId--)
                {
                    var origVertexIndex = vertexIndices[subVertexIndexId];
                    
                    // For every vertexIndex we store a new vertex. (i.e. no reuse of Vector3-vertices for later texture/uv attachment)
                    currentSubMeshFirstListItem.vertices.Add(vertices[origVertexIndex].ToUnityVector());
                    
                    var featureIndex = featureIndices[subVertexIndexId];
                    currentSubMeshFirstListItem.uvs.Add(features[featureIndex].texture.ToUnityVector());
                    currentSubMeshFirstListItem.normals.Add(features[featureIndex].normal.ToUnityVector());

                    currentSubMeshFirstListItem.triangles.Add(currentSubMeshFirstListItem.vertices.Count - 1);
                }
            }

            // DebugPrint(subMeshes, "old");
            return subMeshes;
        }
        
        // FIXME - uvs (and most likely normals) are set wrong for submeshes so far.
        // FIXME - Find a faster algorithm to create and fill the submeshes.
        // FIXME - submeshing isn't finished yet. e.g. materialIndex=4 (grass) is too segregated.
        private Dictionary<int, List<WorldData.SubMeshData>> CreateSubmeshesForUnity_New(WorldData world)
        {
            // Dictionary<int, List<List<int>>> subSubMeshTempArrangement = new();

            // Dict<materialId, List<KVP<List<vertexIndex>, List<featureIndex>>>>
            Dictionary<int, List<ValueTuple<List<int>, List<int>>>> subSubMeshTempArrangement = new();
            
            
            Dictionary<int, List<WorldData.SubMeshData>> returnMeshes = new(world.materials.Length);
            var vertices = world.vertices;
            var vertexIndices = world.vertexIndices;
            var featureIndices = world.featureIndices;
            var features = world.features;

            // We need to put vertex_indices (aka triangles) in reversed order
            // to make Unity draw mesh elements right (instead of upside down)
            for (var loopVertexIndexId = vertexIndices.LongLength - 1; loopVertexIndexId >= 0; loopVertexIndexId-=3)
            {
                // For each 3 vertexIndices (aka each triangle) there's one materialIndex.
                var materialIndex = world.materialIndices[loopVertexIndexId / 3];

                // // DEBUG! SomeWall
                // if (materialIndex != 60)
                //     continue;
                
                // The materialIndex was never used before.
                if (!subSubMeshTempArrangement.ContainsKey(materialIndex))
                {
                    subSubMeshTempArrangement.Add(materialIndex, new List<ValueTuple<List<int>, List<int>>>());
                }

                var currentSubMesh = subSubMeshTempArrangement[materialIndex];

                var vertexIndex0 = vertexIndices[loopVertexIndexId];
                var vertexIndex1 = vertexIndices[loopVertexIndexId - 1];
                var vertexIndex2 = vertexIndices[loopVertexIndexId - 2];

                var featureIndex0 = featureIndices[loopVertexIndexId];
                var featureIndex1 = featureIndices[loopVertexIndexId - 1];
                var featureIndex2 = featureIndices[loopVertexIndexId - 2];

                var addedToSubSubMesh = false;
                for (var indexSearch = 0; indexSearch < currentSubMesh.Count; indexSearch++)
                {
                    var currentSubSubMesh = currentSubMesh[indexSearch];

                    if (Array.IndexOf(currentSubSubMesh.Item1.ToArray(), vertexIndex0) >= 0
                        || Array.IndexOf(currentSubSubMesh.Item1.ToArray(), vertexIndex1) >= 0
                        || Array.IndexOf(currentSubSubMesh.Item1.ToArray(), vertexIndex2) >= 0)
                    {
                        currentSubSubMesh.Item1.AddRange(new[] { vertexIndex0, vertexIndex1, vertexIndex2 });
                        currentSubSubMesh.Item2.AddRange(new[] { featureIndex0, featureIndex1, featureIndex2 });
                        
                        addedToSubSubMesh = true;
                        break;
                    }
                }

                if (addedToSubSubMesh)
                    continue;
                else
                {
                    var newSubSub = new ValueTuple<List<int>, List<int>>()
                    {
                        Item1 = new List<int>(),
                        Item2 = new List<int>()
                    };
                    
                    newSubSub.Item1.AddRange(new[]{ vertexIndex0, vertexIndex1, vertexIndex2});
                    newSubSub.Item2.AddRange(new[] { featureIndex0, featureIndex1, featureIndex2 });
                        
                    currentSubMesh.Add(newSubSub);
                }
            }

            //
            // Merge submeshes which share borders
            //
            foreach (var subMeshes in subSubMeshTempArrangement)
            {
                // Every internal list is checked back and forth with all other list items to see if there are matching elements.
                for (var subSubIndex1 = 0; subSubIndex1 < subMeshes.Value.Count - 1; subSubIndex1++)
                {
                    // Current item is empty already/marked for deletion (i.e. merged into another list element)
                    if (!subMeshes.Value[subSubIndex1].Item1.Any())
                        continue;
                    
                    for (var subSubIndex2 = 0; subSubIndex2 < subMeshes.Value.Count; subSubIndex2++)
                    {
                        // Current item is empty already/marked for deletion (i.e. merged into another list element)
                        if (!subMeshes.Value[subSubIndex2].Item1.Any())
                            continue;
                        
                        // Do not check against yourself
                        if (subSubIndex1 == subSubIndex2)
                            continue;
                        
                        var curArr = subMeshes.Value[subSubIndex1];
                        var checkArr = subMeshes.Value[subSubIndex2];
                        if (!curArr.Item1.Intersect(checkArr.Item1).Any())
                            continue;

                        curArr.Item1.AddRange(checkArr.Item1);
                        curArr.Item2.AddRange(checkArr.Item2);
                        // Clear as it's merged into another. Do not delete now as it would add complexity to loop.
                        subMeshes.Value[subSubIndex2].Item1.Clear();
                        subMeshes.Value[subSubIndex2].Item2.Clear();
                    }
                }
            }

            foreach (var meshLoop in subSubMeshTempArrangement)
            {
                // The materialIndex was never used before.
                if (!returnMeshes.ContainsKey(meshLoop.Key))
                {
                    var newSubMesh = new WorldData.SubMeshData
                    {
                        materialIndex = meshLoop.Key,
                        material = world.materials[meshLoop.Key]
                    };

                    returnMeshes.Add(meshLoop.Key, new List<WorldData.SubMeshData>());
                }
                var currentSubMesh = returnMeshes[meshLoop.Key];

                foreach (var subMeshLoop in meshLoop.Value)
                {
                    // Skip empty triangle entry lists only. (e.g. grass is then shrunk from ~500 submeshes down to ~150)
                    if (!subMeshLoop.Item1.Any())
                        continue;
                    
                    var currentSubSubMesh = new WorldData.SubMeshData
                    {
                        materialIndex = meshLoop.Key,
                        material = world.materials[meshLoop.Key]
                    };
                    currentSubMesh.Add(currentSubSubMesh);

                    for (var pairId = 0; pairId < subMeshLoop.Item1.Count; pairId++)
                    {
                        var curVertexIndex = subMeshLoop.Item1[pairId];
                        var curFeatureIndex = subMeshLoop.Item2[pairId];
                        // For every vertexIndex we store a new vertex. (i.e. no reuse of Vector3-vertices for later texture/uv attachment)
                        currentSubSubMesh.vertices.Add(vertices[curVertexIndex].ToUnityVector());

                        currentSubSubMesh.uvs.Add(features[curFeatureIndex].texture.ToUnityVector());
                        currentSubSubMesh.normals.Add(features[curFeatureIndex].normal.ToUnityVector());

                        currentSubSubMesh.triangles.Add(currentSubSubMesh.vertices.Count - 1);
                    }
                }
            }

            // DebugPrint(returnMeshes, "new");
            
            return returnMeshes;
        }

        private void DebugPrint(Dictionary<int, List<WorldData.SubMeshData>> data, string suffix)
        {
            var fileWriter = new StreamWriter(Application.persistentDataPath + "/" + DateTime.Now.ToString("hh-mm-ss") + "-" + suffix + ".txt", false);

            foreach (var materialData in data)
            {
                fileWriter.WriteLine("newMaterial: " + materialData.Key);
                fileWriter.WriteLine("{");
                foreach (var subMeshData in materialData.Value)
                {
                    fileWriter.WriteLine("  newSubMesh");
                    fileWriter.WriteLine("  {");
                    for (var index=0; index<subMeshData.triangles.Count; index++)
                    {
                        fileWriter.WriteLine($"    vertex: {subMeshData.vertices[index]}");
                        fileWriter.WriteLine($"    uv:     {subMeshData.uvs[index]}");
                    }
                    fileWriter.WriteLine("  }");
                }
                fileWriter.WriteLine("}");
            }
            
            fileWriter.Close();
        }


#if UNITY_EDITOR
        /// <summary>
        /// Loads the world for occlusion culling.
        /// </summary>
        public void LoadEditorWorld(IntPtr vfsPtr, string zen)
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
            GameData.I.VfsPtr = vfsPtr;
            GameData.I.World = world;

            var worldGo = new GameObject("World");

            // We use SampleScene because it contains all the VM pointers and asset cache necesarry to generate the world
            var sampleScene = EditorSceneManager.GetSceneByName("Bootstrap");
            EditorSceneManager.SetActiveScene(sampleScene);
            sampleScene.GetRootGameObjects().Append(worldGo);
            
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