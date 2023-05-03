using GVR.Demo;
using GVR.Phoenix.Data;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data;
using PxCs.Interface;
using System.Collections.Generic;
using UnityEngine;
using static PxCs.Interface.PxWorld;

namespace GVR.Creator
{
    public class VobCreator : SingletonBehaviour<VobCreator>
    {   
        private static AssetCache assetCache;

        private void Start()
        {
            assetCache = SingletonBehaviour<AssetCache>.GetOrCreate();
        }

        public void Create(GameObject root, WorldData world)
        {
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().CreateVobs)
                return;

            var vobs = new Dictionary<PxWorld.PxVobType, List<PxVobData>>();
            GetVobs(world.vobs, vobs);

            CreateItems(root, vobs);
            CreateContainers(root, vobs);
        }

        /// <summary>
        /// Convenient method to return specific vob elements in recursive list of PxVobData.childVobs...
        /// </summary>
        private void GetVobs(PxVobData[] inVobs, Dictionary<PxWorld.PxVobType, List<PxVobData>> outVobs)
        {
            foreach (var vob in inVobs)
            {
                if (!outVobs.ContainsKey(vob.type))
                    outVobs.Add(vob.type, new());

                outVobs[vob.type].Add(vob);
                GetVobs(vob.childVobs, outVobs);
            }
        }

        private void CreateItems(GameObject root, Dictionary<PxWorld.PxVobType, List<PxVobData>> vobs)
        {
            var vobRootObj = new GameObject("Vob-Items");
            vobRootObj.transform.parent = root.transform;

            foreach (var vob in vobs[PxVobType.PxVob_oCItem])
            {
                // FIXME: Add caching of MRM as object will be created multiple times inside a scene.
                var mrm = assetCache.TryGetMrm(vob.vobName);

                if (mrm == null)
                {
                    Debug.LogError($"MultiResolutionModel (MRM) >{vob.vobName}.MRM< not found.");
                    continue;
                }

                SingletonBehaviour<MeshCreator>.GetOrCreate()
                    .Create(vob.vobName, mrm, vob.position.ToUnityVector(), vob.rotation.Value, vobRootObj);
            }
        }

        private void CreateMob(GameObject root, Dictionary<PxVobType, List<PxVobData>> vobs)
        {

        }

        private void CreateContainers(GameObject root, Dictionary<PxVobType, List<PxVobData>> vobs)
        {
            var vobRootObj = new GameObject("Vob-Containers");
            vobRootObj.transform.parent = root.transform;

            foreach (var vob in vobs[PxVobType.PxVob_oCMobContainer])
            {
                var mds = assetCache.TryGetMds(vob.visualName);
                var mdh = assetCache.TryGetMdh(vob.visualName);
                var mdm = assetCache.TryGetMdm(mds.skeleton.name);

                if (mdm == null)
                {
                    Debug.LogWarning($">{mds.skeleton.name}< not found.");
                    continue;
                }

                SingletonBehaviour<MeshCreator>.GetOrCreate()
                    .Create(vob.vobName, mdm, mdh, vob.position.ToUnityVector(), vob.rotation.Value, vobRootObj);
            }
        }
    }
}
