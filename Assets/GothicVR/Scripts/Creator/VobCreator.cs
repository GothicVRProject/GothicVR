using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GothicVR.Vob;
using GVR.Caches;
using GVR.Creator.Meshes;
using GVR.Debugging;
using GVR.Demo;
using GVR.Extensions;
using GVR.GothicVR.Scripts.Manager;
using GVR.Manager;
using GVR.Manager.Culling;
using GVR.Phoenix.Data;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Properties;
using GVR.Vob;
using GVR.Vob.WayNet;
using JetBrains.Annotations;
using PxCs.Data.Struct;
using PxCs.Data.Vm;
using PxCs.Data.Vob;
using PxCs.Interface;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit;
using Vector3 = System.Numerics.Vector3;

namespace GVR.Creator
{
    public static class VobCreator
    {
        private static Dictionary<PxWorld.PxVobType, GameObject> parentGosTeleport = new();
        private static Dictionary<PxWorld.PxVobType, GameObject> parentGosNonTeleport = new();

        private static PxWorld.PxVobType[] nonTeleportTypes =
        {
            PxWorld.PxVobType.PxVob_oCItem,
            PxWorld.PxVobType.PxVob_oCMobLadder,
            PxWorld.PxVobType.PxVob_oCZoneMusic,
            PxWorld.PxVobType.PxVob_zCVobSound,
            PxWorld.PxVobType.PxVob_zCVobSoundDaytime
        };

        private static int totalVObs;

        static VobCreator()
        {
            GvrSceneManager.I.sceneGeneralLoaded.AddListener(PostWorldLoaded);
        }

        private static void PostWorldLoaded()
        {
            // We need to check for all Sounds once, if they need to be activated as they're next to player.
            // As CullingGroup only triggers deactivation once player spawns, but not activation.
            if (!FeatureFlags.I.EnableSounds)
                return;

            var loc = Camera.main!.transform.position;
            foreach (var sound in LookupCache.vobSoundsAndDayTime.Where(i => i != null))
            {
                var soundLoc = sound.transform.position;
                var soundDist = sound.GetComponent<AudioSource>().maxDistance;
                var dist = UnityEngine.Vector3.Distance(loc, soundLoc);
                
                if (dist < soundDist)
                    sound.SetActive(true);
            }
        }
        
        private static int GetTotalVobCount(PxVobData[] vobs)
        {
            int count = vobs.Length;

            foreach (var vob in vobs)
            {
                count += GetTotalVobCount(vob.childVobs);
            }

            return count;
        }

