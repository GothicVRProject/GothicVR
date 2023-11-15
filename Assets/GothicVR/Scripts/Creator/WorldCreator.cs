using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using GVR.Creator.Meshes;
using GVR.Debugging;
using GVR.Extensions;
using GVR.Manager;
using GVR.Phoenix.Data;
using GVR.Phoenix.Interface;
using PxCs.Data.WayNet;
using PxCs.Interface;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.SceneManagement;
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
            /*DijkstraWayPointCreator.Create(world);      //Is there a better place for this call? this might still work  */ 

            DebugAnimationCreator.Create(worldName);
            DebugAnimationCreatorBSFire.Create(worldName);
            DebugAnimationCreatorVelaya.Create(worldName);
            DebugAnimationCreatorBloodwyn.Create();

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
            var worldPtr = PxWorld.pxWorldLoadFromVfs(GameData.VfsPtr, worldName);
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

            if (FeatureFlags.I && FeatureFlags.I.enableFineGrainedWorldMeshCreation)
            {
                var subMeshes = CreateSubMeshesForUnityExperimental(world);
                world.subMeshes = subMeshes;
            }
            else
            {
                var subMeshes = CreateSubMeshesForUnityStable(world);
                world.subMeshes = subMeshes;
            }

            PxWorld.pxWorldDestroy(worldPtr);

            return world;
        }

        private static Dictionary<int, List<WorldData.SubMeshData>> CreateSubMeshesForUnityStable(WorldData world)
        {
            Dictionary<int, List<WorldData.SubMeshData>> subMeshes = new(world.materials.Length);
            var vertices = world.vertices;
            var vertexIndices = world.vertexIndices;
            var featureIndices = world.featureIndices;
            var features = world.features;

            // We need to put vertex_indices (aka triangles) in reversed order
            // to make Unity draw mesh elements right (instead of upside down)
            for (var loopVertexIndexId = vertexIndices.LongLength - 1; loopVertexIndexId >= 0; loopVertexIndexId -= 3)
            {
                // For each 3 vertexIndices (aka each triangle) there's one materialIndex.
                var materialIndex = world.materialIndices[loopVertexIndexId / 3];

                // DEBUG! Some elements to test subMeshing
                // if (materialIndex != 60) // 60 - some walls, 4 - grass
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

        /// <summary>
        /// This method is for initial collection of vertexIndex and featureIndex based on materialId
        /// In a first step, we just collect all the indexes
        /// In a second step, we try to merge elements together, which share vertexIndex
        /// Then it will be transformed into WorldData.SubMeshData as known before
        /// </summary>
        private static Dictionary<int, List<WorldData.SubMeshData>> CreateSubMeshesForUnityExperimental(WorldData world)
        {
            // Dict<materialId, List<KVP<List<vertexIndex>, List<featureIndex>>>>
            Dictionary<int, List<ValueTuple<List<int>, List<int>>>> subSubMeshTempArrangement = new();

            Dictionary<int, List<WorldData.SubMeshData>> returnMeshes = new(world.materials.Length);
            var vertices = world.vertices;
            var vertexIndices = world.vertexIndices;
            var featureIndices = world.featureIndices;
            var features = world.features;

            // We need to put vertex_indices (aka triangles) in reversed order
            // to make Unity draw mesh elements right (instead of upside down)
            for (var loopVertexIndexId = vertexIndices.LongLength - 1; loopVertexIndexId >= 0; loopVertexIndexId -= 3)
            {
                // For each 3 vertexIndices (aka each triangle) there's one materialIndex.
                var materialIndex = world.materialIndices[loopVertexIndexId / 3];

                // DEBUG! Some elements to test subMeshing
                // if (materialIndex != 60) // 60 - some walls, 4 - grass
                //     continue;

                // The materialIndex was never used before.
                if (!subSubMeshTempArrangement.ContainsKey(materialIndex))
                    subSubMeshTempArrangement.Add(materialIndex, new List<ValueTuple<List<int>, List<int>>>());

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

                    newSubSub.Item1.AddRange(new[] { vertexIndex0, vertexIndex1, vertexIndex2 });
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

            return returnMeshes;
        }

        private static void DebugPrint(Dictionary<int, List<WorldData.SubMeshData>> data, string suffix)
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
                    for (var index = 0; index < subMeshData.triangles.Count; index++)
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