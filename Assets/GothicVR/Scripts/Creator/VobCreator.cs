using System;
using System.Collections.Generic;
using GothicVR.Vob;
using GVR.Caches;
using GVR.Debugging;
using GVR.Demo;
using GVR.Phoenix.Data;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Struct;
using PxCs.Data.Vm;
using PxCs.Data.Vob;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Interaction.Toolkit;
using static PxCs.Interface.PxWorld;
using Vector3 = System.Numerics.Vector3;
using System.Threading.Tasks;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace GVR.Creator
{
    public class VobCreator : SingletonBehaviour<VobCreator>
    {
        private MeshCreator meshCreator;
        private SoundCreator soundCreator;
        private AssetCache assetCache;

        private const string editorLabelColor = "sv_label4";

        private Dictionary<PxVobType, GameObject> parentGos = new();

        private void Start()
        {
            meshCreator = SingletonBehaviour<MeshCreator>.GetOrCreate();
            soundCreator = SingletonBehaviour<SoundCreator>.GetOrCreate();
            assetCache = SingletonBehaviour<AssetCache>.GetOrCreate();
        }

        public async Task Create(GameObject root, WorldData world, Action<float> progressCallback)
        {
            if (!FeatureFlags.I.CreateVobs)
                return;

            var vobRootObj = new GameObject("Vobs");
            vobRootObj.SetParent(root);
            parentGos = new();

            CreateParentVobObject(vobRootObj);
            await CreateVobs(vobRootObj, world.vobs, progress => progressCallback?.Invoke(progress));
        }

        private void CreateParentVobObject(GameObject root)
        {
            foreach (var type in (PxVobType[])Enum.GetValues(typeof(PxVobType)))
            {
                var newGo = new GameObject(type.ToString());
                newGo.SetParent(root);

                parentGos.Add(type, newGo);
            }
        }

        private async Task CreateVobs(GameObject root, PxVobData[] vobs, Action<float> progressCallback, float progress = 0.5f)
        {
            float totalProgress = progress;
            float increment = (1f - progress) / (vobs.Length + 1); // +1 for the progress update before creating each vob
            float maxProgress = totalProgress;

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
                            await CreateDefaultMesh(vob);
                        break;
                    default:
                        await CreateDefaultMesh(vob);
                        break;
                }

                // Load children
                await CreateVobs(root, vob.childVobs, progressCallback, totalProgress);

                maxProgress = Mathf.Max(maxProgress, totalProgress);
                totalProgress += increment;
            }
            progressCallback?.Invoke(maxProgress);
        }

        private async void CreateItem(PxVobItemData vob)
        {
            string itemName;

            if (!string.IsNullOrEmpty(vob.instance))
                itemName = vob.instance;
            else if (!string.IsNullOrEmpty(vob.vobName))
                itemName = vob.vobName;
            else
                throw new Exception("PxVobItemData -> no usable INSTANCE name found.");

            var item = assetCache.TryGetItemData(itemName);

            // e.g. ItMiCello is commented out on misc.d file.
            if (item == null)
            {
                Debug.LogError($"Item {itemName} not found.");
                return;
            }

            if (item.visual.ToLower().EndsWith(".mms"))
            {
                Debug.LogError($"Item {item.visual} is of type mms/mmb and we don't have a mesh creator to handle it properly (for now).");
                return;
            }

            var prefabInstance = PrefabCache.I.TryGetObject(PrefabCache.PrefabType.VobItem);
            var vobObj = await CreateItemMesh(vob, item, prefabInstance);

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

            colliderComp.convex = true;
            grabComp.selectExited.AddListener(eventComp.SelectExited);
        }

        private async void CreateMobContainer(PxVobMobContainerData vob)
        {
            var vobObj = await CreateDefaultMesh(vob);

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

            var vobObj = soundCreator.Create(vob, parentGos[vob.type]);
            SetPosAndRot(vobObj, vob.position, vob.rotation!.Value);
        }

        // FIXME - add specific daytime logic!
        private void CreateSoundDaytime(PxVobSoundDaytimeData vob)
        {
            if (!FeatureFlags.I.EnableSounds)
                return;

            var vobObj = soundCreator.Create(vob, parentGos[vob.type]);
            SetPosAndRot(vobObj, vob.position, vob.rotation!.Value);
        }

        private void CreateZoneMusic(PxVobZoneMusicData vob)
        {
            soundCreator.Create(vob, parentGos[vob.type]);
        }

        private void CreateTriggerChangeLevel(PxVobTriggerChangeLevelData vob)
        {

            var gameObject = new GameObject(vob.vobName);
            gameObject.SetParent(parentGos[vob.type]);

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
            spot.tag = "PxVob_zCVobSpot";
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

            spot.name = vob.vobName != string.Empty ? vob.vobName : "START";
            spot.SetParent(parentGos[vob.type]);

            SetPosAndRot(spot, vob.position, vob.rotation!.Value);
        }

        private async void CreateLadder(PxVobData vob)
        {
            // FIXME - use Prefab instead.
            var go = await CreateDefaultMesh(vob);
            var meshGo = go;
            var grabComp = meshGo.AddComponent<XRGrabInteractable>();
            var rigidbodyComp = meshGo.GetComponent<Rigidbody>();

            meshGo.tag = "Climbable";
            rigidbodyComp.isKinematic = true;
            grabComp.trackPosition = false;
            grabComp.trackRotation = false;
        }

        private async Task<GameObject> CreateItemMesh(PxVobItemData vob, PxVmItemData item, GameObject go)
        {
            var mrm = await assetCache.TryGetMrmAsync(item.visual);
            return await meshCreator.Create(item.visual, mrm, vob.position.ToUnityVector(), vob.rotation!.Value, true, parentGos[vob.type], go);
        }

        private async void CreateDecal(PxVobData vob)
        {
            if (!FeatureFlags.Instance.EnableDecals)
            {
                return;
            }
            var parent = parentGos[vob.type];

            await meshCreator.CreateDecal(vob, parent);
        }

        private async Task<GameObject> CreateDefaultMesh(PxVobData vob)
        {
            var parent = parentGos[vob.type];
            var meshName = vob.showVisual ? vob.visualName : vob.vobName;

            if (meshName == string.Empty)
                return null;
            if (meshName.ToLower().EndsWith(".pfx"))
                // FIXME - PFX effects not yet implemented
                return null;

            // MDL
            var mdl = await assetCache.TryGetMdlAsync(meshName);
            if (mdl != null)
            {
                return await meshCreator.Create(meshName, mdl, vob.position.ToUnityVector(), vob.rotation!.Value, parent);
            }

            // MRM
            var mrm = await assetCache.TryGetMrmAsync(meshName);
            if (mrm != null)
            {
                // If the object is a dynamic one, it will collide.
                var withCollider = vob.cdDynamic;

                return await meshCreator.Create(meshName, mrm, vob.position.ToUnityVector(), vob.rotation!.Value, withCollider, parent);
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
