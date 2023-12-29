using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GothicVR.Vob;
using GVR.Caches;
using GVR.Debugging;
using GVR.Demo;
using GVR.Extensions;
using GVR.Globals;
using GVR.GothicVR.Scripts.Manager;
using GVR.Manager;
using GVR.Manager.Culling;
using GVR.Phoenix.Data;
using GVR.Phoenix.Util;
using GVR.Properties;
using GVR.Vob;
using GVR.Vob.WayNet;
using JetBrains.Annotations;
using PxCs.Interface;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit;
using ZenKit.Daedalus;
using ZenKit.Vobs;
using Debug = UnityEngine.Debug;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace GVR.Creator
{
    public static class VobCreator
    {
        private static Dictionary<VirtualObjectType, GameObject> parentGosTeleport = new();
        private static Dictionary<VirtualObjectType, GameObject> parentGosNonTeleport = new();

        private static VirtualObjectType[] nonTeleportTypes =
        {
            VirtualObjectType.oCItem,
            VirtualObjectType.oCMobLadder,
            VirtualObjectType.oCZoneMusic,
            VirtualObjectType.oCZoneMusicDefault,
            VirtualObjectType.zCVobSound,
            VirtualObjectType.zCVobSoundDaytime
        };

        private static int _totalVObs;
        private static int _vobsPerFrame;
        private static int _createdCount;
        private static List<GameObject> _cullingVobObjects = new();

        static VobCreator()
        {
            GVREvents.GeneralSceneLoaded.AddListener(PostWorldLoaded);
        }

        private static void PostWorldLoaded()
        {
            // We need to check for all Sounds once, if they need to be activated as they're next to player.
            // As CullingGroup only triggers deactivation once player spawns, but not activation.
            if (!FeatureFlags.I.enableSounds)
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

        public static async Task CreateAsync(GameObject rootTeleport, GameObject rootNonTeleport, WorldData world,
            int vobsPerFrame)
        {
            PreCreateVobs(world, rootTeleport, rootNonTeleport, vobsPerFrame);
            await CreateVobs(world.vobs);
            PostCreateVobs();
        }

        private static void PreCreateVobs(WorldData world, GameObject rootTeleport, GameObject rootNonTeleport, int vobsPerFrame)
        {
            // HINT - We assume there is only one nested level. At least works in G1 world.zen
            _totalVObs = world.vobs.Count + (int)world.vobs.Sum(i => (decimal)i.ChildCount);

            _createdCount = 0;
            _cullingVobObjects.Clear();
            _vobsPerFrame = vobsPerFrame;

            var vobRootTeleport = new GameObject("Vobs");
            var vobRootNonTeleport = new GameObject("Vobs");
            vobRootTeleport.SetParent(rootTeleport);
            vobRootNonTeleport.SetParent(rootNonTeleport);

            parentGosTeleport = new();
            parentGosNonTeleport = new();

            CreateParentVobObjectTeleport(vobRootTeleport);
            CreateParentVobObjectNonTeleport(vobRootNonTeleport);
        }
        
        private static async Task CreateVobs(List<IVirtualObject> vobs)
        {
            foreach (var vob in vobs)
            {
                GameObject go = null;
                
                switch (vob.Type)
                {
                    case VirtualObjectType.oCItem:
                    {
                        var obj = CreateItem((Item)vob);
                        _cullingVobObjects.Add(obj);
                        break;
                    }
                    case VirtualObjectType.oCMobContainer:
                    {
                        var obj = CreateMobContainer((Container)vob);
                        _cullingVobObjects.Add(obj);
                        break;
                    }
                    case VirtualObjectType.zCVobSound:
                    {
                        var obj = CreateSound((Sound)vob);
                        LookupCache.vobSoundsAndDayTime.Add(obj);
                        break;
                    }
                    case VirtualObjectType.zCVobSoundDaytime:
                    {
                        var obj = CreateSoundDaytime((SoundDaytime)vob);
                        LookupCache.vobSoundsAndDayTime.Add(obj);
                        break;
                    }
                    case VirtualObjectType.oCZoneMusic:
                    case VirtualObjectType.oCZoneMusicDefault:
                    {
                        go = CreateZoneMusic((ZoneMusic)vob);
                        break;
                    }
                    case VirtualObjectType.zCVobSpot:
                    case VirtualObjectType.zCVobStartpoint:
                    {
                        go = CreateSpot(vob);
                        break;
                    }
                    case VirtualObjectType.oCMobLadder:
                    {
                        var obj = CreateLadder(vob);
                        _cullingVobObjects.Add(obj);
                        break;
                    }
                    case VirtualObjectType.oCTriggerChangeLevel:
                    {
                        go = CreateTriggerChangeLevel((TriggerChangeLevel)vob);
                        break;
                    }
                    case VirtualObjectType.zCVob:
                    {
                        GameObject obj;

                        if (vob.Visual == null)
                        {
                            CreateDebugObject(vob);
                            break;
                        }
                            
                        switch (vob.Visual!.Type)
                        {
                            case VisualType.Decal:
                                obj = CreateDecal(vob);
                                break;
                            case VisualType.ParticleEffect:
                                obj = CreatePfx(vob);
                                break;
                            default:
                                obj = CreateDefaultMesh(vob);
                                break;
                        }
                        _cullingVobObjects.Add(obj);
                        break;
                    }
                    case VirtualObjectType.oCMobInter:
                    case VirtualObjectType.oCMobDoor:
                    case VirtualObjectType.oCMobSwitch:
                    case VirtualObjectType.oCMobFire:
                    case VirtualObjectType.oCMOB:
                    case VirtualObjectType.zCVobStair:
                    case VirtualObjectType.oCMobBed:
                    case VirtualObjectType.oCMobWheel:
                    {
                        var obj = CreateDefaultMesh(vob);
                        _cullingVobObjects.Add(obj);
                        break;
                    }
                    case VirtualObjectType.zCVobScreenFX:
                    case VirtualObjectType.zCVobAnimate:
                    case VirtualObjectType.zCTriggerWorldStart:
                    case VirtualObjectType.zCTriggerList:
                    case VirtualObjectType.oCCSTrigger:
                    case VirtualObjectType.oCTriggerScript:
                    case VirtualObjectType.zCVobLensFlare:
                    case VirtualObjectType.zCVobLight:
                    case VirtualObjectType.zCMoverController:
                    case VirtualObjectType.zCPFXController:
                    case VirtualObjectType.zCMover: 
                    case VirtualObjectType.zCVobLevelCompo:
                    case VirtualObjectType.zCZoneZFog:
                    case VirtualObjectType.zCZoneZFogDefault:
                    case VirtualObjectType.zCZoneVobFarPlane:
                    case VirtualObjectType.zCZoneVobFarPlaneDefault:
                    {
                        // FIXME - not yet implemented.
                        break;
                    }
                    default:
                    {
                        throw new Exception($"VobType={vob.Type} not yet handled. And we didn't know we need to do so. ;-)");
                    }
                }
                
                AddToMobInteractableList(vob, go);
                
                LoadingManager.I.AddProgress(LoadingManager.LoadingProgressType.VOb, 1f / _totalVObs);

                if (++_createdCount % _vobsPerFrame == 0)
                    await Task.Yield(); // Wait for the next frame
                
                // Recursive creating sub-vobs
                await CreateVobs(vob.Children);
            }
        }

        private static void PostCreateVobs()
        {
            var nonNullCullingGroupItems = _cullingVobObjects.Where(i => i != null).ToArray();
            VobMeshCullingManager.I.PrepareVobCulling(nonNullCullingGroupItems);
            VobSoundCullingManager.I.PrepareSoundCulling(LookupCache.vobSoundsAndDayTime);
            
            // TODO - warnings about "not implemented" - print them once only.
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

        private static GameObject GetPrefab(IVirtualObject vob)
        {
            GameObject go;
            string name = vob.Name;
            
            switch (vob.Type)
            {
                case VirtualObjectType.oCItem:
                    go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobItem);
                    break;
                case VirtualObjectType.zCVobSpot:
                case VirtualObjectType.zCVobStartpoint:
                    go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobSpot);
                    break;
                case VirtualObjectType.zCVobSound:
                    go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobSound);
                    break;
                case VirtualObjectType.zCVobSoundDaytime:
                    go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobSoundDaytime);
                    break;
                case VirtualObjectType.oCZoneMusic:
                    go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobMusic);
                    break;
                case VirtualObjectType.oCMOB:
                case VirtualObjectType.oCMobFire:
                case VirtualObjectType.oCMobInter:
                case VirtualObjectType.oCMobBed:
                case VirtualObjectType.oCMobDoor:
                case VirtualObjectType.oCMobContainer:
                case VirtualObjectType.oCMobSwitch:
                case VirtualObjectType.oCMobWheel:
                    go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobInteractable);
                    break;
                default:
                    return new GameObject(name);
            }
            
            go.name = name;
            
            // Fill Property data into prefab here
            // Can also be outsourced to a proper method if it becomes a lot.
            go.GetComponent<VobProperties>().SetVisual(vob.Name);
            
            return go;
        }

        private static void AddToMobInteractableList(IVirtualObject vob, GameObject go)
        {
            if (go == null)
                return;
            
            switch (vob.Type)
            {
                case VirtualObjectType.oCMOB:
                case VirtualObjectType.oCMobFire:
                case VirtualObjectType.oCMobInter:
                case VirtualObjectType.oCMobBed:
                case VirtualObjectType.oCMobDoor:
                case VirtualObjectType.oCMobContainer:
                case VirtualObjectType.oCMobSwitch:
                case VirtualObjectType.oCMobWheel:
                    GameData.VobsInteractable.Add(go.GetComponent<VobProperties>());
                    break;
            }
        }

        private static void CreateParentVobObjectTeleport(GameObject root)
        {
            var allTypes = (VirtualObjectType[])Enum.GetValues(typeof(VirtualObjectType));
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
            var allTypes = (VirtualObjectType[])Enum.GetValues(typeof(PxWorld.PxVobType));
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
        private static GameObject CreateItem(Item vob)
        {
            string itemName;

            if (!string.IsNullOrEmpty(vob.Instance))
                itemName = vob.Instance;
            else if (!string.IsNullOrEmpty(vob.Name))
                itemName = vob.Name;
            else
                throw new Exception("PxVobItemData -> no usable INSTANCE name found.");

            var item = AssetCache.TryGetItemData(itemName);

            if (item == null)
                return null;

            if (item.Visual.EndsWithIgnoreCase(".mms"))
            {
                Debug.LogError($"Item {item.Visual} is of type mms/mmb and we don't have a mesh creator to handle it properly (for now).");
                return null;
            }

            var prefabInstance = GetPrefab(vob);
            var vobObj = CreateItemMesh(vob, item, prefabInstance);

            if (vobObj == null)
            {
                GameObject.Destroy(prefabInstance); // No mesh created. Delete the prefab instance again.
                Debug.LogError($"There should be no! object which can't be found n:{vob.Name} i:{vob.Instance}. We need to use >PxVobItem.instance< to do it right!");
                return null;
            }

            // It will set some default values for collider and grabbing now.
            // Adding it now is easier than putting it on a prefab and updating it at runtime (as grabbing didn't work this way out-of-the-box).
            var grabComp = vobObj.AddComponent<XRGrabInteractable>();

            if (FeatureFlags.I.vobItemsDynamicAttach)
            {
                grabComp.useDynamicAttach = true;
                grabComp.selectMode = InteractableSelectMode.Multiple;
            }

            var itemGrabComp = vobObj.GetComponent<ItemGrabInteractable>();
            var colliderComp = vobObj.GetComponent<MeshCollider>();

            grabComp.attachTransform = itemGrabComp.attachPoint1.transform;
            grabComp.secondaryAttachTransform = itemGrabComp.attachPoint2.transform;

            vobObj.layer = Constants.ItemLayer;

            colliderComp.convex = true;
            grabComp.selectEntered.AddListener(itemGrabComp.SelectEntered);
            grabComp.selectExited.AddListener(itemGrabComp.SelectExited);

            return vobObj;
        }

        [CanBeNull]
        private static GameObject CreateMobContainer(Container vob)
        {
            var vobObj = CreateDefaultMesh(vob);

            if (vobObj == null)
            {
                Debug.LogWarning($"{vob.Name} - mesh for MobContainer not found.");
                return null;
            }

            var lootComp = vobObj.AddComponent<DemoContainerLoot>();
            lootComp.SetContent(vob.Contents);

            return vobObj;
        }

        // FIXME - change values for AudioClip based on Sfx and vob value (value overloads itself)
        [CanBeNull]
        private static GameObject CreateSound(Sound vob)
        {
            if (!FeatureFlags.I.enableSounds)
                return null;

            var go = GetPrefab(vob);
            go.name = $"{vob.SoundName}";
            go.SetActive(false); // We don't want to have sound when we boot the game async for 30 seconds in non-spatial blend mode.
            go.SetParent(parentGosNonTeleport[vob.Type]);
            SetPosAndRot(go, vob.Position, vob.Rotation);
            
            var source = go.GetComponent<AudioSource>();

            PrepareAudioSource(source, vob);
            source.clip = VobHelper.GetSoundClip(vob.SoundName);

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
        private static GameObject CreateSoundDaytime(SoundDaytime vob)
        {
            if (!FeatureFlags.I.enableSounds)
                return null;

            var go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobSoundDaytime);
            go.name = $"{vob.SoundName}-{vob.SoundNameDaytime}";
            go.SetActive(false); // We don't want to have sound when we boot the game async for 30 seconds in non-spatial blend mode.
            go.SetParent(parentGosNonTeleport[vob.Type]);
            SetPosAndRot(go, vob.Position, vob.Rotation);
            
            var sources = go.GetComponents<AudioSource>();

            PrepareAudioSource(sources[0], vob);
            sources[0].clip = VobHelper.GetSoundClip(vob.SoundName);

            PrepareAudioSource(sources[1], vob);
            sources[1].clip = VobHelper.GetSoundClip(vob.SoundNameDaytime);
            
            go.GetComponent<VobSoundDaytimeProperties>().soundDaytimeData = vob;
            go.GetComponent<SoundDaytimeHandler>().PrepareSoundHandling();

            return go;
        }

        private static void PrepareAudioSource(AudioSource source, Sound soundData)
        {
            source.maxDistance = soundData.Radius / 100f; // Gothic's values are in cm, Unity's in m.
            source.volume = soundData.Volume / 100f; // Gothic's volume is 0...100, Unity's is 0...1.

            // Random sounds shouldn't play initially, but after certain time.
            source.playOnAwake = (soundData.InitiallyPlaying && soundData.Mode != SoundMode.Random);
            source.loop = (soundData.Mode == SoundMode.Loop);
            source.spatialBlend = soundData.Ambient3d ? 1f : 0f;
        }
        
        private static GameObject CreateZoneMusic(ZoneMusic vob)
        {
            var go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobMusic);
            go.SetParent(parentGosNonTeleport[vob.Type], true, true);
            go.name = vob.Name;

            go.layer = Constants.IgnoreRaycastLayer;

            var min = vob.BoundingBox.Min.ToUnityVector();
            var max = vob.BoundingBox.Max.ToUnityVector();

            go.transform.position = (min + max) / 2f;
            go.transform.localScale = (max - min);

            go.GetComponent<VobMusicProperties>().musicData = vob;

            return go;
        }

        private static GameObject CreateTriggerChangeLevel(TriggerChangeLevel vob)
        {
            var vobObj = new GameObject(vob.Name);
            vobObj.SetParent(parentGosTeleport[vob.Type]);

            vobObj.layer = Constants.IgnoreRaycastLayer;

            var trigger = vobObj.AddComponent<BoxCollider>();
            trigger.isTrigger = true;

            var min = vob.BoundingBox.Min.ToUnityVector();
            var max = vob.BoundingBox.Max.ToUnityVector();
            vobObj.transform.position = (min + max) / 2f;

            vobObj.transform.localScale = (max - min);

            if (FeatureFlags.I.createVobs)
            {
                var triggerHandler = vobObj.AddComponent<ChangeLevelTriggerHandler>();
                triggerHandler.levelName = vob.LevelName;
                triggerHandler.startVob = vob.StartVob;
            }

            return vobObj;
        }

        /// <summary>
        /// Basically a free point where NPCs can do something like sitting on a bench etc.
        /// @see for more information: https://ataulien.github.io/Inside-Gothic/objects/spot/
        /// </summary>
        private static GameObject CreateSpot(IVirtualObject vob)
        {
            // FIXME - change to a Prefab in the future.
            var vobObj = GetPrefab(vob);

            if (!FeatureFlags.I.drawFreePointMeshes)
            {
                // Quick win: If we don't want to render the spots, we just remove the Renderer.
                GameObject.Destroy(vobObj.GetComponent<MeshRenderer>());
            }

            var fpName = vob.Name.IsEmpty() ? "START" : vob.Name;
            vobObj.name = fpName;
            vobObj.SetParent(parentGosTeleport[vob.Type]);

            var freePointData = new FreePoint()
            {
                Name = fpName,
                Position = vob.Position.ToUnityVector()
            };
            vobObj.GetComponent<VobSpotProperties>().fp = freePointData;
            GameData.FreePoints.Add(fpName, freePointData);
            
            SetPosAndRot(vobObj, vob.Position, vob.Rotation);
            return vobObj;
        }

        private static GameObject CreateLadder(IVirtualObject vob)
        {
            // FIXME - use Prefab instead. And be cautious of settings!
            var vobObj = CreateDefaultMesh(vob, true);
            var meshGo = vobObj;
            var grabComp = meshGo.AddComponent<XRGrabInteractable>();
            var rigidbodyComp = meshGo.GetComponent<Rigidbody>();
            var meshColliderComp = vobObj.GetComponentInChildren<MeshCollider>();

            meshColliderComp.convex = true; // We need to set it to overcome Physics.ClosestPoint warnings.
            meshGo.tag = Constants.ClimbableTag;
            rigidbodyComp.isKinematic = true;
            grabComp.throwOnDetach = false; // Throws errors and isn't needed as we don't want to move the kinematic ladder when released.
            grabComp.trackPosition = false;
            grabComp.trackRotation = false;
            grabComp.selectMode = InteractableSelectMode.Multiple; // With this, we can grab with both hands!

            return vobObj;
        }
        
        private static GameObject CreateItemMesh(Item vob, ItemInstance item, GameObject go)
        {
            var mrm = AssetCache.TryGetMrm(item.Visual);
            return MeshObjectCreator.CreateVob(item.Visual, mrm, vob.Position.ToUnityVector(), vob.Rotation.ToUnityQuaternion(), true, parentGosNonTeleport[vob.Type], go);
        }
        
        private static GameObject CreateItemMesh(ItemInstance item, GameObject go)
        {
            var mrm = AssetCache.TryGetMrm(item.Visual);
            return MeshObjectCreator.CreateVob(item.Visual, mrm, default, default, false, parent: go);
        }

        private static GameObject CreateDecal(IVirtualObject vob)
        {
            if (!FeatureFlags.I.enableDecals)
                return null;

            var parent = parentGosTeleport[vob.Type];

            return MeshObjectCreator.CreateVobDecal(vob, (VisualDecal)vob.Visual, parent);
        }

        /// <summary>
        /// Please check description at worldofgothic for more details:
        /// https://www.worldofgothic.de/modifikation/index.php?go=particelfx
        /// </summary>
        private static GameObject CreatePfx(IVirtualObject vob)
        {
            if (!FeatureFlags.I.enableVobParticles)
                return null;

            // FIXME - move to non-teleport
            var parent = parentGosTeleport[vob.Type];

            var pfxGo = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobPfx);
            pfxGo.name = vob.Visual!.Name;
            SetPosAndRot(pfxGo, vob.Position, vob.Rotation);
            pfxGo.SetParent(parent);

            var pfx = AssetCache.TryGetPfxData(vob.Visual.Name);
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

        private static GameObject CreateDefaultMesh(IVirtualObject vob, bool nonTeleport = false)
        {
            var parent = nonTeleport ? parentGosNonTeleport[vob.Type] : parentGosTeleport[vob.Type];
            var meshName = vob.ShowVisual ? vob.Visual!.Name : vob.Name;

            if (meshName.IsEmpty())
                return null;

            // MDL
            var mdl = AssetCache.TryGetMdl(meshName);
            if (mdl != null)
            {
                var go = GetPrefab(vob);
                var ret = MeshObjectCreator.CreateVob(meshName, mdl, vob.Position.ToUnityVector(), vob.Rotation.ToUnityQuaternion(), parent, go);
                
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
                return MeshObjectCreator.CreateVob(meshName, mdm, mdh, vob.Position.ToUnityVector(),
                    vob.Rotation.ToUnityQuaternion(), parent);
            }

            // MRM
            var mrm = AssetCache.TryGetMrm(meshName);
            if (mrm != null)
            {
                // If the object is a dynamic one, it will collide.
                var withCollider = vob.CdDynamic;

                var go = GetPrefab(vob);
                var ret = MeshObjectCreator.CreateVob(meshName, mrm, vob.Position.ToUnityVector(), vob.Rotation.ToUnityQuaternion(), withCollider, parent, go);

                // A few objects are broken and have no meshes. We need to destroy them immediately again.
                if (ret == null)
                    GameObject.Destroy(go);

                return ret;
            }

            Debug.LogWarning($">{meshName}<'s has no mdl/mrm.");
            return null;
        }
        
        private static void SetPosAndRot(GameObject obj, Vector3 position, Quaternion rotation)
        {
            SetPosAndRot(obj, position.ToUnityVector(), rotation.ToUnityQuaternion());
        }

        private static void SetPosAndRot(GameObject obj, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation)
        {
            obj.transform.SetLocalPositionAndRotation(position, rotation);
        }

        /// <summary>
        /// Some object are kind of null. We have Position only. this method is to compare with Gothic Spacer and remove if not needed later.
        /// </summary>
        private static GameObject CreateDebugObject(IVirtualObject vob)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "Empty DEBUG object. Check with Spacer if buggy.";
            SetPosAndRot(go, vob.Position, vob.Rotation);
            return go;
        }
    }
}
