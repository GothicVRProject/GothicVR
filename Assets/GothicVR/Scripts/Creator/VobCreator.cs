using System;
using System.Collections.Generic;
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
using GVR.Util;
using GVR.Vob;
using GVR.Vob.WayNet;
using JetBrains.Annotations;
using PxCs.Data.Sound;
using PxCs.Data.Struct;
using PxCs.Data.Vm;
using PxCs.Data.Vob;
using PxCs.Interface;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Vector3 = System.Numerics.Vector3;

namespace GVR.Creator
{
    public class VobCreator : SingletonBehaviour<VobCreator>
    {
        private AssetCache assetCache;

        private Dictionary<PxWorld.PxVobType, GameObject> parentGosTeleport = new();
        private Dictionary<PxWorld.PxVobType, GameObject> parentGosNonTeleport = new();

        private PxWorld.PxVobType[] nonTeleportTypes =
        {
            PxWorld.PxVobType.PxVob_oCItem,
            PxWorld.PxVobType.PxVob_oCMobLadder,
            PxWorld.PxVobType.PxVob_oCZoneMusic,
            PxWorld.PxVobType.PxVob_zCVobSound,
            PxWorld.PxVobType.PxVob_zCVobSoundDaytime
        };

        private int totalVObs;

        private void Start()
        {
            assetCache = AssetCache.I;
        }

        private int GetTotalVobCount(PxVobData[] vobs)
        {
            int count = vobs.Length;

            foreach (var vob in vobs)
            {
                count += GetTotalVobCount(vob.childVobs);
            }

            return count;
        }

        public async Task CreateAsync(GameObject rootTeleport, GameObject rootNonTeleport, WorldData world,
            int vobsPerFrame)
        {
            if (!FeatureFlags.I.CreateVobs)
                return;

            var cullingGroupObjects = new List<GameObject>();
            var cullingSoundObjects = new List<GameObject>();

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
                        cullingGroupObjects.Add(obj);
                        break;
                    }
                    case PxWorld.PxVobType.PxVob_oCMobContainer:
                    {
                        var obj = CreateMobContainer((PxVobMobContainerData)vob);
                        cullingGroupObjects.Add(obj);
                        break;
                    }
                    case PxWorld.PxVobType.PxVob_zCVobSound:
                    {
                        var obj = CreateSound((PxVobSoundData)vob);
                        cullingSoundObjects.Add(obj);
                        break;
                    }
                    case PxWorld.PxVobType.PxVob_zCVobSoundDaytime:
                    {
                        var obj = CreateSoundDaytime((PxVobSoundDaytimeData)vob);
                        cullingSoundObjects.Add(obj);
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
                        cullingGroupObjects.Add(obj);
                        break;
                    }
                    case PxWorld.PxVobType.PxVob_oCTriggerChangeLevel:
                    {
                        go = CreateTriggerChangeLevel((PxVobTriggerChangeLevelData)vob);
                        break;
                    }
                    case PxWorld.PxVobType.PxVob_zCVobScreenFX:
                    case PxWorld.PxVobType.PxVob_zCVobAnimate:
                    case PxWorld.PxVobType.PxVob_zCTriggerWorldStart:
                    case PxWorld.PxVobType.PxVob_zCTriggerList:
                    case PxWorld.PxVobType.PxVob_oCCSTrigger:
                    case PxWorld.PxVobType.PxVob_oCTriggerScript:
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
                        if (vob.visualType == PxWorld.PxVobVisualType.PxVobVisualDecal)
                            obj = CreateDecal(vob);
                        else
                            obj = CreateDefaultMesh(vob);
                        
