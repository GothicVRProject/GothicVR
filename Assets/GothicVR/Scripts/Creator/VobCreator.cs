using GVR.Caches;
using GVR.Demo;
using GVR.Phoenix.Data;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data.Vob;
using System.Collections.Generic;
using PxCs.Data.Struct;
using UnityEngine;
using static PxCs.Interface.PxWorld;

namespace GVR.Creator
{
	public class VobCreator : SingletonBehaviour<VobCreator>
    {   
        private MeshCreator meshCreator;
        private SoundCreator soundCreator;
        private AssetCache assetCache;

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

            var vobs = new Dictionary<PxVobType, List<PxVobData>>();

            // FIXME - Currently we're loading all objects from all worlds (?)
            GetVobs(world.vobs, vobs);

            CreateAllVobs(root, vobs);
        }

        private void GetVobs(PxVobData[] inVobs, Dictionary<PxVobType, List<PxVobData>> outVobs)
        {
            foreach (var vob in inVobs)
            {
                if (!outVobs.ContainsKey(vob.type))
                    outVobs.Add(vob.type, new());

                outVobs[vob.type].Add(vob);
                GetVobs(vob.childVobs, outVobs);
            }
        }

        private void CreateAllVobs(GameObject root, Dictionary<PxVobType, List<PxVobData>> vobs)
        {
            var vobRootObj = new GameObject("Vobs");
            vobRootObj.transform.parent = root.transform;

            foreach (var vobsByType in vobs)
            {
                switch (vobsByType.Key)
                {
                    case PxVobType.PxVob_oCMobContainer:
                        CreateMobContainers(vobRootObj, vobsByType.Value);
                        break;
                    case PxVobType.PxVob_zCVobSound:
                        CreateSounds(vobRootObj, vobsByType.Value);
                        break;
                    case PxVobType.PxVob_zCVobSoundDaytime:
                        CreateSoundsDaytime(vobRootObj, vobsByType.Value);
                        break;
                    default:
                        CreateDefaultVobs(vobRootObj, vobsByType.Value, vobsByType.Key);
                        break;
                }
            }
        }

        private void CreateMobContainers(GameObject root, List<PxVobData> vobs)
        {
            var typeRoot = new GameObject("PxVob_oCMobContainer");
            typeRoot.SetParent(root);
            
            foreach (PxVobMobContainerData vob in vobs)
            {
                var vobObj = CreateDefaultVob(typeRoot, vob);

                if (vobObj == null)
                    continue;

                var lootComp = vobObj.AddComponent<DemoContainerLoot>();
                lootComp.SetContent(vob.contents);
            }
        }
        
        // FIXME - change values for AudioClip based on Sfx and vob value (value overloads itself)
        private void CreateSounds(GameObject root, List<PxVobData> vobs)
        {
            var typeRoot = new GameObject("PxVob_zCVobSound");
            typeRoot.SetParent(root);
            
            foreach (PxVobSoundData vob in vobs)
            {
                var vobObj = soundCreator.Create(vob, typeRoot);
                if (!vobObj)
                    continue;
                
                SetPosAndRot(vobObj, vob.position, vob.rotation!.Value);
            }
        }
        
        // FIXME - add specific daytime logic!
        private void CreateSoundsDaytime(GameObject root, List<PxVobData> vobs)
        {
            var typeRoot = new GameObject("PxVob_zCVobSoundDaytime");
            typeRoot.SetParent(root);
            
            foreach (PxVobSoundDaytimeData vob in vobs)
            {
                var vobObj = soundCreator.Create(vob, typeRoot);
                vobObj.name = "daytime-" + vobObj.name; // FIXME - For easy debugging purposes only. Can be removed.
                if (!vobObj)
                    continue;
                
                SetPosAndRot(vobObj, vob.position, vob.rotation!.Value);
            }
        }

        private void CreateDefaultVobs(GameObject root, List<PxVobData> vobs, PxVobType type)
        {
            var typeRoot = new GameObject(type.ToString());
            typeRoot.SetParent(root);
            
            foreach (var vob in vobs)
            {
                CreateDefaultVob(typeRoot, vob);
            }
        }

        private GameObject CreateDefaultVob(GameObject root, PxVobData vob)
        {
            var meshName = vob.showVisual ? vob.visualName : vob.vobName;

            if (meshName == string.Empty)
                return null;

            var mds = assetCache.TryGetMds(meshName);
            var mdl = assetCache.TryGetMdl(meshName);
            if (mdl != null)
            {
                return meshCreator.Create(meshName, mdl, vob.position.ToUnityVector(), vob.rotation.Value, root);
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
                
                return meshCreator.Create(meshName, mrm, vob.position.ToUnityVector(), vob.rotation.Value, withCollider, root);
            }
        }
        
        private void SetPosAndRot(GameObject obj, System.Numerics.Vector3 position, PxMatrix3x3Data rotation)
        {
            SetPosAndRot(obj, position.ToUnityVector(), rotation.ToUnityMatrix().rotation);
        }

        private void SetPosAndRot(GameObject obj, Vector3 position, Quaternion rotation)
        {
            // FIXME - This isn't working
            if (position.Equals(default) && rotation.Equals(default))
                return;

            obj.transform.localRotation = rotation;
            obj.transform.localPosition = position;
        }
    }
}
