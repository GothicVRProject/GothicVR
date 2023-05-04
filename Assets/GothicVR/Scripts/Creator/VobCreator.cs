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
using PxCs.Interface;
using GVR.Phoenix.Interface;
using System;

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

            foreach (var vob in vobs.Values.SelectMany(i => i.SelectMany(ii => ii.childVobs)))
            {
                var mdl = assetCache.TryGetMdl(vob.vobName);
                if (mdl != null)
                {
                    if (mdl.mesh.meshes.Length == 0)
                    {
                        var attachmentKeys = mdl.hierarchy.nodes.Select(i => i.name).ToArray();
                        var mdm = assetCache.TryGetMdm(vob.vobName, attachmentKeys);

                        if (mdm != null)
                        {
                            meshCreator.Create(vob.vobName, mdm, mdl.hierarchy, vob.position.ToUnityVector(), vob.rotation.Value, vobRootObj);
                        }
                        else
                        {
                            Debug.LogWarning($">{vob.vobName}<'s .mdm not found.");
                            continue;
                        }
                    }
                    else
                    {
                        meshCreator.Create(vob.vobName, mdl.mesh, mdl.hierarchy, vob.position.ToUnityVector(), vob.rotation.Value, vobRootObj);
                    }
                }
                else
                {
                    var mrm = assetCache.TryGetMrm(vob.vobName);
                    if (mrm == null)
                    {
                        Debug.LogWarning($">{vob.vobName}<'s .mrm not found.");
                        continue;
                    }

                    meshCreator.Create(vob.vobName, mrm, vob.position.ToUnityVector(), vob.rotation.Value, vobRootObj);
                }

            }
        }
    }
}