        public static async Task CreateAsync(GameObject rootTeleport, GameObject rootNonTeleport, WorldData world,
            int vobsPerFrame)
        {
            if (!FeatureFlags.I.CreateVobs)
                return;

            var cullingVobObjects = new List<GameObject>();

            totalVObs = GetTotalVobCount(world.vobs);

            var vobRootTeleport = new GameObject("Vobs");
            var vobRootNonTeleport = new GameObject("Vobs");
            vobRootTeleport.SetParent(rootTeleport);
            vobRootNonTeleport.SetParent(rootNonTeleport);

            parentGosTeleport = new();
            parentGosNonTeleport = new();

            CreateParentVobObjectTeleport(vobRootTeleport);
            CreateParentVobObjectNonTeleport(vobRootNonTeleport);

            var allVobs = new List<PxVobData>();
            AddVobsToList(world.vobs, allVobs);

            var count = 0;
            foreach (var vob in allVobs)
            {
                GameObject go = null;
                
                switch (vob.type)
                {
                    case PxWorld.PxVobType.PxVob_oCItem:
                    {
                        var obj = CreateItem((PxVobItemData)vob);
                        cullingVobObjects.Add(obj);
                        break;
                    }
                    case PxWorld.PxVobType.PxVob_oCMobContainer:
                    {
                        var obj = CreateMobContainer((PxVobMobContainerData)vob);
                        cullingVobObjects.Add(obj);
                        break;
                    }
                    case PxWorld.PxVobType.PxVob_zCVobSound:
                    {
                        var obj = CreateSound((PxVobSoundData)vob);
                        LookupCache.vobSoundsAndDayTime.Add(obj);
                        break;
                    }
                    case PxWorld.PxVobType.PxVob_zCVobSoundDaytime:
                    {
                        var obj = CreateSoundDaytime((PxVobSoundDaytimeData)vob);
                        LookupCache.vobSoundsAndDayTime.Add(obj);
                        break;
                    }
                    case PxWorld.PxVobType.PxVob_oCZoneMusic:
                    {
                        go = CreateZoneMusic((PxVobZoneMusicData)vob);
                        break;
                    }
                    case PxWorld.PxVobType.PxVob_zCVobSpot:
                    case PxWorld.PxVobType.PxVob_zCVobStartpoint:
                    {
                        go = CreateSpot(vob);
                        break;
                    }
                    case PxWorld.PxVobType.PxVob_oCMobLadder:
                    {
                        var obj = CreateLadder(vob);
                        cullingVobObjects.Add(obj);
                        break;
                    }
                    case PxWorld.PxVobType.PxVob_oCTriggerChangeLevel:
                    {
                        go = CreateTriggerChangeLevel((PxVobTriggerChangeLevelData)vob);
                        break;
                    }
                    case PxWorld.PxVobType.PxVob_zCTriggerList:
                        CreateTriggerList((PxVobTriggerListData)vob);
                        break;
                    case PxWorld.PxVobType.PxVob_zCTriggerWorldStart:
                        CreateTriggerWorldStart((PxVobTriggerWorldStartData)vob);
                        break;
                    case PxWorld.PxVobType.PxVob_oCTriggerScript:
                        CreateTriggerScript((PxVobTriggerScriptData)vob);
                        break;
                    case PxWorld.PxVobType.PxVob_zCTrigger:
                    case PxWorld.PxVobType.PxVob_zCTriggerUntouch:
                        CreateTrigger((PxVobTriggerData)vob);
                        break;
                    case PxWorld.PxVobType.PxVob_zCVobScreenFX:
                    case PxWorld.PxVobType.PxVob_zCVobAnimate:
                    case PxWorld.PxVobType.PxVob_oCCSTrigger:
                    case PxWorld.PxVobType.PxVob_zCVobLensFlare:
                    case PxWorld.PxVobType.PxVob_zCVobLight:
                    case PxWorld.PxVobType.PxVob_zCMoverController:
                    case PxWorld.PxVobType.PxVob_zCPFXController:
                    {
                        // FIXME - not yet implemented.
                        break;
                    }
                    // Do nothing
                    case PxWorld.PxVobType.PxVob_zCVobLevelCompo:
                    {
                        break;
                    }
                    case PxWorld.PxVobType.PxVob_zCVob:
                    {
                        GameObject obj;
                        switch (vob.visualType)
                        {
                            case PxWorld.PxVobVisualType.PxVobVisualDecal:
                                obj = CreateDecal(vob);
                                break;
                            case PxWorld.PxVobVisualType.PxVobVisualParticleSystem:
                                obj = CreatePfx(vob);
                                break;
                            default:
                                obj = CreateDefaultMesh(vob);
                                break;
                        }
                        cullingVobObjects.Add(obj);
                        break;
                    }
                    case PxWorld.PxVobType.PxVob_oCMobInter:
                    default:
                    {
                        var obj = CreateDefaultMesh(vob);
                        cullingVobObjects.Add(obj);
                        break;
                    }
                }
                
                AddToMobInteractableList(vob, go);
                
                LoadingManager.I.AddProgress(LoadingManager.LoadingProgressType.VOb, 1f / totalVObs);

                if (++count % vobsPerFrame == 0)
                    await Task.Yield(); // Wait for the next frame
            }

            var nonNullCullingGroupItems = cullingVobObjects.Where(i => i != null).ToArray();
            VobMeshCullingManager.I.PrepareVobCulling(nonNullCullingGroupItems);
            VobSoundCullingManager.I.PrepareSoundCulling(LookupCache.vobSoundsAndDayTime);
            
            // TODO - Not implemented warnings - print them once only.
            foreach (var var in new[]{
                         PxWorld.PxVobType.PxVob_zCVobScreenFX,
                         PxWorld.PxVobType.PxVob_zCVobAnimate,
                         PxWorld.PxVobType.PxVob_zCTriggerWorldStart,
                         PxWorld.PxVobType.PxVob_zCTriggerList,
                         PxWorld.PxVobType.PxVob_oCCSTrigger,
                         PxWorld.PxVobType.PxVob_oCTriggerScript,
                         PxWorld.PxVobType.PxVob_zCVobLensFlare,
                         PxWorld.PxVobType.PxVob_zCVobLight,
                         PxWorld.PxVobType.PxVob_zCMoverController,
                         PxWorld.PxVobType.PxVob_zCPFXController
                     })
            {
                Debug.LogWarning($"{var} not yet implemented.");
            }
        }