                        cullingGroupObjects.Add(obj);
                        break;
                    }
                    case PxWorld.PxVobType.PxVob_oCMobInter:
                    default:
                    {
                        var obj = CreateDefaultMesh(vob);
                        cullingGroupObjects.Add(obj);
                        break;
                    }
                }
                
                AddToMobInteractableList(vob, go);
                
                LoadingManager.I.AddProgress(LoadingManager.LoadingProgressType.VOb, 1f / totalVObs);

                if (++count % vobsPerFrame == 0)
                    await Task.Yield(); // Wait for the next frame
            }

            var nonNullCullingGroupItems = cullingGroupObjects.Where(i => i != null).ToArray();
            VobMeshCullingManager.I.PrepareVobCulling(nonNullCullingGroupItems);
            
            VobSoundCullingManager.I.PrepareSoundCulling(cullingSoundObjects);
            
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

        private GameObject GetPrefab(PxVobData vob)
        {
            GameObject go;
            string name = vob.vobName;
            
            switch (vob.type)
            {
                case PxWorld.PxVobType.PxVob_oCItem:
                    go = PrefabCache.I.TryGetObject(PrefabCache.PrefabType.VobItem);
                    break;
                case PxWorld.PxVobType.PxVob_zCVobSpot:
                case PxWorld.PxVobType.PxVob_zCVobStartpoint:
                    go = PrefabCache.I.TryGetObject(PrefabCache.PrefabType.VobSpot);
                    break;
                case PxWorld.PxVobType.PxVob_zCVobSound:
                    go = PrefabCache.I.TryGetObject(PrefabCache.PrefabType.VobSound);
                    break;
                case PxWorld.PxVobType.PxVob_zCVobSoundDaytime:
                    go = PrefabCache.I.TryGetObject(PrefabCache.PrefabType.VobSoundDaytime);
                    break;
                case PxWorld.PxVobType.PxVob_oCZoneMusic:
                    go = PrefabCache.I.TryGetObject(PrefabCache.PrefabType.VobMusic);
                    break;
                case PxWorld.PxVobType.PxVob_oCMOB:
                case PxWorld.PxVobType.PxVob_oCMobFire:
                case PxWorld.PxVobType.PxVob_oCMobInter:
                case PxWorld.PxVobType.PxVob_oCMobBed:
                case PxWorld.PxVobType.PxVob_oCMobDoor:
                case PxWorld.PxVobType.PxVob_oCMobContainer:
                case PxWorld.PxVobType.PxVob_oCMobSwitch:
                case PxWorld.PxVobType.PxVob_oCMobWheel:
                    go = PrefabCache.I.TryGetObject(PrefabCache.PrefabType.VobInteractable);
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

        private void AddToMobInteractableList(PxVobData vob, GameObject go)
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
                    GameData.I.VobsInteractable.Add(go.GetComponent<VobProperties>());
                    break;
            }
        }

        private void AddVobsToList(PxVobData[] vobs, List<PxVobData> allVobs)
        {
            foreach (var vob in vobs)
            {
                allVobs.Add(vob);
                AddVobsToList(vob.childVobs, allVobs);
            }
        }

        private void CreateParentVobObjectTeleport(GameObject root)
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
        private void CreateParentVobObjectNonTeleport(GameObject root)
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
        public GameObject CreateItem(uint itemId, GameObject go)
        {
            var item = assetCache.TryGetItemData(itemId);

            return CreateItemMesh(item, go);
        }
        
        [CanBeNull]
        private GameObject CreateItem(PxVobItemData vob)
        {
            string itemName;

            if (!string.IsNullOrEmpty(vob.instance))
                itemName = vob.instance;
            else if (!string.IsNullOrEmpty(vob.vobName))
                itemName = vob.vobName;
            else
                throw new Exception("PxVobItemData -> no usable INSTANCE name found.");

            var item = assetCache.TryGetItemData(itemName);

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
                Destroy(prefabInstance); // No mesh created. Delete the prefab instance again.
                Debug.LogError($"There should be no! object which can't be found n:{vob.vobName} i:{vob.instance}. We need to use >PxVobItem.instance< to do it right!");
                return null;
            }

            // It will set some default values for collider and grabbing now.
            // Adding it now is easier than putting it on a prefab and updating it at runtime (as grabbing didn't work this way out-of-the-box).
            var grabComp = vobObj.AddComponent<XRGrabInteractable>();
            var eventComp = vobObj.GetComponent<ItemGrabInteractable>();
            var colliderComp = vobObj.GetComponent<MeshCollider>();

            vobObj.layer = ConstantsManager.I.ItemLayer;

            colliderComp.convex = true;
            grabComp.selectEntered.AddListener(eventComp.SelectEntered);
            grabComp.selectExited.AddListener(eventComp.SelectExited);

            return vobObj;
        }

        [CanBeNull]
        private GameObject CreateMobContainer(PxVobMobContainerData vob)
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
        private GameObject CreateSound(PxVobSoundData vob)
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
            source.clip = VobManager.I.GetSoundClip(vob.soundName);

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
        private GameObject CreateSoundDaytime(PxVobSoundDaytimeData vob)
        {
            if (!FeatureFlags.I.EnableSounds)
                return null;

            var go = PrefabCache.I.TryGetObject(PrefabCache.PrefabType.VobSoundDaytime);
            go.name = $"{vob.soundName}-{vob.soundName2}";
            go.SetActive(false); // We don't want to have sound when we boot the game async for 30 seconds in non-spatial blend mode.
            go.SetParent(parentGosNonTeleport[vob.type]);
            SetPosAndRot(go, vob.position, vob.rotation);
            
            var sources = go.GetComponents<AudioSource>();

            PrepareAudioSource(sources[0], vob);
            sources[0].clip = VobManager.I.GetSoundClip(vob.soundName);

            PrepareAudioSource(sources[1], vob);
            sources[1].clip = VobManager.I.GetSoundClip(vob.soundName2);
            
            go.GetComponent<VobSoundDaytimeProperties>().soundDaytimeData = vob;
            go.GetComponent<SoundDaytimeHandler>().PrepareSoundHandling();

            return go;
        }

        private void PrepareAudioSource(AudioSource source, PxVobSoundData soundData)
        {
            source.maxDistance = soundData.radius / 100f; // Gothic's values are in cm, Unity's in m.
            source.volume = soundData.volume / 100f; // Gothic's volume is 0...100, Unity's is 0...1. 

            source.loop = (soundData.mode == PxWorld.PxVobSoundMode.PxVobSoundModeLoop);
            source.playOnAwake = soundData.initiallyPlaying;
            source.spatialBlend = soundData.ambient3d ? 1f : 0f;
        }
        
        private GameObject CreateZoneMusic(PxVobZoneMusicData vob)
        {
            var go = PrefabCache.I.TryGetObject(PrefabCache.PrefabType.VobMusic);
            go.SetParent(parentGosNonTeleport[vob.type], true, true);
            go.name = vob.vobName;
            
            var min = vob.boundingBox.min.ToUnityVector();
            var max = vob.boundingBox.max.ToUnityVector();

            go.transform.position = (min + max) / 2f;
            go.transform.localScale = (max - min);

            go.GetComponent<VobMusicProperties>().musicData = vob;

            return go;
        }

        private GameObject CreateTriggerChangeLevel(PxVobTriggerChangeLevelData vob)
        {
            var vobObj = new GameObject(vob.vobName);
            gameObject.SetParent(parentGosTeleport[vob.type]);

            var trigger = gameObject.AddComponent<BoxCollider>();
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

        /// <summary>
        /// Basically a free point where NPCs can do something like sitting on a bench etc.
        /// @see for more information: https://ataulien.github.io/Inside-Gothic/objects/spot/
        /// </summary>
        private GameObject CreateSpot(PxVobData vob)
        {
            // FIXME - change to a Prefab in the future.
            var vobObj = GetPrefab(vob);

            if (!FeatureFlags.I.EnableVobFPMesh)
            {
                // Quick win: If we don't want to render the spots, we just remove the Renderer.
                Destroy(vobObj.GetComponent<MeshRenderer>());
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
            GameData.I.FreePoints.Add(fpName, freePointData);
            
            SetPosAndRot(vobObj, vob.position, vob.rotation);
            return vobObj;
        }

        private GameObject CreateLadder(PxVobData vob)
        {
            // FIXME - use Prefab instead.
            var vobObj = CreateDefaultMesh(vob, true);
            var meshGo = vobObj;
            var grabComp = meshGo.AddComponent<XRGrabInteractable>();
            var rigidbodyComp = meshGo.GetComponent<Rigidbody>();

            meshGo.tag = "Climbable";
            rigidbodyComp.isKinematic = true;
            grabComp.trackPosition = false;
            grabComp.trackRotation = false;

            return vobObj;
        }
        
        private GameObject CreateItemMesh(PxVobItemData vob, PxVmItemData item, GameObject go)
        {
            var mrm = assetCache.TryGetMrm(item.visual);
            return VobMeshCreator.I.Create(item.visual, mrm, vob.position.ToUnityVector(), vob.rotation, true, parentGosNonTeleport[vob.type], go);
        }
        
        private GameObject CreateItemMesh(PxVmItemData item, GameObject go)
        {
            var mrm = assetCache.TryGetMrm(item.visual);
            return VobMeshCreator.I.Create(item.visual, mrm, default, default, false, parent: go);
        }

        private GameObject CreateDecal(PxVobData vob)
        {
            if (!FeatureFlags.I.EnableDecals)
                return null;


            var parent = parentGosTeleport[vob.type];

            return VobMeshCreator.I.CreateDecal(vob, parent);
        }

        private GameObject CreateDefaultMesh(PxVobData vob, bool nonTeleport = false)
        {
            var parent = nonTeleport ? parentGosNonTeleport[vob.type] : parentGosTeleport[vob.type];
            var meshName = vob.showVisual ? vob.visualName : vob.vobName;

            if (meshName == string.Empty)
                return null;
            
            // FIXME - PFX effects not yet implemented
            if (meshName.ToLower().EndsWith(".pfx"))
                return null;

            // MDL
            var mdl = assetCache.TryGetMdl(meshName);
            if (mdl != null)
            {
                var go = GetPrefab(vob);
                var ret = VobMeshCreator.I.Create(meshName, mdl, vob.position.ToUnityVector(), vob.rotation.ToUnityMatrix().rotation, parent, go);
                
                // A few objects are broken and have no meshes. We need to destroy them immediately again.
                if (ret == null)
                    Destroy(go);

                return ret;
            }

            // MDH+MDM (without MDL as wrapper)
            var mdh = assetCache.TryGetMdh(meshName);
            var mdm = assetCache.TryGetMdm(meshName);
            if (mdh != null && mdm != null)
            {
                return VobMeshCreator.I.Create(meshName, mdm, mdh, vob.position.ToUnityVector(),
                    vob.rotation!.ToUnityMatrix().rotation, parent);
            }

            // MRM
            var mrm = assetCache.TryGetMrm(meshName);
            if (mrm != null)
            {
                // If the object is a dynamic one, it will collide.
                var withCollider = false; // vob.cdDynamic; // We will create separate colliders for ZS(lots) instead of mesh. 

                var go = GetPrefab(vob);
                var ret = VobMeshCreator.I.Create(meshName, mrm, vob.position.ToUnityVector(), vob.rotation, withCollider, parent, go);

                // A few objects are broken and have no meshes. We need to destroy them immediately again.
                if (ret == null)
                    Destroy(go);

                return ret;
            }

            Debug.LogWarning($">{meshName}<'s has no mdl/mrm.");
            return null;
        }
        
        private void SetPosAndRot(GameObject obj, Vector3 position, PxMatrix3x3Data rotation)
        {
            SetPosAndRot(obj, position.ToUnityVector(), rotation.ToUnityMatrix().rotation);
        }

        private void SetPosAndRot(GameObject obj, UnityEngine.Vector3 position, Quaternion rotation)
        {
            // FIXME - This isn't working - but really needed?
            if (position.Equals(default) && rotation.Equals(default))
                return;

            obj.transform.SetLocalPositionAndRotation(position, rotation);
        }
    }
}
