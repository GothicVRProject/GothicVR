using GVR.Demo;
using GVR.Phoenix.Data;
using GVR.Phoenix.Interface;
using GVR.Phoenix.Util;
using GVR.Util;
using PxCs.Data;
using PxCs.Interface;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace GVR.Creator
{
    public class VobCreator : SingletonBehaviour<VobCreator>
    {
        // Cache helped speed up loading of G1 world textures from 870ms to 230 (~75% speedup)
        private Dictionary<string, Texture2D> cachedTextures = new();

        public void Create(GameObject root, WorldData world)
        {
            if (!SingletonBehaviour<DebugSettings>.GetOrCreate().CreateVobs)
                return;

            CreateItems(root, world);
            CreateContainers(root, world);


            // Currently we don't need to store cachedTextures once the world is loaded.
            cachedTextures.Clear();
        }

        /// <summary>
        /// Convenient method to return specific vob elements in recursive list of PxVobData.childVobs...
        /// </summary>
        private List<PxVobData> GetFlattenedVobsByType(PxVobData[] vobsToFilter, PxWorld.PxVobType type)
        {
            var returnVobs = new List<PxVobData>();
            for (var i = 0; i < vobsToFilter.Length; i++)
            {
                var curVob = vobsToFilter[i];
                if (curVob.type == type)
                    returnVobs.Add(curVob);

                returnVobs.AddRange(GetFlattenedVobsByType(curVob.childVobs, type));
            }

            return returnVobs;
        }


        private void CreateItems(GameObject root, WorldData world)
        {
            var itemVobs = GetFlattenedVobsByType(world.vobs, PxWorld.PxVobType.PxVob_oCItem);
            var vobRootObj = new GameObject("Vob-Items");
            vobRootObj.transform.parent = root.transform;

            foreach (var vob in itemVobs)
            {
                // FIXME: Add caching of MRM as object will be created multiple times inside a scene.
                var mrm = PxMultiResolutionMesh.GetMRMFromVdf(PhoenixBridge.VdfsPtr, $"{vob.vobName}.MRM");

                if (mrm == null)
                {
                    Debug.LogError($"MultiResolutionModel (MRM) >{vob.vobName}.MRM< not found.");
                    continue;
                }

                SingletonBehaviour<MeshCreator>.GetOrCreate()
                    .Create(vob.vobName, mrm, vob.position.ToUnityVector(), vob.rotation.Value, vobRootObj);
            }
        }

        private void CreateContainers(GameObject root, WorldData world)
        {
            var itemVobs = GetFlattenedVobsByType(world.vobs, PxWorld.PxVobType.PxVob_oCMobContainer);
            var vobRootObj = new GameObject("Vob-Containers");
            vobRootObj.transform.parent = root.transform;

            foreach (var vob in itemVobs)
            {
                var mdsName = vob.visualName;
                var mdhName = mdsName.Replace(".MDS", ".MDH", System.StringComparison.OrdinalIgnoreCase);

                var mds = PxModelScript.GetModelScriptFromVdf(PhoenixBridge.VdfsPtr, mdsName);
                var mdh = PxModelHierarchy.LoadFromVdf(PhoenixBridge.VdfsPtr, mdhName);

                var mdmName = mds.skeleton.name.Replace(".ASC", ".MDM", System.StringComparison.OrdinalIgnoreCase);
                var mdm = PxModelMesh.LoadModelMeshFromVdf(PhoenixBridge.VdfsPtr, mdmName);

                if (mdm == null)
                {
                    Debug.LogWarning($">{mdmName}< not found.");
                    continue;
                }

                SingletonBehaviour<MeshCreator>.GetOrCreate()
                    .Create(vob.vobName, mdm, mdh, vob.position.ToUnityVector(), vob.rotation.Value, vobRootObj);
            }
        }
    }
}