        private static GameObject GetPrefab(PxVobData vob)
        {
            GameObject go;
            string name = vob.vobName;
            
            switch (vob.type)
            {
                case PxWorld.PxVobType.PxVob_oCItem:
                    go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobItem);
                    break;
                case PxWorld.PxVobType.PxVob_zCVobSpot:
                case PxWorld.PxVobType.PxVob_zCVobStartpoint:
                    go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobSpot);
                    break;
                case PxWorld.PxVobType.PxVob_zCVobSound:
                    go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobSound);
                    break;
                case PxWorld.PxVobType.PxVob_zCVobSoundDaytime:
                    go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobSoundDaytime);
                    break;
                case PxWorld.PxVobType.PxVob_oCZoneMusic:
                    go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobMusic);
                    break;
                case PxWorld.PxVobType.PxVob_oCMOB:
                case PxWorld.PxVobType.PxVob_oCMobFire:
                case PxWorld.PxVobType.PxVob_oCMobInter:
                case PxWorld.PxVobType.PxVob_oCMobBed:
                case PxWorld.PxVobType.PxVob_oCMobDoor:
                case PxWorld.PxVobType.PxVob_oCMobContainer:
                case PxWorld.PxVobType.PxVob_oCMobSwitch:
                case PxWorld.PxVobType.PxVob_oCMobWheel:
                    go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobInteractable);
                    break;
                default:
                    return new GameObject(name);
            }
            
            go.name = name;
            
            // Fill Property data into prefab here
            // Can also be outsourced to a proper method if it becomes a lot.
            go.GetComponent<VobProperties>().SetVisual(vob.visualName);
            
            return go;
        }

        private static void AddToMobInteractableList(PxVobData vob, GameObject go)
        {
            if (go == null)
                return;
            
            switch (vob.type)
            {
                case PxWorld.PxVobType.PxVob_oCMOB:
                case PxWorld.PxVobType.PxVob_oCMobFire:
                case PxWorld.PxVobType.PxVob_oCMobInter:
                case PxWorld.PxVobType.PxVob_oCMobBed:
                case PxWorld.PxVobType.PxVob_oCMobDoor:
                case PxWorld.PxVobType.PxVob_oCMobContainer:
                case PxWorld.PxVobType.PxVob_oCMobSwitch:
                case PxWorld.PxVobType.PxVob_oCMobWheel:
                    GameData.VobsInteractable.Add(go.GetComponent<VobProperties>());
                    break;
            }
        }

        private static void AddVobsToList(PxVobData[] vobs, List<PxVobData> allVobs)
        {
            foreach (var vob in vobs)
            {
                allVobs.Add(vob);
                AddVobsToList(vob.childVobs, allVobs);
            }
        }

        private static void CreateParentVobObjectTeleport(GameObject root)
        {
            var allTypes = (PxWorld.PxVobType[])Enum.GetValues(typeof(PxWorld.PxVobType));
            foreach (var type in allTypes.Except(nonTeleportTypes))
            {
                var newGo = new GameObject(type.ToString());
                newGo.SetParent(root);

                parentGosTeleport.Add(type, newGo);
            }
        }

        /// <summary>
        /// As PxVobType.PxVob_oCItem get Grabbable Component, they already own a Collider
        /// AND we don't want to teleport on top of them. We therefore exclude them from being added to Teleporter.
        /// </summary>
        private static void CreateParentVobObjectNonTeleport(GameObject root)
        {
            var allTypes = (PxWorld.PxVobType[])Enum.GetValues(typeof(PxWorld.PxVobType));
            foreach (var type in allTypes.Intersect(nonTeleportTypes))
            {
                var newGo = new GameObject(type.ToString());
                newGo.SetParent(root);

                parentGosNonTeleport.Add(type, newGo);
            }
        }

        /// <summary>
        /// Render item inside GameObject
        /// </summary>
        public static GameObject CreateItem(uint itemId, GameObject go)
        {
            var item = AssetCache.TryGetItemData(itemId);

            return CreateItemMesh(item, go);
        }
        
        [CanBeNull]
        private static GameObject CreateItem(PxVobItemData vob)
        {
            string itemName;

            if (!string.IsNullOrEmpty(vob.instance))
                itemName = vob.instance;
            else if (!string.IsNullOrEmpty(vob.vobName))
                itemName = vob.vobName;
            else
                throw new Exception("PxVobItemData -> no usable INSTANCE name found.");

            var item = AssetCache.TryGetItemData(itemName);

            if (item == null)
            {
                // eItMiCello is commented out on misc.d file. No need for an error log entry.
                if ("itmicello".Equals(itemName.ToLower()))
                    return null;
                
                Debug.LogError($"Item {itemName} not found.");
                return null;
            }

            if (item.visual!.ToLower().EndsWith(".mms"))
            {
                Debug.LogError($"Item {item.visual} is of type mms/mmb and we don't have a mesh creator to handle it properly (for now).");
                return null;
            }

            var prefabInstance = GetPrefab(vob);
            var vobObj = CreateItemMesh(vob, item, prefabInstance);

            if (vobObj == null)
            {
                GameObject.Destroy(prefabInstance); // No mesh created. Delete the prefab instance again.
                Debug.LogError($"There should be no! object which can't be found n:{vob.vobName} i:{vob.instance}. We need to use >PxVobItem.instance< to do it right!");
                return null;
            }

            // It will set some default values for collider and grabbing now.
            // Adding it now is easier than putting it on a prefab and updating it at runtime (as grabbing didn't work this way out-of-the-box).
            var grabComp = vobObj.AddComponent<XRGrabInteractable>();
            var eventComp = vobObj.GetComponent<ItemGrabInteractable>();
            var colliderComp = vobObj.GetComponent<MeshCollider>();

            vobObj.layer = ConstantsManager.ItemLayer;

            colliderComp.convex = true;
            grabComp.selectEntered.AddListener(eventComp.SelectEntered);
            grabComp.selectExited.AddListener(eventComp.SelectExited);

            return vobObj;
        }

        [CanBeNull]
        private static GameObject CreateMobContainer(PxVobMobContainerData vob)
        {
            var vobObj = CreateDefaultMesh(vob);

            if (vobObj == null)
            {
                Debug.LogWarning($"{vob.vobName} - mesh for MobContainer not found.");
                return null;
            }

            var lootComp = vobObj.AddComponent<DemoContainerLoot>();
            lootComp.SetContent(vob.contents);

            return vobObj;
        }

        // FIXME - change values for AudioClip based on Sfx and vob value (value overloads itself)
        [CanBeNull]
        private static GameObject CreateSound(PxVobSoundData vob)
        {
            if (!FeatureFlags.I.EnableSounds)
                return null;

            var go = GetPrefab(vob);
            go.name = $"{vob.soundName}";
            go.SetActive(false); // We don't want to have sound when we boot the game async for 30 seconds in non-spatial blend mode.
            go.SetParent(parentGosNonTeleport[vob.type]);
            SetPosAndRot(go, vob.position, vob.rotation);
            
            var source = go.GetComponent<AudioSource>();

            PrepareAudioSource(source, vob);
            source.clip = VobHelper.GetSoundClip(vob.soundName);

            go.GetComponent<VobSoundProperties>().soundData = vob;
            go.GetComponent<SoundHandler>().PrepareSoundHandling();

            return go;
        }

        /// <summary>
        /// FIXME - add specific daytime logic!
        /// Creating AudioSource from PxVobSoundDaytimeData is very similar to PxVobSoundData one.
        /// There are only two differences:
        ///     1. This one has two AudioSources
        ///     2. The sources will be toggled during gameplay when start/end time is reached.
        /// </summary>
        [CanBeNull]
        private static GameObject CreateSoundDaytime(PxVobSoundDaytimeData vob)
        {
            if (!FeatureFlags.I.EnableSounds)
                return null;

            var go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobSoundDaytime);
            go.name = $"{vob.soundName}-{vob.soundName2}";
            go.SetActive(false); // We don't want to have sound when we boot the game async for 30 seconds in non-spatial blend mode.
            go.SetParent(parentGosNonTeleport[vob.type]);
            SetPosAndRot(go, vob.position, vob.rotation);
            
            var sources = go.GetComponents<AudioSource>();

            PrepareAudioSource(sources[0], vob);
            sources[0].clip = VobHelper.GetSoundClip(vob.soundName);

            PrepareAudioSource(sources[1], vob);
            sources[1].clip = VobHelper.GetSoundClip(vob.soundName2);
            
            go.GetComponent<VobSoundDaytimeProperties>().soundDaytimeData = vob;
            go.GetComponent<SoundDaytimeHandler>().PrepareSoundHandling();

            return go;
        }

        private static void PrepareAudioSource(AudioSource source, PxVobSoundData soundData)
        {
            source.maxDistance = soundData.radius / 100f; // Gothic's values are in cm, Unity's in m.
            source.volume = soundData.volume / 100f; // Gothic's volume is 0...100, Unity's is 0...1. 

            source.loop = (soundData.mode == PxWorld.PxVobSoundMode.PxVobSoundModeLoop);
            source.playOnAwake = soundData.initiallyPlaying;
            source.spatialBlend = soundData.ambient3d ? 1f : 0f;
        }
        
        private static GameObject CreateZoneMusic(PxVobZoneMusicData vob)
        {
            var go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobMusic);
            go.SetParent(parentGosNonTeleport[vob.type], true, true);
            go.name = vob.vobName;

            go.layer = ConstantsManager.IgnoreRaycastLayer;

            var min = vob.boundingBox.min.ToUnityVector();
            var max = vob.boundingBox.max.ToUnityVector();

            go.transform.position = (min + max) / 2f;
            go.transform.localScale = (max - min);

            go.GetComponent<VobMusicProperties>().musicData = vob;

            return go;
        }

        private static GameObject CreateTriggerChangeLevel(PxVobTriggerChangeLevelData vob)
        {
            var vobObj = new GameObject(vob.vobName);
            vobObj.SetParent(parentGosTeleport[vob.type]);

            vobObj.layer = ConstantsManager.IgnoreRaycastLayer;

            var trigger = vobObj.AddComponent<BoxCollider>();
            trigger.isTrigger = true;

            var min = vob.boundingBox.min.ToUnityVector();
            var max = vob.boundingBox.max.ToUnityVector();
            vobObj.transform.position = (min + max) / 2f;

            vobObj.transform.localScale = (max - min);

            if (FeatureFlags.I.CreateVobs)
            {
                var triggerHandler = vobObj.AddComponent<ChangeLevelTriggerHandler>();
                triggerHandler.levelName = vob.levelName;
                triggerHandler.startVob = vob.startVob;
            }

            return vobObj;
        }

        private void CreateTriggerList(PxVobTriggerListData vob)
        {
            var gameObject = new GameObject(vob.vobName);
            gameObject.SetParent(parentGosNonTeleport[vob.type]);
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            var trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;

            var min = vob.boundingBox.min.ToUnityVector();
            var max = vob.boundingBox.max.ToUnityVector();
            gameObject.transform.position = (min + max) / 2f;

            gameObject.transform.localScale = (max - min);
        }

        private void CreateTriggerWorldStart(PxVobTriggerWorldStartData vob)
        {
            var gameObject = new GameObject(vob.vobName);
            gameObject.SetParent(parentGosNonTeleport[vob.type]);
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            var trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;

            var min = vob.boundingBox.min.ToUnityVector();
            var max = vob.boundingBox.max.ToUnityVector();
            gameObject.transform.position = (min + max) / 2f;

            gameObject.transform.localScale = (max - min);
        }

        private void CreateTriggerScript(PxVobTriggerScriptData vob)
        {
            var gameObject = new GameObject(vob.vobName);
            gameObject.SetParent(parentGosNonTeleport[vob.type]);
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            var trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;

            var min = vob.boundingBox.min.ToUnityVector();
            var max = vob.boundingBox.max.ToUnityVector();
            gameObject.transform.position = (min + max) / 2f;

            gameObject.transform.localScale = (max - min);
        }

        private void CreateTrigger(PxVobTriggerData vob)
        {
            var gameObject = new GameObject(vob.vobName);
            gameObject.SetParent(parentGosNonTeleport[vob.type]);
            gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

            var trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;

            var min = vob.boundingBox.min.ToUnityVector();
            var max = vob.boundingBox.max.ToUnityVector();
            gameObject.transform.position = (min + max) / 2f;

            gameObject.transform.localScale = (max - min);
        }

        /// <summary>
        /// Basically a free point where NPCs can do something like sitting on a bench etc.
        /// @see for more information: https://ataulien.github.io/Inside-Gothic/objects/spot/
        /// </summary>
        private static GameObject CreateSpot(PxVobData vob)
        {
            // FIXME - change to a Prefab in the future.
            var vobObj = GetPrefab(vob);

            if (!FeatureFlags.I.EnableVobFPMesh)
            {
                // Quick win: If we don't want to render the spots, we just remove the Renderer.
                GameObject.Destroy(vobObj.GetComponent<MeshRenderer>());
            }

            var fpName = vob.vobName != string.Empty ? vob.vobName : "START";
            vobObj.name = fpName;
            vobObj.SetParent(parentGosTeleport[vob.type]);

            var freePointData = new FreePoint()
            {
                Name = fpName,
                Position = vob.position.ToUnityVector()
            };
            vobObj.GetComponent<VobSpotProperties>().fp = freePointData;
            GameData.FreePoints.Add(fpName, freePointData);
            
            SetPosAndRot(vobObj, vob.position, vob.rotation);
            return vobObj;
        }

        private static GameObject CreateLadder(PxVobData vob)
        {
            // FIXME - use Prefab instead. And be cautious of settings!
            var vobObj = CreateDefaultMesh(vob, true);
            var meshGo = vobObj;
            var grabComp = meshGo.AddComponent<XRGrabInteractable>();
            var rigidbodyComp = meshGo.GetComponent<Rigidbody>();
            var meshColliderComp = vobObj.GetComponentInChildren<MeshCollider>();

            meshColliderComp.convex = true; // We need to set it to overcome Physics.ClosestPoint warnings.
            meshGo.tag = ConstantsManager.ClimbableTag;
            rigidbodyComp.isKinematic = true;
            grabComp.throwOnDetach = false; // Throws errors and isn't needed as we don't want to move the kinematic ladder when released.
            grabComp.trackPosition = false;
            grabComp.trackRotation = false;
            grabComp.selectMode = InteractableSelectMode.Multiple; // With this, we can grab with both hands!

            return vobObj;
        }
        
        private static GameObject CreateItemMesh(PxVobItemData vob, PxVmItemData item, GameObject go)
        {
            var mrm = AssetCache.TryGetMrm(item.visual);
            return VobMeshCreator.Create(item.visual, mrm, vob.position.ToUnityVector(), vob.rotation, true, parentGosNonTeleport[vob.type], go);
        }
        
        private static GameObject CreateItemMesh(PxVmItemData item, GameObject go)
        {
            var mrm = AssetCache.TryGetMrm(item.visual);
            return VobMeshCreator.Create(item.visual, mrm, default, default, false, parent: go);
        }

        private static GameObject CreateDecal(PxVobData vob)
        {
            if (!FeatureFlags.I.EnableDecals)
                return null;

            var parent = parentGosTeleport[vob.type];

            return VobMeshCreator.CreateDecal(vob, parent);
        }

        /// <summary>
        /// Please check description at worldofgothic for more details:
        /// https://www.worldofgothic.de/modifikation/index.php?go=particelfx
        /// </summary>
        private static GameObject CreatePfx(PxVobData vob)
        {
            if (!FeatureFlags.I.enableVobParticles)
                return null;

            // FIXME - move to non-teleport
            var parent = parentGosTeleport[vob.type];

            var pfxGo = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobPfx);
            pfxGo.name = vob.visualName;
            SetPosAndRot(pfxGo, vob.position, vob.rotation);
            pfxGo.SetParent(parent);

            var pfx = AssetCache.TryGetPfxData(vob.visualName);
            var particleSystem = pfxGo.GetComponent<ParticleSystem>();

            pfxGo.GetComponent<VobPfxProperties>().pfxData = pfx;

            particleSystem.Stop();

            var gravity = pfx.flyGravity.Split();
            float gravityX = 1f, gravityY = 1f, gravityZ = 1f;
            if (gravity.Length == 3)
            {
                // Gravity seems too low. Therefore *10k.
                gravityX = float.Parse(gravity[0], CultureInfo.InvariantCulture) * 10000;
                gravityY = float.Parse(gravity[1], CultureInfo.InvariantCulture) * 10000;
                gravityZ = float.Parse(gravity[2], CultureInfo.InvariantCulture) * 10000;
            }

            // Main module
            {
                var mainModule = particleSystem.main;
                var minLifeTime = (pfx.lspPartAvg - pfx.lspPartVar) / 1000; // I assume we need to change milliseconds to seconds.
                var maxLifeTime = (pfx.lspPartAvg + pfx.lspPartVar) / 1000;
                mainModule.duration = 1f; // I assume pfx data wants a cycle being 1 second long.
                mainModule.startLifetime = new (minLifeTime, maxLifeTime);
                mainModule.loop = pfx.ppsIsLooping;

                var minSpeed = (pfx.velAvg - pfx.velVar) / 1000;
                var maxSpeed = (pfx.velAvg + pfx.velVar) / 1000;
                mainModule.startSpeed = new(minSpeed, maxSpeed);
            }

            // Emission module
            {
                var emissionModule = particleSystem.emission;
                emissionModule.rateOverTime = new ParticleSystem.MinMaxCurve(pfx.ppsValue);
            }

            // Force over Lifetime module
            {
                var forceModule = particleSystem.forceOverLifetime;
                if (gravity.Length == 3)
                {
                    forceModule.enabled = true;
                    forceModule.x = gravityX;
                    forceModule.y = gravityY;
                    forceModule.z = gravityZ;
                }
            }

            // Color over Lifetime module
            {
                var colorOverTime = particleSystem.colorOverLifetime;
                colorOverTime.enabled = true;
                var gradient = new Gradient();
                var colorStart = pfx.visTexColorStart.Split();
                var colorEnd = pfx.visTexColorEnd.Split();
                gradient.SetKeys(
                    new GradientColorKey[]
                    {
                        new GradientColorKey(
                            new Color(float.Parse(colorStart[0]) / 255, float.Parse(colorStart[1]) / 255,
                                float.Parse(colorStart[2]) / 255),
                            0f),
                        new GradientColorKey(
                            new Color(float.Parse(colorEnd[0]) / 255, float.Parse(colorEnd[1]) / 255,
                                float.Parse(colorEnd[2]) / 255), 1f)
                    },
                    new GradientAlphaKey[]
                    {
                        new GradientAlphaKey(pfx.visAlphaStart / 255, 0),
                        new GradientAlphaKey(pfx.visAlphaEnd / 255, 1),
                    });
                colorOverTime.color = gradient;
            }

            // Size over lifetime module
            {
                var sizeOverTime = particleSystem.sizeOverLifetime;
                sizeOverTime.enabled = true;

                AnimationCurve curve = new AnimationCurve();
                var shapeScaleKeys = pfx.shpScaleKeys.Split();
                if (shapeScaleKeys.Length > 1 && pfx.shpScaleKeys != "")
                {
                    var curveTime = 0f;

                    for (var i = 0; i < shapeScaleKeys.Length; i++)
                    {
                        curve.AddKey(curveTime, float.Parse(shapeScaleKeys[i]) / 100 * float.Parse(pfx.shpDim));
                        curveTime += 1f / shapeScaleKeys.Length;
                    }

                    sizeOverTime.size = new ParticleSystem.MinMaxCurve(1f, curve);
                }
            }

            // Renderer module
            {
                var rendererModule = pfxGo.GetComponent<ParticleSystemRenderer>();
                // FIXME - Move to a cached constant value
                var standardShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
                var material = new Material(standardShader);
                rendererModule.material = material;
                TextureManager.I.SetTexture(pfx.visName, rendererModule.material);
                // renderer.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest; // First check with no change.

                switch (pfx.visAlphaFunc.ToUpper())
                {
                    case "BLEND":
                        rendererModule.material.ToTransparentMode(); // e.g. leaves.pfx.
                        break;
                    case "ADD":
                        rendererModule.material.ToAdditiveMode();
                        break;
                    default:
                        Debug.LogWarning($"Particle AlphaFunc {pfx.visAlphaFunc} not yet handled.");
                        break;
                }
                // makes the material render both faces
                rendererModule.material.SetInt("_Cull", (int)CullMode.Off);

                switch (pfx.visOrientation)
                {
                    case "NONE":
                        rendererModule.alignment = ParticleSystemRenderSpace.View;
                        break;
                    case "WORLD":
                        rendererModule.alignment = ParticleSystemRenderSpace.World;
                        break;
                    case "VELO":
                        rendererModule.alignment = ParticleSystemRenderSpace.Velocity;
                        break;
                    default:
                        Debug.LogWarning($"visOrientation {pfx.visOrientation} not yet handled.");
                        break;
                }
            }

            // Shape module
            {
                var shapeModule = particleSystem.shape;
                switch (pfx.shpType.ToUpper())
                {
                    case "SPHERE":
                        shapeModule.shapeType = ParticleSystemShapeType.Sphere;
                        break;
                    case "CIRCLE":
                        shapeModule.shapeType = ParticleSystemShapeType.Circle;
                        break;
                    case "MESH":
                        shapeModule.shapeType = ParticleSystemShapeType.Mesh;
                        break;
                    default:
                        Debug.LogWarning($"Particle ShapeType {pfx.shpType} not yet handled.");
                        break;
                }

                var shapeDimensions = pfx.shpDim.Split();
                switch (shapeDimensions.Length)
                {
                    case 1:
                        shapeModule.radius = float.Parse(shapeDimensions[0], CultureInfo.InvariantCulture) / 100; // cm in m
                        break;
                    default:
                        Debug.LogWarning($"shpDim >{pfx.shpDim}< not yet handled");
                        break;
                }

                shapeModule.rotation = new(pfx.dirAngleElev, 0, 0);

                var shapeOffsetVec = pfx.shpOffsetVec.Split();
                if (float.TryParse(shapeOffsetVec[0], out var x) && float.TryParse(shapeOffsetVec[1], out var y) &&
                    float.TryParse(shapeOffsetVec[2], out var z))
                    shapeModule.position = new UnityEngine.Vector3(x / 100, y / 100, z / 100);
                else
                    Debug.LogError(
                        "One or more of the shape offset vector components could not be parsed into a float");

                shapeModule.alignToDirection = true;

                shapeModule.radiusThickness = pfx.shpIsVolume ? 1f : 0f;
            }

            particleSystem.Play();

            return pfxGo;
        }

        private static GameObject CreateDefaultMesh(PxVobData vob, bool nonTeleport = false)
        {
            var parent = nonTeleport ? parentGosNonTeleport[vob.type] : parentGosTeleport[vob.type];
            var meshName = vob.showVisual ? vob.visualName : vob.vobName;

            if (meshName == string.Empty)
                return null;

            // MDL
            var mdl = AssetCache.TryGetMdl(meshName);
            if (mdl != null)
            {
                var go = GetPrefab(vob);
                var ret = VobMeshCreator.Create(meshName, mdl, vob.position.ToUnityVector(), vob.rotation.ToUnityMatrix().rotation, parent, go);
                
                // A few objects are broken and have no meshes. We need to destroy them immediately again.
                if (ret == null)
                    GameObject.Destroy(go);

                return ret;
            }

            // MDH+MDM (without MDL as wrapper)
            var mdh = AssetCache.TryGetMdh(meshName);
            var mdm = AssetCache.TryGetMdm(meshName);
            if (mdh != null && mdm != null)
            {
                return VobMeshCreator.Create(meshName, mdm, mdh, vob.position.ToUnityVector(),
                    vob.rotation!.ToUnityMatrix().rotation, parent);
            }

            // MRM
            var mrm = AssetCache.TryGetMrm(meshName);
            if (mrm != null)
            {
                // If the object is a dynamic one, it will collide.
                var withCollider = vob.cdDynamic;

                var go = GetPrefab(vob);
                var ret = VobMeshCreator.Create(meshName, mrm, vob.position.ToUnityVector(), vob.rotation, withCollider, parent, go);

                // A few objects are broken and have no meshes. We need to destroy them immediately again.
                if (ret == null)
                    GameObject.Destroy(go);

                return ret;
            }

            Debug.LogWarning($">{meshName}<'s has no mdl/mrm.");
            return null;
        }
        
        private static void SetPosAndRot(GameObject obj, Vector3 position, PxMatrix3x3Data rotation)
        {
            SetPosAndRot(obj, position.ToUnityVector(), rotation.ToUnityMatrix().rotation);
        }

        private static void SetPosAndRot(GameObject obj, UnityEngine.Vector3 position, Quaternion rotation)
        {
            // FIXME - This isn't working - but really needed?
            if (position.Equals(default) && rotation.Equals(default))
                return;

            obj.transform.SetLocalPositionAndRotation(position, rotation);
        }
    }
}
