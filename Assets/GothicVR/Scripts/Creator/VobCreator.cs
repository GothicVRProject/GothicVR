using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GothicVR.Vob;
using GVR.Caches;
using GVR.Creator.Meshes;
using GVR.Debugging;
using GVR.Demo;
using GVR.Manager;
using GVR.Phoenix.Data;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Struct;
using PxCs.Data.Vm;
using PxCs.Data.Vob;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using static PxCs.Interface.PxWorld;
using Vector3 = System.Numerics.Vector3;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace GVR.Creator
{
    public class VobCreator : SingletonBehaviour<VobCreator>
    {
        private SoundCreator soundCreator;
        private AssetCache assetCache;

        private const string editorLabelColor = "sv_label4";

        private Dictionary<PxVobType, GameObject> parentGosTeleport = new();
        private Dictionary<PxVobType, GameObject> parentGosNonTeleport = new();
        private readonly PxVobType[] nonTeleportTypes =
        {
            PxVobType.PxVob_oCItem ,
            PxVobType.PxVob_oCMobLadder,
            PxVobType.PxVob_oCZoneMusic
        };
        
        private int totalVObs;

        private void Start()
        {
            soundCreator = SoundCreator.I;
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

        public async Task CreateAsync(GameObject rootTeleport, GameObject rootNonTeleport, WorldData world, int vobsPerFrame)
        {
            if (!FeatureFlags.I.CreateVobs)
                return;

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
                switch (vob.type)
                {
                    case PxVobType.PxVob_oCItem:
                        CreateItem((PxVobItemData)vob);
                        break;
                    case PxVobType.PxVob_oCMobContainer:
                        CreateMobContainer((PxVobMobContainerData)vob);
                        break;
                    case PxVobType.PxVob_zCVobSound:
                        CreateSound((PxVobSoundData)vob);
                        break;
                    case PxVobType.PxVob_zCVobSoundDaytime:
                        CreateSoundDaytime((PxVobSoundDaytimeData)vob);
                        break;
                    case PxVobType.PxVob_oCZoneMusic:
                        CreateZoneMusic((PxVobZoneMusicData)vob);
                        break;
                    case PxVobType.PxVob_zCVobSpot:
                    case PxVobType.PxVob_zCVobStartpoint:
                        CreateSpot(vob);
                        break;
                    case PxVobType.PxVob_oCMobLadder:
                        CreateLadder(vob);
                        break;
                    case PxVobType.PxVob_oCTriggerChangeLevel:
                        CreateTriggerChangeLevel((PxVobTriggerChangeLevelData)vob);
                        break;
                    case PxVobType.PxVob_zCVobScreenFX:
                    case PxVobType.PxVob_zCVobAnimate:
                    case PxVobType.PxVob_zCTriggerWorldStart:
                    case PxVobType.PxVob_zCTriggerList:
                    case PxVobType.PxVob_oCCSTrigger:
                    case PxVobType.PxVob_oCTriggerScript:
                    case PxVobType.PxVob_zCVobLensFlare:
                    case PxVobType.PxVob_zCVobLight:
                    case PxVobType.PxVob_zCMoverController:
                    case PxVobType.PxVob_zCPFXController:
                        Debug.LogWarning($"{vob.type} not yet implemented.");
                        break;
                    // Do nothing
                    case PxVobType.PxVob_zCVobLevelCompo:
                        break;
                    case PxVobType.PxVob_zCVob:
                        // if (vob.visualType == PxVobVisualType.PxVobVisualDecal)
                        // CreateDecal(vob);
                        // else
                        CreateDefaultMesh(vob);
                        break;
                    default:
                        CreateDefaultMesh(vob);
                        break;
                }

                LoadingManager.I.AddProgress(LoadingManager.LoadingProgressType.VOb, 1f / totalVObs);

                if (++count % vobsPerFrame == 0)
                    await Task.Yield(); // Wait for the next frame
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
            var allTypes = (PxVobType[])Enum.GetValues(typeof(PxVobType));
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
            var allTypes = (PxVobType[])Enum.GetValues(typeof(PxVobType));
            foreach (var type in allTypes.Intersect(nonTeleportTypes))
            {
                var newGo = new GameObject(type.ToString());
                newGo.SetParent(root);

                parentGosNonTeleport.Add(type, newGo);
            }
        }

        private void CreateVobs(GameObject root, PxVobData[] vobs)
        {
            foreach (var vob in vobs)
            {
                switch (vob.type)
                {
                    case PxVobType.PxVob_oCItem:
                        CreateItem((PxVobItemData)vob);
                        break;
                    case PxVobType.PxVob_oCMobContainer:
                        CreateMobContainer((PxVobMobContainerData)vob);
                        break;
                    case PxVobType.PxVob_zCVobSound:
                        CreateSound((PxVobSoundData)vob);
                        break;
                    case PxVobType.PxVob_zCVobSoundDaytime:
                        CreateSoundDaytime((PxVobSoundDaytimeData)vob);
                        break;
                    case PxVobType.PxVob_oCZoneMusic:
                        CreateZoneMusic((PxVobZoneMusicData)vob);
                        break;
                    case PxVobType.PxVob_zCVobSpot:
                    case PxVobType.PxVob_zCVobStartpoint:
                        CreateSpot(vob);
                        break;
                    case PxVobType.PxVob_oCMobLadder:
                        CreateLadder(vob);
                        break;
                    case PxVobType.PxVob_oCTriggerChangeLevel:
                        CreateTriggerChangeLevel((PxVobTriggerChangeLevelData)vob);
                        break;
                    case PxVobType.PxVob_zCVobScreenFX:
                    case PxVobType.PxVob_zCVobAnimate:
                    case PxVobType.PxVob_zCTriggerWorldStart:
                    case PxVobType.PxVob_zCTriggerList:
                    case PxVobType.PxVob_oCCSTrigger:
                    case PxVobType.PxVob_oCTriggerScript:
                    case PxVobType.PxVob_zCVobLensFlare:
                    case PxVobType.PxVob_zCVobLight:
                    case PxVobType.PxVob_zCMoverController:
                    case PxVobType.PxVob_zCPFXController:
                        Debug.LogWarning($"{vob.type} not yet implemented.");
                        break;
                    // Do nothing
                    case PxVobType.PxVob_zCVobLevelCompo:
                        break;
                    case PxVobType.PxVob_zCVob:
                        // if (vob.visualType == PxVobVisualType.PxVobVisualDecal)
                        // CreateDecal(vob);
                        // else
                        CreateDefaultMesh(vob);
                        break;
                    default:
                        CreateDefaultMesh(vob);
                        break;
                }

                LoadingManager.I.AddProgress(LoadingManager.LoadingProgressType.VOb, 1f / totalVObs);
                // Load children
                CreateVobs(root, vob.childVobs);
            }
        }

        private void CreateItem(PxVobItemData vob)
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
                    return;
                
                Debug.LogError($"Item {itemName} not found.");
                return;
            }

            if (item.visual!.ToLower().EndsWith(".mms"))
            {
                Debug.LogError($"Item {item.visual} is of type mms/mmb and we don't have a mesh creator to handle it properly (for now).");
                return;
            }

            var prefabInstance = PrefabCache.I.TryGetObject(PrefabCache.PrefabType.VobItem);
            var vobObj = CreateItemMesh(vob, item, prefabInstance);

            if (vobObj == null)
            {
                Destroy(prefabInstance); // No mesh created. Delete the prefab instance again.
                Debug.LogError($"There should be no! object which can't be found n:{vob.vobName} i:{vob.instance}. We need to use >PxVobItem.instance< to do it right!");
                return;
            }

            // It will set some default values for collider and grabbing now.
            // Adding it now is easier than putting it on a prefab and updating it at runtime (as grabbing didn't work this way out-of-the-box).
            var grabComp = vobObj.AddComponent<XRGrabInteractable>();
            var eventComp = vobObj.GetComponent<ItemGrabInteractable>();
            var colliderComp = vobObj.GetComponent<MeshCollider>();

            vobObj.layer = ConstantsManager.I.ItemLayer;

            colliderComp.convex = true;
            grabComp.selectExited.AddListener(eventComp.SelectExited);
        }

        private void CreateMobContainer(PxVobMobContainerData vob)
        {
            var vobObj = CreateDefaultMesh(vob);

            if (vobObj == null)
            {
                Debug.LogWarning($"{vob.vobName} - mesh for MobContainer not found.");
                return;
            }

            var lootComp = vobObj.AddComponent<DemoContainerLoot>();
            lootComp.SetContent(vob.contents);
        }

        // FIXME - change values for AudioClip based on Sfx and vob value (value overloads itself)
        private void CreateSound(PxVobSoundData vob)
        {
            if (!FeatureFlags.I.EnableSounds)
                return;

            var vobObj = soundCreator.Create(vob, parentGosTeleport[vob.type]);
            SetPosAndRot(vobObj, vob.position, vob.rotation!.Value);
        }

        // FIXME - add specific daytime logic!
        private void CreateSoundDaytime(PxVobSoundDaytimeData vob)
        {
            if (!FeatureFlags.I.EnableSounds)
                return;

            var vobObj = soundCreator.Create(vob, parentGosTeleport[vob.type]);
            SetPosAndRot(vobObj, vob.position, vob.rotation!.Value);
        }

        private void CreateZoneMusic(PxVobZoneMusicData vob)
        {
            var go = PrefabCache.I.TryGetObject(PrefabCache.PrefabType.VobMusic);
            go.SetParent(parentGosNonTeleport[vob.type], true, true);
            go.name = vob.vobName;
            
            var min = vob.boundingBox.min.ToUnityVector();
            var max = vob.boundingBox.max.ToUnityVector();

            go.transform.position = (min + max) / 2f;
            go.transform.localScale = (max - min);

            go.GetComponent<VobMusicProperties>().SetMusicData(vob);
        }

        private void CreateTriggerChangeLevel(PxVobTriggerChangeLevelData vob)
        {

            var gameObject = new GameObject(vob.vobName);
            gameObject.SetParent(parentGosTeleport[vob.type]);

            var trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;

            var min = vob.boundingBox.min.ToUnityVector();
            var max = vob.boundingBox.max.ToUnityVector();
            gameObject.transform.position = (min + max) / 2f;

            gameObject.transform.localScale = (max - min);

            if (FeatureFlags.I.CreateVobs)
            {
                var triggerHandler = gameObject.AddComponent<ChangeLevelTriggerHandler>();
                triggerHandler.levelName = vob.levelName;
                triggerHandler.startVob = vob.startVob;
            }
        }

        /// <summary>
        /// Basically a free point where NPCs can do something like sitting on a bench etc.
        /// @see for more information: https://ataulien.github.io/Inside-Gothic/objects/spot/
        /// </summary>
        private void CreateSpot(PxVobData vob)
        {
            // FIXME - change to a Prefab in the future.
            var spot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            spot.tag = ConstantsManager.SpotTag;
            Destroy(spot.GetComponent<SphereCollider>()); // No need for collider here!

            if (FeatureFlags.I.EnableVobFPMesh)
            {
#if UNITY_EDITOR
                if (FeatureFlags.I.EnableVobFPMeshEditorLabel)
                {
                    var iconContent = EditorGUIUtility.IconContent(editorLabelColor);
                    EditorGUIUtility.SetIconForObject(spot, (Texture2D)iconContent.image);
                }
#endif
            }
            else
            {
                // Quick win: If we don't want to render the spots, we just remove the Renderer.
                // FIXME - Loading can be optimized with a proper Prefab
                Destroy(spot.GetComponent<MeshRenderer>());
            }

            var fpName = vob.vobName != string.Empty ? vob.vobName : "START";
            spot.name = fpName;
            spot.SetParent(parentGosTeleport[vob.type]);

            GameData.I.FreePoints.Add(fpName, new()
            {
                Name = fpName,
                Position = vob.position.ToUnityVector()
            });
            
            SetPosAndRot(spot, vob.position, vob.rotation!.Value);
        }

        private void CreateLadder(PxVobData vob)
        {
            // FIXME - use Prefab instead.
            var go = CreateDefaultMesh(vob, true);
            var meshGo = go;
            var grabComp = meshGo.AddComponent<XRGrabInteractable>();
            var rigidbodyComp = meshGo.GetComponent<Rigidbody>();

            meshGo.tag = "Climbable";
            rigidbodyComp.isKinematic = true;
            grabComp.trackPosition = false;
            grabComp.trackRotation = false;
        }

        private GameObject CreateItemMesh(PxVobItemData vob, PxVmItemData item, GameObject go)
        {
            var mrm = assetCache.TryGetMrm(item.visual);
            return VobMeshCreator.I.Create(item.visual, mrm, vob.position.ToUnityVector(), vob.rotation!.Value, true, parentGosNonTeleport[vob.type], go);
        }

        private void CreateDecal(PxVobData vob)
        {
            if (!FeatureFlags.I.EnableDecals)
            {
                return;
            }
            var parent = parentGosTeleport[vob.type];

            VobMeshCreator.I.CreateDecal(vob, parent);
        }

        private GameObject CreateDefaultMesh(PxVobData vob, bool nonTeleport = false)
        {
            var parent = nonTeleport ? parentGosNonTeleport[vob.type] : parentGosTeleport[vob.type];
            var meshName = vob.showVisual ? vob.visualName : vob.vobName;

            if (meshName == string.Empty)
                return null;
            if (meshName.ToLower().EndsWith(".pfx"))
                // FIXME - PFX effects not yet implemented
                return null;

            // MDL
            var mdl = assetCache.TryGetMdl(meshName);
            if (mdl != null)
            {
                return VobMeshCreator.I.Create(meshName, mdl, vob.position.ToUnityVector(), vob.rotation!.Value.ToUnityMatrix().rotation, parent);
            }

            // MRM
            var mrm = assetCache.TryGetMrm(meshName);
            if (mrm != null)
            {
                // If the object is a dynamic one, it will collide.
                var withCollider = vob.cdDynamic;

                return VobMeshCreator.I.Create(meshName, mrm, vob.position.ToUnityVector(), vob.rotation!.Value, withCollider, parent);
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
            // FIXME - This isn't working
            if (position.Equals(default) && rotation.Equals(default))
                return;

            obj.transform.localRotation = rotation;
            obj.transform.localPosition = position;
        }
    }
}
