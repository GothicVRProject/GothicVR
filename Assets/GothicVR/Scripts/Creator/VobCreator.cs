using System;
using System.Collections.Generic;
using GothicVR.Vob;
using GVR.Caches;
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

        private Dictionary<PxVobType, GameObject> parentGos = new();

        private void Start()
        {
            meshCreator = SingletonBehaviour<MeshCreator>.GetOrCreate();
            soundCreator = SingletonBehaviour<SoundCreator>.GetOrCreate();
            assetCache = SingletonBehaviour<AssetCache>.GetOrCreate();
        }

        public void Create(GameObject root, WorldData world)
        {
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().CreateVobs)
                return;
            
            var vobRootObj = new GameObject("Vobs");
            vobRootObj.SetParent(root);

            CreateParentVobObject(vobRootObj);
            CreateVobs(vobRootObj, world.vobs);
        }

        private void CreateParentVobObject(GameObject root)
        {
            foreach (var type in (PxVobType[]) Enum.GetValues(typeof(PxVobType)))
            {
                var newGo = new GameObject(type.ToString());
                newGo.SetParent(root);
                
                parentGos.Add(type, newGo);
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
                        CreateSpot(vob);
                        break;
                    case PxVobType.PxVob_oCTriggerChangeLevel:
                        CreateTriggerChangeLevel((PxVobTriggerChangeLevelData)vob);
                        break;
                    case PxVobType.PxVob_zCVobScreenFX:
                    case PxVobType.PxVob_zCVobAnimate:
                    case PxVobType.PxVob_zCVobStartpoint:
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
                        if (vob.visualType == PxVobVisualType.PxVobVisualDecal)
                            CreateDecal(vob);
                        else
                            CreateDefaultMesh(vob);
                        break;
                    default:
                        CreateDefaultMesh(vob);
                        break;
                }
                
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

            // e.g. ItMiCello is commented out on misc.d file.
            if (item == null)
                return;
            
            var prefabInstance = PrefabCache.Instance.TryGetObject(PrefabCache.PrefabType.VobItem);
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
            if (!DebugSettings.Instance.EnableSounds)
                return;
            
            var vobObj = soundCreator.Create(vob, parentGos[vob.type]);
            SetPosAndRot(vobObj, vob.position, vob.rotation!.Value);
        }
        
        // FIXME - add specific daytime logic!
        private void CreateSoundDaytime(PxVobSoundDaytimeData vob)
        {
            if (!DebugSettings.Instance.EnableSounds)
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

            // SetPosAndRot(gameObject, vob.position, vob.rotation!.Value);

            var trigger = gameObject.AddComponent<BoxCollider>();
            trigger.isTrigger = true;

            var min = vob.boundingBox.min.ToUnityVector();
            var max = vob.boundingBox.max.ToUnityVector();
            gameObject.transform.position = (min + max) / 2f;

            gameObject.transform.localScale = (max-min);

            if(SingletonBehaviour<DebugSettings>.GetOrCreate().CreateVobs)
                gameObject.AddComponent<ChangeLevelTriggerHandler>();
        }

        /// <summary>
        /// Basically a free point where NPCs can do something like sitting on a bench etc.
        /// @see for more information: https://ataulien.github.io/Inside-Gothic/objects/spot/
        /// </summary>
        private void CreateSpot(PxVobData vob)
        {
            var spot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(spot.GetComponent<SphereCollider>()); // No need for collider here!
            
            if (DebugSettings.Instance.EnableVobFPMesh)
            {
#if UNITY_EDITOR
                if (DebugSettings.Instance.EnableVobFPMeshEditorLabel)
                {
                    var iconContent = EditorGUIUtility.IconContent("sv_label_4");
                    EditorGUIUtility.SetIconForObject(spot, (Texture2D) iconContent.image);
                }
#endif
            }
            else
            {
                // Quick win: If we don't want to render the spots, we just remove the Renderer.
                // FIXME - Loading can be optimized with a proper Prefab
                Destroy(spot.GetComponent<MeshRenderer>());
            }

            spot.name = vob.vobName;
            spot.SetParent(parentGos[vob.type]);
            
            SetPosAndRot(spot, vob.position, vob.rotation!.Value);
        }

        private GameObject CreateItemMesh(PxVobItemData vob, PxVmItemData item, GameObject go)
        {
            var mrm = assetCache.TryGetMrm(item.visual);
            return meshCreator.Create(item.visual, mrm, vob.position.ToUnityVector(), vob.rotation!.Value, true, parentGos[vob.type], go);
        }

        private void CreateDecal(PxVobData vob)
        {
            var parent = parentGos[vob.type];
            
            meshCreator.CreateDecal(vob, parent);
        }
        
        private GameObject CreateDefaultMesh(PxVobData vob)
        {
            var parent = parentGos[vob.type];
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
                return meshCreator.Create(meshName, mdl, vob.position.ToUnityVector(), vob.rotation!.Value, parent);
            }
            
            // MRM
            var mrm = assetCache.TryGetMrm(meshName);
            if (mrm != null)
            {
                // If the object is a dynamic one, it will collide.
                var withCollider = vob.cdDynamic;

                return meshCreator.Create(meshName, mrm, vob.position.ToUnityVector(), vob.rotation!.Value, withCollider, parent);
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
