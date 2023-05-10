using GVR.Demo;
using GVR.Caches;
using GVR.Phoenix.Data;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PxCs.Interface.PxWorld;

namespace GVR.Creator
{
    public class VobCreator : SingletonBehaviour<VobCreator>
    {   
        private MeshCreator meshCreator;
        private AssetCache assetCache;

        private void Start()
        {
            meshCreator = SingletonBehaviour<MeshCreator>.GetOrCreate();
            assetCache = SingletonBehaviour<AssetCache>.GetOrCreate();
        }

        public void Create(GameObject root, WorldData world)
        {
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().CreateVobs)
                return;

            var vobs = new Dictionary<PxVobType, List<PxVobData>>();

            // FIXME - Currently we're loading all objects from all worlds (?)
            GetVobs(world.vobs, vobs);

            CreateMrmVobs(root, vobs);
            //CreateItems(root, vobs);
            //CreateContainers(root, vobs);
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

        private void CreateItems(GameObject root, Dictionary<PxVobType, List<PxVobData>> vobs)
        {
            var vobRootObj = new GameObject("Vob-Items");
            vobRootObj.transform.parent = root.transform;

            foreach (var vob in vobs[PxVobType.PxVob_oCItem])
            {
                var mrm = assetCache.TryGetMrm(vob.vobName);

                if (mrm == null)
                {
                    Debug.LogWarning($"MultiResolutionModel (MRM) >{vob.vobName}.MRM< not found.");
                    continue;
                }

                meshCreator.Create(vob.vobName, mrm, vob.position.ToUnityVector(), vob.rotation.Value, vobRootObj);
            }
        }

        private void CreateMrmVobs(GameObject root, Dictionary<PxVobType, List<PxVobData>> vobs)
        {
            var vobRootObj = new GameObject("Vobs");
            vobRootObj.transform.parent = root.transform;

            foreach (var vob in vobs.Values.SelectMany(i => i.SelectMany(ii => ii.childVobs)))
            {
                var mrm = assetCache.TryGetMrm(vob.vobName);

                if (mrm == null)
                    continue;

                meshCreator.Create(vob.vobName, mrm, vob.position.ToUnityVector(), vob.rotation.Value, vobRootObj);
            }
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

                meshCreator.Create(vob.vobName, mdm, mdh, vob.position.ToUnityVector(), vob.rotation.Value, vobRootObj);
            }
        }
    }
}
