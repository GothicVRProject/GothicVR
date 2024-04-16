using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GothicVR.Vob;
using GVR.Caches;
using GVR.Creator.Meshes.V2;
using GVR.Debugging;
using GVR.Demo;
using GVR.Extensions;
using GVR.Globals;
using GVR.GothicVR.Scripts.Manager;
using GVR.Manager;
using GVR.Manager.Culling;
using GVR.Npc;
using GVR.Player.Interactive;
using GVR.Properties;
using GVR.Vob;
using GVR.Vob.WayNet;
using GVR.World;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit;
using ZenKit;
using ZenKit.Daedalus;
using ZenKit.Util;
using ZenKit.Vobs;
using Debug = UnityEngine.Debug;
using Light = ZenKit.Vobs.Light;
using LightType = ZenKit.Vobs.LightType;
using Material = UnityEngine.Material;
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
            VirtualObjectType.zCVobSoundDaytime,
            VirtualObjectType.zCVobAnimate
        };

        private static int _totalVObs;
        private static int _vobsPerFrame;
        private static int _createdCount;
        private static List<GameObject> _cullingVobObjects = new();
        private static Dictionary<string, IWorld> vobTreeCache = new();

        static VobCreator()
        {
            GvrEvents.GeneralSceneLoaded.AddListener(PostWorldLoaded);
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

        public static async Task CreateAsync(GameObject rootTeleport, GameObject rootNonTeleport, WorldData world, int vobsPerFrame)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            PreCreateVobs(world, rootTeleport, rootNonTeleport, vobsPerFrame);
            await CreateVobs(world.Vobs);
            PostCreateVobs();
            stopwatch.Stop();
            Debug.Log($"Created vobs in {stopwatch.Elapsed.TotalSeconds} s");
        }

        private static void PreCreateVobs(WorldData world, GameObject rootTeleport, GameObject rootNonTeleport, int vobsPerFrame)
        {
            _totalVObs = GetTotalVobCount(world.Vobs);

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

        private static int GetTotalVobCount(List<IVirtualObject> vobs)
        {
            return vobs.Count + vobs.Sum(vob => GetTotalVobCount(vob.Children));
        }

        private static async Task CreateVobs(List<IVirtualObject> vobs, GameObject parent = null, bool reparent = false)
        {
            foreach (var vob in vobs)
            {
                GameObject go = null;

                // Debug - Skip loading if not wanted.
                if (FeatureFlags.I.vobTypeToSpawn.IsEmpty() || FeatureFlags.I.vobTypeToSpawn.Contains(vob.Type))
                {
                    go = reparent ? LoadVob(vob, parent) : LoadVob(vob);
                }

                AddToMobInteractableList(vob, go);

                if (++_createdCount % _vobsPerFrame == 0)
                    await Task.Yield(); // Wait for the next frame

                // Recursive creating sub-vobs
                await CreateVobs(vob.Children, go, reparent);
                LoadingManager.I.AddProgress(LoadingManager.LoadingProgressType.VOb, 1f / _totalVObs);
            }
        }

        [CanBeNull]
        private static GameObject LoadVob(IVirtualObject vob, GameObject parent = null)
        {
            GameObject go = null;
            switch (vob.Type)
            {
                case VirtualObjectType.oCItem:
                {
                    go = CreateItem((Item)vob, parent);
                    _cullingVobObjects.Add(go);
                    break;
                }
                case VirtualObjectType.zCVobLight:
                {
                    go = CreateLight((Light)vob, parent);
                    break;
                }
                case VirtualObjectType.oCMobContainer:
                {
                    go = CreateMobContainer((Container)vob, parent);
                    _cullingVobObjects.Add(go);
                    break;
                }
                case VirtualObjectType.zCVobSound:
                {
                    go = CreateSound((Sound)vob, parent);
                    LookupCache.vobSoundsAndDayTime.Add(go);
                    break;
                }
                case VirtualObjectType.zCVobSoundDaytime:
                {
                    go = CreateSoundDaytime((SoundDaytime)vob, parent);
                    LookupCache.vobSoundsAndDayTime.Add(go);
                    break;
                }
                case VirtualObjectType.oCZoneMusic:
                case VirtualObjectType.oCZoneMusicDefault:
                {
                    go = CreateZoneMusic((ZoneMusic)vob, parent);
                    break;
                }
                case VirtualObjectType.zCVobSpot:
                case VirtualObjectType.zCVobStartpoint:
                {
                    go = CreateSpot(vob, parent);
                    break;
                }
                case VirtualObjectType.oCMobLadder:
                {
                    go = CreateLadder(vob, parent);
                    _cullingVobObjects.Add(go);
                    break;
                }
                case VirtualObjectType.oCTriggerChangeLevel:
                {
                    go = CreateTriggerChangeLevel((TriggerChangeLevel)vob, parent);
                    break;
                }
                case VirtualObjectType.zCVob:
                {
                    if (vob.Visual == null)
                    {
                        CreateDebugObject(vob, parent);
                        break;
                    }

                    switch (vob.Visual!.Type)
                    {
                        case VisualType.Decal:
                            go = CreateDecal(vob, parent);
                            break;
                        case VisualType.ParticleEffect:
                            go = CreatePfx(vob, parent);
                            break;
                        default:
                            go = CreateDefaultMesh(vob, parent);
                            break;
                    }

                    _cullingVobObjects.Add(go);
                    break;
                }
                case VirtualObjectType.oCMobFire:
                {
                    go = CreateFire((Fire)vob, parent);
                    _cullingVobObjects.Add(go);
                    break;
                }
                case VirtualObjectType.oCMobInter:
                    {
                        if (vob.Name.ContainsIgnoreCase("bench") ||
                            vob.Name.ContainsIgnoreCase("chair") ||
                            vob.Name.ContainsIgnoreCase("throne") ||
                            vob.Name.ContainsIgnoreCase("barrelo"))
                        {
                            go = CreateSeat(vob, parent);
                            _cullingVobObjects.Add(go);
                            break;
                        }
                        else
                        {
                            go = CreateDefaultMesh(vob);
                            _cullingVobObjects.Add(go);
                            break;
                        }
                    }
                case VirtualObjectType.oCMobDoor:
                case VirtualObjectType.oCMobSwitch:
                case VirtualObjectType.oCMOB:
                case VirtualObjectType.zCVobStair:
                case VirtualObjectType.oCMobBed:
                case VirtualObjectType.oCMobWheel:
                {
                    go = CreateDefaultMesh(vob, parent);
                    _cullingVobObjects.Add(go);
                    break;
                }
                case VirtualObjectType.zCVobAnimate:
                {
                    go = CreateAnimatedVob((Animate)vob, parent);
                    _cullingVobObjects.Add(go);
                    break;
                }
                case VirtualObjectType.zCVobScreenFX:
                case VirtualObjectType.zCTriggerWorldStart:
                case VirtualObjectType.zCTriggerList:
                case VirtualObjectType.oCCSTrigger:
                case VirtualObjectType.oCTriggerScript:
                case VirtualObjectType.zCVobLensFlare:
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
                    throw new Exception(
                        $"VobType={vob.Type} not yet handled. And we didn't know we need to do so. ;-)");
                }
            }

            return go;
        }

        /// <summary>
        /// Some fire slots have the light too low to cast light onto the mesh and the surroundings.
        /// </summary>
        private static GameObject CreateFire(Fire vob, GameObject parent = null)
        {
            var go = CreateDefaultMesh(vob, parent);

            if (vob.VobTree == "")
                return go;

            if (!vobTreeCache.TryGetValue(vob.VobTree.ToLower(), out IWorld vobTree))
            {
                vobTree = new ZenKit.World(GameData.Vfs, vob.VobTree, GameVersion.Gothic1);
                vobTreeCache.Add(vob.VobTree.ToLower(), vobTree);
            }
            
            foreach (var vobRoot in vobTree.RootObjects)
            {
                ResetVobTreePositions(vobRoot.Children, vobRoot.Position, vobRoot.Rotation);
                vobRoot.Position = Vector3.Zero;
            }

            CreateVobs(vobTree.RootObjects, go.FindChildRecursively(vob.Slot) ?? go, true);

            return go;
        }

        /// <summary>
        /// Reset the positions of the objects in the list to subtract position of the parent
        /// In the zen files all the vobs have the position represented for the world not per parent
        /// and as we might load multiple copies of the same vob tree but for different parents we need to reset the position
        /// </summary>
        /// <param name="vobList"></param>
        /// <param name="position"></param>
        private static void ResetVobTreePositions(List<IVirtualObject> vobList, Vector3 position = default, Matrix3x3 rotation = default)
        {
            if (vobList == null)
                return;

            foreach (var vob in vobList)
            {
                ResetVobTreePositions(vob.Children, position, rotation);

                vob.Position -= position;

                vob.Rotation = new Matrix3x3(vob.Rotation.M11 - rotation.M11, vob.Rotation.M12 - rotation.M12,
                    vob.Rotation.M13 - rotation.M13, vob.Rotation.M21 - rotation.M21,
                    vob.Rotation.M22 - rotation.M22, vob.Rotation.M23 - rotation.M23,
                    vob.Rotation.M31 - rotation.M31, vob.Rotation.M32 - rotation.M32,
                    vob.Rotation.M33 - rotation.M33);
            }
        }

        private static void PostCreateVobs()
        {
            VobMeshCullingManager.I.PrepareVobCulling(_cullingVobObjects);
            VobSoundCullingManager.I.PrepareSoundCulling(LookupCache.vobSoundsAndDayTime);

            vobTreeCache.ClearAndReleaseMemory();
            
            // TODO - warnings about "not implemented" - print them once only.
            foreach (var var in new[]
                     {
                         VirtualObjectType.zCVobScreenFX,
                         VirtualObjectType.zCVobAnimate,
                         VirtualObjectType.zCTriggerWorldStart,
                         VirtualObjectType.zCTriggerList,
                         VirtualObjectType.oCCSTrigger,
                         VirtualObjectType.oCTriggerScript,
                         VirtualObjectType.zCVobLensFlare,
                         VirtualObjectType.zCMoverController,
                         VirtualObjectType.zCPFXController
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
                    go = PrefabCache.TryGetObject(PrefabCache.PrefabType.Vob);
                    break;
                case VirtualObjectType.oCMobFire:
                case VirtualObjectType.oCMobInter:
                case VirtualObjectType.oCMobBed:
                case VirtualObjectType.oCMobWheel:
                case VirtualObjectType.oCMobSwitch:
                    go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobInteractable);
                    break;
                case VirtualObjectType.oCMobDoor:
                    go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobDoor);
                    break;
                case VirtualObjectType.oCMobContainer:
                    go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobContainer);
                    break;
                case VirtualObjectType.zCVobAnimate:
                    go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobAnimate);
                    break;
                default:
                    return new GameObject(name);
            }

            go.name = name;

            // Fill Property data into prefab here
            // Can also be outsourced to a proper method if it becomes a lot.
            go.GetComponent<VobProperties>().SetData(vob);

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
                    var propertiesComponent = go.GetComponent<VobProperties>();

                    if (propertiesComponent == null)
                        Debug.LogError($"VobProperties component missing on {go.name} ({vob.Type})");

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
            var allTypes = (VirtualObjectType[])Enum.GetValues(typeof(VirtualObjectType));
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
        public static void CreateItem(int itemId, GameObject go)
        {
            var item = AssetCache.TryGetItemData(itemId);

            CreateItemMesh(item, go);
        }

        public static void CreateItem(int itemId, string spawnpoint, GameObject go)
        {
            var item = AssetCache.TryGetItemData(itemId);

            var position = WayNetHelper.GetWayNetPoint(spawnpoint).Position;

            CreateItemMesh(item, go, position);
        }

        public static void CreateItem(string itemName, GameObject go)
        {
            if (itemName == "")
                return;

            var item = AssetCache.TryGetItemData(itemName);

            CreateItemMesh(item, go);
        }

        [CanBeNull]
        private static GameObject CreateItem(Item vob, GameObject parent = null)
        {
            string itemName;

            if (!string.IsNullOrEmpty(vob.Instance))
                itemName = vob.Instance;
            else if (!string.IsNullOrEmpty(vob.Name))
                itemName = vob.Name;
            else
                throw new Exception("Vob Item -> no usable name found.");

            var item = AssetCache.TryGetItemData(itemName);

            if (item == null)
                return null;

            var prefabInstance = GetPrefab(vob);
            var vobObj = CreateItemMesh(vob, item, prefabInstance, parent);

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
        private static GameObject CreateLight(Light vob, GameObject parent = null)
        {
            if (vob.LightStatic)
            {
                return null;
            }

            GameObject vobObj = new GameObject($"{vob.LightType} Light {vob.Name}");
            vobObj.SetParent(parent ?? parentGosTeleport[vob.Type], true, true);
            SetPosAndRot(vobObj, vob.Position, vob.Rotation);

            StationaryLight lightComp = vobObj.AddComponent<StationaryLight>();
            lightComp.Color = new Color(vob.Color.R / 255f, vob.Color.G / 255f, vob.Color.B / 255f, vob.Color.A / 255f);
            lightComp.Type = vob.LightType == LightType.Point
                ? UnityEngine.LightType.Point
                : UnityEngine.LightType.Spot;
            lightComp.Range = vob.Range * .01f;
            lightComp.SpotAngle = vob.ConeAngle;
            lightComp.Intensity = 1;

            return vobObj;
        }

        [CanBeNull]
        private static GameObject CreateMobContainer(Container vob, GameObject parent = null)
        {
            var vobObj = CreateDefaultMesh(vob, parent);

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
        private static GameObject CreateSound(Sound vob, GameObject parent = null)
        {
            if (!FeatureFlags.I.enableSounds)
                return null;

            var go = GetPrefab(vob);
            go.name = $"{vob.SoundName}";
            go.SetActive(false); // We don't want to have sound when we boot the game async for 30 seconds in non-spatial blend mode.
            go.SetParent(parent ?? parentGosNonTeleport[vob.Type], true, true);
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
        private static GameObject CreateSoundDaytime(SoundDaytime vob, GameObject parent = null)
        {
            if (!FeatureFlags.I.enableSounds)
                return null;

            var go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobSoundDaytime);
            go.name = $"{vob.SoundName}-{vob.SoundNameDaytime}";
            go.SetActive(false); // We don't want to have sound when we boot the game async for 30 seconds in non-spatial blend mode.
            go.SetParent(parent ?? parentGosNonTeleport[vob.Type], true, true);
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

        private static GameObject CreateZoneMusic(ZoneMusic vob, GameObject parent = null)
        {
            var go = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobMusic);
            go.SetParent(parent ?? parentGosNonTeleport[vob.Type], true, true);
            go.name = vob.Name;

            go.layer = Constants.IgnoreRaycastLayer;

            var min = vob.BoundingBox.Min.ToUnityVector();
            var max = vob.BoundingBox.Max.ToUnityVector();

            go.transform.position = (min + max) / 2f;
            go.transform.localScale = (max - min);

            go.GetComponent<VobMusicProperties>().musicData = vob;

            return go;
        }

        private static GameObject CreateTriggerChangeLevel(TriggerChangeLevel vob, GameObject parent = null)
        {
            var vobObj = new GameObject(vob.Name);
            vobObj.SetParent(parent ?? parentGosTeleport[vob.Type], true, true);

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
        private static GameObject CreateSpot(IVirtualObject vob, GameObject parent = null)
        {
            // FIXME - change to a Prefab in the future.
            var vobObj = GetPrefab(vob);

            if (!FeatureFlags.I.drawFreePoints)
            {
                // Quick win: If we don't want to render the spots, we just remove the Renderer.
                GameObject.Destroy(vobObj.GetComponent<MeshRenderer>());
            }

            var fpName = vob.Name.IsEmpty() ? "START" : vob.Name;
            vobObj.name = fpName;
            vobObj.SetParent(parent ?? parentGosTeleport[vob.Type], true, true);

            var freePointData = new FreePoint
            {
                Name = fpName,
                Position = vob.Position.ToUnityVector(),
                Direction = vob.Rotation.ToUnityQuaternion().eulerAngles
            };
            vobObj.GetComponent<VobSpotProperties>().fp = freePointData;
            GameData.FreePoints.Add(fpName, freePointData);

            SetPosAndRot(vobObj, vob.Position, vob.Rotation);
            return vobObj;
        }

        private static GameObject CreateLadder(IVirtualObject vob, GameObject parent = null)
        {
            var vobObj = CreateDefaultMesh(vob, parent, true);

            // We will set some default values for collider and grabbing now.
            // Adding it now is easier than putting it on a prefab and updating it at runtime (as grabbing didn't work this way out-of-the-box).
            // e.g. grabComp's colliders aren't recalculated if we have the XRGrabInteractable set in Prefab.
            var grabComp = vobObj.AddComponent<XRGrabInteractable>();
            var rigidbodyComp = vobObj.GetComponent<Rigidbody>();
            var meshColliderComp = vobObj.GetComponentInChildren<MeshCollider>();

            meshColliderComp.convex = true; // We need to set it to overcome Physics.ClosestPoint warnings.
            vobObj.tag = Constants.ClimbableTag;
            rigidbodyComp.isKinematic = true;
            grabComp.throwOnDetach = false; // Throws errors and isn't needed as we don't want to move the kinematic ladder when released.
            grabComp.trackPosition = false;
            grabComp.trackRotation = false;
            grabComp.selectMode = InteractableSelectMode.Multiple; // With this, we can grab with both hands!

            return vobObj;
        }

        private static GameObject CreateSeat(IVirtualObject vob, GameObject parent = null)
        {
            //to be used for creating chairs, benches etc
            //based on Creating Ladder
            var vobObj = CreateDefaultMesh(vob);
            var meshColliderComp = vobObj.GetComponentInChildren<MeshCollider>();

            var grabComp = meshColliderComp.gameObject.AddComponent<XRGrabInteractable>();
            var rigidbodyComp = meshColliderComp.gameObject.GetComponent<Rigidbody>();
            
            Seat seat = meshColliderComp.gameObject.AddComponent<Seat>();

            meshColliderComp.convex = true; 

            rigidbodyComp.isKinematic = true;
            grabComp.throwOnDetach = false; 
            grabComp.trackPosition = false;
            grabComp.trackRotation = false;

            return vobObj;
        }

        private static GameObject CreateItemMesh(Item vob, ItemInstance item, GameObject go, GameObject parent = null)
        {
            var mrm = AssetCache.TryGetMrm(item.Visual);
            return MeshFactory.CreateVob(item.Visual, mrm, vob.Position.ToUnityVector(), vob.Rotation.ToUnityQuaternion(),
                true, parent ?? parentGosNonTeleport[vob.Type], go, useTextureArray: false);
        }

        private static GameObject CreateItemMesh(ItemInstance item, GameObject parentGo, UnityEngine.Vector3 position = default)
        {
            var mrm = AssetCache.TryGetMrm(item.Visual);
            return MeshFactory.CreateVob(item.Visual, mrm, position, default, false, parent: parentGo, useTextureArray: false);
        }

        private static GameObject CreateDecal(IVirtualObject vob, GameObject parent = null)
        {
            if (!FeatureFlags.I.enableDecals)
                return null;

            return MeshFactory.CreateVobDecal(vob, (VisualDecal)vob.Visual, parent ?? parentGosTeleport[vob.Type]);
        }

        /// <summary>
        /// Please check description at worldofgothic for more details:
        /// https://www.worldofgothic.de/modifikation/index.php?go=particelfx
        /// </summary>
        private static GameObject CreatePfx(IVirtualObject vob, GameObject parent = null)
        {
            if (!FeatureFlags.I.enableVobParticles)
                return null;

            var pfxGo = PrefabCache.TryGetObject(PrefabCache.PrefabType.VobPfx);
            pfxGo.name = vob.Visual!.Name;

            // if parent exists then set rotation before parent (for correct rotation vob trees)
            if (parent)
            {
                SetPosAndRot(pfxGo, vob.Position, vob.Rotation);
                pfxGo.SetParent(parent, true);
            }
            else
            {
                pfxGo.SetParent(parent??parentGosTeleport[vob.Type], true, true);
                SetPosAndRot(pfxGo, vob.Position, vob.Rotation);
            }

            var pfx = AssetCache.TryGetPfxData(vob.Visual.Name);
            var particleSystem = pfxGo.GetComponent<ParticleSystem>();

            pfxGo.GetComponent<VobPfxProperties>().pfxData = pfx;

            particleSystem.Stop();

            var gravity = pfx.FlyGravityS.Split();
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
                var minLifeTime = (pfx.LspPartAvg - pfx.LspPartVar) / 1000; // I assume we need to change milliseconds to seconds.
                var maxLifeTime = (pfx.LspPartAvg + pfx.LspPartVar) / 1000;
                mainModule.duration = 1f; // I assume pfx data wants a cycle being 1 second long.
                mainModule.startLifetime = new(minLifeTime, maxLifeTime);
                mainModule.loop = Convert.ToBoolean(pfx.PpsIsLooping);

                var minSpeed = (pfx.VelAvg - pfx.VelVar) / 1000;
                var maxSpeed = (pfx.VelAvg + pfx.VelVar) / 1000;
                mainModule.startSpeed = new(minSpeed, maxSpeed);
            }

            // Emission module
            {
                var emissionModule = particleSystem.emission;
                emissionModule.rateOverTime = new ParticleSystem.MinMaxCurve(pfx.PpsValue);
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
                var colorStart = pfx.VisTexColorStartS.Split();
                var colorEnd = pfx.VisTexColorEndS.Split();
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
                        new GradientAlphaKey(pfx.VisAlphaStart / 255, 0),
                        new GradientAlphaKey(pfx.VisAlphaEnd / 255, 1),
                    });
                colorOverTime.color = gradient;
            }

            // Size over lifetime module
            {
                var sizeOverTime = particleSystem.sizeOverLifetime;
                sizeOverTime.enabled = true;

                AnimationCurve curve = new AnimationCurve();
                var shapeScaleKeys = pfx.ShpScaleKeysS.Split();
                if (shapeScaleKeys.Length > 1 && !pfx.ShpScaleKeysS.IsEmpty())
                {
                    var curveTime = 0f;

                    foreach (var key in shapeScaleKeys)
                    {
                        curve.AddKey(curveTime, float.Parse(key) / 100 * float.Parse(pfx.ShpDimS));
                        curveTime += 1f / shapeScaleKeys.Length;
                    }

                    sizeOverTime.size = new ParticleSystem.MinMaxCurve(1f, curve);
                }
            }

            // Renderer module
            {
                var rendererModule = pfxGo.GetComponent<ParticleSystemRenderer>();
                // FIXME - Move to a cached constant value
                var standardShader = Constants.ShaderUnlitParticles;
                var material = new Material(standardShader);
                rendererModule.material = material;
                TextureManager.I.SetTexture(pfx.VisNameS, rendererModule.material);
                // renderer.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest; // First check with no change.

                switch (pfx.VisAlphaFuncS.ToUpper())
                {
                    case "BLEND":
                        rendererModule.material.ToTransparentMode(); // e.g. leaves.pfx.
                        break;
                    case "ADD":
                        rendererModule.material.ToAdditiveMode();
                        break;
                    default:
                        Debug.LogWarning($"Particle AlphaFunc {pfx.VisAlphaFuncS} not yet handled.");
                        break;
                }
                // makes the material render both faces
                rendererModule.material.SetInt("_Cull", (int)CullMode.Off);

                switch (pfx.VisOrientationS)
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
                        Debug.LogWarning($"visOrientation {pfx.VisOrientationS} not yet handled.");
                        break;
                }
            }

            // Shape module
            {
                var shapeModule = particleSystem.shape;
                switch (pfx.ShpTypeS.ToUpper())
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
                        Debug.LogWarning($"Particle ShapeType {pfx.ShpTypeS} not yet handled.");
                        break;
                }

                var shapeDimensions = pfx.ShpDimS.Split();
                switch (shapeDimensions.Length)
                {
                    case 1:
                        shapeModule.radius = float.Parse(shapeDimensions[0], CultureInfo.InvariantCulture) / 100; // cm in m
                        break;
                    default:
                        Debug.LogWarning($"shpDim >{pfx.ShpDimS}< not yet handled");
                        break;
                }

                shapeModule.rotation = new(pfx.DirAngleElev, 0, 0);

                var shapeOffsetVec = pfx.ShpOffsetVecS.Split();
                if (float.TryParse(shapeOffsetVec[0], out var x) && float.TryParse(shapeOffsetVec[1], out var y) &&
                    float.TryParse(shapeOffsetVec[2], out var z))
                    shapeModule.position = new UnityEngine.Vector3(x / 100, y / 100, z / 100);

                shapeModule.alignToDirection = true;

                shapeModule.radiusThickness = Convert.ToBoolean(pfx.ShpIsVolume) ? 1f : 0f;
            }

            particleSystem.Play();

            return pfxGo;
        }
        
        private static GameObject CreateAnimatedVob(Animate vob, GameObject parent = null)
        {
            var go = CreateDefaultMesh(vob, parent, true);
            var morph = go.AddComponent<VobAnimateMorph>();
            morph.StartAnimation(vob.Visual!.Name);
            return go;
        }

        private static GameObject CreateDefaultMesh(IVirtualObject vob, GameObject parent = null, bool nonTeleport = false)
        {
            var parentGo = nonTeleport ? parentGosNonTeleport[vob.Type] : parentGosTeleport[vob.Type];
            var meshName = vob.ShowVisual ? vob.Visual!.Name : vob.Name;

            if (meshName.IsEmpty())
                return null;

            var go = GetPrefab(vob);

            // MDL
            var mdl = AssetCache.TryGetMdl(meshName);
            if (mdl != null)
            {
                var ret = MeshFactory.CreateVob(meshName, mdl, vob.Position.ToUnityVector(), vob.Rotation.ToUnityQuaternion(), parent ?? parentGo, go);

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
                var ret = MeshFactory.CreateVob(meshName, mdm, mdh, vob.Position.ToUnityVector(),
                    vob.Rotation.ToUnityQuaternion(), parent ?? parentGo, go);

                // A few objects are broken and have no meshes. We need to destroy them immediately again.
                if (ret == null)
                    GameObject.Destroy(go);

                return ret;
            }
            
            // MMB
            var mmb = AssetCache.TryGetMmb(meshName);
            if (mmb != null)
            {
                var ret = MeshFactory.CreateVob(meshName, mmb, vob.Position.ToUnityVector(),
                    vob.Rotation.ToUnityQuaternion(), parent ?? parentGo, go);
                
                // this is a dynamic object 

                // A few objects are broken and have no meshes. We need to destroy them immediately again.
                if (ret == null)
                    GameObject.Destroy(go);
                
                return ret;
            }

            // MRM
            var mrm = AssetCache.TryGetMrm(meshName);
            if (mrm != null)
            {
                // If the object is a dynamic one, it will collide.
                var withCollider = vob.CdDynamic;

                var ret = MeshFactory.CreateVob(meshName, mrm, vob.Position.ToUnityVector(), vob.Rotation.ToUnityQuaternion(), withCollider, parent ?? parentGo, go);

                // A few objects are broken and have no meshes. We need to destroy them immediately again.
                if (ret == null)
                    GameObject.Destroy(go);

                return ret;
            }

            Debug.LogWarning($">{meshName}<'s has no mdl/mrm.");
            return null;
        }

        private static void SetPosAndRot(GameObject obj, Vector3 position, Matrix3x3 rotation)
        {
            SetPosAndRot(obj, position.ToUnityVector(), rotation.ToUnityQuaternion());
        }

        private static void SetPosAndRot(GameObject obj, UnityEngine.Vector3 position, Quaternion rotation)
        {
            obj.transform.SetLocalPositionAndRotation(position, rotation);
        }

        /// <summary>
        /// Some object are kind of null. We have Position only. this method is to compare with Gothic Spacer and remove if not needed later.
        /// </summary>
        private static GameObject CreateDebugObject(IVirtualObject vob, GameObject parent = null)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"{vob.Name} - Empty DEBUG object. Check with Spacer if buggy.";
            SetPosAndRot(go, vob.Position, vob.Rotation);
            return go;
        }
    }
}
