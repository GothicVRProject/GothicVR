using GVR.Caches;
using GVR.Demo;
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

            foreach (var vobType in vobs)
            {
                foreach (var vob in vobType.Value)
                {
                    string meshName;
                    if (vob.showVisual)
                        meshName = vob.visualName;
                    else
                        meshName = vob.visualName;

                    var mdl = assetCache.TryGetMdl(meshName);
                    if (mdl != null)
                    {
                        meshCreator.Create(meshName, mdl, vob.position.ToUnityVector(), vob.rotation.Value, vobRootObj);
                    }
                    else
                    {
                        var mrm = assetCache.TryGetMrm(meshName);
                        if (mrm == null)
                        {
                            Debug.LogWarning($">{meshName}<'s .mrm not found.");
                            continue;
                        }

                        meshCreator.Create(meshName, mrm, vob.position.ToUnityVector(), vob.rotation.Value, vobRootObj);
                    }
                }
            }
        }
    }
}
