using System;
using System.Collections.Generic;
using GothicVR.Vob;
using GVR.Caches;
using GVR.Demo;
using GVR.Phoenix.Data;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Struct;
using PxCs.Data.Vm;
using PxCs.Data.Vob;
using PxCs.Interface;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using static PxCs.Interface.PxWorld;
using Vector3 = System.Numerics.Vector3;

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
                        CreateSoundDaytime((PxVobSoundData)vob);
                        break;
                    case PxVobType.PxVob_oCZoneMusic:
                        CreateZoneMusic((PxVobZoneMusicData)vob);
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
            
            var item = PxVm.InitializeItem(PhoenixBridge.VmGothicPtr, itemName);

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
            var vobObj = soundCreator.Create(vob, parentGos[vob.type]);
            SetPosAndRot(vobObj, vob.position, vob.rotation!.Value);
        }
        
        // FIXME - add specific daytime logic!
        private void CreateSoundDaytime(PxVobSoundData vob)
        {
            var vobObj = soundCreator.Create(vob, parentGos[vob.type]);
            SetPosAndRot(vobObj, vob.position, vob.rotation!.Value);
        }

        private void CreateZoneMusic(PxVobZoneMusicData vob)
        {
            soundCreator.Create(vob, parentGos[vob.type]);
        }

        private GameObject CreateItemMesh(PxVobItemData vob, PxVmItemData item, GameObject go)
        {
            var mrm = assetCache.TryGetMrm(item.visual);
            return meshCreator.Create(item.visual, mrm, vob.position.ToUnityVector(), vob.rotation!.Value, true, parentGos[vob.type], go);
        }
        
        private GameObject CreateDefaultMesh(PxVobData vob)
        {
            var parent = parentGos[vob.type];
            var meshName = vob.showVisual ? vob.visualName : vob.vobName;

            if (meshName == string.Empty)
                return null;

            var mds = assetCache.TryGetMds(meshName);
            var mdl = assetCache.TryGetMdl(meshName);
            if (mdl != null)
            {
                return meshCreator.Create(meshName, mdl, vob.position.ToUnityVector(), vob.rotation!.Value, parent);
            }
            else
            {
                var mrm = assetCache.TryGetMrm(meshName);
                if (mrm == null)
                {
                    Debug.LogWarning($">{meshName}<'s .mrm not found.");
                    return null;
                }

                // If the object is a dynamic one, it will collide.
                var withCollider = vob.cdDynamic;
                
                return meshCreator.Create(meshName, mrm, vob.position.ToUnityVector(), vob.rotation!.Value, withCollider, parent);
            }
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
